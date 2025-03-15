using System;
using System.Collections.Generic;
using Godot;

public interface IExecutor{
    public Godot.Collections.Dictionary<string, string> Shorthands{get;set;}
    public void Init();
    public void ExecuteOrder(string order);
    public void OnExecuteFailed(string order);
    public void OnUpdate();
}

public abstract partial class Executor : Node, IExecutor{
    [Export]
    public Godot.Collections.Dictionary<string, string> Shorthands{get;set;}

    public Executor() : base(){
        Shorthands ??= new Godot.Collections.Dictionary<string, string>();
    }

    public abstract void Init();

    public void AddShorthand(string alias, string order){
        if(!Shorthands.TryAdd<string,string>(alias, order)){
            GD.Print("Shorthand already present");
        }
    }

    public void RemoveShorthand(string alias){
        Shorthands.Remove(alias);
    }

    public virtual void ExecuteOrder(string order){
        if (!order.Contains("/"))
        {
            foreach (var shorthand in Shorthands.Keys)
            {
                if (shorthand == order)
                {
                    ExecuteOrder(Shorthands[shorthand]);
                    return;
                }
            }
        }
    }
    public abstract void OnExecuteFailed(string order);

    public abstract void OnUpdate();
}
