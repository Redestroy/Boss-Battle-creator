using System.Collections.Generic;
using System.Threading;
using Godot;

[GlobalClass]
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
    public string move{get;set;} = "Idle";
    private bool interrupted;
    private string force_state_change;


    [Export]
    public double Timeout{get;set;} //Used for an external timer

    private string next_state{get;set;} //Used on Exit state. May be replaced by State reference
    [Export]
    public string next_state_on_timeout{get;set;} //Used on Exit state. May be replaced by State reference
    private bool IsActive{get;set;}

    protected bool update_move = true;
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
        if(update_move){
            EmitSignal(SignalName.SigInState, activity); // TODO Add activity as argument argument
            update_move = false;
        }
        //Sleep
        //Also, maybe only notify on enter and exit
    }
    public virtual void OnExit(){
        EmitSignal(SignalName.SigOnExit);
    }

    public virtual void OnUpdate(Godot.Collections.Dictionary<string, Variant> observations){
        
    }
}