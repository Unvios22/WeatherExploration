using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using WeatherExploration.Source.Config;
using WeatherExploration.Source.Helper;
using WeatherExploration.Source.Signals;
using WeatherExploration.Source.Signals.Types;
using WeatherExploration.Source.Unit.Logic;
using WeatherExploration.Source.Unit.Model;

namespace WeatherExploration.Source.Unit;

[Meta(typeof(IAutoNode))]
public partial class UnitInteractionController : Node3D, IUnitInteractionController, IProvide<IUnitInteractionController> {
    public override void _Notification(int what) => this.Notify(what);

    [Dependency] private SignalBus SignalBus => DependentExtensions.DependOn<SignalBus>(this);

    IUnitInteractionController IProvide<IUnitInteractionController>.Value() => this;

    private Camera3D _playerCamera;
    [Export] private UnitRouteMarkersDisplay _unitRouteMarkersDisplay;
    private const float MoveOrderRaycastDistance = 20f;
    
    private Unit _currentlyHoveredUnit;
    private Unit _currentlySelectedUnit;

    public override void _Ready() {
        _playerCamera = GetViewport().GetCamera3D();
    }

    public void OnResolved() {
        RegisterSignalBusCallbacks();
        this.Provide();
    }

    private void RegisterSignalBusCallbacks() {
        SignalBus.RegisterListener<InputCursorClickSignal>(OnUnitSelectionSignal);
        SignalBus.RegisterListener<InputMoveOrderSignal>(OnUnitMoveOrderSignal);
    }

    public void OnUnitHoverStart(Unit unit) {
        _currentlyHoveredUnit = unit;
    }

    public void OnUnitHoverStop(Unit unit) {
        _currentlyHoveredUnit = null;
    }

    private void OnUnitSelectionSignal(InputCursorClickSignal signal) {
        if (_currentlyHoveredUnit is not null) {
            _currentlySelectedUnit = _currentlyHoveredUnit;
            DisplayUnitWaypointMesh(_currentlySelectedUnit);
        }
        else {
            _unitRouteMarkersDisplay.ClearDisplayedMarkers();
            _currentlySelectedUnit = null;
        }
    }

    private void OnUnitMoveOrderSignal(InputMoveOrderSignal signal) {
        if (_currentlySelectedUnit is null) {
            return;
        }
        
        var clickScreenSpacePos = signal.ScreenSpaceClickPos;
        var isRayCastMapObjectHit = RaycastForMapObjectCollision(clickScreenSpacePos, out var moveOrderPos);
        if (!isRayCastMapObjectHit) {
            return;
        }
        
        if (signal.IsMultiselect) {
            HandleMoveOrderMultiselect(moveOrderPos);
        }
        else {
            HandleMoveOrderSingle(moveOrderPos);
        }

        DisplayUnitWaypointMesh(_currentlySelectedUnit);
    }

    private bool RaycastForMapObjectCollision(Vector2 screenSpaceClickPos, out Vector3 moveOrderPos) {
        moveOrderPos = default;
        
        var rayOrigin = _playerCamera.ProjectRayOrigin(screenSpaceClickPos);
        var rayTarget = rayOrigin + _playerCamera.ProjectRayNormal(screenSpaceClickPos) * MoveOrderRaycastDistance;

        var isRaycastHit = GetAutoload.RayCasterNode3D().RayCastToTarget(rayOrigin, rayTarget, out var hitInfo);
        if (!isRaycastHit) {
            return false;
        }
        
        var hitNode = hitInfo.CollisionObject as Node;
        if (!hitNode.IsInGroup(Groups.MAP_TERRAIN)) {
            return false;
        }
        
        moveOrderPos = hitInfo.CollisionPoint;
        return true;
    }

    private void HandleMoveOrderMultiselect(Vector3 movePos) {
        var newWaypoint = new UnitWaypoint(movePos);
        _currentlySelectedUnit.UnitData.RouteWaypoints.Enqueue(newWaypoint);
    }

    private void HandleMoveOrderSingle(Vector3 movePos) {
        var currentlySelectedUnitWaypoints = _currentlySelectedUnit.UnitData.RouteWaypoints;
        currentlySelectedUnitWaypoints.Clear();
        
        var newWaypoint = new UnitWaypoint(movePos);
        currentlySelectedUnitWaypoints.Enqueue(newWaypoint);
    }

    private void DisplayUnitWaypointMesh(Unit unit) {
        var unitWaypointVertices = unit.UnitData.RouteWaypoints.Select(x => x.Position).ToList();
        unitWaypointVertices.Insert(0, unit.Position);
        _unitRouteMarkersDisplay.RenderUnitWaypointPath(unitWaypointVertices);
    }
}