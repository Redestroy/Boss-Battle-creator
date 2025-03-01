using System.Collections.Generic;
using BossBattleClash;
using Godot;

public partial class StatePlayer : Node{
    
    public Timer state_timer;
    public StateMachine stateMachine{ get; set; }
    public Godot.Collections.Dictionary<string, GodotState> States{get;set;}
    
    [Export]
    public Godot.Collections.Array<GodotState> StateValues{get;set;}
    
    
    public override void _Ready()
	{
        state_timer = new Timer();
        state_timer.OneShot = true;
        state_timer.Autostart = false;
        state_timer.Timeout += OnTimerTimeout;
        AddChild(state_timer);
        //state_timer.Start((float)stateMachine.CurrentState.Timeout);
        //GD.Print($"Started Timer {stateMachine.CurrentState.Timeout}");
    }

    public StateMachine AttachStateMachine(StateMachine stateMachine, System.Action<string> isf){
        stateMachine ??= new StateMachine();
        if(stateMachine != null){
            //stateMachine_.Init();
            stateMachine.AttachToAll_InState(isf);
            GD.Print($"Active state {stateMachine.ActiveState}");
            if(stateMachine.HasState()) stateMachine.CurrentState.Trigger();
            if(stateMachine.HasState()) state_timer.Start((float)stateMachine.CurrentState.Timeout);
        }
        else{
            GD.PrintErr("No state machine to attach");
        }
        return stateMachine;
    }

    public void OnTimerTimeout(){
        GD.Print("Timed out");
        stateMachine.ChangeState(stateMachine.CurrentState.next_state_on_timeout); // On timeout go to state stored in the state
        state_timer.Start(stateMachine.CurrentState.Timeout);
        GD.Print($"Started Timer {stateMachine.CurrentState.Timeout} for state {stateMachine.ActiveState}");
    }
}