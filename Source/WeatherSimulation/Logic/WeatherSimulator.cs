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
            StepPressure(); 
        }
        
        return _model;
    }

    private void StepPressure() {
        var textureRes = _settings.TextureResolution;
        var pressureMap = _model.AtmosphereModel.PressureTex;
        //todo should prob be initialized once at start and then used as a cache each time
        var newPressureMap = new float[_model.TextureResolution,_model.TextureResolution];
        
        //omitting edge texel indices to avoid index out of range when getting neighbouring cells
        for (var x = 1; x < textureRes - 1; x++) {
            for (var y = 1; y < textureRes - 1; y++) {

                var cellAmount = pressureMap[x, y];
                var amountToDisperse = cellAmount * DisspationPercentage;

                newPressureMap[x, y] += cellAmount - amountToDisperse;

                var amountPerNeighbour = amountToDisperse / 8;

                newPressureMap[x-1,y-1] += amountPerNeighbour;
                newPressureMap[x-1,y] += amountPerNeighbour;
                newPressureMap[x-1,y+1] += amountPerNeighbour;
                
                newPressureMap[x,y-1] += amountPerNeighbour;
                newPressureMap[x,y+1] += amountPerNeighbour;
                
                newPressureMap[x+1,y-1] += amountPerNeighbour;
                newPressureMap[x+1,y] += amountPerNeighbour;
                newPressureMap[x+1,y+1] += amountPerNeighbour;
            }
        }

        //handle upper & lower bound (without outlying borders)
        for (var x = 1; x < textureRes - 1; x++) {
            
            //lower bound
            var cellAmount = pressureMap[x, 0];
            var amountToDisperse = cellAmount * DisspationPercentage;
        
            newPressureMap[x, 0] += cellAmount - amountToDisperse;
            var amountPerNeighbour = amountToDisperse / 5;
        
            newPressureMap[x - 1, 0] += amountPerNeighbour;
            newPressureMap[x + 1, 0] += amountPerNeighbour;
            
            newPressureMap[x - 1, 1] += amountPerNeighbour;
            newPressureMap[x, 1] += amountPerNeighbour;
            newPressureMap[x + 1, 1] += amountPerNeighbour;
            
            //upper bound
            cellAmount = pressureMap[x, textureRes - 1];
            amountToDisperse = cellAmount * DisspationPercentage;
            
            newPressureMap[x, textureRes -1] += cellAmount - amountToDisperse;
            amountPerNeighbour = amountToDisperse / 5;
            
            newPressureMap[x - 1, textureRes-1] += amountPerNeighbour;
            newPressureMap[x + 1, textureRes-1] += amountPerNeighbour;
            
            newPressureMap[x - 1, textureRes-2] += amountPerNeighbour;
            newPressureMap[x, textureRes-2] += amountPerNeighbour;
            newPressureMap[x + 1, textureRes-2] += amountPerNeighbour;
        }
        
        //handle left & right bound (without outlying borders)
        for (var y = 1; y < textureRes - 1; y++) {
            
            var cellAmount = pressureMap[0, y];
            var amountToDisperse = cellAmount * DisspationPercentage;
        
            newPressureMap[0, y] += cellAmount - amountToDisperse;
            var amountPerNeighbour = amountToDisperse / 5;
        
            newPressureMap[0, y - 1] += amountPerNeighbour;
            newPressureMap[0, y + 1] += amountPerNeighbour;
            
            newPressureMap[1, y - 1] += amountPerNeighbour;
            newPressureMap[1, y] += amountPerNeighbour;
            newPressureMap[1, y+1] += amountPerNeighbour;
            
            
            cellAmount = pressureMap[textureRes - 1, y];
            amountToDisperse = cellAmount * DisspationPercentage;
            
            newPressureMap[textureRes - 1, y] += cellAmount - amountToDisperse;
            amountPerNeighbour = amountToDisperse / 5;
            
            newPressureMap[textureRes - 1, y-1] += amountPerNeighbour;
            newPressureMap[textureRes - 1, y+1] += amountPerNeighbour;
            
            newPressureMap[textureRes - 2, y-1] += amountPerNeighbour;
            newPressureMap[textureRes - 2, y] += amountPerNeighbour;
            newPressureMap[textureRes - 2, y+1] += amountPerNeighbour;
        }
        
        //handle outlying borders
        //1
        var outlierAmount = pressureMap[0, 0];
        var outlierAmountToDisperse = outlierAmount * DisspationPercentage;
        
        newPressureMap[0, 0] += outlierAmount - outlierAmountToDisperse;
        
        var outlierAmountPerNeighbour = outlierAmountToDisperse / 3;
        
        newPressureMap[0, 1] += outlierAmountPerNeighbour;
        newPressureMap[1, 1] += outlierAmountPerNeighbour;
        newPressureMap[1, 0] += outlierAmountPerNeighbour;
        
        //2
        outlierAmount = pressureMap[0, textureRes-1];
        outlierAmountToDisperse = outlierAmount * DisspationPercentage;
        
        newPressureMap[0, textureRes-1] += outlierAmount - outlierAmountToDisperse;
        
        outlierAmountPerNeighbour = outlierAmountToDisperse / 3;
        
        newPressureMap[0, textureRes-2] += outlierAmountPerNeighbour;
        newPressureMap[1, textureRes -1] += outlierAmountPerNeighbour;
        newPressureMap[1, textureRes-2] += outlierAmountPerNeighbour;
        
        //3
        outlierAmount = pressureMap[textureRes -1 , 0];
        outlierAmountToDisperse = outlierAmount * DisspationPercentage;
        
        newPressureMap[textureRes -1 , 0] += outlierAmount - outlierAmountToDisperse;
        
        outlierAmountPerNeighbour = outlierAmountToDisperse / 3;
        
        newPressureMap[textureRes -2, 0] += outlierAmountPerNeighbour;
        newPressureMap[textureRes -1, 1] += outlierAmountPerNeighbour;
        newPressureMap[textureRes -2, 1] += outlierAmountPerNeighbour;
        
        //4
        outlierAmount = pressureMap[textureRes -1 , textureRes -1];
        outlierAmountToDisperse = outlierAmount * DisspationPercentage;
        
        newPressureMap[textureRes -1 , textureRes -1] += outlierAmount - outlierAmountToDisperse;
        
        outlierAmountPerNeighbour = outlierAmountToDisperse / 3;
        
        newPressureMap[textureRes -1, textureRes -2] += outlierAmountPerNeighbour;
        newPressureMap[textureRes -2, textureRes -1] += outlierAmountPerNeighbour;
        newPressureMap[textureRes -2, textureRes -2] += outlierAmountPerNeighbour;
        
        
        _model.AtmosphereModel.PressureTex = newPressureMap;
    }
}