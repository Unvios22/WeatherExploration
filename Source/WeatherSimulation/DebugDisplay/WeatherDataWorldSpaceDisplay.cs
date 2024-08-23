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
        var meshInstanceMaterial = _simulationWorldDisplaySettings.PressureGradientMeshesMaterial;
        multimeshInstanceTemplate.SurfaceSetMaterial(0, meshInstanceMaterial);
        _multiMesh.SetMesh(multimeshInstanceTemplate);
        
        _multiMesh.SetUseColors(true);

        var multiMeshInstance3d = new MultiMeshInstance3D();
        multiMeshInstance3d.Multimesh = _multiMesh;
        _displayNodesHolder.AddChild(multiMeshInstance3d);
    }

    public void InitWorldSpaceDisplay() {
        GeneratePressureGradientMultimeshGrid();
        GenerateTextureDisplayPlane();
        if (_simulationWorldDisplaySettings.IsDisplayBoundingBox) {
            GenerateBoundingBox();
        }
    }

    private void GeneratePressureGradientMultimeshGrid() {
        var gridRes = (float)_simulationSettings.TextureResolution;
        var gridCellLength = _simulationWorldDisplaySettings.WorldSpaceGridCellSize;
        var centerToEdgeCellDistance = (gridRes - 1) / 2.0f * gridCellLength;
        var toInitialElementTranslation = new Vector3(-centerToEdgeCellDistance, _simulationWorldDisplaySettings.DynamicElementsVerticalOffset, -centerToEdgeCellDistance);
        var initialElementPos = _displayOriginNode.GlobalPosition + toInitialElementTranslation;
        
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
        var planeWorldSpaceScale = (_simulationWorldDisplaySettings.WorldSpaceGridCellSize *_simulationSettings.TextureResolution)/2; 
        _texDisplayPlane.Scale = new Vector3(planeWorldSpaceScale, 1, planeWorldSpaceScale);
        
        var displayPlaneMaterial = _simulationWorldDisplaySettings.TexturePlaneMaterial;
        _texDisplayPlane.SetMaterialOverride(displayPlaneMaterial);
        _displayNodesHolder.AddChild(imagePlane);
    }
    
    //TODO: can be encapsulated into a util class
    private void GenerateBoundingBox() {
        var boundingBoxHolder = new MeshInstance3D();
        var immediateMesh = new ImmediateMesh();
        boundingBoxHolder.Mesh = immediateMesh;
        
        immediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip, _simulationWorldDisplaySettings.BoundingBoxMaterial);
        var gridEdgeLength = _simulationWorldDisplaySettings.WorldSpaceGridCellSize * _simulationSettings.TextureResolution;
        var originPos = _displayOriginNode.GlobalPosition;
        var verticalOffset = _simulationWorldDisplaySettings.DynamicElementsVerticalOffset;
        
        immediateMesh.SurfaceAddVertex(originPos + new Vector3(-gridEdgeLength/2, verticalOffset, -gridEdgeLength/2));
        immediateMesh.SurfaceAddVertex(originPos + new Vector3(gridEdgeLength/2, verticalOffset, -gridEdgeLength/2));
        immediateMesh.SurfaceAddVertex(originPos + new Vector3(gridEdgeLength/2, verticalOffset, gridEdgeLength/2));
        immediateMesh.SurfaceAddVertex(originPos + new Vector3(-gridEdgeLength/2, verticalOffset, gridEdgeLength/2));
        immediateMesh.SurfaceAddVertex(originPos + new Vector3(-gridEdgeLength/2, verticalOffset, -gridEdgeLength/2));
        
        immediateMesh.SurfaceEnd();
        
        _displayOriginNode.AddChild(boundingBoxHolder);
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
        var shaderMaterial = _texDisplayPlane.GetActiveMaterial(0) as ShaderMaterial;
        shaderMaterial.SetShaderParameter("pressureTex", texture2D);
    }

    private void UpdatePressureGradientMultimeshDisplay(Vector4Grid pressureGradient) {
        var gridResolution = _simulationSettings.TextureResolution;
        _multiMesh.InstanceCount = (int)(gridResolution * gridResolution);
        var gradientData = pressureGradient.GetDataAs2DArray();
        var instanceId = 0;
        for (var x = 0; x < gridResolution; x++) {
            for (var y = 0; y < gridResolution; y++) {
                //TODO, PPU: can generally be optimized to use "SetCustomData" to then render & modify meshes from a GPU shader
                var instanceTransform = _worldSpaceGrid[x, y];
                var cellGradient = gradientData[x, y];
                var gradientVector = new Vector3(cellGradient.X, 0, cellGradient.Y);
                var gradientMagnitude = gradientVector.Length();
                if (gradientMagnitude < 0.0001f) {
                    _multiMesh.SetInstanceColor(instanceId, new Color(1, 1, 1, 0));
                }
                else {
                    var gradientVectorTargetPos = instanceTransform.Origin + gradientVector;
                    var rotatedInstanceTransform = instanceTransform.LookingAt(gradientVectorTargetPos, null, true);
                    _worldSpaceGrid[x, y] = rotatedInstanceTransform;
                    _multiMesh.SetInstanceTransform(instanceId, rotatedInstanceTransform);
                    _multiMesh.SetInstanceColor(instanceId, new Color(1, 1, 1, 1));
                }
                
                //TODO: also apply scale depending on the gradient magnitude
                instanceId++;
            }
        }
    }

    public void HideDisplay() {
        //TODO
    }
}