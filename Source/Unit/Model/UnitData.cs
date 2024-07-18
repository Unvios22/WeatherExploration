using Godot;

namespace WeatherExploration.Source.Unit.Model;

[GlobalClass]
public partial class UnitData : Resource {
    //todo should be a structm but godot can't properly serialize struct variables?
    //https://github.com/godotengine/godot-proposals/issues/438
    
    //todo: currently using [GlobalClass] and inheriting from Resource to create a serialized field of custom class type,
    //as per https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_global_classes.html
    //Make sure it works as expected, though
    
    [Export] public string UnitDisplayName;
    [Export] public int UnitId;
    //todo: change id to be immutable?
    [Export] public Vector2I CurrentGridCell;
    [Export] public Vector3 WorldSpacePos;
}