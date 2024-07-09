using Godot;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Logic;

public class WeatherModelBuilder {
    private WeatherModel _model = new WeatherModel();
    private int _modelResolution;

    public void Reset() {
        _model = new WeatherModel();
        _modelResolution = 0;
    }

    public void SetSimResolution(int simResolution) {
        _model.TextureResolution = simResolution;
        _modelResolution = simResolution;
    }
    
    public void InitializeTextures() {
        _model.GroundModel.TemperatureTex = CreateTexture();
        _model.GroundModel.HeightTex = CreateTexture();
        _model.GroundModel.MoistureTex = CreateTexture();
        _model.GroundModel.SlopeTex = CreateTexture();

        _model.AtmosphereModel.TemperatureTex = CreateTexture();
        _model.AtmosphereModel.MoistureTex = CreateTexture();
        _model.AtmosphereModel.PressureTex = CreateTexture();
    }

    public WeatherModel Build() {
        var result = _model;
        Reset();
        return result;
    }

    private Texture2D CreateTexture() {
        var image = Image.Create(_modelResolution, _modelResolution, false, Image.Format.Rgb8);
        var imageTexture = ImageTexture.CreateFromImage(image);
        return imageTexture;
    }
}