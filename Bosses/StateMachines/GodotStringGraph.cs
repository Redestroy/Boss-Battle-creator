using Godot;
using System;

[GlobalClass]
public partial class GodotStringGraph : Resource, MRS.Task.IStringGraph{

            [Export]
            public Godot.Collections.Array<GodotStringPair> edge_list{get; set;}
            [Export]
            public Godot.Collections.Array<GodotStringPair> wildcards{get;set;} // wildcards work as follows - if one side has a wildcard, it means that all edges either can transition to a state, or from a state, basically a check override

            public string wildcard = "*";

			public GodotStringGraph() : this(0, null, null) {}

            public GodotStringGraph(int id, Godot.Collections.Array<GodotStringPair> EdgeList, Godot.Collections.Array<GodotStringPair> Wildcards){
                edge_list = EdgeList ?? new Godot.Collections.Array<GodotStringPair>();
                wildcards = Wildcards ?? new Godot.Collections.Array<GodotStringPair>();
            }

            public void AddVertex(string from, string to){
                // Duplicate check advised
                if(Contains(from,to)) return; // do not add duplicate vertices
                edge_list.Add(new GodotStringPair(from, to));
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
