using System;
using System.Collections.Generic;



namespace MRS {
	namespace Device {

		public class View
		{
			static Dictionary<string, string> type_strings{get; set;}
			string Type{get; set;}
			string timeCreated;
			byte[] data{get; set;}
			public View() : this(1, "Base_type") {

			}

			public View(int id, string view_type){
				Type = view_type;
				long time_c = Utility.GetNowUnixTime();
				OnCreate(time_c); 
			}

			public View(string description){
				//TODO add parsing
			}

			public string GetViewType() {
				return Type;
			}

			public virtual void OnCreate(long time){
				timeCreated = Utility.ConvertUnixTimeToDate(time);
			}

			public override string ToString(){
				return $"view_{base.ToString()}";
			}
            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }
                View comp_obj = obj as View;
                return (this == comp_obj);
            }

			public int TypeToInt(string view_type){
				return view_type.GetHashCode(); //TODO may add creation time to string for unique hash 
			}
            
            public override int GetHashCode()
            {
                return TypeToInt(GetViewType());
            }

			public static bool operator ==(View a, View b) {
				return a.Type == b.Type; //TODO implement real comparison, type is naive (use an abstract method)
			}

            public static bool operator !=(View a, View b) {
				return a.Type != b.Type; //TODO implement real comparison, type is naive (use an abstract method)
			}
			//virtual int GetAspect(int, TypeDefinitions::ViewType, char[]) = 0; // Writes the data value of the view to the buffer, returns buffer size in bytes/, args: aspect id, aspect type, buffer in which aspect is written

		};
	}
}
