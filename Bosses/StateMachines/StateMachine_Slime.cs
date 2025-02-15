using System.Collections.Generic;
using Godot;
using MRS.Task;
namespace BossBattleClash
{
    public partial class SlimeState_Idle : GodotState{
        public SlimeState_Idle(){
            StateTag = "Idle";
        }
    }

    public partial class SlimeState_Cutscene : GodotState{
        public SlimeState_Cutscene(){
            StateTag = "Cutscene";
        }
    }

    public partial class SlimeState_Wander : GodotState{
        public SlimeState_Wander(){
            StateTag = "Wander";
        }
    }

    public partial class SlimeState_Chase : GodotState{
        public SlimeState_Chase(){
            StateTag = "Chase";
        }
    }

    public partial class SlimeState_Attack : GodotState{
        
        private bool in_long_distance = false;
        private bool in_short_distance = false;
        private bool visible = false;
        private bool done = false;

        private Move activity;
        
        public SlimeState_Attack(){
            StateTag = "Attack";
        }

        public void OnUpdate(bool in_short_distance, bool in_long_distance, bool visible){
            this.in_long_distance = in_long_distance;
            this.in_short_distance = in_short_distance;
        }

        public void GetMoveFromSubDeck(List<Move> moves){
            //TODO implement - either use random or logic based on internal state
            activity = moves[0]; 
        }

        public override void OnInterrupt(){
            base.OnInterrupt();
            if(in_long_distance){
                //next move is LightAttack
                //activity = new Move("LACTION");
            }else if(in_short_distance){
                //next move is 
                //activity = new Move("LACTION");
            }
        }
        public override void OnTrigger(){
            base.OnTrigger();
            done = false;
        }
        public override bool check_done(){
            return done;
        }
        public override void InState(string activity)
        {
            base.InState(activity);
            // TODO Add activity as argument argument
            //Also, maybe only notify on enter and exit
        }
        public override void OnExit(){
            base.OnExit();
            done = true;
        }
    }

    public partial class SlimeState_Stance : GodotState{
        public SlimeState_Stance(){
            StateTag = "Stance";
        }
    }

    public partial class SlimeState_Evade : GodotState{
        
        public SlimeState_Evade(){
            StateTag = "Evade";
        }
    }

    [GlobalClass]
    public partial class StateMachine_Slime : Resource, IStateMachine
    {
        [Export]
        public GodotStringGraph StringGraph { get; set; }

        public IStringGraph stateDiagram{get;set;}

        [Export]
        public Godot.Collections.Dictionary<string, GodotState> States{get;set;}

        [Export]
        public string ActiveState{get; set;} = "Idle";

        public MRS.Task.IStateMachine stateMachine;
        // Make sure you provide a parameterless constructor.
        // In C#, a parameterless constructor is different from a
        // constructor with all default values.
        // Without a parameterless constructor, Godot will have problems
        // creating and editing your resource via the inspector.
        public StateMachine_Slime() : this(0, null, null) {}

        public StateMachine_Slime( int id, GodotStringGraph string_graph, Godot.Collections.Dictionary<string, GodotState> states)
        {
            StringGraph = string_graph ?? new GodotStringGraph();
            States = states ?? new Godot.Collections.Dictionary<string, GodotState>();
            Init();
        }

        public void Init(){ // Inits the state machine from code
            InitStates();
            InitGraph();
        }

        public void InitStates(){
            SlimeState_Idle idle = new SlimeState_Idle();
            AddState(idle);
            SlimeState_Cutscene cutscene  = new SlimeState_Cutscene();
            AddState(cutscene);
            SlimeState_Wander wander = new SlimeState_Wander();
            AddState(wander);
            SlimeState_Chase chase = new SlimeState_Chase();
            AddState(chase);
            SlimeState_Attack attack = new SlimeState_Attack();
            AddState(attack);
            SlimeState_Stance stance = new SlimeState_Stance();
            AddState(stance);
            SlimeState_Evade evade = new SlimeState_Evade();
            AddState(evade);
        }

        public delegate void InStateFunction(Move move);

        public void AttachToAll_InState(System.Action<string> isf){
            foreach(var state in States){
                state.Value.Connect( nameof(GodotState.SigInState), Callable.From(isf));
            }
        }

        public void AddState(GodotState state){
            States.Add(state.StateTag, state);
        }

        public void InitGraph(){
            StringGraph.AddVertex("Idle", "Cutscene");      // OnEntered
            StringGraph.AddVertex("Idle", "Wander");        // OnTimeout
            StringGraph.AddVertex("Idle", "Chase");         // OnPlayerSeen
            StringGraph.AddVertex("Cutscene", "Wander");    // OnFinished
            StringGraph.AddVertex("Wander", "Chase");       // OnPlayerSeen 
            StringGraph.AddVertex("Wander", "Idle");        // OnTimeout
            StringGraph.AddVertex("Chase", "Attack");       // OnWithinRange
            StringGraph.AddVertex("Chase", "Stance");       // OnHit
            StringGraph.AddVertex("Chase", "Evade");        // OnCrit
            StringGraph.AddVertex("Attack", "Attack");      // OnFinished(playerwithinrange)
            StringGraph.AddVertex("Attack", "Chase");       // OnFinished(playeroutofrange)
            StringGraph.AddVertex("Attack", "Stance");      // OnDamageInterrupt
            StringGraph.AddVertex("Attack", "Evade");       // OnCritInterrupt
            StringGraph.AddVertex("Stance", "Attack");      // OnTimeout(PlayerInsideRange)
            StringGraph.AddVertex("Stance", "Chase");       // OnTimeout(PlayerOutOfRange)
            StringGraph.AddVertex("Stance", "Wander");      // OnTimeout(PlayerMissing)
            StringGraph.AddVertex("Evade", "Idle");         // OnTimeout()
        }

        public void OnUpdate(){
            GD.Print(ActiveState);
            switch(ActiveState){
                case "Idle":
                    GD.Print("Idle");
                break;
                case "Cutscene":
                    GD.Print("Cutscene");
                break;
                case "Wander":
                    GD.Print("Wander");
                break;
                case "Chase":
                    GD.Print("Chase");
                break;
                case "Attack":
                    GD.Print("Attack");
                break;
                case "Stance":
                    GD.Print("Stance");
                break;
                case "Evade":
                    GD.Print("Evade");
                break;
                default:
                    GD.Print("Unknown state");
                break;
            }
        }

        public bool ChangeState(string target){
            if(target == ActiveState){
                States[ActiveState].Restart(); // Only useful if parameters are passed
                return true; // This will return very late so use threads or something
            }
            if(stateDiagram.Contains(ActiveState, target)){
                States[ActiveState].Interrupt(target); // Maybe call these later from a different spot like OnUpdate
                States[target].Trigger();
                return true; // This will return very late so use threads or something
            }else{
                return false;
            }
        }
    }
}