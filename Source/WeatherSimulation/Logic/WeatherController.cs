
using System;
using Godot;
using WeatherExploration.Source.WeatherSimulation.DebugDisplay;
using WeatherExploration.Source.WeatherSimulation.Model;
using WeatherExploration.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Logic;

public partial class WeatherController : Node3D {

    [Export] private MeshInstance3D _displayPlaneMesh;
    [Export] private SimulationSettings _simulationSettings;

    private const double SimTimeStep = 0.2f;
    private double _stepTimer;

    private FastNoiseLite _noiseGenerator;
    private RandomNumberGenerator _randomNumberGenerator;

    private Image _airMovementTex;
    
    //TODO: this is pass-by-value (class), and thus is actually also directly referenced in th ShaderHandler class
    //either refactor to have only one class hold the VectorGrid or have VectorGrid be a struct(?)
    private VectorGrid _pressureGradient;

    private ShaderHandler _simulationShaderHandler;
    private WeatherDataDebugDisplay _weatherDataWorldSpaceDisplay;
    private const float CellWorldSpaceSize = 0.5f;
    

    public override void _Ready() {
        Initialize();
    }

    private void Initialize() {
        //it is assumed the simulation settings are already set up in-editor
        SetupNoiseGenerator();
        SetupSimulation();
        SetupDebugWorldSpaceDisplay();
    }

    private void SetupNoiseGenerator() {
        _noiseGenerator = new FastNoiseLite();
        _noiseGenerator.NoiseType = FastNoiseLite.NoiseTypeEnum.ValueCubic;
        
        _randomNumberGenerator = new RandomNumberGenerator();
        _randomNumberGenerator.Randomize();
    }

    private void SetupSimulation() {
        var windData = CreateWindData();
        _pressureGradient = new VectorGrid(windData, 2);
        _simulationShaderHandler = new ShaderHandler(_simulationSettings, _pressureGradient);
    }

    private void SetupDebugWorldSpaceDisplay() {
        _weatherDataWorldSpaceDisplay = new WeatherDataDebugDisplay(2, CellWorldSpaceSize, this, this);
        _weatherDataWorldSpaceDisplay.InitWorldSpaceDisplay();
    }

    private Vector4[,] CreateWindData() {
        return new Vector4[,] {
            { new Vector4(1, 1, 1, 1), new Vector4(2, 2, 2, 2) },
            { new Vector4(3, 3, 3, 3), new Vector4(4, 4, 4, 4) }
        };
    }
    
    private Image CreateAirMovementTexture() {
        _noiseGenerator.Seed = _randomNumberGenerator.RandiRange(-100, 100);
        var noiseImage = _noiseGenerator.GetImage((int)_simulationSettings.TextureResolution, (int)_simulationSettings.TextureResolution,false, false, false);
        noiseImage.Convert(Image.Format.BptcRgbf);
        
        return noiseImage;
    }
    
    public override void _Process(double delta) {
        if (_stepTimer >= SimTimeStep) {
            Step();
            _stepTimer = 0f;
        }
        _stepTimer += delta;
    }

    private void Step() {
        
        GD.Print("Step Simulation");
        var newPressureGradient = _simulationShaderHandler.Step();
        _pressureGradient = newPressureGradient;
        UpdateWorldSpaceDisplay();
        
        // var totalTemp = 0f;
        // foreach (var value in newTemperatureTex) {
        //     totalTemp += value;
        // }
        // GD.Print("Total temp: " + totalTemp);
    }

    // private void DisplayFloatTexAsImage(float[,] floatArray) {
    //     var array1D = Parse2DArrayTo1DArray(floatArray);
    //     var byteStream = ConvertFloatArrayToByteStream(array1D);
    //
    //     var image = Image.CreateFromData((int)_simulationSettings.TextureResolution, (int)_simulationSettings.TextureResolution,
    //         false, Image.Format.Rf, byteStream);
    //
    //     var imageTex = ImageTexture.CreateFromImage(image);
    //     
    //     AssignImageToPlane(imageTex);
    // }

    private void UpdateWorldSpaceDisplay() {
        _weatherDataWorldSpaceDisplay.SetPressureGradient(_pressureGradient);
        _weatherDataWorldSpaceDisplay.RefreshDisplay();
    }
    
    private void AssignImageToPlane(Image image) {
        var texture2D = ImageTexture.CreateFromImage(image);
        _displayPlaneMesh.GetSurfaceOverrideMaterial(0).Set("albedo_texture", texture2D);
    }
    
}