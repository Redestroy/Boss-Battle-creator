using System;
using System.Reflection.Metadata;
using Godot;

namespace MRS{
    public abstract class Action{

        public const string className = "Action";
        public const string sep = "||";
        string Type{get;set;}
        byte[] data; 

        protected string ActionParameter{get;set;}

        public string asString{
            get{
                return $"{sep[0]}{Action.className}: {Type} {ActionParameter}{sep[1]}";
            }
        }

        public Action(){

        }

        public Action(string type, string parameter){
            this.Type = type;
            this.ActionParameter = parameter;
            data = toBytes();
        }

        public Action(Action action){
            this.Type = action.Type;
            //this.data = action.data.Clone();
        }

        public string toString(){
            return asString;
        }

        public byte[] toBytes(){
            return Encoded(this);
        }

        static byte[] Encoded(Action action){
            byte[] data = new byte[32];
            // place stuff in data
            // place type as data
            // place parameter as data 
            // set last as 0 if no method is used
            data[31] = 0;
            return data;
        }

        // char to action is done via the built in is operator or TryAndConvert
        // rest of conversion methods are not currently required as well  
    }
}