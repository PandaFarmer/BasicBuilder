using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

public class BuildableSaveStateInfo
{
	public float PosX;
	public float PosY;
	public float ScaleX;
	public float ScaleY;
	public float trueRotation;
	public int buildableId;
	public float textureScaleX;
	public float textureScaleY;
	public string buildableName;
	public string buildablePathName;
	public float dimensionsX;
	public float dimensionsY;
	public bool oddOrthogonal;
	public string labelName;
	public int placementLayer;
}
public class Buildable : Area2D
{
	public bool _DEBUG = true;
	public bool _SOCKET_DEBUG = false;
	public static uint _BUILD_COLLISION_LAYER = 1234;
	// public static string _BUILD_GROUP = "BUILD_GROUP";
	public int buildableId;
	public Texture smallMenuTexture;
	public Texture mediumMenuTexture;
	public Texture largeMenuTexture;

	public Vector2 textureScale;
	// public Vector2 _baseScale;//?
	public string buildableName;
	public string buildablePathName;
	public Vector2 dimensions;
	public Dictionary<Vector2, Buildable> attachedBuildables;

	public bool oddOrthogonal;
	public float trueRotation;

	// public bool isIsometric;
	// public bool isRotationallyIsomorphic;

	public string labelName;
	public bool automatedLayerPlacement;

	public int placementLayer;

	public BuildableSaveStateInfo Save()
	{
		BuildableSaveStateInfo buildableSaveStateInfo = new BuildableSaveStateInfo();
		buildableSaveStateInfo.PosX = Position.x;
		buildableSaveStateInfo.PosY = Position.y;
		buildableSaveStateInfo.ScaleX = Scale.x;
		buildableSaveStateInfo.ScaleY = Scale.y;
		buildableSaveStateInfo.trueRotation = trueRotation;
		buildableSaveStateInfo.buildableId = buildableId;
		buildableSaveStateInfo.textureScaleX = textureScale.x;
		buildableSaveStateInfo.textureScaleY = textureScale.y;
		buildableSaveStateInfo.buildableName = buildableName;
		buildableSaveStateInfo.buildablePathName = buildablePathName;
		buildableSaveStateInfo.oddOrthogonal = oddOrthogonal;
		buildableSaveStateInfo.labelName = labelName;
		buildableSaveStateInfo.placementLayer = placementLayer;
		return buildableSaveStateInfo;
	}

	// public Dictionary<string, Buildable> Save()
	// {
	// 	return new Dictionary<string, Buildable>()
	// 	{
	// 		{"PosX", Position.x},
	// 		{"PosY", Position.y},
	// 		{"ScaleX", Scale.x},
	// 		{"ScaleY", Scale.y},
	// 		{"RotationX", trueRotation.x},
	// 		{"RotationY", trueRotation.y},
	// 		{"buildableId", buildableId},
	// 		{"textureScaleX", textureScale.x},
	// 		{"textureScaleY", textureScale.y},
	// 		{"buildableName", buildableName},
	// 		{"buildablePathName", buildablePathName},
	// 		{"dimensionsX", dimensions.x},
	// 		{"dimensionsY", dimensions.y},
	// 		{"oddOrthogonal", oddOrthogonal},
	// 		{"labelName", labelName},
	// 		{"placementLayer", placementLayer}
	// 	};
	// }

	public override void _Ready()
	{

		// addToGroup(_BUILD_GROUP);
		CollisionLayer = _BUILD_COLLISION_LAYER;
		foreach (Node child in GetChildren())
		{
			if (child is CollisionShape2D collisionShape2D)
			{
				RectangleShape2D collisionRect = new RectangleShape2D();
				collisionRect.Extents = dimensions * BuildableEditor._GRID_BLOCK_SIZE / 2f;
				collisionShape2D.Shape = collisionRect;
				return;
			}
		}
		SafeInitializeAttachedBuildables();
		ZIndex = -1;
		oddOrthogonal = false;
	}

