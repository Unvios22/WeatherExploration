using Godot;
using WeatherExploration.Source.WeatherSimulation.DebugDisplay;
using WeatherExploration.Source.WeatherSimulation.InputDataGeneration;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Logic;

public partial class WeatherSimulationController : Node3D {

    [Export] private MeshInstance3D _displayPlaneMesh;
    
    //it is assumed the "settings"-type references are already set up in-editor
    [Export] private SimulationSettings _simulationSettings;
    [Export] private SimulationWorldDisplaySettings _simulationDisplaySettings;

    private SimulationCSHandler _simulationCsHandler;
    private SourceWeatherDataProvider _sourceWeatherDataProvider;
    private WeatherDataWorldSpaceDisplay _weatherDataWorldSpaceDisplay;

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
        _simulationCsHandler = new SimulationCSHandler(_simulationSettings);
    }

    private void SetupWorldSpaceDisplay() {
        _weatherDataWorldSpaceDisplay = new WeatherDataWorldSpaceDisplay(_simulationSettings, _simulationDisplaySettings, this, this);
        _weatherDataWorldSpaceDisplay.InitWorldSpaceDisplay();
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
        var newWeatherState = _simulationCsHandler.Step(_currentWeatherState);
        _currentWeatherState = newWeatherState;
        UpdateWorldSpaceDisplay();
    }
    
    private void UpdateWorldSpaceDisplay() {
        _weatherDataWorldSpaceDisplay.RefreshDisplay(_currentWeatherState);
    }
}