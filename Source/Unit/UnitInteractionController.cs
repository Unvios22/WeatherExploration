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
        SignalBus.RegisterListener<InputCursorClickSignal>(OnUnitSelectionCheck);
    }

    public void OnUnitHoverStart(Unit unit) {
        _currentlyHoveredUnit = unit;
        GD.Print("Unit hover");
    }

    public void OnUnitHoverStop(Unit unit) {
        _currentlyHoveredUnit = null;
        GD.Print("Unit hover stop");
    }

    private void OnUnitSelectionCheck(InputCursorClickSignal signal) {
        if (_currentlyHoveredUnit is not null) {
            _currentlySelectedUnit = _currentlyHoveredUnit;
            GD.Print("Unit selected");
        }
        else {
            _currentlySelectedUnit = null;
            GD.Print("Unit deselected");
        }
    }
}