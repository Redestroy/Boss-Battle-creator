using System.Collections.Generic;
using System.Text;
using Godot;

namespace MRS{
    namespace Task{

        public interface IState{
            public string StateTag{get;set;}
            public void OnTrigger();
            public bool check_done();
            public void InState(string activity);
            public void OnExit();
        }

        public interface IStringGraph{
            public void AddVertex(string from, string to);
            public bool Contains(string from, string to);
        }

        public class StringGraph : IStringGraph{
            public List<KeyValuePair<string,string>> edge_list;
            public List<KeyValuePair<string,string>> wildcards; // wildcards work as follows - if one side has a wildcard, it means that all edges either can transition to a state, or from a state, basically a check overrride

            public string wildcard = "*";

            public StringGraph(){
                edge_list = new List<KeyValuePair<string, string>>();
                wildcards = new List<KeyValuePair<string, string>>();
            }

            public void AddVertex(string from, string to){
                // Duplicate check advised
                if(Contains(from,to)) return; // do not add duplicate vertices
                edge_list.Add(new KeyValuePair<string,string>(from, to));
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

        public interface IStateMachine{
            public IStringGraph stateDiagram{get;set;} // Can be represented as a List of pairs representing <From, To> states, wildcards allowed
            public string ActiveState{get; protected set;}     
            public bool ChangeState(string target); // changes state to target, if not possible, return false
            //                               if(target == ActiveState) state_dict[ActiveState].restart() // Only useful if parameters are passed
            // Logic works as follows - call if(stateDiagram.Contains(ActiveState, target)){
            //                                     state_dict[ActiveState].Interrupt(target)
            //                                     //state_dict[target].Trigger()      
            //                               }
        }

        public abstract class State : Task, IState{ // May extend behavior

            //[Signal]
            //public delegate void SignalInterruptedEventHandler(); // May add arguments for carry over
            public string StateTag{get;set;} = "Idle";
            public string move;
            private bool interrupted;
            private string next_state; //Used on Exit state. May be replaced by State reference
            private bool IsActive{get;set;}
            public IStateMachine stateMachine{get;set;}

            public State(){
                move = "new Move()";
                interrupted = false;
                IsActive = false;
            }

            public void Trigger(){
                OnTrigger();
                IsActive = true;
                Run();
            }

            public void Run(){ // This should be done in a separate thread. Probably godot would need to use IState local implemantation
                while(!check_done()){ // && !interrupted
                    if(interrupted){
                        // sleep or something
                        // also pause if needed to stop floating
                        //if(force_state_change){
                        //    ForceExitState(force_state_change);
                        //}
                    }
                    InState(move);
                }
                ExitState(next_state);
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

            public abstract void OnInterrupt();
            public abstract void EmitSignalForGodot();
            public abstract void OnTrigger();
            public abstract bool check_done();
            public abstract void InState(string activity);
            public abstract void OnExit();
        
        }
    }
}