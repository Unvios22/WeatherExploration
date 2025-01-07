using Godot;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Display.Model;

public abstract class PartialSimulationDataDisplay {

    protected SimulationSettings SimulationSettings;
    protected Node3D PartialDisplaysOriginNode;
    
    protected bool IsInitialized;
    
    public PartialSimulationDataDisplay(SimulationSettings simulationSettings, Node3D partialDisplaysOriginNode) {
        SimulationSettings = simulationSettings;
        PartialDisplaysOriginNode = partialDisplaysOriginNode;
    }
    
    public abstract void EvalDisplayPredicates(SimulationWorldDisplaySettings displaySettings);
    public abstract void RefreshDisplay(WeatherState weatherState);
}