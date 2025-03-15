using Godot;
using System;
using System.Collections.Generic;

// Use this as template to build an arena


public partial class ArenaAutoBuild : Arena
{
	// Using this should be preferred
	// Godot.Collections.Dictionary<string, Variant>

	// Note: Using this script to "compile" a scene that can be saved to a godot scene might be useful
	[Export]
	public string ArenaFile {get;set;} = "Arena1"; 

    //private File arena_file;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		string arena_path = $"user://{ArenaFile}.arena";
		if (!FileAccess.FileExists(arena_path))
    	{
			GD.Print("Arena File missing");
        	return;
    	}

		// Create a file handle and open file.
		using var ArenaFileHandle = FileAccess.Open(arena_path, FileAccess.ModeFlags.Read);
		while (ArenaFileHandle.GetPosition() < ArenaFileHandle.GetLength())
    	{
			var jsonString = ArenaFileHandle.GetLine();
			var json = new Json();
        	var parseResult = json.Parse(jsonString);
		    if (parseResult != Error.Ok)
        	{
            	GD.Print($"JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}");
            	continue;
        	}
			var nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);
			// Add node to the dictionary list?
			// Create node and set it's default properties
			//var newObjectScene = GD.Load<PackedScene>(nodeData["Filename"].ToString());
        	//var newObject = newObjectScene.Instantiate<Node>();
        	//GetNode(nodeData["Parent"].ToString()).AddChild(newObject);
        	//newObject.Set(Node2D.PropertyName.Position, new Vector2((float)nodeData["PosX"], (float)nodeData["PosY"]));

			// Then set the remaining properties
			//foreach (var (key, value) in nodeData)
        	//{
            //	if (key == "Filename" || key == "Parent" || key == "PosX" || key == "PosY")
            //	{
            //    	continue;
            //	}
            //	newObject.Set(key, value);
        	//}
		}
		// Create a dictionary from file
		// 0) Display loading screen
		// 1) Add World info
		Dictionary<string, string> env_dict = new Dictionary<string, string>();
		//env_dict.Add("environment_path" ,"res://Assets/BuildAssets/Env_Sky.tres");
		BuildWorldEnvironment(env_dict);
		// 1) Create GridMap
		//	1.1) Tile gridmap via code
		// 2) Add spawn points to the level
		// 3) Spawn entities in order
		//	3.1) Lights
		//	3.2) RigidObjects
		//	3.3) Non-character entities
		//	3.3.5) Set up viewports
		//	3.4) NPCs
		//	3.5) Enemy characters
		//	3.6) Boss characters
		//	3.7) Player (from the local player file)
		// 4) Arena event setup
		// 5) Hiding the Cutscene
		// 6) Starting the Timer
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void BuildWorldEnvironment(Dictionary<string, string> world_blueprint){
		WorldEnvironment env = new WorldEnvironment();
		env.Environment = ResourceLoader.Load<Godot.Environment>(world_blueprint["environment_path"]);
		AddChild(env);
	}

	
	public override IVitriolic GetEnemy(){
		return null;
	}
}
