using System;
using System.Collections.Generic;

namespace MRS {
	namespace Task {
		public interface IConditionCreator{

		}

		public abstract class Task
		{
		    static Dictionary<string, string> state_strings;

			static Dictionary<string, string> type_strings;

			public Condition startCondition{get; protected set;}
			public Condition executedCondition{get; protected set;}
			public string state{get; protected set;}
			int actionIterator;
			public string type{get; protected set;}

			public float givenPriority{get; protected set;}
			public UInt64 ID{get; protected set;}
			public string timestamp{get; protected set;}
			public string taskString{get; protected set;}

            public bool IsBuilt{get;set;} // TODO implement a proper check
			private UInt32 size;
			IConditionCreator c_creator;
		    public Task(){

            }
			public Task(UInt64 id){

            }
			public Task(UInt64 id, float priority){

            }
			public Task(string description){

            }
			public Task(string description, IConditionCreator creator){
                
            }

			void GoToNext(){

            }

			public UInt32 GetSize(){
				return size;
            }

			public abstract Task CreateTaskFromString(string description);
			public abstract MRS.Action GetNextCommand(); // Returns the next command to be executed
			public abstract string toString();

			//static string GetTaskStateFromInteger(int state_idx){ // Maybe use lists for read in instead
//
            //}
            
			//static string GetTaskTypeFromInteger(int type_idx){
            //    if(type_strings.TryGetValue
            //}

		};
	}
}