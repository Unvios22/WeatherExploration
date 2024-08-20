using System;
using Godot;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.DebugDisplay;

public class WeatherDataWorldSpaceDisplay {
    
    private Node3D _displayOriginNode;
    private Node3D _displayNodesHolder;
    
    private SimulationSettings _simulationSettings;
    private SimulationWorldDisplaySettings _simulationWorldDisplaySettings;

    private MeshInstance3D _texDisplayPlane;
    private MultiMesh _multiMesh;
    private Transform3D[,] _worldSpaceGrid;

    private bool _isDisplayInitialized;

    public WeatherDataWorldSpaceDisplay(
        SimulationSettings simulationSettings,
        SimulationWorldDisplaySettings simulationWorldDisplaySettings,
        Node3D displayOriginNode,
        Node3D displayNodesHolder) {
        
        _simulationSettings = simulationSettings;
        _simulationWorldDisplaySettings = simulationWorldDisplaySettings;
        _displayOriginNode = displayOriginNode;
        _displayNodesHolder = displayNodesHolder;
        
        InitMultimesh();
    }
    
    private void InitMultimesh() {
        _multiMesh = new MultiMesh();
        _multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        var multimeshInstanceTemplate = _simulationWorldDisplaySettings.PressureGradientDisplayInstanceMesh;
        _multiMesh.SetMesh(multimeshInstanceTemplate);

        var multiMeshInstance3d = new MultiMeshInstance3D();
        multiMeshInstance3d.Multimesh = _multiMesh;
        _displayNodesHolder.AddChild(multiMeshInstance3d);
    }

    public void InitWorldSpaceDisplay() {
        GeneratePressureGradientMultimeshGrid();
        GenerateTextureDisplayPlane();
    }

    private void GeneratePressureGradientMultimeshGrid() {
        var worldSpaceGridEdgeLength = _simulationSettings.TextureResolution * _simulationWorldDisplaySettings.WorldSpaceGridCellSize;
        var initialElementFromOriginTranslation = new Vector3(-worldSpaceGridEdgeLength, 0, -worldSpaceGridEdgeLength);
        var initialElementPos = _displayOriginNode.Position + initialElementFromOriginTranslation;
        PopulateWorldSpaceGrid(initialElementPos);
    }

    private void PopulateWorldSpaceGrid(Vector3 initialElementPos) {
        var gridResolution = _simulationSettings.TextureResolution;
        var worldSpaceGridUnitSize = _simulationWorldDisplaySettings.WorldSpaceGridCellSize;
        _worldSpaceGrid = new Transform3D[gridResolution, gridResolution];
        for (var x = 0; x < gridResolution; x++) {
            for (var y = 0; y < gridResolution; y++) {
                var elementOffsetFromInitialPos = new Vector3(x * worldSpaceGridUnitSize, 0, y * worldSpaceGridUnitSize);
                var elementPos = initialElementPos + elementOffsetFromInitialPos;
                _worldSpaceGrid[x, y] = new Transform3D(Basis.Identity, elementPos);
            }
        }
    }

    private void GenerateTextureDisplayPlane() {
        var imagePlane = new MeshInstance3D();
        imagePlane.Mesh = new PlaneMesh();
        _texDisplayPlane = imagePlane;
        
        //assuming the plane has worldspace size of 1 by default @ scale 1
        var planeWorldSpaceScale = _simulationWorldDisplaySettings.WorldSpaceGridCellSize *_simulationSettings.TextureResolution; 
        _texDisplayPlane.Scale = new Vector3(planeWorldSpaceScale, 1, planeWorldSpaceScale);
        _texDisplayPlane.SetMaterialOverride(new Material());
        _displayNodesHolder.AddChild(imagePlane);
    }

    public void DestroyWorldSpaceDisplay() {
        //TODO (?)
    }

    public void RefreshDisplay(WeatherState weatherState) {
        if (_worldSpaceGrid is null) {
            throw new ArgumentException("The Weather data display has not been initialized");
        }
        UpdatePressureTexDisplay(weatherState.PressureTex);
        UpdatePressureGradientMultimeshDisplay(weatherState.PressureGradient);
    }

    private void UpdatePressureTexDisplay(Image pressureTex) {
        var texture2D = ImageTexture.CreateFromImage(pressureTex);
        _texDisplayPlane.GetActiveMaterial(0).Set("albedo_texture", texture2D);
    }

    private void UpdatePressureGradientMultimeshDisplay(Vector4Grid pressureGradient) {
        var gridResolution = _simulationSettings.TextureResolution;
        _multiMesh.InstanceCount = (int)(gridResolution * gridResolution);
        var gradientData = pressureGradient.GetDataAs2DArray();
        var instanceId = 0;
        for (var x = 0; x < gridResolution; x++) {
            for (var y = 0; y < gridResolution; y++) {
                var instanceTransform = _worldSpaceGrid[x, y];
                var cellGradient = gradientData[x, y];
                var gradientVector = new Vector3(cellGradient.X, 0, cellGradient.Y);
                var gradientVectorTargetPos = instanceTransform.Origin + gradientVector;

                var rotatedInstanceTransform = instanceTransform.LookingAt(gradientVectorTargetPos, null, true);
                _worldSpaceGrid[x, y] = rotatedInstanceTransform;
                //TODO: also apply scale depending on the gradient magnitude
                
                //TODO, PPU: can be optimized to use "SetCustomData" to then render & modify meshes from a GPU shader
                _multiMesh.SetInstanceTransform(instanceId, rotatedInstanceTransform);
                instanceId++;
            }
        }
    }

    public void HideDisplay() {
        //TODO
    }
}