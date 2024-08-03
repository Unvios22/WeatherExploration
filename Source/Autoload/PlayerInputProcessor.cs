using Godot;
using WeatherExploration.Source.Helper;
using WeatherExploration.Source.Input;
using WeatherExploration.Source.Signals;
using WeatherExploration.Source.Signals.Types;

namespace WeatherExploration.Source.Autoload;

public partial class PlayerInputProcessor : Node {
    
    private SignalBus _signalBus;
    
    public override void _EnterTree() {
        var global = GetAutoload.Global();
        _signalBus = global.SignalBus;
    }

    public override void _Input(InputEvent @event) {
        if (@event.IsAction(InputActions.CURSOR_CLICK)) {
            HandleCursorClick(@event);
        } 
        else if (@event.IsAction(InputActions.MOVE_ORDER)) {
            HandleMoveOrder(@event);
        }
        else if (@event.IsAction(InputActions.MOVE_ORDER_MULTISELECT)) {
            HandleMoveOrderMultiselect(@event);
        }
    }

    private void HandleCursorClick(InputEvent @event) {
        var inputEventMouseButton = @event as InputEventMouseButton;
        //the event fires twice - for both press and release of the button. Pressed == false if it's the release.
        if (!inputEventMouseButton.Pressed) {
            _signalBus.FireSignal(new InputCursorClickSignal());
        }
    }

    private void HandleMoveOrder(InputEvent @event) {
        
    }

    private void HandleMoveOrderMultiselect(InputEvent @event) {
        
    }
}