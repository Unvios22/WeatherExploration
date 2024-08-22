namespace WeatherExploration.Source.WeatherSimulation.InputDataGeneration;

public abstract class WeatherSourceDataGenerator<T> {
    protected uint InputResolution;

    protected WeatherSourceDataGenerator(uint inputResolution) {
        InputResolution = inputResolution;
    }
    
    public abstract T ProvideInput(double delta);
}