using Godot;
using System;
using System.Collections.Generic;
// using System.Numerics;

public class Buildable : Area2D
{
	public bool _DEBUG = false;
	public bool _SOCKET_DEBUG = false;
	public static uint _BUILD_COLLISION_LAYER = 1234;
	// public static string _BUILD_GROUP = "BUILD_GROUP";
	public int buildableId;
	public Texture smallMenuTexture;
	public Texture mediumMenuTexture;
	public Texture LargeMenuTexture;

	public Vector2 textureScale;
	// public Vector2 _baseScale;//?
	public string buildableName;
	public string buildablePathName;
	public Vector2 dimensions;
	
	public Dictionary<Vector2, Buildable> attachedBuildables;

	// public bool isIsometric;
	// public bool isRotationallyIsomorphic;

	public string labelName;

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
		ZIndex = -1;
	}


	public void RotateCounterClockwiseOrthogonal()
	{
		this.Rotation = this.Rotation + (float)Math.PI / 2f;
	}

	public void RotateClockwiseOrthogonal()
	{
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

	public bool HasOverlap(Buildable buildable)
	{
		float gridBlockSize = BuildableEditor._GRID_BLOCK_SIZE;
		// return GetOverlappingAreas().Contains(buildable);
		//in case a more transparent custom version is required:
		List<Vector2> mmBoundsThis = MaxMinBounds();
		List<Vector2> mmBoundsOther = buildable.MaxMinBounds();
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
		List<Vector2> mmBoundsThis = MaxMinBounds();
		List<Vector2> mmBoundsOther = buildable.MaxMinBounds();
		Vector2 minVThis = mmBoundsThis[0];
		Vector2 maxVThis = mmBoundsThis[1];
		Vector2 minVOther = mmBoundsOther[0];
		Vector2 maxVOther = mmBoundsOther[1];
		bool hasXequal = maxVThis.x == minVOther.x || minVThis.x == maxVOther.x;
		bool hasYequal = maxVThis.y == minVOther.y || minVThis.y == maxVOther.y;
		bool isCorner = hasXequal && hasYequal;
		if(isCorner) return false;
		return !(maxVThis.x < minVOther.x || maxVThis.y < minVOther.y || minVThis.x > maxVOther.x || minVThis.y > maxVOther.y);
	}

	public Dictionary<Vector2, int> SocketConnectabilityMap()
	{
		return ((BuildableEditor)GetParent())._buildables_socketConnectabilityMap[buildableId];
	}

	public Dictionary<Vector2, int> SocketRequirementMap()
	{
		return ((BuildableEditor)GetParent())._buildables_socketRequirementMap[buildableId];
	}

	public bool HasMatchingSocket(Buildable buildable)
	{
		Dictionary<Vector2, int> socketConnectabilityMap = SocketConnectabilityMap();
		Dictionary<Vector2, int> sceneSocketConnectabilityMap = buildable.SocketConnectabilityMap();
		if(buildable == null)
		{
			if(_SOCKET_DEBUG)
			{
				GD.Print("WARNING: buildable being checked for matching socket is null!");
			}
			return false;
		}
		foreach (Vector2 socketCoord in socketConnectabilityMap.Keys)
		{
			foreach (Vector2 sceneSocketCoord in sceneSocketConnectabilityMap.Keys)
			{
				if (MatchingSocketLocation(this, buildable, socketCoord, sceneSocketCoord))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasAllRequiredSockets(Buildable buildable)
	{
		Dictionary<Vector2, int> socketRequirementMap = SocketRequirementMap();
		Dictionary<Vector2, int> sceneSocketRequirementMap = buildable.SocketRequirementMap();
		
		if(buildable == null || socketRequirementMap == null)
		{
			if(_SOCKET_DEBUG)
			{
				GD.Print("WARNING: other buildable, socketRequirementMap, or socketConnectabilityMap is uninitialized");
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
			if(!hasRequiredSocket)
			{
				return false;
			}
		}
		return true;
	}


	public bool HasMismatchedSockets(Buildable buildable)
	{
		Dictionary<Vector2, int> socketRequirementMap = SocketRequirementMap();
		Dictionary<Vector2, int> sceneSocketRequirementMap = buildable.SocketRequirementMap();
		
		if(buildable == null)
		{
			if(_SOCKET_DEBUG)
			{
				GD.Print("WARNING: buildable being checked for mismatched socket is null!");
			}
			return true;
		}
		foreach (Vector2 socketCoord in socketRequirementMap.Keys)
		{
			foreach (Vector2 sceneSocketCoord in sceneSocketRequirementMap.Keys)
			{
				// if (socketCoord + Position == sceneSocketCoord + buildable.Position)
				if (OpposingSocketDirection(socketCoord, sceneSocketCoord) && 
					MatchingSocketLocation(this, buildable, socketCoord, sceneSocketCoord) &&
					MatchingSockets(this, buildable, socketCoord, sceneSocketCoord))
				{
					if ((socketRequirementMap[socketCoord] & sceneSocketRequirementMap[sceneSocketCoord]) == 0)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public bool MatchingSockets(Buildable b1, Buildable b2, Vector2 v1, Vector2 v2)
	{
		int sockettype_bit_mask1 = b1.SocketConnectabilityMap()[v1];
		int sockettype_bit_mask2 = b2.SocketConnectabilityMap()[v2];
		return (sockettype_bit_mask1&sockettype_bit_mask2) != 0;
	}

	public bool MatchingSocketLocation(Buildable b1, Buildable b2, Vector2 v1, Vector2 v2)
	{
		
		float gridBlockSize = BuildableEditor._GRID_BLOCK_SIZE;
		return b1.Position + v1*gridBlockSize == b2.Position + v2*gridBlockSize;
	}

	public bool OpposingSocketDirection(Vector2 v1, Vector2 v2)
	{
		Vector2 normV1 = v1.Normalized();
		Vector2 normV2 = v2.Normalized();
		return normV1.x == -normV2.x && normV1.y == -normV2.y;
	}

	public List<Vector2> MaxMinBounds()
	{
		BuildableEditor buildableEditor = (BuildableEditor)GetParent();
		float gridBlockSize = BuildableEditor._GRID_BLOCK_SIZE;
		dimensions = buildableEditor._buildables_dimensions[buildableId];
		float halfdimX = (gridBlockSize * dimensions.x) / 2f;
		float halfdimY = (gridBlockSize * dimensions.y) / 2f;
		Vector2 minV = new Vector2(Position.x - halfdimX, Position.y - halfdimY);
		Vector2 maxV = new Vector2(Position.x + halfdimX, Position.y + halfdimY);
		List<Vector2> mmList = new List<Vector2>();
		mmList.Add(minV);
		mmList.Add(maxV);
		return mmList;
	}

	public void SetTextureHueNeutral()
	{
		Modulate = new Color(1f, 1f, 1f, 1f);

	}
	public void SetTextureOpaque()
	{
		Modulate = new Color(1f, 1f, 1f, .5f);
	}
	public void SetTextureHueRed()
	{
		Modulate = new Color(1f, .2f, .2f, .5f);
	}
}
