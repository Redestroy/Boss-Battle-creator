using System.Collections.Generic;
using Godot;
using MRS.Task;

[GlobalClass]
    public partial class StateMachine : Resource, IStateMachine
    {
        
        [Export]
        public GodotStringGraph StringGraph { get; set; }

        [Export]
        public string ActiveState{get; set;} = "Idle";

        [Export]
        public Godot.Collections.Array<string> StateKeys { get; set; }

        [Export]
        public Godot.Collections.Array<GodotState> StateValues {get; set;}

        [Export]
        public Godot.Collections.Dictionary<string, GodotState> States{get;set;}

        public GodotState CurrentState{
            get{
                return States.GetValueOrDefault(ActiveState, null);
            }
        }

        public MRS.Task.IStateMachine stateMachine;
        public IStringGraph stateDiagram{get;set;}

        public delegate void InStateFunction(Move move);
        
        // Make sure you provide a parameterless constructor.
        // In C#, a parameterless constructor is different from a
        // constructor with all default values.
        // Without a parameterless constructor, Godot will have problems
        // creating and editing your resource via the inspector.
        public StateMachine() : this(0, null, null) {}

        public StateMachine( int id, GodotStringGraph string_graph, Godot.Collections.Dictionary<string, GodotState> states)
        {
            StringGraph = string_graph ?? new GodotStringGraph();
            States = states ?? new Godot.Collections.Dictionary<string, GodotState>();
            Init();
            GD.Print(States);
        }

        public void Init(){ // Inits the state machine from code
            if(States.Count == 0){
                InitStates();
            }
            if(StringGraph.edge_list.Count == 0){
                InitGraph();
            }
        }

        public void InitStates(){
            if(StateKeys != null && StateValues != null){
                if(StateKeys.Count != StateValues.Count){
                    //Throw warning
                    GD.PrintErr("Warning: state machine might not be properly set up.");
                }else if(StateKeys.Count == 0){
                    //Throw warning
                    GD.PrintErr("Warning: state machine might be empty");
                }else{
                    for(int i = 0; i < StateKeys.Count; i++){
                        States.Add(StateKeys[i], StateValues[i]);
                    }
                }
            }
            OnInitStates();
        }

        public virtual void OnInitStates(){
            //Meant to be overridden
        }

        public void AttachToAll_InState(System.Action<string> isf){
            foreach(var state in States){
                state.Value.Connect( nameof(GodotState.SigInState), Callable.From(isf));
            }
        }

        public void AddState(GodotState state){
            if(States.ContainsKey(state.StateTag)) return;
            States.Add(state.StateTag, state);
        }

        public void InitGraph(){
            OnInitGraph();
        }

        public virtual void OnInitGraph(){

        }

        public bool HasState(){
            return CurrentState != null;
        }

        public virtual void OnUpdate(Godot.Collections.Dictionary<string, Variant> observations){
            //switch(ActiveState){
            //    case "Idle":
            //        //GD.Print($"State: Idle, activity {this.States[ActiveState].move}");
            //    break;
            //    case "Cutscene":
            //        GD.Print("Cutscene");
            //    break;
            //    case "Wander":
            //        //GD.Print("Wander");
            //    break;
            //    case "Chase":
            //        GD.Print("Chase");
            //    break;
            //    case "Attack":
            //        GD.Print("Attack");
            //    break;
            //    case "Stance":
            //        GD.Print("Stance");
            //    break;
            //    case "Evade":
            //        //GD.Print("Evade");
            //    break;
            //    default:
            //        GD.Print("Unknown state");
            //    break;
            //}
            if(HasState())CurrentState.OnUpdate(observations);
        }

        public bool ChangeState(string target){
            if(target == ActiveState){
                States[ActiveState].Restart(); // Only useful if parameters are passed
                return true; // This will return very late so use threads or something
            }
            if(StringGraph.Contains(ActiveState, target)){
                GD.Print($"Active state {ActiveState}");
                GD.Print($"Target {target}");
                GD.Print($"Active state {States[ActiveState]}");
                GD.Print($"Target {States[target]}");
                States[ActiveState].ExitState(target); // Maybe call these later from a different spot like OnUpdate
                States[target].Trigger();
                ActiveState = target;
                GD.Print($"Active state of state machine{ActiveState}");
                GD.Print($"State: {ActiveState}, activity {this.States[ActiveState].move}");
                return true; // This will return very late so use threads or something
            }else{
                GD.Print($"Transition not in graph");
                GD.Print($"Active state of state machine{ActiveState}");
                GD.Print($"State: {ActiveState}, activity {this.States[ActiveState].move}");
                return false;
            }
        }
    }
