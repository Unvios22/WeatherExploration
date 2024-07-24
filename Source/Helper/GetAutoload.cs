using Godot;
using WeatherExploration.Source.Autoload;

namespace WeatherExploration.Source.Helper;

public static class GetAutoload {
    //Helper class for getting Autoload references (that are intended by Godot devs for C# Godot workflow)
    
    //e.g. getting Global would require searching the scene tree by tree every time
    //one could also write an extension method for Godot.Node -> but then still you need an instance to use the ".GetGlobal()" on.
    //also - since Global is a node itself, the code suggestion quickly ends up proposing .GetGlobal().GetGlobal().GetGlobal().
    //one other workaround (than the static Get class) could be just using a custom Node class inheriting Godot.Node for custom stuff like this?
    
    //Method to get Global class that's set up to be Autoloaded in project settings
    //It should always be directly under root; any instantiated scenes being siblings
    //In GdScript Autoload refs are automatically available in code, but not in C#, alas:
    public static Global Global() {
        
        //getting scene root as Node to use GetNode there and allow Get.Global() usage in a static context
        //PPU: consider caching the root reference to improve performance; (assuming the reference will stay valid at runtime at all times?)
        var sceneTree = (SceneTree)Engine.GetMainLoop();
        var root = sceneTree.CurrentScene.GetParent();
        
        return (Global) root.GetNode("Global");
    }
}