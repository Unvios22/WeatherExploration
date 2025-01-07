using Godot;
using WeatherExploration.Source.WeatherSimulation.Display.Model;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Display;

public class ArrowGridDisplay : PartialSimulationDataDisplay {
    
    private MultiMesh _multiMesh;
    private Transform3D[,] _worldSpaceGrid;

    public ArrowGridDisplay(SimulationSettings simulationSettings, Node3D partialDisplaysOriginNode) : base(simulationSettings, partialDisplaysOriginNode) { }

    public override void EvalDisplayPredicates(SimulationWorldDisplaySettings displaySettings) {
        if (!IsInitialized) {
            InitMultimesh(displaySettings);
            GenerateArrowWorldSpaceMultimeshGrid(displaySettings);
            IsInitialized = true;
        }
    }
    public override void RefreshDisplay(WeatherState weatherState) {
        UpdateArrowMultimeshDisplay(weatherState.PressureGradient);
    }
    
    private void InitMultimesh(SimulationWorldDisplaySettings displaySettings) {
        _multiMesh = new MultiMesh();
        _multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        var multimeshInstanceTemplate = displaySettings.PressureGradientDisplayInstanceMesh;
        var meshInstanceMaterial = displaySettings.PressureGradientMeshesMaterial;
        multimeshInstanceTemplate.SurfaceSetMaterial(0, meshInstanceMaterial);
        _multiMesh.SetMesh(multimeshInstanceTemplate);
        
        _multiMesh.SetUseColors(true);

        var multiMeshInstance3d = new MultiMeshInstance3D();
        multiMeshInstance3d.Multimesh = _multiMesh;
        PartialDisplaysOriginNode.AddChild(multiMeshInstance3d);
    }
    
    private void GenerateArrowWorldSpaceMultimeshGrid(SimulationWorldDisplaySettings displaySettings) {
        var gridRes = (float)SimulationSettings.TextureResolution;
        var gridCellLength = displaySettings.WorldSpaceGridCellSize;
        var centerToEdgeCellDistance = (gridRes - 1) / 2.0f * gridCellLength;
        var toInitialElementTranslation = new Vector3(-centerToEdgeCellDistance, displaySettings.DynamicElementsVerticalOffset, -centerToEdgeCellDistance);
        var initialElementPos = PartialDisplaysOriginNode.GlobalPosition + toInitialElementTranslation;
        
        PopulateArrowWorldSpaceGrid(initialElementPos, gridCellLength);
    }

    private void PopulateArrowWorldSpaceGrid(Vector3 initialElementPos, float worldSpaceGridUnitSize) {
        var gridResolution = SimulationSettings.TextureResolution;
        _worldSpaceGrid = new Transform3D[gridResolution, gridResolution];
        for (var x = 0; x < gridResolution; x++) {
            for (var y = 0; y < gridResolution; y++) {
                var elementOffsetFromInitialPos = new Vector3(x * worldSpaceGridUnitSize, 0, y * worldSpaceGridUnitSize);
                var elementPos = initialElementPos + elementOffsetFromInitialPos;
                _worldSpaceGrid[x, y] = new Transform3D(Basis.Identity, elementPos);
            }
        }
    }
    
    private void UpdateArrowMultimeshDisplay(Vector4Grid vectorDataToDisplay) {
        var gridResolution = SimulationSettings.TextureResolution;
        _multiMesh.InstanceCount = (int)(gridResolution * gridResolution);
        var vectorData = vectorDataToDisplay.GetDataAs2DArray();
        var instanceId = 0;
        for (var x = 0; x < gridResolution; x++) {
            for (var y = 0; y < gridResolution; y++) {
                //TODO, PPU: can generally be optimized to use "SetCustomData" to then render & modify meshes from a GPU shader
                var instanceTransform = _worldSpaceGrid[x, y];
                var cellVector = vectorData[x, y];
                var toDisplayVector = new Vector3(cellVector.X, 0, cellVector.Y);
                var gradientMagnitude = toDisplayVector.Length();
                if (gradientMagnitude < 0.0001f) {
                    _multiMesh.SetInstanceColor(instanceId, new Color(1, 1, 1, 0));
                }
                else {
                    var toDisplayVectorTargetPos = instanceTransform.Origin + toDisplayVector;
                    var rotatedInstanceTransform = instanceTransform.LookingAt(toDisplayVectorTargetPos, null, true);
                    _worldSpaceGrid[x, y] = rotatedInstanceTransform;
                    _multiMesh.SetInstanceTransform(instanceId, rotatedInstanceTransform);
                    _multiMesh.SetInstanceColor(instanceId, new Color(1, 1, 1, 1));
                }
                
                //TODO: also apply scale depending on the gradient magnitude
                instanceId++;
            }
        }
    }
}