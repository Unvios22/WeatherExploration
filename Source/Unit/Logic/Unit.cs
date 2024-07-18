using Godot;
using WeatherExploration.Source.Unit.Model;

namespace WeatherExploration.Source.Unit.Logic;

public partial class Unit : Node {
    [Export] private UnitData _unitData;
    [Export] private CollisionObject3D _unitCollisionObject;

    public override void _Ready() {
        SubscribeToChildrenEvents();
    }

    private void OnCollisionObjectClicked(Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeidx) {
        //todo: implement proper checking for mouse clicks
        GD.Print("Unit clicked at mouse position:" + position);
    }

    private void SubscribeToChildrenEvents() {
        //TODO: check if the chickensoft di package does this better
        _unitCollisionObject.InputEvent += OnCollisionObjectClicked;
    }

    private void UnsubscribeFromChildrenEvents() {
        //TODO: unsubscribe from event properly to avoid overloading the GC or getting memory leaks
    }
    
    //todo: movement logic
    //todo interaction logic
}