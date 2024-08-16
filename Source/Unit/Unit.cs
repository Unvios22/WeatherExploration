using System.Collections.Generic;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using WeatherExploration.Source.Signals;
using WeatherExploration.Source.Unit.Logic;
using WeatherExploration.Source.Unit.Model;

namespace WeatherExploration.Source.Unit;

[Meta(typeof(IAutoNode))]
public partial class Unit : Node3D {
    public override void _Notification(int what) => this.Notify(what);
    
    [Node("RigidBody3D")] public CollisionObject3D UnitCollisionObject { get; set; }
    [Node("Sprite3D")] public Sprite3D UnitSprite { get; set; }
    
    [Export] private UnitData _unitData;

    private Vector3 _defaultLocalUnitSpritePos;
        
    [Dependency] private IUnitInteractionController UnitInteractionController => this.DependOn<IUnitInteractionController>();

    private const float WaypointAchieveDistance = 0.05f;
    private readonly Vector3 UnitSpriteSelectedPositionChange = new Vector3(0f, 0.5f, 0f);
    private bool _isCurrentlySelected;

    public void OnResolved() {
        CreateColliderCallbackSignals();
        InitUnitData();
        _defaultLocalUnitSpritePos = UnitSprite.Position;
    }
    
    private void CreateColliderCallbackSignals() {
        UnitCollisionObject.MouseEntered += () => { UnitInteractionController.OnUnitHoverStart(this);};
        UnitCollisionObject.MouseExited += () => { UnitInteractionController.OnUnitHoverStop(this);};
        
        //TODO: the anonymous callbacks aren't ever unsubscribed; *shouldn't* cause memory leaks since the only time both 
        //the Unit Node and the collision object are deleted is when they're both being removed from the scene
        //Still, make sure it's safe and will work expectedly
    }

    private void InitUnitData() {
        _unitData = new UnitData {
            RouteWaypoints = new Queue<UnitWaypoint>(),
            UnitMoveSpeed = 0.03f
        };
    }

    public void SetUnitSelectedStatus(bool isSelected) {
        _isCurrentlySelected = isSelected;
        UpdateUnitSpriteDisplayToSelectedStatus(isSelected);
    }

    private void UpdateUnitSpriteDisplayToSelectedStatus(bool isSelected) {
        if (isSelected) {
            UnitSprite.Position = _defaultLocalUnitSpritePos + UnitSpriteSelectedPositionChange;
        }
        else {
            UnitSprite.Position = _defaultLocalUnitSpritePos;;
        }
    }

    public override void _Process(double delta) {
        ProcessUnitBehavior();
    }

    private void ProcessUnitBehavior() {
        if (_unitData.RouteWaypoints.Count == 0) {
            return;
        }
        
        MoveUnitTowardsWaypoint();
        
        if (IsCurrentWaypointWithinAchieveRange()) {
            AchieveCurrentWaypoint();
        }
    }

    private void MoveUnitTowardsWaypoint() {
        var targetPos = UnitData.RouteWaypoints.Peek().Position;
        var towardsTargetVector = (targetPos - Position).Normalized();
        var movementVector = towardsTargetVector * UnitData.UnitMoveSpeed;
        
        Translate(movementVector);
    }

    private bool IsCurrentWaypointWithinAchieveRange() {
        var nextWaypoint = UnitData.RouteWaypoints.Peek();
        var distanceToWaypoint = GlobalPosition.DistanceTo(nextWaypoint.Position);
        return distanceToWaypoint <= WaypointAchieveDistance;
    }
    
    private void AchieveCurrentWaypoint() {
        UnitData.RouteWaypoints.Dequeue();
    }
    
    public UnitData UnitData => _unitData;
}