using Godot;

    [GlobalClass]
    public partial class ArenaInfo : Resource
    {
        [Export]
        public string arenaName { get; set; }

        [Export]
        public Godot.Environment WorldEnvironment{get;set;}

        [Export]
        public MarkerInfoC SkymoveInfo{get;set;}

        [Export]
        public Godot.Collections.Array<MarkerInfoC> SpawnMarkers{ get; set; }

        [Export]
        public Godot.Collections.Dictionary<string, string> NodeScenePaths{ get; set; } // Used to load packed scenes 

        [Export]
        public Godot.Collections.Array<DoorInfo> DoorInfoDeck{ get; set; } 

        // Make sure you provide a parameterless constructor.
        // In C#, a parameterless constructor is different from a
        // constructor with all default values.
        // Without a parameterless constructor, Godot will have problems
        // creating and editing your resource via the inspector.
        public ArenaInfo() : this("", null, null, "") {}

        public ArenaInfo(string arenaName_, Godot.Environment worldEnvironment_,MarkerInfoC skymove_, Godot.Collections.Array<MarkerInfoC> markers_, Godot.Collections.Dictionary<string,string> NodeScenePaths_){
            arenaName = arenaName_;
            WorldEnvironment = worldEnvironment_;
            //TODO marker file
            SkymoveInfo = skymove_;
            SpawnMarkers = markers_;
            //TODO scene paths
            NodeScenePaths = NodeScenePaths_;        }

        public ArenaInfo(string arenaName, Godot.Environment worldEnvironment, string marker_file_path, string NodeScenePaths)
        {
            this.arenaName = arenaName;
            this.WorldEnvironment = worldEnvironment;
            //TODO read marker info from file
            //TODO read node scene paths from file
        }
    }
