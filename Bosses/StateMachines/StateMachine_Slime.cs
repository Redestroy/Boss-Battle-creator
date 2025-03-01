using System.Collections.Generic;
using Godot;
using MRS.Task;
    

    [GlobalClass]
    public partial class SlimeState_Cutscene : GodotState{
        public SlimeState_Cutscene(){
            StateTag = "Cutscene";
        }
    }

    

    [GlobalClass]
    public partial class SlimeState_Chase : GodotState{
        
        List<string> move_group;

        public SlimeState_Chase(){
            StateTag = "Chase";
            next_state_on_timeout = "Idle";
            Timeout = 5;
            move_group = new List<string>(){
                "SmallJump",
                "Leap"
            };
            move = move_group[0];
        }

        public override void OnUpdate(Godot.Collections.Dictionary<string, Variant> observations)
        {
            base.OnUpdate(observations);
        }
    }

    [GlobalClass]    
    public partial class SlimeState_Attack : GodotState{
        
        private bool in_max_attack_distance = false;
        private bool in_min_attack_distance = false;
        private bool visible = false;
        private bool done = false;

        private string activity;
        
        public SlimeState_Attack(){
            StateTag = "Attack";
            next_state_on_timeout = "Stance";
            Timeout = 5;
        }

        public override void OnUpdate(Godot.Collections.Dictionary<string, Variant> observations){
            this.in_max_attack_distance = (bool)observations["in_max_attack_distance"];
            this.in_min_attack_distance = (bool)observations["in_min_attack_distance"];
        }

        public void GetMoveFromSubDeck(List<string> moves){
            //TODO implement - either use random or logic based on internal state
            activity = moves[0]; 
            move = activity;
        }

        public override void OnInterrupt(){
            base.OnInterrupt();
            if(in_max_attack_distance){
                //next move is LightAttack
                //activity = new Move("LACTION");
            }else if(in_min_attack_distance){
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

    [GlobalClass]
    public partial class SlimeState_Stance : GodotState{
        public SlimeState_Stance(){
            StateTag = "Stance";
            next_state_on_timeout = "Idle";
            Timeout = 5;
            move = "Idle";
        }
    }

    [GlobalClass]
    public partial class SlimeState_Evade : GodotState{
        
        public SlimeState_Evade(){
            StateTag = "Evade";
            next_state_on_timeout = "Idle";
            Timeout = 5;
            move = "Idle";
        }
    }

    [GlobalClass]
    public partial class StateMachine_Slime : StateMachine
    {
        public StateMachine_Slime() : this(0, null, null) {}

        public StateMachine_Slime( int id, GodotStringGraph string_graph, Godot.Collections.Dictionary<string, GodotState> states) : base(id,string_graph, states){}

        public override void OnInitStates(){
            base.OnInitStates();
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

        public override void OnInitGraph(){
            StringGraph.AddVertex("Idle", "Cutscene");      // OnEntered
            StringGraph.AddVertex("Idle", "Wander");        // OnTimeout
            StringGraph.AddVertex("Idle", "Chase");         // OnPlayerSeen
            StringGraph.AddVertex("Idle", "Evade");         // OnCrit
            StringGraph.AddVertex("Cutscene", "Wander");    // OnFinished
            StringGraph.AddVertex("Wander", "Chase");       // OnPlayerSeen 
            StringGraph.AddVertex("Wander", "Idle");        // OnTimeout
            StringGraph.AddVertex("Wander", "Evade");       // OnCrit
            StringGraph.AddVertex("Chase", "Attack");       // OnWithinRange
            StringGraph.AddVertex("Chase", "Stance");       // OnHit
            StringGraph.AddVertex("Chase", "Evade");        // OnCrit
            StringGraph.AddVertex("Attack", "Attack");      // OnFinished(player_within_range)
            StringGraph.AddVertex("Attack", "Chase");       // OnFinished(player_out_of_range)
            StringGraph.AddVertex("Attack", "Stance");      // OnDamageInterrupt
            StringGraph.AddVertex("Attack", "Evade");       // OnCritInterrupt
            StringGraph.AddVertex("Stance", "Attack");      // OnTimeout(PlayerInsideRange)
            StringGraph.AddVertex("Stance", "Chase");       // OnTimeout(PlayerOutOfRange)
            StringGraph.AddVertex("Stance", "Wander");      // OnTimeout(PlayerMissing)
            StringGraph.AddVertex("Evade", "Idle");         // OnTimeout()
            stateDiagram = StringGraph;
        }

        public override void OnUpdate(Godot.Collections.Dictionary<string, Variant> observations){
            base.OnUpdate(observations);
            switch(ActiveState){
                case "Idle":
                    //GD.Print($"State: Idle, activity {this.States[ActiveState].move}");
                break;
                case "Cutscene":
                    GD.Print("Cutscene");
                break;
                case "Wander":
                    //GD.Print("Wander");
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
                    //GD.Print("Evade");
                break;
                default:
                    GD.Print("Unknown state");
                break;
            }
        }
    }
