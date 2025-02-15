using System.Collections.Generic;
using System.Threading;
using Godot;

public partial class GodotStringPair : Resource{
    [Export]
    public string From{get; set;} = "";
    [Export]
    public string To{get; set;} = "";

    public string Key{
        get{
            return From;
        } 
        set{
            From = value;
        }
    }
    public string Value{
        get{
            return To;
        } 
        set{
            To = value;
        }
    }

    public GodotStringPair() : this("", "") {}

    public GodotStringPair(string from = "", string to = ""){
        From = from;
        To = to;
    }
}

public partial class GodotStringGraph : Resource, MRS.Task.IStringGraph{

            [Export]
            public Godot.Collections.Array<GodotStringPair> edge_list;
            [Export]
            public Godot.Collections.Array<GodotStringPair> wildcards; // wildcards work as follows - if one side has a wildcard, it means that all edges either can transition to a state, or from a state, basically a check override

            public string wildcard = "*";

            public GodotStringGraph(){
                edge_list = new Godot.Collections.Array<GodotStringPair>();
                wildcards = new Godot.Collections.Array<GodotStringPair>();
            }

            public void AddVertex(string from, string to){
                // Duplicate check advised
                if(Contains(from,to)) return; // do not add duplicate vertices
                edge_list.Add(new GodotStringPair(from, to));
            }

            public bool Contains(string from, string to){
                foreach(var pair in wildcards){
                    if(pair.Key != wildcard && from != pair.Key) continue;
                    if(pair.Value != wildcard && to != pair.Value) continue;
                    return true;
                }
                foreach(var pair in edge_list){
                    if(pair.Key != from) continue;
                    if(pair.Value != to) continue;
                    return true;
                }
                return false;
            }
        }



public abstract partial class GodotState : Resource, MRS.Task.IState{

    [Signal]
    public delegate void SignalInterruptedEventHandler(); // May add arguments for carry over

    [Signal]
    public delegate void SigOnInterruptEventHandler(); // May add arguments for carry over
    [Signal]
    public delegate void SigOnTriggerEventHandler(); // May add arguments for carry over
    [Signal]
    public delegate void SigInStateEventHandler(); // May add arguments for carry over
    [Signal]
    public delegate void SigOnExitEventHandler(); // May add arguments for carry over

    [Export]
    public string StateTag{get;set;} = "Idle";
    [Export]
    public string move;
    private bool interrupted;
    private string force_state_change;
    private string next_state{get;set;} //Used on Exit state. May be replaced by State reference
    private bool IsActive{get;set;}
    public MRS.Task.IStateMachine stateMachine{get;set;}
    public GodotState(){
        move = "new string()"; // Check type
        interrupted = false;
        IsActive = false;
    }

    public void Trigger(){
        OnTrigger();
        IsActive = true;
        Run();
    }

    public void Run(){ // This should be done in a separate thread. Probably godot would need to use IState local implementation
        //Thread version
        //while(!check_done()){ // && !interrupted
        //    if(interrupted){
                        // sleep or something
                        // also pause if needed to stop floating
                        //if(force_state_change){
                        //    ForceExitState(force_state_change);
                        //}
        //    }
        //    InState(move);
        //}
        //
        //Polling version - poll each on update while active
        if(!check_done()){ // && !interrupted
            if(interrupted){
                if(force_state_change != ""){
                    ForceExitState(force_state_change);
                }
                else{
                    ExitState("");
                }
            }
            InState(move);
        }
        else ExitState(next_state);
    }

    public void ExitState(string next_state){
        OnExit();
        IsActive=false;
        if(next_state == StateTag){
            Trigger();
        }
    }

    public void ForceExitState(string next_state){
        EmitSignalForGodot();
        IsActive=false;
    }

    public void Interrupt(string type){ // Attach this to a signal
        interrupted = true;
        next_state = type;
        OnInterrupt();
    }

    public void Restart(){
        if(IsActive){
            ExitState(StateTag);
        }else{
            Trigger();
        }
    }

    public void EmitSignalForGodot(){
        EmitSignal(SignalName.SignalInterrupted);
    }

    public virtual void OnInterrupt(){
        EmitSignal(SignalName.SigOnInterrupt);
    }
    public virtual void OnTrigger(){
        EmitSignal(SignalName.SigOnTrigger);
    }
    public virtual bool check_done(){
        return interrupted; // TODO Maybe add second as well?
    }
    public virtual void InState(string activity)
    {
        EmitSignal(SignalName.SigInState); // TODO Add activity as argument argument
        //Sleep
        //Also, maybe only notify on enter and exit
    }
    public virtual void OnExit(){
        EmitSignal(SignalName.SigOnExit);
    }
}