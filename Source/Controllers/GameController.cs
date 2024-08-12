using Godot;
using WeatherExploration.Source.WeatherSimulation.Logic;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.Controllers;

public partial class GameController : Node {
    
    //todo: this is tightly coupled, change to some DI
    private WeatherController _weatherController;
    private const int SimResolution = 1024;

    [Export] public MeshInstance3D DebugPlaneMesh;
    
    public override void _Ready() {
        InitializeVariables();
        InitializeWeatherSim();
    }

    private void InitializeVariables() {
        _weatherController = new WeatherController();
    }

    private void InitializeWeatherSim() {
        var simulationSettings = new SimulationSettings();
        simulationSettings.TextureResolution = SimResolution;
        // _weatherController.Initialize(simulationSettings);
    }
    
    public override void _Process(double delta) {
        GD.Print("GameController Tick");
        // var sampledTexture = _weatherController.SampleTexture();
        // SetDebugPlaneTexture(sampledTexture);
    }

    private void SetDebugPlaneTexture(Texture2D texture2D) {
        var meshMaterial = DebugPlaneMesh.GetSurfaceOverrideMaterial(0);
        meshMaterial.Set("albedo_texture", texture2D);
    }
}