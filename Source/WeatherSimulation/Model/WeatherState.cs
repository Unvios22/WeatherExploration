using Godot;

namespace WeatherExploration.Source.WeatherSimulation.Model;

public struct WeatherState {
    public Image PressureTex;
    public Vector4Grid PressureGradient;
    
    public WeatherState(Image pressureTex, Vector4Grid pressureGradient) {
        PressureTex = pressureTex;
        PressureGradient = pressureGradient;
    }
}