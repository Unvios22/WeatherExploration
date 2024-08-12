using Godot;

namespace WeatherExploration.Source.WeatherSimulation.Model;

[GlobalClass]
public partial class SimulationSettings : Resource {
    [Export] public int TextureResolution;
    [Export] public int IterationsPerStep;
}