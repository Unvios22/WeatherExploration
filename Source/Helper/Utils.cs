using Godot;

namespace WeatherExploration.Source.Helper;

public static class Utils {
    //TODO: temp class (&name) for lack of a better workflow; refactor when a few more use cases appear

    private static Node _rootCached;
    
    public static Node GetNodeTreeRoot() {
        //the NodeTree root should be persistent throughout the entire game's runtime
        if (_rootCached is not null) {
            return _rootCached;
        }

        var sceneTree = (SceneTree)Engine.GetMainLoop();
        var root = sceneTree.CurrentScene.GetParent();
        _rootCached = root;

        return _rootCached;
    }
}