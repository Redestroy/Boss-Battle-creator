using Godot;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
public partial class AnimationExecutor : Executor
{
    public Dictionary<string, AnimationPlayer> players = new Dictionary<string, AnimationPlayer>();
    public Godot.Collections.Dictionary<string, Godot.Collections.Array<GodotStringPair>> animation_bundles = new Godot.Collections.Dictionary<string, Godot.Collections.Array<GodotStringPair>>();

    public AnimationExecutor() : base(){
    }

    public override void Init()
    {
        GD.Print("AnimationExecutor initiated");
    }

    public override void _Ready()
    {
        base._Ready();
    }

    public void GetPlayersFromCharacter(Character character){
        // Add character animation player
        players.Add("Character", character.animationPlayer);
        // Add item animation players
        foreach(var item in character.item_instances){
            // Add if check
            players.Add(item.Value.equippableInfo.EquippableType, item.Value.animationPlayer);
        }
        // Add additional players: particle emitters, cosmetics, hats, etc.
    }

    public void AddAnimationPlayer(string key, AnimationPlayer animationPlayer){
        players.TryAdd<string, AnimationPlayer>(key, animationPlayer);
    }

    public void RemoveAnimationPlayer(string key){
        if(players.ContainsKey(key)){
            players.Remove(key);
        }
    }

    public void AddPairToBundle(string bundle, GodotStringPair player_animation_pair){
        if(!animation_bundles.ContainsKey(bundle)){
            animation_bundles[bundle] = new Godot.Collections.Array<GodotStringPair>();
        }
        animation_bundles[bundle].Add(player_animation_pair);
    }

    public void AddPairToBundle(string bundle, string animationPlayer, string animation){
        AddPairToBundle(bundle, new GodotStringPair(animationPlayer, animation));
    }

    

    public void AddBundle(string bundle_key, Godot.Collections.Array<GodotStringPair> bundle){
        if(!animation_bundles.ContainsKey(bundle_key)){
            animation_bundles[bundle_key] = bundle;
        }else{
            GD.Print("Bundle with same key already present");
        }
    }

    public void AddBundleFromMove(MoveInfo moveInfo){
        AddBundle(moveInfo.Alias_label, moveInfo.move_animation);
    }

    public void RemoveAnimationFromBundle(string bundle_key, string animation){
        if(animation_bundles.ContainsKey(bundle_key)){
            var bundle_ = animation_bundles[bundle_key];
            foreach(var pair in bundle_){
                if(pair.Value == animation){
                    bundle_.Remove(pair);
                    return;
                }
            }
        }
    }

    public void RemoveBundleFromMove(MoveInfo moveInfo){
        RemoveBundle(moveInfo.Alias_label);
    }

    public void RemoveBundle(string bundle_key){
        if(animation_bundles.ContainsKey(bundle_key)){
            animation_bundles.Remove(bundle_key);
        }
    }

    public override void ExecuteOrder(string order)
    {
        base.ExecuteOrder(order);
        string[] parts = order.Split(new[] { "/" }, StringSplitOptions.None);
        switch (parts[0])
        {
            case "Play":
                if (parts.Length > 1){
                    PlayAnimation(parts[1]);
                }
            break;
            default:
                OnExecuteFailed(order);
            break;
        }
    }

    public override void OnExecuteFailed(string order)
    {
        
    }

    public void PlayAnimation(string animation_bundle){
        if(!animation_bundles.ContainsKey(animation_bundle)){
            GD.Print($"Animation not in executor: {animation_bundle}");
            return;
        }
        var bundle = animation_bundles[animation_bundle];
        foreach(var pair in bundle){
            if(players.TryGetValue(pair.Key, out AnimationPlayer player)){
                if(player.HasAnimation(pair.Value)) player.Play(pair.Value);
                else GD.Print($"No animation {pair.Value} in player {pair.Key}");
            }else{
                GD.Print($"No player {pair.Key} in Animation Players");
            }
        }
    }

    public override void OnUpdate()
    {
        
    }
}