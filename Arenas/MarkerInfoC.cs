using Godot;
using System;

[GlobalClass]
    public partial class MarkerInfoC : Resource
    {
    [Export]
    public string Name{get;set;}
    [Export]
    public string NodeType{get;set;} //Used to determine in which subdeck the node should be saved
    [Export]
    public string SpawnedNode{get;set;}
    [Export]
    public float Px{get;set;}
    [Export]
    public float Py{get;set;}
    [Export]
    public float Pz{get;set;}
    [Export]
    public Quaternion angle{get;set;}

    public MarkerInfoC() : this(0, "", 0.0f, 0.0f, 0.0f, Godot.Quaternion.Identity) {} //

    public MarkerInfoC(int id, string name_, float px_, float py_, float pz_ , Quaternion angle) //
    {
        Name = name_;
        Px = px_;
        Py = py_;
        Pz = pz_;
        this.angle = angle;
    }

    public MarkerInfoC(int id, string name, Marker3D marker) : this (id, name, marker.GlobalPosition.X, marker.GlobalPosition.X, marker.GlobalPosition.X, marker.GlobalBasis.GetRotationQuaternion()){ //
    }

    public static Marker3D CreateMarker3D(MarkerInfoC markerInfo){
        Marker3D marker = new Marker3D();
        marker.Position = new Vector3(markerInfo.Px, markerInfo.Py, markerInfo.Pz);
        marker.Quaternion = markerInfo.angle;
        return marker;
    }
    }

    