	public void RotateCounterClockwiseOrthogonal()
	{
		oddOrthogonal = !oddOrthogonal;
		this.Rotation = this.Rotation + (float)Math.PI / 2f;
	}

	public void RotateClockwiseOrthogonal()
	{
		oddOrthogonal = !oddOrthogonal;
		this.Rotation = this.Rotation - (float)Math.PI / 2f;
	}

	public void FlipHorizontally()
	{
		this.Scale = new Vector2(-this.Scale.x, this.Scale.y);
	}

	public void FlipVertically()
	{
		this.Scale = new Vector2(this.Scale.x, -this.Scale.y);
	}

	public bool isAttached()
	{
		// throw NotImplementedException;
		return false;
		//if it is the 1st Buildable attached to buildablesRoot or has an adjacent Buildable that follows socket connection rules
	}

	public void SafeInitializeAttachedBuildables()
	{
		if (attachedBuildables == null)
		{
			attachedBuildables = new Dictionary<Vector2, Buildable>();
		}
	}

	public Vector2 TextureScaledDimensions(Texture texture, Vector2 dimensions)
	{
		float gridBlockSize = BuildableEditor._GRID_BLOCK_SIZE;
		if (_DEBUG)
		{
			GD.Print("Found a texture with dims: ", new Vector2());
		}
		return new Vector2(gridBlockSize * (dimensions.x / texture.GetWidth()), gridBlockSize * (dimensions.y / texture.GetHeight()));
	}

	public void SetTexture(Texture texture)
	{
		// GetGlobalTransformWithCanvas
		float gridBlockSize = BuildableEditor._GRID_BLOCK_SIZE;
		foreach (Node node in GetChildren())
		{
			if (node is Sprite sprite)
			{
				sprite.Texture = texture;
				// sprite.GetGlobalTransformWithCanvas();//?
				sprite.Scale = TextureScaledDimensions(texture, dimensions);
				if (_DEBUG)
				{
					GD.Print("gridBlockSize: ", gridBlockSize);
					GD.Print("dimensions: ", dimensions);//? 0, 0 after Duplicate?
					GD.Print("new sprite scale for buildable: ", sprite.Scale);
				}
				return;
			}
		}
	}

	public List<Buildable> AllOverlappingBuildables()
	{
		List<Buildable> overlappingBuildables = new List<Buildable>();
		if (GetParent() == null)
		{
			GD.Print("WARN: Buildable not attached to BuildableEditor sceneroot NULL");
			return overlappingBuildables;
		}
		foreach (Node node in ((BuildableEditor)GetParent()).GetChildren())
		{

			if (node is Buildable buildable)
			{
				if (buildable == this) continue;
				if (HasOverlap(buildable)) overlappingBuildables.Add(buildable);
			}
		}
		return overlappingBuildables;
	}

	public bool PointInBounds(Vector2 point)
	{
		// return GetOverlappingAreas().Contains(buildable);
		//in case a more transparent custom version is required:
		List<Vector2> mmBoundsThis = MinMaxBounds();
		Vector2 minVThis = mmBoundsThis[0];
		Vector2 maxVThis = mmBoundsThis[1];
		
		bool rv =maxVThis.x >= point.x && maxVThis.y >= point.y && minVThis.x <= point.x && minVThis.y <= point.y;
		if (_DEBUG)
		{
			GD.Print($"mmBoundsThis=> min: {minVThis}, max: {maxVThis}");
			GD.Print("point: ", point);
			GD.Print($"Point in bounds: {rv}");
		}
		return rv;
	}

