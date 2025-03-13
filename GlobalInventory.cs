using Godot;
using System;

public partial class GlobalInventory : Node
{
    public System.Collections.Generic.Dictionary<string, Inventory> inventories = new System.Collections.Generic.Dictionary<string, Inventory>();

    public override void _Ready()
    {
        GD.Print("GlobalInventory Singleton is ready!");
    }
     public void AddInventory(string key, Inventory obj)
    {
        inventories[key] = obj;
    }

    // Function to retrieve an object
    public Inventory GetInventory(string key)
    {
        return inventories.ContainsKey(key) ? inventories[key] : null;
    }

    // Function to remove an object
    public void RemoveObject(string key)
    {
        if (inventories.ContainsKey(key))
        {
            inventories.Remove(key);
        }
    }

    public void PrintGlobalinventory(){
        foreach(var inventory in inventories){
            GD.Print($"Inventory {inventory.Key} : {inventory.Value}");
            //inventory.Value.PrintContents();
        }
    }

    public void CopyInventory(string name, Inventory inventory){
        if(!inventories.ContainsKey(name)){
            var inventory_ = inventory.Duplicate() as Inventory;
            inventories.TryAdd(name, inventory_);
        }
    }

    public void SaveInventory(string name, Inventory inventory){
        PrintGlobalinventory();
        if(!inventories.ContainsKey(name)){
            CopyInventory(name, inventory);
        }else{
            inventories[name].UpdateInventory(inventory);
        }
    }

    public Inventory LoadInventory(string name){
        return this.GetInventory(name);
    }
}
