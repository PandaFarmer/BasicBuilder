using Godot;
using System.IO;
// using System.Collections.Generic;

using Newtonsoft.Json;
using System.Collections.Generic;
// using System.Security.Cryptography.X509Certificates;

//https://docs.godotengine.org/en/stable/tutorials/io/saving_games.html
public sealed class SaveHandler : Node
{
    bool _DEBUG = false;
    public SaveMenuContainer _save_menu_container;
    private SaveHandler() { }
    private static SaveHandler instance = null;

    private string saveFolderPath;

    public SaveInfoButton selectedSaveButton;
    public static SaveHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SaveHandler();
            }
            return instance;
        }
    }

    public void FlipVisibility()
    {
        if (_save_menu_container.Visible)
            EnableSaveMenu();
        else
            DisableSaveMenu();
    }
    void EnableSaveMenu()
    {
        if (_DEBUG)
        {
            GD.Print("Setting to Menu mode and Enabling Menu");
        }
        if (_save_menu_container == null)
        {
            return;
        }
        _save_menu_container.SetBlockSignals(false);
        _save_menu_container.Visible = true;
    }

    void DisableSaveMenu()
    {
        if (_save_menu_container == null)
        {
            return;
        }
        _save_menu_container.Visible = false;
        _save_menu_container.SetBlockSignals(true);
    }
    public void LoadAndPopulateSaveMenuContainer()
    {
        var saveMenuScene = GD.Load<PackedScene>("res://SaveMenu.tscn"); // Will load when the script is instanced.
        _save_menu_container = (SaveMenuContainer)saveMenuScene.Instance();

        SaveInfoButton saveInfoButton;
        // string baseDirectory = System.IO.Directory.GetCurrentDirectory();

        // System.IO.Directory.SetCurrentDirectory(saveFolderPath);

        string[] files =
            System.IO.Directory.GetFiles(saveFolderPath);
        foreach (string filepath in files)
        {
            var saveInfoButtonScene = GD.Load<PackedScene>("res://SaveInfoButton.tscn"); // Will load when the script is instanced.
            saveInfoButton = (SaveInfoButton)saveInfoButtonScene.Instance();

            BuildableSaveStateInfoListWrapper buildableSaveStateInfoListWrapper = Utils.GetContent<BuildableSaveStateInfoListWrapper>(filepath);

            saveInfoButton.timeSaved = buildableSaveStateInfoListWrapper.timeSaved;
            saveInfoButton.baseInfo = buildableSaveStateInfoListWrapper.baseInfo;
            saveInfoButton.saveID = buildableSaveStateInfoListWrapper.saveID;
            _save_menu_container.AddChild(saveInfoButton);
        }

        int sld_button_margin_top = 480;
        // int sld_button_anchor_left = ;
        //handle Save, Load, and Delete Buttons
        foreach(Node node in GetChildren())
        {
            if(node is Button button && !(node is SaveInfoButton))
            {
                if(button.Name == "Save")
                {
                    button.MarginTop = sld_button_margin_top;
                    button.AnchorLeft = 7;
                }
                if(button.Name == "Load")
                {
                    button.MarginTop = sld_button_margin_top;
                    button.AnchorLeft = 15;
                }
                if(button.Name == "Delete")
                {
                    button.MarginTop = sld_button_margin_top;
                    button.AnchorLeft = 23;
                }

            }
        }
    }

    public void DeleteSave(int saveID)
    {
        foreach (Node node in _save_menu_container.GetChildren())
        {
            if (node is SaveInfoButton saveInfoButton)
            {
                //TODO: add saveID check
                if (System.IO.File.Exists(@saveInfoButton.savePath))
                {
                    System.IO.File.Delete(@saveInfoButton.savePath);
                }
                _save_menu_container.RemoveChild(saveInfoButton);
                saveInfoButton.QueueFree();
                return;
            }
        }

    }

    public List<Buildable> DisconnectedPieces(BuildableEditor buildableEditor)
    {
        List<Buildable> disconnectedPieces = new List<Buildable>();
        List<List<Node>> traversedNodes = new List<List<Node>>();
        foreach (Node node1 in buildableEditor.GetChildren())
        {
            if (node1 is Buildable buildable)
            {
                if (node1 != buildableEditor._queued_buildable)
                {
                    foreach (Node node2 in buildableEditor.GetChildren())
                    {
                        AddIfNotInAGroup(node2, traversedNodes);
                    }
                }
            }
        }
        List<Node> largestGroup = new List<Node>();

        foreach (List<Node> nodeGroup in traversedNodes)
        {
            if (largestGroup.Count < nodeGroup.Count)
                largestGroup = nodeGroup;
        }

        foreach (Node node in buildableEditor.GetChildren())
        {
            if (node is Buildable buildable)
            {
                if (!largestGroup.Contains(node))
                    disconnectedPieces.Add(buildable);
            }
        }
        return disconnectedPieces;
    }

    public bool AddIfNotInAGroup(Node node, List<List<Node>> groups)
    {
        foreach (List<Node> nodeGroup in groups)
        {
            if (nodeGroup.Contains(node))
                return false;

            if (node is Buildable buildable)
            {
                if (NodeGroupHasConnection(buildable, nodeGroup))
                {
                    nodeGroup.Add(buildable);
                    return false;
                }
            }

        }
        groups.Add(new List<Node> { node });
        return true;
    }

    public bool NodeGroupHasConnection(Buildable buildable, List<Node> nodeGroup)
    {
        foreach (Node node in nodeGroup)
        {
            if (node is Buildable sceneBuildable)
            {
                if (sceneBuildable.HasSocketConnection(buildable))
                {
                    return true;
                }
            }
        }
        return false;
    }
    // Note: This can be called from anywhere inside the tree. This function is
    // path independent.
    // Go through everything in the persist category and ask them to return a
    // dict of relevant variables.
    public void SaveGame()
    {
        string path = $"user://savegame.save";
        //https://learn.microsoft.com/en-us/dotnet/api/system.io.file?view=net-8.0
        using (StreamWriter sw = System.IO.File.CreateText(path))
        {

            var saveNodes = GetTree().GetNodesInGroup("Persist");
            foreach (Node saveNode in saveNodes)
            {
                // Check the node is an instanced scene so it can be instanced again during load.
                // if (string.IsNullOrEmpty(saveNode.SceneFilePath))
                if (!(saveNode is Buildable))
                {
                    GD.Print($"persistent node '{saveNode.Name}' is not an instanced scene, skipped");
                    continue;
                }

                // Check the node has a save function.
                if (!saveNode.HasMethod("Save"))
                {
                    GD.Print($"persistent node '{saveNode.Name}' is missing a Save() function, skipped");
                    continue;
                }

                // Call the node's save function.
                var nodeData = saveNode.Call("Save");

                // Json provides a static method to serialized JSON string.
                var jsonString = JsonConvert.SerializeObject(nodeData);

                // Store the save dictionary as a new line in the save file.
                sw.WriteLine(path, jsonString);
                // saveGame.StoreLine(jsonString);
            }
        }

    }


    // Note: This can be called from anywhere inside the tree. This function is
    // path independent.
    public void LoadGame(BuildableEditor buildableEditor)
    {
        string savePath = "user://savegame.save";
        if (!System.IO.File.Exists(savePath))
        {
            return; // Error! We don't have a save to load.
        }

        // We need to revert the game state so we're not cloning objects during loading.
        // This will vary wildly depending on the needs of a project, so take care with
        // this step.
        // For our example, we will accomplish this by deleting saveable objects.
        var saveNodes = GetTree().GetNodesInGroup("Persist");
        foreach (Node saveNode in saveNodes)
        {
            saveNode.QueueFree();
        }

        BuildableSaveStateInfoListWrapper buildableSaveStateInfoListWrapper = Utils.GetContent<BuildableSaveStateInfoListWrapper>(savePath);
        // string fileName = "C:\\Users\\NoSpacesForWSL\\OneDrive\\Documents\\GODOTProjs\\BasicBuilder\\Buildables.json";

        foreach (BuildableSaveStateInfo buildableSaveStateInfo in buildableSaveStateInfoListWrapper.buildableSaveStateInfos)
        {
            buildableEditor.LoadBuildable(buildableSaveStateInfo);
        }
        // // Load the file line by line and process that dictionary to restore the object
        // // it represents.
        // using (StreamReader sr = new StreamReader(savePath))
        // {
        //     // using var saveGame = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Read);

        //     while (saveGame.GetPosition() < saveGame.GetLength())
        //     {
        //         var jsonString = saveGame.GetLine();

        //         // Creates the helper class to interact with JSON
        //         var json = new Json();
        //         var parseResult = json.Parse(jsonString);
        //         if (parseResult != Error.Ok)
        //         {
        //             GD.Print($"JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}");
        //             continue;
        //         }

        //         // Get the data from the JSON object
        //         var nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);

        //         // Firstly, we need to create the object and add it to the tree and set its position.
        //         var newObjectScene = GD.Load<PackedScene>(nodeData["Filename"].ToString());
        //         var newObject = newObjectScene.Instantiate<Node>();
        //         GetNode(nodeData["Parent"].ToString()).AddChild(newObject);
        //         newObject.Set(Node2D.PropertyName.Position, new Vector2((float)nodeData["PosX"], (float)nodeData["PosY"]));

        //         // Now we set the remaining variables.
        //         foreach (var (key, value) in nodeData)
        //         {
        //             if (key == "Filename" || key == "Parent" || key == "PosX" || key == "PosY")
        //             {
        //                 continue;
        //             }
        //             newObject.Set(key, value);
        //         }
        //     }
        // }

    }
}