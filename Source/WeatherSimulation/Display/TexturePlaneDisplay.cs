using Godot;
using WeatherExploration.Source.WeatherSimulation.Display.Model;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Display;

public class TexturePlaneDisplay : PartialSimulationDataDisplay {
    
    private MeshInstance3D _texDisplayPlane;

    public TexturePlaneDisplay(SimulationSettings simulationSettings, Node3D partialDisplaysOriginNode) : base(simulationSettings, partialDisplaysOriginNode) { }
    
    public override void EvalDisplayPredicates(SimulationWorldDisplaySettings displaySettings) {
        if (!IsInitialized) {
            GenerateTextureDisplayPlane(displaySettings);
            IsInitialized = true;
        }

        if (displaySettings.TexturePlaneDisplayType == TexturePlaneDisplayType.None) {
            _texDisplayPlane.Visible = false;
        } else if (displaySettings.TexturePlaneDisplayType == TexturePlaneDisplayType.PressureTexture) {
            _texDisplayPlane.Visible = true;
        }
    }
    
    public override void RefreshDisplay(WeatherState weatherState) {
        UpdatePressureTexDisplay(weatherState.PressureTex);
    }
    
    private void GenerateTextureDisplayPlane(SimulationWorldDisplaySettings displaySettings) {
        var imagePlane = new MeshInstance3D();
        imagePlane.Mesh = new PlaneMesh();
        _texDisplayPlane = imagePlane;
        
        //assuming the plane has worldspace size of 1 by default @ scale 1
        var planeWorldSpaceScale = (displaySettings.WorldSpaceGridCellSize *SimulationSettings.TextureResolution)/2; 
        _texDisplayPlane.Scale = new Vector3(planeWorldSpaceScale, 1, planeWorldSpaceScale);
        
        var displayPlaneMaterial = displaySettings.TexturePlaneMaterial;
        _texDisplayPlane.SetMaterialOverride(displayPlaneMaterial);
        PartialDisplaysOriginNode.AddChild(imagePlane);
    }

    private void UpdatePressureTexDisplay(Image pressureTex) {
        var texture2D = ImageTexture.CreateFromImage(pressureTex);
        var shaderMaterial = _texDisplayPlane.GetActiveMaterial(0) as ShaderMaterial;
        shaderMaterial.SetShaderParameter("pressureTex", texture2D);
    }
}