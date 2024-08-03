using Chickensoft.GodotNodeInterfaces;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using WeatherExploration.Source.Unit.Model;

namespace WeatherExploration.Source.Unit.Logic;

[Meta(typeof(IAutoConnect))]
public partial class Unit : Node {
    public override void _Notification(int what) => this.Notify(what);
    
    [Export] private UnitData _unitData;
    [Node("RigidBody3D")] public CollisionObject3D UnitCollisionObject { get; set; }

    public override void _Ready() {
        SubscribeToChildrenEvents();
    }

    private void OnCollisionObjectClicked(Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeidx) {
        //todo: implement proper checking for mouse clicks
        GD.Print("Unit clicked at mouse position:" + position);
    }

    private void SubscribeToChildrenEvents() {
        //TODO: check if the chickensoft di package does this better
        UnitCollisionObject.InputEvent += OnCollisionObjectClicked;
    }

    private void UnsubscribeFromChildrenEvents() {
        //TODO: unsubscribe from event properly to avoid overloading the GC or getting memory leaks
    }

    public override void _Process(double delta) {
        ProcessUnitBehavior();
    }

    private void ProcessUnitBehavior() {
        ApplyCurrentCellEffects();
        ApplyUnitModifiers();
        ApplyUnitMovement();
    }

    private void ApplyCurrentCellEffects() {
        
    }

    private void ApplyUnitModifiers() {
        
    }

    private void ApplyUnitMovement() {
        
    }
    
    //todo: movement logic
    //todo interaction logic
}