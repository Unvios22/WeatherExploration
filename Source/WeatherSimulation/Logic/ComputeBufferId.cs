namespace WeatherExploration.Source.WeatherSimulation.Logic;

public enum ComputeBufferId {
    //make sure the int values always match the bindings in .glsl file
    Constants,
    PressureTex,
    PressureGradient
}