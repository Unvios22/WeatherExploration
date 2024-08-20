using Godot;

namespace WeatherExploration.Source.WeatherSimulation.Model;

[GlobalClass]
public partial class SimulationWorldDisplaySettings : Resource {
    [Export] public float WorldSpaceGridCellSize;
    [Export] public Gradient PressureTexValueGradient;
    [Export] public Mesh PressureGradientDisplayInstanceMesh;
    [Export] public Material PressureGradientMeshesMaterial;
    [Export] public Material TexturePlaneMaterial;
    [Export] public Material BoundingBoxMaterial;
    [Export] public float DynamicElementsVerticalOffset;
}