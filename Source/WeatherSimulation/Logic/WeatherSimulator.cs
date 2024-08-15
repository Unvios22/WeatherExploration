using Godot;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Logic;

public class WeatherSimulator {
    private WeatherModel _model;
    private SimulationSettings _settings;
    
    private const float DisspationPercentage = 0.45f;
    public WeatherSimulator(WeatherModel model, SimulationSettings settings) {
        _model = model;
        _settings = settings;
    }

    public WeatherModel Step() {
        for (var i = 0; i < _settings.IterationsPerStep; i++) {
            StepTemperature(); 
        }
        
        return _model;
    }

    private void StepTemperature() {
        var textureRes = _settings.TextureResolution;
        var temperatureMap = _model.AtmosphereModel.TemperatureTex;
        //todo should prob be initialized once at start and then used as a cache each time
        var newTemperatureMap = new float[_model.TextureResolution,_model.TextureResolution];
        
        //omitting edge texel indices to avoid index out of range when getting neighbouring cells
        for (var x = 1; x < textureRes - 1; x++) {
            for (var y = 1; y < textureRes - 1; y++) {

                var cellAmount = temperatureMap[x, y];
                var amountToDisperse = cellAmount * DisspationPercentage;

                newTemperatureMap[x, y] += cellAmount - amountToDisperse;

                var amountPerNeighbour = amountToDisperse / 8;

                newTemperatureMap[x-1,y-1] += amountPerNeighbour;
                newTemperatureMap[x-1,y] += amountPerNeighbour;
                newTemperatureMap[x-1,y+1] += amountPerNeighbour;
                
                newTemperatureMap[x,y-1] += amountPerNeighbour;
                newTemperatureMap[x,y+1] += amountPerNeighbour;
                
                newTemperatureMap[x+1,y-1] += amountPerNeighbour;
                newTemperatureMap[x+1,y] += amountPerNeighbour;
                newTemperatureMap[x+1,y+1] += amountPerNeighbour;
            }
        }

        //handle upper & lower bound (without outlying borders)
        for (var x = 1; x < textureRes - 1; x++) {
            
            //lower bound
            var cellAmount = temperatureMap[x, 0];
            var amountToDisperse = cellAmount * DisspationPercentage;
        
            newTemperatureMap[x, 0] += cellAmount - amountToDisperse;
            var amountPerNeighbour = amountToDisperse / 5;
        
            newTemperatureMap[x - 1, 0] += amountPerNeighbour;
            newTemperatureMap[x + 1, 0] += amountPerNeighbour;
            
            newTemperatureMap[x - 1, 1] += amountPerNeighbour;
            newTemperatureMap[x, 1] += amountPerNeighbour;
            newTemperatureMap[x + 1, 1] += amountPerNeighbour;
            
            //upper bound
            cellAmount = temperatureMap[x, textureRes - 1];
            amountToDisperse = cellAmount * DisspationPercentage;
            
            newTemperatureMap[x, textureRes -1] += cellAmount - amountToDisperse;
            amountPerNeighbour = amountToDisperse / 5;
            
            newTemperatureMap[x - 1, textureRes-1] += amountPerNeighbour;
            newTemperatureMap[x + 1, textureRes-1] += amountPerNeighbour;
            
            newTemperatureMap[x - 1, textureRes-2] += amountPerNeighbour;
            newTemperatureMap[x, textureRes-2] += amountPerNeighbour;
            newTemperatureMap[x + 1, textureRes-2] += amountPerNeighbour;
        }
        
        //handle left & right bound (without outlying borders)
        for (var y = 1; y < textureRes - 1; y++) {
            
            var cellAmount = temperatureMap[0, y];
            var amountToDisperse = cellAmount * DisspationPercentage;
        
            newTemperatureMap[0, y] += cellAmount - amountToDisperse;
            var amountPerNeighbour = amountToDisperse / 5;
        
            newTemperatureMap[0, y - 1] += amountPerNeighbour;
            newTemperatureMap[0, y + 1] += amountPerNeighbour;
            
            newTemperatureMap[1, y - 1] += amountPerNeighbour;
            newTemperatureMap[1, y] += amountPerNeighbour;
            newTemperatureMap[1, y+1] += amountPerNeighbour;
            
            
            cellAmount = temperatureMap[textureRes - 1, y];
            amountToDisperse = cellAmount * DisspationPercentage;
            
            newTemperatureMap[textureRes - 1, y] += cellAmount - amountToDisperse;
            amountPerNeighbour = amountToDisperse / 5;
            
            newTemperatureMap[textureRes - 1, y-1] += amountPerNeighbour;
            newTemperatureMap[textureRes - 1, y+1] += amountPerNeighbour;
            
            newTemperatureMap[textureRes - 2, y-1] += amountPerNeighbour;
            newTemperatureMap[textureRes - 2, y] += amountPerNeighbour;
            newTemperatureMap[textureRes - 2, y+1] += amountPerNeighbour;
        }
        
        //handle outlying borders
        //1
        var outlierAmount = temperatureMap[0, 0];
        var outlierAmountToDisperse = outlierAmount * DisspationPercentage;
        
        newTemperatureMap[0, 0] += outlierAmount - outlierAmountToDisperse;
        
        var outlierAmountPerNeighbour = outlierAmountToDisperse / 3;
        
        newTemperatureMap[0, 1] += outlierAmountPerNeighbour;
        newTemperatureMap[1, 1] += outlierAmountPerNeighbour;
        newTemperatureMap[1, 0] += outlierAmountPerNeighbour;
        
        //2
        outlierAmount = temperatureMap[0, textureRes-1];
        outlierAmountToDisperse = outlierAmount * DisspationPercentage;
        
        newTemperatureMap[0, textureRes-1] += outlierAmount - outlierAmountToDisperse;
        
        outlierAmountPerNeighbour = outlierAmountToDisperse / 3;
        
        newTemperatureMap[0, textureRes-2] += outlierAmountPerNeighbour;
        newTemperatureMap[1, textureRes -1] += outlierAmountPerNeighbour;
        newTemperatureMap[1, textureRes-2] += outlierAmountPerNeighbour;
        
        //3
        outlierAmount = temperatureMap[textureRes -1 , 0];
        outlierAmountToDisperse = outlierAmount * DisspationPercentage;
        
        newTemperatureMap[textureRes -1 , 0] += outlierAmount - outlierAmountToDisperse;
        
        outlierAmountPerNeighbour = outlierAmountToDisperse / 3;
        
        newTemperatureMap[textureRes -2, 0] += outlierAmountPerNeighbour;
        newTemperatureMap[textureRes -1, 1] += outlierAmountPerNeighbour;
        newTemperatureMap[textureRes -2, 1] += outlierAmountPerNeighbour;
        
        //4
        outlierAmount = temperatureMap[textureRes -1 , textureRes -1];
        outlierAmountToDisperse = outlierAmount * DisspationPercentage;
        
        newTemperatureMap[textureRes -1 , textureRes -1] += outlierAmount - outlierAmountToDisperse;
        
        outlierAmountPerNeighbour = outlierAmountToDisperse / 3;
        
        newTemperatureMap[textureRes -1, textureRes -2] += outlierAmountPerNeighbour;
        newTemperatureMap[textureRes -2, textureRes -1] += outlierAmountPerNeighbour;
        newTemperatureMap[textureRes -2, textureRes -2] += outlierAmountPerNeighbour;
        
        
        _model.AtmosphereModel.TemperatureTex = newTemperatureMap;
    }