	public bool HasOverlap(Buildable buildable)
	{
		float gridBlockSize = BuildableEditor._GRID_BLOCK_SIZE;
		// return GetOverlappingAreas().Contains(buildable);
		//in case a more transparent custom version is required:
		List<Vector2> mmBoundsThis = MinMaxBounds();
		List<Vector2> mmBoundsOther = buildable.MinMaxBounds();
		if (_SOCKET_DEBUG)
		{
			GD.Print("mmBoundsThis: ", mmBoundsThis[0], ", ", mmBoundsThis[1]);
			GD.Print("mmBoundsOther: ", mmBoundsOther[0], ", ", mmBoundsOther[1]);
		}
		Vector2 minVThis = mmBoundsThis[0];
		Vector2 maxVThis = mmBoundsThis[1];
		Vector2 minVOther = mmBoundsOther[0];
		Vector2 maxVOther = mmBoundsOther[1];
		return !(maxVThis.x <= minVOther.x || maxVThis.y <= minVOther.y || minVThis.x >= maxVOther.x || minVThis.y >= maxVOther.y);
	}

	public bool IsTouching(Buildable buildable)
	{
		float gridBlockSize = BuildableEditor._GRID_BLOCK_SIZE;
		//also in case a more transparent custom version is required:
		List<Vector2> mmBoundsThis = MinMaxBounds();
		List<Vector2> mmBoundsOther = buildable.MinMaxBounds();
		Vector2 minVThis = mmBoundsThis[0];
		Vector2 maxVThis = mmBoundsThis[1];
		Vector2 minVOther = mmBoundsOther[0];
		Vector2 maxVOther = mmBoundsOther[1];
		bool hasXequal = maxVThis.x == minVOther.x || minVThis.x == maxVOther.x;
		bool hasYequal = maxVThis.y == minVOther.y || minVThis.y == maxVOther.y;
		bool isCorner = hasXequal && hasYequal;
		if (isCorner) return false;
		return !(maxVThis.x < minVOther.x || maxVThis.y < minVOther.y || minVThis.x > maxVOther.x || minVThis.y > maxVOther.y);
	}

	public Dictionary<Vector2, int> SocketConnectabilityMap
	{
		get { return ((BuildableEditor)GetParent())._buildables_socketConnectabilityMap[buildableId]; }
	}

	public Dictionary<Vector2, int> SocketRequirementMap
	{
		get { return ((BuildableEditor)GetParent())._buildables_socketRequirementMap[buildableId]; }
	}

	public int BuildableLayerMask
	{
		get
		{
			if ((BuildableEditor)GetParent() == null)
			{
				GD.Print("WARN: Buildable not attached to BuildableEditor sceneroot NULL");
				return 0;
			}
			return ((BuildableEditor)GetParent()).buildables_layer_masks[buildableId];
		}
	}

	public int BuildableLayerRequirementMask
	{

		get
		{
			if ((BuildableEditor)GetParent() == null)
			{
				GD.Print("WARN: Buildable not attached to BuildableEditor sceneroot NULL");
				return 0;
			}
			return ((BuildableEditor)GetParent())._buildables_layer_requirement_masks[buildableId];
		}
	}

	//
	public int PlacementLayer()
	{
		//auto place.. also add controls later to move piece up/down? +ui for layer indication?
		//place simply by overlap check+placeable layer bit gen? howto consider sockets..
		int layer = 0;
		int layer_mask = BuildableLayerRequirementMask;

		foreach (Buildable _buildable in AllOverlappingBuildables())
		{
			//need to do an bit op that gives the viable ranges of non overlapping layers
			layer_mask &= ~_buildable.placementLayer;
		}
		while ((layer_mask & 1) != 0)
		{
			layer_mask >>= 1;
			layer++;
		}

		return layer;
	}

	//should be passing in _queued_buildable as param here
	public bool CanPlaceOver(Buildable buildable)
	{
		int placeable_layer_bit = 1 << (buildable.placementLayer + 1);
		return (placeable_layer_bit & BuildableLayerRequirementMask) != 0;
	}

	public bool OddX()
	{
		return oddOrthogonal ? dimensions.y % 2 == 1 : dimensions.x % 2 == 1;
	}

