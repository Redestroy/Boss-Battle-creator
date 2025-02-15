using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Deck<T>
{
    public Dictionary<string, T> cards;
    public List<string> keys;

    public T Draw(){
        int options = 6;
        string key = GetRandomKey(keys, options); //Later replace with selector
        return cards[key];
    }

    string GetRandomKey(List<string> keys, int options){
        return keys.FirstOrDefault();
    }
}

public partial class Move : Resource{
    int size;

    int num_of_actions;

    [Export]
    Godot.Collections.Array<int> action_sequence;
    [Export]
    Godot.Collections.Dictionary<int, string> actions;
    [Export]
    Godot.Collections.Array<double> timings;
    
    public int AddAction(string action){
        actions[size] = action;
        size++;
        return size - 1;
    }

    public void AddActionToSequence(int action){
        //if(actions.Contains(action)){
        //    AddAction(action);
        //}
        int index = action;
        if(actions.ContainsKey(action)){
            action_sequence.Append(index);
        }
    }
    
    public void AddActionToSequence(string action){
        int index = 0;
        //actions.ContainsKey(action);
        //if(actions.Contains<string>(action)){
        //    index = AddAction(action);
        //    action_sequence.Append(index);
        //}else
        {
            index = FindByValue(actions, action);
            action_sequence.Append(index);
        }
    }

    public int FindByValue(Godot.Collections.Dictionary<int, string> actions, string action){
        foreach (var pair in actions){
            if (pair.Value == action){
                return pair.Key;
            }
        }
        return -1;
    }

    public void play_move(AnimatedCharacter character){
        Timer timer = new Timer();
    }
}



public partial class AnimatedCharacter: CharacterBody3D{
    private Skeleton3D skeleton3D;
    //private Model3D model3D;
    private Deck<Animation> animation_deck;
    private Deck<Move> move_deck;

    
}