using Godot;

namespace WeatherExploration.WeatherSimulation.Model;

public struct AtmosphereModel {
    public float[,] TemperatureTex;
    public float[,] MoistureTex;
    public float[,] PressureTex;
    
    //optional/might happen to be emergent
//     public Texture2D PrecipitationTex;
//     public Texture2D WindTex;
//     public Texture2D LightningTex;
}