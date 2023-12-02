using Godot;
using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;

public class BuildableInfoListWrapper
{
	public List<BuildableInfo> buildableInfos;
}

public class Vector2Info
{
	public float x_value;
	public float y_value;
	public int sockettype_bit_mask;
}
public class BuildableInfo
{
	public int buildable_id;
	public string buildable_name;
	public string path_name;
	public List<string> categories;
	public List<Vector2Info> socket_connectabilty_points;
	public int dimension_x;
	public int dimension_y;
	public float texture_scale_x;
	public float texture_scale_y;
}

public class TextureScaleHandler
{
	public Texture smallTexture;
	public Texture mediumTexture;
	public Texture largeTexture;

	public TextureScaleHandler(Texture smallTexture, Texture mediumTexture, Texture largeTexture)
	{
		this.smallTexture = smallTexture;
		this.mediumTexture = mediumTexture;
		this.largeTexture = largeTexture;
	}
}

public class BuildableEditor : Node2D
{
	public bool _DEBUG = true;
	public bool _SCREEN_DEBUG;

	public static string _BUILD_GROUP = "BUILD_GROUP";

	public Vector2 _cursorLocation;

	public Dictionary<int, Buildable> _buildables_dictionary;

	public Dictionary<int, List<string>> _buildables_categories;
	// public Dictionary<string, List<int>> _categories_buildables;
	public Dictionary<int, TextureScaleHandler> _buildables_icon_textures;
	public Dictionary<int, int> _buildables_palette;
	public Dictionary<int, float> _buildables_palette_rotations;
	public Dictionary<int, Vector2> _buildables_dimensions;//sprite and socket dimensions..
	public Dictionary<int, Vector2> _buildables_texture_scales;

	public Buildable _queued_buildable;

	public List<Buildable> _selected_buildables;

	public MenuContainer _menu_container;
	public Panel _palette_container;

	public static float _GRID_BLOCK_SIZE = 100;
	public Vector2 _GRID_ORIGIN;

	public bool _in_build_mode;
	public bool _in_menu_mode;
	public bool _in_selection_mode;
	public Node2D _buildablesRoot;

