using Godot;
using System;
using System.Collections.Generic;

public class MenuContainer : TabContainer
{
	public bool _DEBUG = true;
	public TabContainer tabContainer;

	public void AddToTab(String tabCategory, InventoryItemButton inventoryItemButton)
	{
		InventoryItemList inventoryItemList = (InventoryItemList)FindNode(tabCategory+"Grid");
		if(_DEBUG)
		{
			GD.Print("Adding InventoryItemButton to tabCategory: ", tabCategory);
		}
		inventoryItemList.Add(inventoryItemButton);
		return;
	}

	public void ClearItemList(String tabCategory)
	{
		// foreach(Node node in _item_list)
	}
}
