using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using WeatherExploration.Source.Signals;
using WeatherExploration.Source.Signals.Types;

namespace WeatherExploration.Source.Unit.Logic;

[Meta(typeof(IAutoNode))]
public partial class UnitInteractionController : Node, IUnitInteractionController, IProvide<IUnitInteractionController> {
    public override void _Notification(int what) => this.Notify(what);

    [Dependency] private SignalBus SignalBus => this.DependOn<SignalBus>();

    IUnitInteractionController IProvide<IUnitInteractionController>.Value() => this;
    
    private Unit _currentlyHoveredUnit;
    private Unit _currentlySelectedUnit;
    
    //todo: currently implement a simple unit manager
    
    //if unit gets clicked, select it
    
    //if cLick targets nothing, clear selection
    
    //if click is move order, tell unit to move to target

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
        }
        else {
            _currentlySelectedUnit = null;
        }
    }

    private void OnUnitMoveOrderSignal(InputMoveOrderSignal signal) {
        if (signal.IsMultiselect) {
            GD.Print("Received move multiselect");
        }
        else {
            GD.Print("Received move");
        }
    }
}