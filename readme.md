## **Godot Grid Builder**
An asset that serves as a foundation that developers can use to create 2-D games involving base building and part manipulation, related to themes/logic involving strategy, puzzles, or automation. Currently includes a responsive ui and control scheme for pc keyboard.

##**Overview**

[Classes Explained](#Classes Explained)
[Adding More Controls and System Signals](#Adding More Controls and System Signals)
[Adding More Pieces and Textures](#Adding More Pieces and Textures)
[Debugging](#Debugging)
[Changing the Grid Scale](#Changing the Grid Scale)
[Editing Placement Validation Logic](#Editing Placement Validation Logic)
[Socket and Layers System Explained](#Socket and Layers System Explained)

There are three modes in the current program, represented by state in BuildableEditor:
_in_build_mode
_in_menu_mode
_in_selection_mode

All classes outside of main controller should have non -static fields in camelcase
Otherwise the controller has non -static fields in snake case with underscore prefix

##**Classes Explained**
Buildable- represents a piece or part in the grid space, extends Area2D
BuildableEditor-provides core placement logic and information about buildables, should be the root of the gamescene
BuildableButton-an interactable ui component that displays the corresponding part being referenced in the ui
MenuContainer-a tab container subtype that can be dynamically populated, contains ScrollContainers
MenuGridContainer-a grid container subtype that can be dynamically populated, should be nested under ScrollContainers

##**Adding Custom Controls and Event Signals**
See this [link](https://docs.godotengine.org/en/stable/getting_started/step_by_step/signals.html#connecting-signals-in-code) on information for implementing custom input signals for ui or placement interaction. Example methods _OnMouseEntered and _OnMouseExited are utilized in the BuildableButton class.

##**Adding More Pieces and Textures**
To add more pieces and textures, place a new folder containing appropriately named image textures in the project file. Edit the Buildables.json file to contain another formatted entry descripting the new part. If you are interested in some automated 2D texture asset generation, see this [github]() for some python scripts that provide easy texture generation from a 3-D asset rendered in blender.

##**Debugging**
Each file contains the _DEBUG, _SOCKET_DEBUG, and _SCENE_DEBUG fields/flags. Feel free to use another method of logging and debugging, including writing to a godot project log file or creating some sort of test framework. The current implementation outputs information to the "Output" viewport tab. 

##**Changing the Grid Scale**
Edit the field **_GRID_BLOCK_SIZE** in the class **BuildableEditor**. Plans to add 2D scaling functionality may happen in the future.

##**Editing Placement Validation Logic**


##**Socket and Layers System Brief**
Ideally one should use only powers of two as the layerMask, and use 1-3 layers (say powers of 2 one to three) to avoid excessive cluttered stacking. 