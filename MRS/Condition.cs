using System.Collections.Generic;

namespace MRS {
	namespace Task {
		public abstract class Condition
		{
			static Dictionary<string, string> type_strings;

			public string conditionType{get; private set;}
		    public Condition(){

            }

		    public Condition(string type, string view_description){

            }

		    public Condition(string description){

            }

		    public abstract Condition CreateCondition(string description);
			public abstract bool isMet(Environment.Worldview worldview);
			public abstract string toString();
		};
	}
}