	public bool OddY()
	{
		return oddOrthogonal ? dimensions.x % 2 == 1 : dimensions.y % 2 == 1;
	}

	public bool HasSocketConnection(Buildable buildable)
	{
		return MatchingSocket(buildable)!=Vector2.Zero;
	}

	public Vector2 MatchingSocket(Buildable buildable)
	{
		Dictionary<Vector2, int> socketConnectabilityMap = SocketConnectabilityMap;
		Dictionary<Vector2, int> sceneSocketConnectabilityMap = buildable.SocketConnectabilityMap;
		if (buildable == null)
		{
			if (_SOCKET_DEBUG)
			{
				GD.Print("WARNING: buildable being checked for matching socket is null!");
			}
			return Vector2.Zero;
		}
		foreach (Vector2 socketCoord in socketConnectabilityMap.Keys)
		{
			foreach (Vector2 sceneSocketCoord in sceneSocketConnectabilityMap.Keys)
			{
				if (MatchingSocketLocation(this, buildable, socketCoord, sceneSocketCoord))
				{
					return socketCoord;
				}
			}
		}
		return Vector2.Zero;
	}

	public bool HasAllRequiredSockets(Buildable buildable)
	{
		//requirement bitmask may be 0->optional, 1->at least1, 2-> at most 1 ??
		Dictionary<Vector2, int> socketRequirementMap = SocketRequirementMap;
		Dictionary<Vector2, int> sceneSocketRequirementMap = buildable.SocketRequirementMap;

		if (buildable == null || socketRequirementMap == null)
		{
			if (_SOCKET_DEBUG)
			{
				GD.Print("WARNING: other buildable, socketRequirementMap, or socketConnectabilityMap is uninitialized?");
			}
			return false;
		}
		bool hasRequiredSocket;
		foreach (Vector2 socketCoord in socketRequirementMap.Keys)
		{
			hasRequiredSocket = false;
			foreach (Vector2 sceneSocketCoord in sceneSocketRequirementMap.Keys)
			{
				if (MatchingSocketLocation(this, buildable, socketCoord, sceneSocketCoord))
				{
					hasRequiredSocket = true;
					break;
				}
			}
			if (!hasRequiredSocket)
			{
				return false;
			}
		}
		return true;
	}


	public bool HasMismatchedSockets(Buildable buildable)
	{
		Dictionary<Vector2, int> socketConnectabilityMap = SocketConnectabilityMap;
		Dictionary<Vector2, int> sceneSocketConnectabilityMap = buildable.SocketConnectabilityMap;

		if (buildable == null)
		{
			if (_SOCKET_DEBUG)
			{
				GD.Print("WARNING: buildable being checked for mismatched socket is null!");
			}
			return true;
		}
		foreach (Vector2 socketCoord in socketConnectabilityMap.Keys)
		{
			foreach (Vector2 sceneSocketCoord in sceneSocketConnectabilityMap.Keys)
			{
				// if (socketCoord + Position == sceneSocketCoord + buildable.Position)
				if (OpposingSocketDirection(socketCoord, sceneSocketCoord) &&
					MatchingSocketLocation(this, buildable, socketCoord, sceneSocketCoord) &&
					MatchingSockets(this, buildable, socketCoord, sceneSocketCoord))
				{
					if (_SOCKET_DEBUG)
					{
						GD.Print("Found Matching Sockets on: \n", socketCoord.ToString(), "\n", sceneSocketCoord.ToString());
					}
					if ((socketConnectabilityMap[socketCoord] & sceneSocketConnectabilityMap[sceneSocketCoord]) == 0)
					{
						return true;//should cause NOTVALID
					}
				}
			}
		}
		return false;
	}

	public bool MatchingSockets(Buildable b1, Buildable b2, Vector2 v1, Vector2 v2)
	{
		int sockettype_bit_mask1 = b1.SocketConnectabilityMap[v1];
		int sockettype_bit_mask2 = b2.SocketConnectabilityMap[v2];
		return (sockettype_bit_mask1 & sockettype_bit_mask2) != 0;
	}

