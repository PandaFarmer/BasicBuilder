using Godot;
using System;
using System.Collections.Generic;

public class BuildableButton : Button
{
	public bool _DEBUG = true;
	public int buildableId;
	public bool isPaletteItem;
	public Vector2 textureScale;
	public bool isHovered;

	public override void _Ready()
	{
		isPaletteItem = false;
		isHovered = false;
	}

	public override void _Pressed()
	{
		
		BuildableEditor buildableEditor = (BuildableEditor)FindParent("BuildableEditor");
		buildableEditor.ProcessBuildableButtonPress(buildableId);
		if(_DEBUG)
		{
			GD.Print("Button pressed for: ", buildableEditor._buildables_dictionary[buildableId].buildableName);
		}
	}

	public void _OnMouseEntered()
	{
		if(_DEBUG)
		{
			GD.Print("mouse entered buildable button");
		}
		isHovered = true;
	}

	public void _OnMouseExited()
	{
		if(_DEBUG)
		{
			GD.Print("mouse exited buildable button");
		}
		isHovered = false;
	}

	// public void SetTexture(Texture texture, Vector2 textureScale)
	public void SetTexture(Texture texture)
	{
		this.Icon = texture;
		// this.RectScale = new Vector2(.001f, .001f);
	}
}
