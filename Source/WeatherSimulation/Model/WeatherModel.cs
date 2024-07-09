using WeatherExploration.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Model;

public struct WeatherModel {
    public int TextureResolution;
    public AtmosphereModel AtmosphereModel;
    public GroundModel GroundModel;

    public WeatherModel(int textureResolution) {
        TextureResolution = textureResolution;
        AtmosphereModel = new AtmosphereModel();
        GroundModel = new GroundModel();
    }
}