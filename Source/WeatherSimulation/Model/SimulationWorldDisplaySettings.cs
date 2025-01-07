using Godot;

namespace WeatherExploration.Source.WeatherSimulation.Model;

[GlobalClass]
public partial class SimulationWorldDisplaySettings : Resource {
    [ExportGroup("General")]
    [Export] public float WorldSpaceGridCellSize;
    [Export] public ArrowGridDisplayType ArrowGridDisplayType;
    [Export] public TexturePlaneDisplayType TexturePlaneDisplayType;
    
    [ExportGroup("Bounding Box")]
    [Export] public bool IsDisplayBoundingBox;
    [Export] public Material BoundingBoxMaterial;
    
    [ExportGroup("Display Meshes & Materials")]
    [Export] public Material TexturePlaneMaterial;
    [Export] public Material PressureGradientMeshesMaterial;
    [Export] public Mesh PressureGradientDisplayInstanceMesh;
    [Export] public float DynamicElementsVerticalOffset;
}