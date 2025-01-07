using System;
using System.Collections.Generic;
using Godot;
using WeatherExploration.Source.WeatherSimulation.Display.Model;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Display;

public class WeatherDataDisplayController {
    
    private Node3D _displayOriginNode;
    private Node3D _displayNodesHolder;
    
    private SimulationSettings _simulationSettings;
    private SimulationWorldDisplaySettings _simulationWorldDisplaySettings;

    private bool _isDisplayInitialized;

    private List<PartialSimulationDataDisplay> _simulationPartialDisplayList;

    public WeatherDataDisplayController(
        SimulationSettings simulationSettings,
        SimulationWorldDisplaySettings simulationWorldDisplaySettings,
        Node3D displayOriginNode,
        Node3D displayNodesHolder) {

        _simulationSettings = simulationSettings;
        _simulationWorldDisplaySettings = simulationWorldDisplaySettings;
        _displayOriginNode = displayOriginNode;
        _displayNodesHolder = displayNodesHolder;

        InitPartialDisplays();
    }

    private void InitPartialDisplays() {
        _simulationPartialDisplayList = new List<PartialSimulationDataDisplay>();
        _simulationPartialDisplayList.Add(new ArrowGridDisplay(_simulationSettings, _displayOriginNode));
        _simulationPartialDisplayList.Add(new BoundingBoxDisplay(_simulationSettings, _displayOriginNode));
        _simulationPartialDisplayList.Add(new TexturePlaneDisplay(_simulationSettings, _displayOriginNode));
    }

    public void DestroyWorldSpaceDisplay() {
        //TODO (?)
    }

    public void RefreshDisplay(WeatherState weatherState) {
        foreach (var partialDisplay in _simulationPartialDisplayList) {
            partialDisplay.EvalDisplayPredicates(_simulationWorldDisplaySettings);
            partialDisplay.RefreshDisplay(weatherState);
        }
    }

    public void HideDisplay() {
        //TODO
    }
}