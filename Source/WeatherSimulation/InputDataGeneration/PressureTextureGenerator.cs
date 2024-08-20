using Godot;

namespace WeatherExploration.Source.WeatherSimulation.InputDataGeneration;

public class PressureTextureGenerator : WeatherSourceDataGenerator<Image> {
    private FastNoiseLite _noiseGenerator;
    private RandomNumberGenerator _randomNumberGenerator;

    //TODO: this can be done better
    private const Image.Format ImageFormat = Image.Format.R8;
    
    public PressureTextureGenerator(uint inputResolution) : base(inputResolution) {
        SetupNoiseGenerator();
    }

    private void SetupNoiseGenerator() {
        _noiseGenerator = new FastNoiseLite();
        _noiseGenerator.NoiseType = FastNoiseLite.NoiseTypeEnum.ValueCubic;
        
        _randomNumberGenerator = new RandomNumberGenerator();
        _randomNumberGenerator.Randomize();
    }

    public override Image ProvideInput() {
        _noiseGenerator.Seed = _randomNumberGenerator.RandiRange(-100, 100);
        var noiseImage = _noiseGenerator.GetImage((int)InputResolution, (int)InputResolution,false, false, false);
        noiseImage.Convert(ImageFormat);
        
        return noiseImage;
    }
}