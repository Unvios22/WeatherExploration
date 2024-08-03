using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using WeatherExploration.Source.Unit.Logic;
using WeatherExploration.Source.Unit.Model;

namespace WeatherExploration.Source.Unit;

[Meta(typeof(IAutoNode))]
public partial class Unit : Node3D {
    public override void _Notification(int what) => this.Notify(what);
    
    [Node("RigidBody3D")] public CollisionObject3D UnitCollisionObject { get; set; }
    
    [Export] private UnitData _unitData;

    [Dependency]
    private IUnitInteractionController UnitInteractionController => this.DependOn<IUnitInteractionController>();
    
    private const float WaypointCollisionDistance = 0.02f;

    public void OnResolved() {
        CreateColliderCallbackSignals();
    }
    
    private void CreateColliderCallbackSignals() {
        UnitCollisionObject.MouseEntered += () => { UnitInteractionController.OnUnitHoverStart(this);};
        UnitCollisionObject.MouseExited += () => { UnitInteractionController.OnUnitHoverStop(this);};
        
        //TODO: the anonymous callbacks aren't ever unsubscribed; *shouldn't* cause memory leaks since the only time both 
        //the Unit Node and the collision object are deleted is when they're both being removed from the scene
        //Still, make sure it's safe and will work expectedly
    }

    public override void _Process(double delta) {
        ProcessUnitBehavior();
    }

    private void ProcessUnitBehavior() {
        //
        // if (IsCurrentWaypointWithinCollision()) {
        //     AchieveCurrentWaypoint();
        // }
        //
        // if (_unitData.UnitStatus == UnitStatus.Idle) {
        //     return;
        // }
        //
        // if (_unitData.RouteWaypoints.Count == 0) {
        //     HandleUnitHasNoWaypoints();
        // }

    }

    private bool IsCurrentWaypointWithinCollision() {
        var nextWaypoint = _unitData.RouteWaypoints.Peek();
        var distanceToWaypoint = GlobalPosition.DistanceTo(nextWaypoint.Position);
        return distanceToWaypoint <= WaypointCollisionDistance;
    }
    
    private void AchieveCurrentWaypoint() {
        _unitData.RouteWaypoints.Dequeue();
    }

    private void HandleUnitHasNoWaypoints() {
        _unitData.UnitStatus = UnitStatus.Idle;
    }
    
    private void MoveTowardsCurrentWaypoint() {
        
    }
    
    //todo: movement logic
    //todo interaction logic
}