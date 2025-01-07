using Godot;
using WeatherExploration.Source.WeatherSimulation.Display;
using WeatherExploration.Source.WeatherSimulation.InputDataGeneration;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Logic;

public partial class WeatherSimulationController : Node3D {

    [Export] private MeshInstance3D _displayPlaneMesh;
    
    //it is assumed the "settings"-type references are already set up in-editor
    [Export] private SimulationSettings _simulationSettings;
    [Export] private SimulationWorldDisplaySettings _simulationDisplaySettings;

    private SimulationComputeHandler _simulationComputeHandler;
    private SourceWeatherDataProvider _sourceWeatherDataProvider;
    private WeatherDataDisplayController _weatherDataDisplayController;

    private WeatherState _currentWeatherState;
    private double _stepTimer;

    public override void _Ready() {
        Initialize();
    }

    private void Initialize() {
        SetupSimulation();
        SetupWorldSpaceDisplay();
    }
    
    private void SetupSimulation() {
        _sourceWeatherDataProvider = new SourceWeatherDataProvider();
        _currentWeatherState = _sourceWeatherDataProvider.CreateInitialWeatherState(_simulationSettings);
        _simulationComputeHandler = new SimulationComputeHandler(_simulationSettings);
    }

    private void SetupWorldSpaceDisplay() {
        _weatherDataDisplayController = new WeatherDataDisplayController(_simulationSettings, _simulationDisplaySettings, this, this);
    }
    
    public override void _Process(double delta) {
        // if (_stepTimer >= _simulationSettings.TimeStep) {
        //     
        //     _stepTimer = 0f;
        // }
        // _stepTimer += delta;
        Step(delta);
    }

    private void Step(double delta) {
        GD.Print("Step Simulation");
        _currentWeatherState = _sourceWeatherDataProvider.UpdateWeatherStateSourceData(_currentWeatherState, delta);
        var newWeatherState = _simulationComputeHandler.Step(_currentWeatherState);
        _currentWeatherState = newWeatherState;
        UpdateWorldSpaceDisplay();
    }
    
    private void UpdateWorldSpaceDisplay() {
        _weatherDataDisplayController.RefreshDisplay(_currentWeatherState);
    }
}