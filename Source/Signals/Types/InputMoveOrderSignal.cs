using Godot;

namespace WeatherExploration.Source.Signals.Types;

public class InputMoveOrderSignal : BaseSignal {
    public bool IsMultiselect { get; set; }
    public Vector2 ScreenSpaceClickPos { get; set; }
}