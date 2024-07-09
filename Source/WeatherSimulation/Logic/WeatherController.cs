using Godot;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Logic;

public class WeatherController {

    private WeatherModel _weatherModel;
    private SimulationSettings _simulationSettings;
    
    public void Initialize(SimulationSettings settings) {
        _simulationSettings = settings;
        InitializeSimModel();
        //todo: init models, setup data, start simulation
    }

    private void InitializeSimModel() {
        var modelBuilder = new WeatherModelBuilder();
        modelBuilder.SetSimResolution(_simulationSettings.TextureResolution);
        modelBuilder.InitializeTextures();
        _weatherModel = modelBuilder.Build();
    }

    public Texture2D SampleTexture() {
        return _weatherModel.AtmosphereModel.TemperatureTex;
    }
    
    public void Step() {
        //todo: do a step of simulation
    }
    
    public WeatherModel GetCurrentState() {
        return _weatherModel;
    }
}