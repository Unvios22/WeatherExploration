
using System;
using Godot;
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

    public override void _Ready() {
        Initialize();
    }

    private void Initialize() {
        //it is assumed the simulation settings are already set up in-editor
        SetupNoiseGenerator();
        SetupSimulation();
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
        var newWindData = _simulationShaderHandler.Step();
        var newWindData2D = _pressureGradient.GetDataAs2DArray();
        GD.Print(newWindData.ToString());
        
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
    
    private void AssignImageToPlane(Image image) {
        var texture2D = ImageTexture.CreateFromImage(image);
        _displayPlaneMesh.GetSurfaceOverrideMaterial(0).Set("albedo_texture", texture2D);
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
}