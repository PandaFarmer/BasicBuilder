using Godot;
using System;
using System.Collections.Generic;
using System.Data;

public class Buildable : Area2D
{
	public bool _DEBUG = true;
	public static uint _BUILD_COLLISION_LAYER = 1234;
	// public static string _BUILD_GROUP = "BUILD_GROUP";
	public Texture smallMenuTexture;
	public Texture mediumMenuTexture;
	public Texture LargeMenuTexture;

	public Vector2 textureScale;
	// public Vector2 _baseScale;//?
	public string buildableName;
	public string buildablePathName;
	public Vector2 dimensions;
	public List<Vector2> socketConnectabilityPoints;//Vector2 should b hashable..
	public Dictionary<Vector2, Buildable> attachedBuildables;
	public Dictionary<ulong, Buildable> buildableCollisions;
	public bool isIsometric;
	public bool isRotationallyIsomorphic;
	public CollisionShape2D collisionShape2D;
	public RectangleShape2D rectangleShape2D;

	public string labelName;

	public override void _Ready()
	{
		// addToGroup(_BUILD_GROUP);
		CollisionLayer = _BUILD_COLLISION_LAYER;
		foreach(Node child in GetChildren())
		{
			if(child is CollisionShape2D collisionShape2D)
			{
				RectangleShape2D collisionRect = new RectangleShape2D();
				collisionRect.Extents = dimensions*BuildableEditor._GRID_BLOCK_SIZE/2f;
				collisionShape2D.Shape = collisionRect;
				if(_DEBUG)
				{
					GD.Print("collisionRect.Extents: ", collisionRect.Extents);
					GD.Print("collisionShape2D.Shape", collisionShape2D.Shape);
				}
				return;
			}
		}

	}


	public void RotateCounterClockwiseOrthogonal()
	{
		this.Rotation = this.Rotation + (float)Math.PI/2f;
	}

	public void RotateClockwiseOrthogonal()
	{
		this.Rotation = this.Rotation - (float)Math.PI/2f;
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

	public Vector2 TextureScaledDimensions(Texture texture, Vector2 dimensions, float gridBlockSize)
	{
		if(_DEBUG)
		{
			GD.Print("Found a texture with dims: ", new Vector2());
		}
		return new Vector2(gridBlockSize*(dimensions.x/texture.GetWidth()), gridBlockSize*(dimensions.y/texture.GetHeight()));
	}

	public void SetTexture(Texture texture, float gridBlockSize)
	{
		// GetGlobalTransformWithCanvas
		
		foreach(Node node in GetChildren())
		{
			if(node is Sprite sprite)
			{
				sprite.Texture = texture;
				sprite.GetGlobalTransformWithCanvas();
				sprite.Scale = TextureScaledDimensions(texture, dimensions, gridBlockSize);
				if(_DEBUG)
				{
					GD.Print("gridBlockSize: ", gridBlockSize);
					GD.Print("dimensions: ", dimensions);//? 0, 0 after Duplicate?
					GD.Print("new sprite scale for buildable: ", sprite.Scale);
				}
				return;
			}
		}
	}

	public void _OnAreaEntered(Area2D area)
	{
		if (area is Buildable buildable)
		{
			buildableCollisions[area.GetInstanceId()] = (Buildable)area;
			return;
		}
		if(_DEBUG)
		{
			GD.Print("WARNING COLLISION ENTER SIGNAL WITH NON BUILDABLE AREA2D");
		}
		
	}

	public void _OnAreaExited(Area2D area)
	{
		if (area is Buildable buildable)
		{
			buildableCollisions.Remove(area.GetInstanceId());
			return;
		}
		if(_DEBUG)
		{
			GD.Print("WARNING COLLISION EXIT SIGNAL WITH NON BUILDABLE AREA2D");
		}
	}
}


// private void _OnAreaEntered(object area)
// {
// 	// Replace with function body.
// }


// private void _OnAreaExited(object area)
// {
// 	// Replace with function body.
// }
