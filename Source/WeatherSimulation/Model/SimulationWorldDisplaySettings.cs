using Godot;

namespace WeatherExploration.Source.WeatherSimulation.Model;

[GlobalClass]
public partial class SimulationWorldDisplaySettings : Resource {
    [Export] public float WorldSpaceGridCellSize;
    [Export] public Gradient PressureTexValueGradient;
    [Export] public Mesh PressureGradientDisplayInstanceMesh;
}