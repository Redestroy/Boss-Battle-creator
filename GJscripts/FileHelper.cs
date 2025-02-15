using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;

public static class FileHelper{

    public static void SaveStringListAsJson(List<string> keys, string path){
		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
		var jsonString = Json.Stringify(ConvertToGodotList(keys));
		file.StoreLine(jsonString);
	}

    public static Godot.Collections.Array<Variant> ConvertToGodotList(List<string> arr){
		var res = new Godot.Collections.Array<Variant>();
		foreach(var item in arr){
			res.Add(item);
		}
		return res;
	}

    public static void SaveAliasDictionaryAsJson(Dictionary<string,string> data, string path){
		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
		 var jsonString = Json.Stringify(ConvertToGodot(data));
		file.StoreLine(jsonString);
	}

	public static void SaveIdsAsJson(Dictionary<int, string> ids, string path){
		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
		var jsonString = Json.Stringify(ConvertToGodotIds(ids));
		file.StoreLine(jsonString);
	}

    public static List<string> ConvertToList(Godot.Collections.Array<Variant> arr){
		var res = new List<string>();
		foreach(var item in arr){
			res.Add(item.ToString());
		}
		return res;
	} 

    public static Godot.Collections.Array<Variant> LoadJsonAsList(string path){
		if (!Godot.FileAccess.FileExists(path))
    	{
        	return null; // Error! We don't have a save to load.
    	}
		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
		string contents = file.GetAsText();
		Json json = new Json();
		var json_var = json.Parse(contents);
		var list = new Godot.Collections.Array<Variant>((Godot.Collections.Array)json.Data);
		return list;
	}

    public static Godot.Collections.Dictionary<string, Variant> ConvertToGodot(Dictionary<string, string> dict){
		var res = new Godot.Collections.Dictionary<string, Variant>();
		foreach(var item in dict){
			res[item.Key] = item.Value;
		}
		return res;
	}

	public static Godot.Collections.Dictionary<string, Variant> ConvertToGodotIds(Dictionary<int, string> dict){
		var res = new Godot.Collections.Dictionary<string, Variant>();
		foreach(var item in dict){
			res[$"{item.Key}"] = item.Value;
		}
		return res;
	}

    public static Dictionary<string, string> ConvertToString(Godot.Collections.Dictionary<string, Variant> dict){
		var res = new Dictionary<string, string>();
		foreach(var item in dict){
			res[item.Key] = item.Value.ToString();
		}
		return res;
	}

	public static Dictionary<int, string> ConvertToIdMap(Godot.Collections.Dictionary<string, Variant> dict){
		var res = new Dictionary<int, string>();
		foreach(var item in dict){
			res[int.Parse(item.Key)] = item.Value.ToString();
		}
		return res;
	}

    public static Godot.Collections.Dictionary<string, Variant> LoadJsonAsDict(string path){
		if (!Godot.FileAccess.FileExists(path))
    	{
        	return null; // Error! We don't have a save to load.
    	}
		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
		string contents = file.GetAsText();
		Json json = new Json();
		var json_var = json.Parse(contents);
		var dict = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);
		return dict;
	}

	
}