    private void StepPressure() {
        var textureRes = _settings.TextureResolution;
        var pressureMap = _model.AtmosphereModel.PressureTex;
        //todo should prob be initialized once at start and then used as a cache each time
        var newPressureTex = new Vector3[_model.TextureResolution,_model.TextureResolution];
        
        //omitting edge texel indices to avoid index out of range when getting neighbouring cells
        for (var x = 1; x < textureRes - 1; x++) {
            for (var y = 1; y < textureRes - 1; y++) {

                var cellAmount = pressureMap[x, y];
                var amountToDisperse = cellAmount * DisspationPercentage;

                newPressureTex[x, y] += cellAmount - amountToDisperse;

                var amountPerNeighbour = amountToDisperse / 8;

                newPressureTex[x-1,y-1] += amountPerNeighbour;
                newPressureTex[x-1,y] += amountPerNeighbour;
                newPressureTex[x-1,y+1] += amountPerNeighbour;
                
                newPressureTex[x,y-1] += amountPerNeighbour;
                newPressureTex[x,y+1] += amountPerNeighbour;
                
                newPressureTex[x+1,y-1] += amountPerNeighbour;
                newPressureTex[x+1,y] += amountPerNeighbour;
                newPressureTex[x+1,y+1] += amountPerNeighbour;
            }
        }

        //handle upper & lower bound (without outlying borders)
        for (var x = 1; x < textureRes - 1; x++) {
            
            //lower bound
            var cellAmount = pressureMap[x, 0];
            var amountToDisperse = cellAmount * DisspationPercentage;
        
            newPressureTex[x, 0] += cellAmount - amountToDisperse;
            var amountPerNeighbour = amountToDisperse / 5;
        
            newPressureTex[x - 1, 0] += amountPerNeighbour;
            newPressureTex[x + 1, 0] += amountPerNeighbour;
            
            newPressureTex[x - 1, 1] += amountPerNeighbour;
            newPressureTex[x, 1] += amountPerNeighbour;
            newPressureTex[x + 1, 1] += amountPerNeighbour;
            
            //upper bound
            cellAmount = pressureMap[x, textureRes - 1];
            amountToDisperse = cellAmount * DisspationPercentage;
            
            newPressureTex[x, textureRes -1] += cellAmount - amountToDisperse;
            amountPerNeighbour = amountToDisperse / 5;
            
            newPressureTex[x - 1, textureRes-1] += amountPerNeighbour;
            newPressureTex[x + 1, textureRes-1] += amountPerNeighbour;
            
            newPressureTex[x - 1, textureRes-2] += amountPerNeighbour;
            newPressureTex[x, textureRes-2] += amountPerNeighbour;
            newPressureTex[x + 1, textureRes-2] += amountPerNeighbour;
        }
        
        //handle left & right bound (without outlying borders)
        for (var y = 1; y < textureRes - 1; y++) {
            
            var cellAmount = pressureMap[0, y];
            var amountToDisperse = cellAmount * DisspationPercentage;
        
            newPressureTex[0, y] += cellAmount - amountToDisperse;
            var amountPerNeighbour = amountToDisperse / 5;
        
            newPressureTex[0, y - 1] += amountPerNeighbour;
            newPressureTex[0, y + 1] += amountPerNeighbour;
            
            newPressureTex[1, y - 1] += amountPerNeighbour;
            newPressureTex[1, y] += amountPerNeighbour;
            newPressureTex[1, y+1] += amountPerNeighbour;
            
            
            cellAmount = pressureMap[textureRes - 1, y];
            amountToDisperse = cellAmount * DisspationPercentage;
            
            newPressureTex[textureRes - 1, y] += cellAmount - amountToDisperse;
            amountPerNeighbour = amountToDisperse / 5;
            
            newPressureTex[textureRes - 1, y-1] += amountPerNeighbour;
            newPressureTex[textureRes - 1, y+1] += amountPerNeighbour;
            
            newPressureTex[textureRes - 2, y-1] += amountPerNeighbour;
            newPressureTex[textureRes - 2, y] += amountPerNeighbour;
            newPressureTex[textureRes - 2, y+1] += amountPerNeighbour;
        }
        
        //handle outlying borders
        //1
        var outlierAmount = pressureMap[0, 0];
        var outlierAmountToDisperse = outlierAmount * DisspationPercentage;
        
        newPressureTex[0, 0] += outlierAmount - outlierAmountToDisperse;
        
        var outlierAmountPerNeighbour = outlierAmountToDisperse / 3;
        
        newPressureTex[0, 1] += outlierAmountPerNeighbour;
        newPressureTex[1, 1] += outlierAmountPerNeighbour;
        newPressureTex[1, 0] += outlierAmountPerNeighbour;
        
        //2
        outlierAmount = pressureMap[0, textureRes-1];
        outlierAmountToDisperse = outlierAmount * DisspationPercentage;
        
        newPressureTex[0, textureRes-1] += outlierAmount - outlierAmountToDisperse;
        
        outlierAmountPerNeighbour = outlierAmountToDisperse / 3;
        
        newPressureTex[0, textureRes-2] += outlierAmountPerNeighbour;
        newPressureTex[1, textureRes -1] += outlierAmountPerNeighbour;
        newPressureTex[1, textureRes-2] += outlierAmountPerNeighbour;
        
        //3
        outlierAmount = pressureMap[textureRes -1 , 0];
        outlierAmountToDisperse = outlierAmount * DisspationPercentage;
        
        newPressureTex[textureRes -1 , 0] += outlierAmount - outlierAmountToDisperse;
        
        outlierAmountPerNeighbour = outlierAmountToDisperse / 3;
        
        newPressureTex[textureRes -2, 0] += outlierAmountPerNeighbour;
        newPressureTex[textureRes -1, 1] += outlierAmountPerNeighbour;
        newPressureTex[textureRes -2, 1] += outlierAmountPerNeighbour;
        
        //4
        outlierAmount = pressureMap[textureRes -1 , textureRes -1];
        outlierAmountToDisperse = outlierAmount * DisspationPercentage;
        
        newPressureTex[textureRes -1 , textureRes -1] += outlierAmount - outlierAmountToDisperse;
        
        outlierAmountPerNeighbour = outlierAmountToDisperse / 3;
        
        newPressureTex[textureRes -1, textureRes -2] += outlierAmountPerNeighbour;
        newPressureTex[textureRes -2, textureRes -1] += outlierAmountPerNeighbour;
        newPressureTex[textureRes -2, textureRes -2] += outlierAmountPerNeighbour;
        
        
        _model.AtmosphereModel.PressureTex = newPressureTex;
    }
}