using Godot;
using System;

[GlobalClass]
    public partial class DoorInfo : Resource{
        [Export]
        public string DoorName{get;set;}
        [Export]
        public string scene_triggered{get;set;}
        [Export]
        public string door_text{get;set;}

        public DoorInfo() : this("", "", ""){}
        public DoorInfo(string DoorName_, string scene_triggered_, string door_text_){
            this.DoorName = DoorName_;
            this.scene_triggered = scene_triggered_;
            this.door_text = door_text_;
        }
        public DoorInfo(Door door) : this(door.Name, door.scene_triggered, door.door_text){}

        public static DoorInfo FindDoorInfoByName(string name, Godot.Collections.Array<DoorInfo> doorInfos){
            foreach( var door_info in doorInfos){
                if(door_info.DoorName == name){
                    return door_info;
                }
            }
            return null;
        }
    }