using Godot;
using System;
using System.Collections.Generic;

public class SaveMenuContainer : ScrollContainer
{
    public void Add(SaveInfoButton saveInfoButton)
	{
		foreach(Node node in GetChildren())
		{
			if(node is ItemList saves)
			{
				saves.AddChild(saveInfoButton);
				return;
			}
		}
	}
	
}