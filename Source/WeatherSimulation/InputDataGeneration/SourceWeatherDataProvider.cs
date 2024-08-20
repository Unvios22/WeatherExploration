using Godot;
using WeatherExploration.Source.WeatherSimulation.Logic;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.InputDataGeneration;

public class SourceWeatherDataProvider {

    private WeatherSourceDataGenerator<Image> _pressureTextureGenerator;
    
    /*TODO: "initial state" may not be the best name - it is "initial" in the sense that this is the basic WeatherState to
     start the simulation with, but the data will be fully populated ony after an x amount of iteration steps with the sim shader*/
    public WeatherState CreateInitialWeatherState(SimulationSettings simulationSettings) {
        var inputResolution = simulationSettings.TextureResolution;
        InitializeDataGenerators(inputResolution);
        
        var pressureTexture = _pressureTextureGenerator.ProvideInput();
        var pressureGradient = GenerateVector4GridPlaceholder(inputResolution);
        
        var weatherState = new WeatherState(pressureTexture, pressureGradient);
        return weatherState;
    }

    private Vector4Grid GenerateVector4GridPlaceholder(uint resolution) {
        var placeholderData = new Vector4[resolution,resolution];
        for (int x = 0; x < resolution; x++) {
            for (int y = 0; y < resolution; y++) {
                placeholderData[x,y] = new Vector4(1,1,1,0);
            }
        }

        var pressureGradient = new Vector4Grid(placeholderData, resolution);
        return pressureGradient;
    }

    private void InitializeDataGenerators(uint inputResolution) {
        _pressureTextureGenerator = new PressureTextureGenerator(inputResolution);
    }

    public WeatherState UpdateWeatherStateSourceData(WeatherState weatherState) {
        //TODO: divide/model the WeatherState fields into "inputs" and "results" for clarity?
        weatherState.PressureTex = _pressureTextureGenerator.ProvideInput();
        return weatherState;
    }
}