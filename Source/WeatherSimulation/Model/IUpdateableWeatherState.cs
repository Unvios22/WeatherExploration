using Godot;

namespace WeatherExploration.Source.WeatherSimulation.Model;

public interface IUpdateableWeatherState {
    public void UpdatePressureTex(Image pressureTex);
    public void UpdatePressureGradient(Vector4Grid pressureGradient);
}