	public override void _Ready()
	{
		_buildables_dictionary = new Dictionary<int, Buildable>();
		_buildables_categories = new Dictionary<int, List<string>>();
		_buildables_icon_textures = new Dictionary<int, TextureScaleHandler>();
		_buildables_palette = new Dictionary<int, int>();
		_buildables_palette_rotations = new Dictionary<int, float>();
		_buildables_dimensions = new Dictionary<int, Vector2>();
		_buildables_texture_scales = new Dictionary<int, Vector2>();

		_palette_container = (Panel)FindNode("PanelPalette");
		

		SetToSelectionMode();
		_buildablesRoot = new Node2D();
		AddChild(_buildablesRoot);
		LoadBuildableInfoFromJson();
		LoadAndPopulateMenuContainer();
		DisableMenu();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion eventMouseMotion)
		{

			_cursorLocation = eventMouseMotion.Position;
			if(_SCREEN_DEBUG && _in_selection_mode)
			{
				GD.Print("new selection mode _cursorLocation: ", _cursorLocation);
			}
			if(!_in_menu_mode && _queued_buildable != null)
			{
				_queued_buildable.Position = SnapToGrid(_cursorLocation);
			}
		}
		if (@event is InputEventMouseButton eventMouseButton)
		{
			if (!eventMouseButton.Pressed)
			{
				return;
			}
			if (_in_build_mode && _queued_buildable != null)
			{
				if (ValidPlacement(_queued_buildable))
				{
					AddChild((Buildable)_queued_buildable.Duplicate());
				}
				//add code for invalid placement
			}
		}
		if (@event is InputEventKey eventKeyPress)
		{
			if (Input.IsActionPressed("ui_focus_next"))
			{
				if (!_in_menu_mode)
					SetToMenuMode();
				else
					SetToBuildMode();
				return;
			}
			if (_in_menu_mode)
			{
				int hovered_buildableId = HoveredBuildableId();
				if(_DEBUG)
				{
					GD.Print("keypress in menu mode detected");
					GD.Print("Hovered buildable id: ", hovered_buildableId);
					if(hovered_buildableId == -1)
						GD.Print("WARNING: no hovered buildable found");
				}
				
				//Buildable hovered_buildable = _buildables_dictionary[_buildableId].Duplicate();
				if(Input.IsActionPressed("ui_1"))
					UpdatePalette(1, hovered_buildableId);
				if(Input.IsActionPressed("ui_2"))
					UpdatePalette(2, hovered_buildableId);
				if(Input.IsActionPressed("ui_3"))
					UpdatePalette(3, hovered_buildableId);
			}
			if (_in_build_mode)//update _queued_buildable
			{
				int buildableId = -1;
				if (Input.IsActionPressed("ui_1"))
					AssignQueuedBuildableFromPalette(1);
				else if (Input.IsActionPressed("ui_2"))
					AssignQueuedBuildableFromPalette(2);
				else if (Input.IsActionPressed("ui_3"))
					AssignQueuedBuildableFromPalette(3);
				else
					return;
			}
		}
	}

	public override void _PhysicsProcess(float Delta)
	{

	}

	public void AssignQueuedBuildableFromPalette(int paletteBlock)
	{
		int buildableId = _buildables_palette[paletteBlock];
		// RemoveChild(_queued_buildable);
		if(_queued_buildable != null)
		{
			RemoveChild(_queued_buildable);
			// _queued_buildable.Dispose();
		}

		_queued_buildable = (Buildable)_buildables_dictionary[buildableId].Duplicate();
		string texture_path_prefix = "top_down";
		Buildable buildable = _buildables_dictionary[buildableId];
		float rotation = _buildables_palette_rotations[paletteBlock];
		String texture_path = String.Format("res://{0}/{1}{2}_{3}.png", buildable.buildablePathName, buildable.buildablePathName, texture_path_prefix, (int)rotation);
		Texture texture = GD.Load<Texture>(texture_path);
		// _queued_buildable.Rotation = ;
		_queued_buildable.dimensions = _buildables_dimensions[buildableId];//sprite and socket dimensions..
		_queued_buildable.SetTexture(texture, _GRID_BLOCK_SIZE);
		// _queued_buildable.Scale = buildable.textureScale;
		AddChild(_queued_buildable);
	}

	public void SetPaletteButtons()//might not need this?
	{
		foreach(Node node in _palette_container.GetChildren())
		{
			if(node is BuildableButton buildableButton)
			{
				buildableButton.isPaletteItem = true;
			}
		}
	}

	public int HoveredBuildableId()
	{
		ScrollContainer scrollContainer = (ScrollContainer)_menu_container.GetCurrentTabControl();
		
		foreach(Node node in scrollContainer.GetChildren())
		{
			if(node is MenuGridContainer menuGridContainer)
			{
				return menuGridContainer.HoveredBuildableId();
			}
		}
		return -1;

	}


	public void LoadBuildableInfoFromJson()
	{
		string content;
		File file = new File();
		file.Open("res://Buildables.json", File.ModeFlags.Read);
		content = file.GetAsText();
		file.Close();
		// string fileName = "C:\\Users\\NoSpacesForWSL\\OneDrive\\Documents\\GODOTProjs\\BasicBuilder\\Buildables.json";
		BuildableInfoListWrapper buildableInfoListWrapper = JsonConvert.DeserializeObject<BuildableInfoListWrapper>(content);

		string texture_path_prefix = "icon";
		// int rotation = 0;
		string small_texture_path_suffix = "32x32";
		string medium_texture_path_suffix = "64x64";
		string large_texture_path_suffix = "128x128";

		string texture_path;
		Texture small_texture;
		Texture medium_texture;
		Texture large_texture;
		Buildable buildable;
		TextureScaleHandler textureScaleHandler;

		foreach (BuildableInfo buildableInfo in buildableInfoListWrapper.buildableInfos)
		{
			
			var buildableScene = GD.Load<PackedScene>("res://Buildable.tscn"); // Will load when the script is instanced.
			buildable = (Buildable)buildableScene.Instance();

			buildable.dimensions = new Vector2(buildableInfo.dimension_x, buildableInfo.dimension_y);
			if(_DEBUG)
			{
				GD.Print("buildable with dimensions: ", buildable.dimensions);
			}
			buildable.buildableName = buildableInfo.buildable_name;
			buildable.buildablePathName = buildableInfo.path_name;

			Dictionary<Vector2, int> socketConnectabilityPoints = new Dictionary<Vector2, int>();
			foreach (Vector2Info vector2Info in buildableInfo.socket_connectabilty_points)
			{
				socketConnectabilityPoints[new Vector2(vector2Info.x_value, vector2Info.y_value)] = vector2Info.sockettype_bit_mask;
			}
			buildable.socketConnectabilityMap = socketConnectabilityPoints;

			texture_path = String.Format("res://{0}/{1}{2}_size{3}.png", buildableInfo.path_name, buildableInfo.path_name, texture_path_prefix, small_texture_path_suffix);
			small_texture = GD.Load<Texture>(texture_path);
			buildable.smallMenuTexture = small_texture;

			texture_path = String.Format("res://{0}/{1}{2}_size{3}.png", buildableInfo.path_name, buildableInfo.path_name, texture_path_prefix, medium_texture_path_suffix);
			medium_texture = GD.Load<Texture>(texture_path);
			buildable.mediumMenuTexture = medium_texture;

			texture_path = String.Format("res://{0}/{1}{2}_size{3}.png", buildableInfo.path_name, buildableInfo.path_name, texture_path_prefix, large_texture_path_suffix);
			large_texture = GD.Load<Texture>(texture_path);
			buildable.LargeMenuTexture = large_texture;

			textureScaleHandler = new TextureScaleHandler(small_texture, medium_texture, large_texture);


			_buildables_categories[buildableInfo.buildable_id] = buildableInfo.categories;
			_buildables_icon_textures[buildableInfo.buildable_id] = textureScaleHandler;
			_buildables_dimensions[buildableInfo.buildable_id] = new Vector2(buildableInfo.dimension_x, buildableInfo.dimension_y);
			_buildables_texture_scales[buildableInfo.buildable_id] = new Vector2(buildableInfo.texture_scale_x, buildableInfo.texture_scale_y);
			_buildables_dictionary[buildableInfo.buildable_id] = buildable;
		}
	}

	public void LoadAndPopulateMenuContainer()
	{
		var menuScene = GD.Load<PackedScene>("res://Menu.tscn"); // Will load when the script is instanced.
		_menu_container = (MenuContainer)menuScene.Instance();
		AddChild(_menu_container);

		var buildableButtonScene = GD.Load<PackedScene>("res://BuildableButton.tscn"); // Will load when the script is instanced.
		BuildableButton buildableButton;

		if (_DEBUG)
		{
			GD.Print("_buildables_dictionary:\n", _buildables_dictionary.Count);
			GD.Print("_buildables_dictionary Buildable 0 dimensions:\n", _buildables_dictionary[0].dimensions);
			GD.Print("_buildables_categories:\n", _buildables_categories.Count);
			GD.Print("_buildables_categories Buildable 0 category 0:\n", _buildables_categories[0][0][0]);
			GD.Print("_buildables_icon_textures:\n", _buildables_icon_textures.Count);
		}

		foreach (KeyValuePair<int, List<string>> kvp in _buildables_categories)
		{
			foreach (string category in kvp.Value)
			{
				if (_DEBUG)
				{
					GD.Print("Adding buildable with\nid: ", kvp.Key, "\ncategory: ", kvp.Value, "\nname: ", _buildables_dictionary[kvp.Key].Name);
				}
				buildableButton = (BuildableButton)buildableButtonScene.Instance();
				buildableButton.SetTexture(_buildables_icon_textures[kvp.Key].mediumTexture);//, _buildables_texture_scales[kvp.Key]);
				buildableButton.buildableId = kvp.Key;
				buildableButton.isPaletteItem = false;
				_menu_container.AddToTab(category, buildableButton);
			}
		}
	}

	public void DisableMenu()
	{
		if (_menu_container == null)
		{
			return;
		}
		_menu_container.Visible = false;
		_menu_container.SetBlockSignals(true);
	}

	public void EnableMenu()
	{
		if(_DEBUG)
		{
			GD.Print("Setting to Menu mode and Enabling Menu");
		}
		if (_menu_container == null)
		{
			return;
		}
		_menu_container.SetBlockSignals(false);
		_menu_container.Visible = true;
	}

	public void SetToBuildMode()
	{
		if(_DEBUG)
		{
			GD.Print("Setting to Build mode and Disabling Menu");
		}
		DisableMenu();
		_in_build_mode = true;
		_in_menu_mode = false;
		_in_selection_mode = false;
	}

	public void SetToMenuMode()
	{
		EnableMenu();
		_in_build_mode = false;
		_in_menu_mode = true;
		_in_selection_mode = false;

	}

	public void SetToSelectionMode()
	{
		DisableMenu();
		_in_build_mode = false;
		_in_menu_mode = false;
		_in_selection_mode = true;
	}

	public void ProcessBuildableButtonPress(int buildableId)
	{
		_queued_buildable = (Buildable)_buildables_dictionary[buildableId].Duplicate();
		AddChild(_queued_buildable);
	}

	public void UpdatePalette(int paletteBlock, int buildableId)
	{
		_buildables_palette[paletteBlock] = buildableId;
		_buildables_palette_rotations[paletteBlock] = 0;


		foreach(Node node in _palette_container.GetChildren())
		{
			if(node is BuildableButton buildableButton)
			{
				if(_DEBUG)
				{
					GD.Print("Panel palette has child with name: ", buildableButton.Name);
				}
				int buttonPaletteIndex = (int)Char.GetNumericValue(buildableButton.Name[buildableButton.Name.Length - 1]);
				if(buttonPaletteIndex == paletteBlock)
				{
					if(_DEBUG)
					{
						GD.Print("Found matching paletteBlock: ", paletteBlock, "\n with buildableId: ", buildableId);
					}
					buildableButton.buildableId = buildableId;
					buildableButton.SetTexture(_buildables_icon_textures[buildableId].smallTexture);
					return;
				}
			}
		}
	}

	public Vector2 SnapToGrid(Vector2 position)
	{
		return new Vector2(ClosestGlobalCoordOnGrid(_cursorLocation.x, 0), ClosestGlobalCoordOnGrid(_cursorLocation.y, 1));
	}

	public Vector2 BuildableFlipScale(Buildable buildable, int buildableId)
	{
		return _buildables_dictionary[buildableId].Scale / buildable.Scale;
	}

	public float ClosestGlobalCoordOnGrid(float value, int axis)
	{
		float offset = axis == 0 ? _GRID_ORIGIN.x : _GRID_ORIGIN.y;
		float diff = value - offset;
		if (diff > 0)
			diff += _GRID_BLOCK_SIZE / 2;
		else if (diff < 0)
			diff -= _GRID_BLOCK_SIZE / 2;
		int block_coord = (int)(diff / _GRID_BLOCK_SIZE);
		return block_coord * _GRID_BLOCK_SIZE + offset;
	}

	public bool ValidPlacement(Buildable buildable)
	{
		if(_DEBUG)
		{
			GD.Print("buildable.GetOverlappingAreas().Count: ", buildable.GetOverlappingAreas().Count);
		}
		int bitmaskBuildable, bitmaskSceneBuildable = 0;
		
		foreach(Node node in GetChildren())//if GetOverlapping Areas doesn't include lightly touching/adjacent but nonoverlap
		{
			if(node is Buildable sceneBuildable)
			{
				if(buildable.HasOverlap(sceneBuildable))
				{
					return false;//unless.. there is an allowed interaction layering.. see _BUILD_COLLISION_LAYER
				}
				if(buildable.IsTouching(sceneBuildable))//what about interior sockets?
				{
					if(buildable.HasMismatchedSockets(sceneBuildable))
					{
						return false;
					}
					//check sockets
				}
			}
		}
		return buildable.GetOverlappingAreas().Count == 0;

		//if you wanted to complicate things with groups:
		// foreach(Area2D area2D in buildable.getOverlappingAreas())
		// {
		//     if(!area2D.isInGroup(_BUILD_GROUP))//omit?
		//     {
		//         return false;
		//     }

		// }
	}

}
