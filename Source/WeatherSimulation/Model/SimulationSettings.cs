using Godot;

namespace WeatherExploration.Source.WeatherSimulation.Model;

[GlobalClass]
public partial class SimulationSettings : Resource {
    [Export] public uint TextureResolution;
    [Export] public uint IterationsPerStep;
    [Export] public float TimeStep;
}