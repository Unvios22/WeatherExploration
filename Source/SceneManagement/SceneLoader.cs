using System.Linq;
using Godot;
using WeatherExploration.Source.Config;

namespace WeatherExploration.Source.Helper;

public class SceneLoader {
    private Node _currentScene;
    private SceneInfo _currentSceneInfo;
    
    private SceneInfo _currentSceneInfoTempDeferred;
    
    public SceneLoader() {
        FindInitialSceneRoot();
        _currentSceneInfo = Scenes.INITIAL_SCENE;
    }

    private void FindInitialSceneRoot() {
        var nodeTreeRoot = Utils.GetNodeTreeRoot();
        
        //all autoloads and loaded scenes are siblings and direct children of root, but the scene root node is always last
        //as per https://docs.godotengine.org/en/stable/tutorials/scripting/singletons_autoload.html
        _currentScene = nodeTreeRoot.GetChildren().Last();
    }
    
    //TODO: current logic for loading is synchronous; add overloads for async and deferred loading
    public void ChangeToScene(SceneInfo sceneInfo) {
        //the scene load has to be deferred to make sure all scripts have finished their current frame logic
        //as per https://docs.godotengine.org/en/stable/tutorials/scripting/singletons_autoload.html
        
        //Instead of changing SceneLoader to a GD object I'm using Callable.From, as per https://github.com/godotengine/godot-proposals/issues/8258
        _currentSceneInfoTempDeferred = sceneInfo;
        Callable.From(DeferredChangeToScene).CallDeferred();
    }

    private void DeferredChangeToScene() {
        var sceneInfo = _currentSceneInfoTempDeferred;
        
        //caching tree root, beacuse the Utils class uses NodeTree.CurrentScene which is about to be null
        //TODO: refactor Utils to be more redundant in getting the base tree root
        var treeRoot = Utils.GetNodeTreeRoot();
        
        //remove current scene
        _currentScene.Free();

        //load new scene to a variable
        var newScene = GD.Load<PackedScene>(sceneInfo.Path);
        _currentScene = newScene.Instantiate();

        //instantiate the saved node into the node tree
        treeRoot.AddChild(_currentScene);

        //update displayed current scene data
        _currentSceneInfo = sceneInfo;
    }
    
    public SceneInfo CurrentSceneInfo => _currentSceneInfo;
}