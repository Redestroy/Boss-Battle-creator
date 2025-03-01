using System.Collections.Generic;
using Godot;
using MRS.Task;

[GlobalClass]
public partial class SlimeState_Wander : GodotState{

        double sec;
        bool playing_animation; 

        public SlimeState_Wander(){
            StateTag = "Wander";
            next_state_on_timeout = "Idle";
            sec = 1;
            Timeout = 5*sec;
            move = "UltimateMove";
        }

        public override void OnTrigger()
        {
            base.OnTrigger();
        }

        public override void InState(string activity)
        {
            base.InState(activity);
            if(!playing_animation){
                update_move = true;
            }
        }

        public override void OnUpdate(Godot.Collections.Dictionary<string, Variant> observations){
            base.OnUpdate(observations);
            playing_animation = (bool)observations["playing_animation"];
        }
    }