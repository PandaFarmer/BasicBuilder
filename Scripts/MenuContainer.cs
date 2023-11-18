using Godot;
using System;
using System.Collections.Generic;

public class MenuContainer : TabContainer
{
	public bool _DEBUG = true;
	public TabContainer tabContainer;

	public void AddToTab(String tabCategory, BuildableButton buildableButton)
	{
		MenuGridContainer menuGridContainer = (MenuGridContainer)FindNode(tabCategory+"Grid");
		if(_DEBUG)
		{
			GD.Print("Adding buildableButton to tabCategory: ", tabCategory);
		}
		menuGridContainer.Add(buildableButton);
		return;
	}

	public void ClearItemList(String tabCategory)
	{
		// foreach(Node node in _item_list)
	}
}
