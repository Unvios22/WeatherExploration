using Godot;
using WeatherExploration.Source.WeatherSimulation.Display.Model;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Display;

public class BoundingBoxDisplay : PartialSimulationDataDisplay{
    
    private Node3D _boundingBoxNode;

    public BoundingBoxDisplay(SimulationSettings simulationSettings, Node3D partialDisplaysOriginNode) : base(simulationSettings, partialDisplaysOriginNode) { }

    public override void EvalDisplayPredicates(SimulationWorldDisplaySettings displaySettings) {
        if (displaySettings.IsDisplayBoundingBox && !IsInitialized) {
            GenerateBoundingBox(displaySettings);
        }
        else if (!displaySettings.IsDisplayBoundingBox && IsInitialized) {
            DestroyBoundingBox();
        }
    }

    public override void RefreshDisplay(WeatherState weatherState) { }
    
    private void GenerateBoundingBox(SimulationWorldDisplaySettings displaySettings) {
        var boundingBoxHolder = new MeshInstance3D();
        var immediateMesh = new ImmediateMesh();
        boundingBoxHolder.Mesh = immediateMesh;
        
        immediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip, displaySettings.BoundingBoxMaterial);
        var gridEdgeLength = displaySettings.WorldSpaceGridCellSize * SimulationSettings.TextureResolution;
        var originPos = PartialDisplaysOriginNode.GlobalPosition;
        var verticalOffset = displaySettings.DynamicElementsVerticalOffset;
        
        immediateMesh.SurfaceAddVertex(originPos + new Vector3(-gridEdgeLength/2, verticalOffset, -gridEdgeLength/2));
        immediateMesh.SurfaceAddVertex(originPos + new Vector3(gridEdgeLength/2, verticalOffset, -gridEdgeLength/2));
        immediateMesh.SurfaceAddVertex(originPos + new Vector3(gridEdgeLength/2, verticalOffset, gridEdgeLength/2));
        immediateMesh.SurfaceAddVertex(originPos + new Vector3(-gridEdgeLength/2, verticalOffset, gridEdgeLength/2));
        immediateMesh.SurfaceAddVertex(originPos + new Vector3(-gridEdgeLength/2, verticalOffset, -gridEdgeLength/2));
        
        immediateMesh.SurfaceEnd();
        
        PartialDisplaysOriginNode.AddChild(boundingBoxHolder);
        _boundingBoxNode = boundingBoxHolder;
        
        IsInitialized = true;
    }

    private void DestroyBoundingBox() {
        _boundingBoxNode.QueueFree();
        IsInitialized = false;
    }
}