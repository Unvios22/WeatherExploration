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

    private static Global _globalCache;
    private static RayCasterNode3D _rayCasterNode3DCache;
    
    //Method to get Global class that's set up to be Autoloaded in project settings
    //It should always be directly under root; any instantiated scenes being siblings
    public static Global Global() {
        if (_globalCache is not null) {
            return _globalCache;
        }
        
        //getting NodeTree root as Node to use GetNode there and allow Get.Global() usage in a static context
        var root = Utils.GetNodeTreeRoot();
        _globalCache = (Global) root.GetNode("Global");
        return _globalCache;
    }

    public static RayCasterNode3D RayCasterNode3D() {
        if (_rayCasterNode3DCache is not null) {
            return _rayCasterNode3DCache;
        }
        
        var root = Utils.GetNodeTreeRoot();
        _rayCasterNode3DCache = (RayCasterNode3D) root.GetNode("RayCasterNode3D");
        return _rayCasterNode3DCache;
    }
}