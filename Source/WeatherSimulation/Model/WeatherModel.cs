using WeatherExploration.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Model;

public struct WeatherModel {
    public uint TextureResolution;
    public AtmosphereModel AtmosphereModel;

    public WeatherModel(uint textureResolution) {
        TextureResolution = textureResolution;
        AtmosphereModel = new AtmosphereModel();
    }
}