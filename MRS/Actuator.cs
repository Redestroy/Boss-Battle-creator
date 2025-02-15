using System;
using System.Collections.Generic;

namespace MRS {
	namespace Device {
	public abstract class Device {
		static int device_count = 0; // ?
		public string Type{ // Device Type
                get;
                private set;
        }
		UInt64 DeviceId{get;set;}
		bool IsInitialized{get;set;}
		Dictionary<string, string> tags;
		public Device(): this("Null"){
        }
		public Device(string device_type){
            Type = device_type;
            tags = new Dictionary<string, string>();
        }
		public abstract void Init();
		public abstract void DeInit();
		void AddTag(string tag, string value){ // I think tags were parameters
            tags[tag] = value;
        }
		string GetTag(string tag){
            if(tags.TryGetValue(tag, out string val)){
                return val;
            }
            return $"Not found @{tag}";
        }

	    }

	public abstract class Actuator : Device
		{
		string ActuatorType{get;set;}
		String description{get;set;} // This is the serialization of the actuator

		public Actuator(){}
		public Actuator(string actuator_string){
            //parse string
        }

		public override void Init(){

        }

		public override void DeInit(){

        }

		public virtual bool DoAction(Action action){
            return OnDoAction(action);
        }

		public virtual bool OnDoAction(Action action){
			return false;
		}
		//virtual string ToString(){}
		};
	}
}