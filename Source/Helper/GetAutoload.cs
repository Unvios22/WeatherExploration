using Godot;
using WeatherExploration.Source.Autoload;

namespace WeatherExploration.Source.Helper;

public static class GetAutoload {
    //Helper class for getting Autoload references
    //(the Autoloads are intended by Godot devs for both GdScript & C# Godot workflow; especially for any DI or setups; problem is,
    //while in GdScript refs to these are available automatically in global scope, it needs custom searches in C#)
    
    //e.g. getting Global would require searching the scene tree by tree every time
    //one could also write an extension method for Godot.Node -> but then still you need an instance to use the ".GetGlobal()" on.
    //also - since Global is a node itself, the code suggestion quickly ends up proposing .GetGlobal().GetGlobal().GetGlobal().
    //one other workaround (than the static Get class) could be just using a custom Node class inheriting Godot.Node for custom stuff like this?
    
    
    //Method to get Global class that's set up to be Autoloaded in project settings
    //It should always be directly under root; any instantiated scenes being siblings
    public static Global Global() {
        //getting NodeTree root as Node to use GetNode there and allow Get.Global() usage in a static context
        var root = Utils.GetNodeTreeRoot();
        var global = (Global) root.GetNode("Global");
        return global;
    }
}