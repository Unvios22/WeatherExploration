using Godot;
using WeatherExploration.Source.Helper;
using WeatherExploration.Source.Input;
using WeatherExploration.Source.Signals;
using WeatherExploration.Source.Signals.Types;

namespace WeatherExploration.Source.Autoload;

public partial class PlayerInputProcessor : Node3D {
    
    private SignalBus _signalBus;

    private bool _isMoveOrderMultiselectModifierPressed;
    
    public override void _EnterTree() {
        var global = GetAutoload.Global();
        _signalBus = global.SignalBus;
    }

    public override void _Input(InputEvent @event) {
        if (@event.IsAction(InputActions.CURSOR_CLICK)) {
            HandleCursorClick(@event);
        } 
        if (@event.IsAction(InputActions.MOVE_ORDER)) {
            HandleMoveOrder(@event);
        }
        if (@event.IsAction(InputActions.MOVE_ORDER_MULTISELECT_MODIFIER)) {
            HandleMoveOrderMultiselectModifier(@event);
        }
    }

    private void HandleCursorClick(InputEvent @event) {
        var inputEventMouseButton = @event as InputEventMouseButton;
        //the event fires twice - for both press and release of the button. Pressed == false if it's the release.
        if (inputEventMouseButton.IsPressed()) {
            _signalBus.FireSignal(new InputCursorClickSignal());
        }
    }

    private void HandleMoveOrder(InputEvent @event) {
        if (!@event.IsPressed()) {
            return;
        }
        var inputEventMouseButton = @event as InputEventMouseButton;
        var eventClickScreenSpacePos = inputEventMouseButton.Position;
        
        _signalBus.FireSignal(new InputMoveOrderSignal {
            IsMultiselect = _isMoveOrderMultiselectModifierPressed,
            ScreenSpaceClickPos = eventClickScreenSpacePos
        });
    }

    private void HandleMoveOrderMultiselectModifier(InputEvent @event) {
        if (@event.IsPressed() && !_isMoveOrderMultiselectModifierPressed) {
            _isMoveOrderMultiselectModifierPressed = true;
        } else if (@event.IsReleased()) {
            _isMoveOrderMultiselectModifierPressed = false;
        }
    }
}