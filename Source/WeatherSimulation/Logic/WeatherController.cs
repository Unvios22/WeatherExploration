using System;
using Godot;
using WeatherExploration.Source.WeatherSimulation.Model;
using WeatherExploration.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Logic;

public partial class WeatherController : Node3D {

    [Export] private MeshInstance3D _displayPlaneMesh;
    [Export] private SimulationSettings _simulationSettings;
    
    private WeatherModel _weatherModel;

    private const double SimTimeStep = 0.2f;
    private double _stepTimer;

    private FastNoiseLite _noiseGenerator;
    private RandomNumberGenerator _randomNumberGenerator;

    private WeatherSimulator _weatherSimulator;

    public override void _Ready() {
        Initialize();
    }

    private void Initialize() {
        //it is assumed the simulation settings are already set up in-editor
        SetupNoiseGenerator();
        SetupSimulation();
    }

    // private void InitializeSimModel() {
    //     var modelBuilder = new WeatherModelBuilder();
    //     modelBuilder.SetSimResolution(_simulationSettings.TextureResolution);
    //     modelBuilder.InitializeTextures();
    //     _weatherModel = modelBuilder.Build();
    // }

    private void SetupNoiseGenerator() {
        _noiseGenerator = new FastNoiseLite();
        _noiseGenerator.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
        
        _randomNumberGenerator = new RandomNumberGenerator();
        _randomNumberGenerator.Randomize();
    }

    // public Texture2D SampleTexture() {
    //     return _weatherModel.AtmosphereModel.TemperatureTex;
    // }

    private void SetupSimulation() {
        var atmosphereModel = new AtmosphereModel();
        var pressureTexture = CreatePressureTexture();
        atmosphereModel.PressureTex = pressureTexture;
        
        var weatherModel = new WeatherModel();
        weatherModel.TextureResolution = _simulationSettings.TextureResolution;
        weatherModel.AtmosphereModel = atmosphereModel;
        
        _weatherSimulator = new WeatherSimulator(weatherModel, _simulationSettings);
    }

    private float[,] CreatePressureTexture() {
        _noiseGenerator.Seed = _randomNumberGenerator.RandiRange(-100, 100);
        var noiseImage = _noiseGenerator.GetImage(_simulationSettings.TextureResolution, _simulationSettings.TextureResolution);
        noiseImage.Convert(Image.Format.Rf);
        
        var imageDataByteStream = noiseImage.GetData();
        var floatArray1D = ConvertByteStreamToFloatArray(imageDataByteStream);
        var floatArray2D = Parse1Dto2DArray(floatArray1D);
        return floatArray2D;
    }

    private float[,] Parse1Dto2DArray(float[] array1D) {
        var textureRes = _simulationSettings.TextureResolution;
        var array2D = new float[textureRes,textureRes];

        var index = 0;
        
        //todo check whether texture is x or y first
        for (var x = 0; x < textureRes; x++) {
            for (var y = 0; y < textureRes; y++) {
                array2D[x, y] = array1D[index];
                index++;
            }
        }

        return array2D;
    }

    private float[] Parse2DArrayTo1DArray(float[,] array2D) {
        var textureRes = _simulationSettings.TextureResolution;
        var array1D = new float[textureRes * textureRes];

        var index = 0;
        for (var x = 0; x < textureRes; x++) {
            for (var y = 0; y < textureRes; y++) {
                array1D[index] = array2D[x, y];
                index++;
            }
        }

        return array1D;
    }

    public override void _Process(double delta) {
        // if (_stepTimer >= SimTimeStep) {
        //     Step();
        //     _stepTimer = 0f;
        // }
        // _stepTimer += delta;
        Step();
    }

    private void Step() {
        //currently operating on pressure texture only

        var stepResult = _weatherSimulator.Step();
        var newPressureTex = stepResult.AtmosphereModel.PressureTex;
        
        DisplayFloatTexAsImage(newPressureTex);
        GD.Print("Step Simulation");

        var totalPressure = 0f;
        foreach (var value in newPressureTex) {
            totalPressure += value;
        }
        GD.Print("Total pressure: " + totalPressure);
    }

    private void DisplayFloatTexAsImage(float[,] floatArray) {
        var array1D = Parse2DArrayTo1DArray(floatArray);
        var byteStream = ConvertFloatArrayToByteStream(array1D);

        var image = Image.CreateFromData(_simulationSettings.TextureResolution, _simulationSettings.TextureResolution,
            false, Image.Format.Rf, byteStream);

        var imageTex = ImageTexture.CreateFromImage(image);
        
        AssignTextureToPlane(imageTex);
    }

    private float[] ConvertByteStreamToFloatArray(byte[] stream) {
        var convertedFloatArray = new float[stream.Length / sizeof(float)];
        for (var i = 0; i < convertedFloatArray.Length; i++) {
            convertedFloatArray[i] = BitConverter.ToSingle(stream, i * sizeof(float));
        }

        return convertedFloatArray;
    }

    private byte[] ConvertFloatArrayToByteStream(float[] floatArray) {
        var byteStream = new byte[floatArray.Length * sizeof(float)];
        for (var i = 0; i < floatArray.Length; i++) {
            BitConverter.GetBytes(floatArray[i]).CopyTo(byteStream, i * sizeof(float));
        }

        return byteStream;
    }
    
    public void AssignTextureToPlane(Texture2D tex) {
        _displayPlaneMesh.GetSurfaceOverrideMaterial(0).Set("albedo_texture", tex);
    }
    
    public WeatherModel GetCurrentState() {
        return _weatherModel;
    }
}