	public bool MatchingSocketLocation(Buildable b1, Buildable b2, Vector2 v1, Vector2 v2)
	{

		float gridBlockSize = BuildableEditor._GRID_BLOCK_SIZE;
		return b1.Position + v1 * gridBlockSize == b2.Position + v2 * gridBlockSize;
	}

	public bool OpposingSocketDirection(Vector2 v1, Vector2 v2)
	{
		Vector2 normV1 = v1.Normalized();
		Vector2 normV2 = v2.Normalized();
		return normV1 == -normV2;
	}

	public List<Vector2> MinMaxBounds()
	{
		BuildableEditor buildableEditor = (BuildableEditor)GetParent();
		float gridBlockSize = BuildableEditor._GRID_BLOCK_SIZE;
		dimensions = buildableEditor._buildables_dimensions[buildableId];
		float halfdimX = (gridBlockSize * (oddOrthogonal?dimensions.y:dimensions.x)) / 2f;
		float halfdimY = (gridBlockSize * (oddOrthogonal?dimensions.x:dimensions.y)) / 2f;
		Vector2 minV = new Vector2(Position.x - halfdimX, Position.y - halfdimY);
		Vector2 maxV = new Vector2(Position.x + halfdimX, Position.y + halfdimY);
		List<Vector2> mmList = new List<Vector2>();
		mmList.Add(minV);
		mmList.Add(maxV);
		return mmList;
	}

	public void SetTextureHueNeutral()
	{
		Modulate = new Color(1f, 1f, 1f, Modulate.a);
	}

	public void SetTextureOpaque()
	{
		Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, 1f);
	}
	private void SetTextureTransparent()
	{

		Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, .5f);
	}
	private void SetTextureHueRed()
	{
		Modulate = new Color(1f, .2f, .2f, Modulate.a);
	}

	private void SetTextureHueGreen()
	{
		Modulate = new Color(.2f, 1f, .2f, Modulate.a);
	}

	public void SetTextureHueNeutralTransparent()
	{
		SetTextureHueNeutral();
		SetTextureTransparent();
	}

	public void SetTextureHueRedTransparent()
	{
		SetTextureHueRed();
		SetTextureTransparent();
	}
	public void SetTextureHueGreenTransparent()
	{
		SetTextureHueGreen();
		SetTextureTransparent();
	}

	public bool BuildableHasConnection(int socketType, Buildable buildable)
	{
		Dictionary<Vector2, int> socketConnectabilityMap = SocketConnectabilityMap;
		List<Buildable> traversedBuildables = new List<Buildable>();
		Queue<KeyValuePair<Vector2, Buildable>> buildableSearchQueue = new Queue<KeyValuePair<Vector2, Buildable>>();
		foreach (KeyValuePair<Vector2, Buildable> _kvp in attachedBuildables)
		{
			buildableSearchQueue.Enqueue(_kvp);
		}
		KeyValuePair<Vector2, Buildable> kvp;
		////Vector2 searchSocketLocalPos = kvp.Key;
		////Buildable searchBuildable = kvp.Value;
		while (buildableSearchQueue.Count > 0)
		{
			kvp = buildableSearchQueue.Dequeue();
			if (kvp.Value == null)
			{
				attachedBuildables.Remove(kvp.Key);
				continue;
			}
			else if ((socketType & socketConnectabilityMap[kvp.Key]) == 0)
			{
				traversedBuildables.Add(kvp.Value);
				continue;
			}
			else if (kvp.Value == buildable)
			{
				return true;
			}
			else if (!traversedBuildables.Contains(kvp.Value))
			{
				foreach (KeyValuePair<Vector2, Buildable> b_kvp in kvp.Value.attachedBuildables)
				{
					buildableSearchQueue.Enqueue(b_kvp);
				}
			}
		}
		return false;
	}
}
