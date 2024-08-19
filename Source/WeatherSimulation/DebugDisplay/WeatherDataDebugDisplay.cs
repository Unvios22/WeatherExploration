using System;
using Godot;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.DebugDisplay;

public class WeatherDataDebugDisplay {
    
    private Node3D _displayOriginNode;
    private Node3D _multiMeshNodeHolder;
    
    private float _worldSpaceGridUnitSize;
    private uint _gridResolution;
    
    private Image _pressureTexImage;
    private VectorGrid _pressureGradient;

    private MultiMesh _multiMesh;

    private Mesh _displayMeshTemplate;
    private Transform3D[,] _worldSpaceGrid;

    public WeatherDataDebugDisplay(uint gridResolution, float worldSpaceGridUnitSize, Node3D displayOriginNode, Node3D multiMeshNodeHolder) {
        _gridResolution = gridResolution;
        _worldSpaceGridUnitSize = worldSpaceGridUnitSize;
        _displayOriginNode = displayOriginNode;
        _multiMeshNodeHolder = multiMeshNodeHolder;
        DefineDisplayMeshTemplate();
        InitMultimesh();
    }
    
    private void DefineDisplayMeshTemplate() {
        _displayMeshTemplate = new BoxMesh();
        //TODO
    }
    
    private void InitMultimesh() {
        _multiMesh = new MultiMesh();
        _multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        _multiMesh.SetMesh(_displayMeshTemplate);

        var multiMeshInstance3d = new MultiMeshInstance3D();
        multiMeshInstance3d.Multimesh = _multiMesh;
        _multiMeshNodeHolder.AddChild(multiMeshInstance3d);
    }
    
    public void SetPressureImage(Image pressureImage) {
        _pressureTexImage = pressureImage;
    }

    public void SetPressureGradient(VectorGrid pressureGradient) {
        _pressureGradient = pressureGradient;
    }

    public void InitWorldSpaceDisplay() {
        var worldSpaceGridEdgeLength = _gridResolution * _worldSpaceGridUnitSize;
        var initialElementFromOriginTranslation = new Vector3(-worldSpaceGridEdgeLength, 0, -worldSpaceGridEdgeLength);
        var initialElementPos = _displayOriginNode.Position + initialElementFromOriginTranslation;
        
        PopulateWorldSpaceGrid(initialElementPos);
    }

    private void PopulateWorldSpaceGrid(Vector3 initialElementPos) {
        _worldSpaceGrid = new Transform3D[_gridResolution, _gridResolution];
        for (var x = 0; x < _gridResolution; x++) {
            for (var y = 0; y < _gridResolution; y++) {
                var elementOffsetFromInitialPos = new Vector3(x * _worldSpaceGridUnitSize, 0, y * _worldSpaceGridUnitSize);
                var elementPos = initialElementPos + elementOffsetFromInitialPos;
                _worldSpaceGrid[x, y] = new Transform3D(Basis.Identity, elementPos);
            }
        }
    }

    public void DestroyWorldSpaceDisplay() {
        //TODO (?)
    }

    public void RefreshDisplay() {
        if (_worldSpaceGrid is null) {
            throw new ArgumentException("The Weather data display has not been initialized");
        }

        UpdateMultimeshDisplay();
    }

    private void UpdateMultimeshDisplay() {
        _multiMesh.InstanceCount = (int)(_gridResolution * _gridResolution);
        var gradientData = _pressureGradient.GetDataAs2DArray();
        var instanceId = 0;
        for (var x = 0; x < _gridResolution; x++) {
            for (var y = 0; y < _gridResolution; y++) {
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