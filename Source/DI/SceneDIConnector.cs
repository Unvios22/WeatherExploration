using Godot;

namespace WeatherExploration.Source.DI;

public abstract partial class SceneDIConnector : Node {
    //DI connector template that should be assigned as a script to root of every scene
    //When the scene is first loaded it gets the reference to Global and caches & sets up any DI refs for descendant nodes
    //Implement one for each scene (or scene type) separately
    
}