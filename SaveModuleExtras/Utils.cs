using System;
using Newtonsoft.Json;

public static class Utils
{
    // public static ApplyOnConditionSingle<T>(Func <>)
    // {

    // }

    // public static ReturnOnConditionSingle
    // {

    // }

    // public static ApplyToAll
    // {

    // }

    public static InfoWrapperType GetContent<InfoWrapperType>(string savePath)
    {
        string content;
        Godot.File file = new Godot.File();
        // file.Open("res://Buildables.json", Godot.File.ModeFlags.Read);
        file.Open(savePath, Godot.File.ModeFlags.Read);
        content = file.GetAsText();
        file.Close();
        return JsonConvert.DeserializeObject<InfoWrapperType>(content);
    }
}