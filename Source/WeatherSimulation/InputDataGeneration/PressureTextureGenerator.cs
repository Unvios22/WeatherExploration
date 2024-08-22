using Godot;

namespace WeatherExploration.Source.WeatherSimulation.InputDataGeneration;

public class PressureTextureGenerator : WeatherSourceDataGenerator<Image> {
    private FastNoiseLite _noiseGenerator;
    private RandomNumberGenerator _randomNumberGenerator;

    private float _cumDelta;

    //TODO: this can be done better
    private const Image.Format ImageFormat = Image.Format.R8;
    
    public PressureTextureGenerator(uint inputResolution) : base(inputResolution) {
        SetupNoiseGenerator();
    }

    private void SetupNoiseGenerator() {
        _noiseGenerator = new FastNoiseLite();
        _noiseGenerator.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
        _noiseGenerator.Frequency = 0.018f;
        
        _randomNumberGenerator = new RandomNumberGenerator();
        _randomNumberGenerator.Randomize();
        
        _noiseGenerator.Seed = _randomNumberGenerator.RandiRange(-100, 100);
    }

    public override Image ProvideInput(double delta) {
        _cumDelta += (float)delta;
        _noiseGenerator.SetOffset(new Vector3(_cumDelta, _cumDelta, 0f));
        var noiseImage = _noiseGenerator.GetImage((int)InputResolution, (int)InputResolution,false, false, true);
        noiseImage.Convert(ImageFormat);
        
        return noiseImage;
    }
}