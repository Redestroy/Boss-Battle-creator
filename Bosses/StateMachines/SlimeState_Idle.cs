
using System.Collections.Generic;
using Godot;
using MRS.Task;
[GlobalClass]
public partial class SlimeState_Idle : GodotState{
        Timer internal_timer;
        double sec;
        
        
        public SlimeState_Idle() : base(){
            StateTag = "Idle";
            next_state_on_timeout = "Wander";
            sec = 1;
            Timeout = 4*sec;
            move = "Idle";
        }

        public override void OnTrigger()
        {
            base.OnTrigger();
        }
}