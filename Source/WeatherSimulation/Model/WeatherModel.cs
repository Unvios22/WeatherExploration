using Godot;

namespace WeatherExploration.Source.WeatherSimulation.Model;

public struct WeatherModel {
    public uint TextureResolution;

    public Image PressureImage;
    public VectorGrid PressureGradient;

    public WeatherModel(uint textureResolution) {
        TextureResolution = textureResolution;
    }
}