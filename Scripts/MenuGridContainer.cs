using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public class MenuGridContainer : GridContainer
{
    private List<BuildableButton> buildableButtons;

    public override void _Ready()
    {
        buildableButtons = new List<BuildableButton>();
    }

    public void Add(BuildableButton buildableButton)
    {
        this.AddChild(buildableButton);
        buildableButtons.Add(buildableButton);
    }

    public int HoveredBuildableId()
    {
        foreach(Node node in GetChildren())
        {
            if(node is BuildableButton buildableButton)
            {
                if(buildableButton.isHovered)
                {
                    return buildableButton.buildableId;
                }
            }
        }
        return -1;
    }

}