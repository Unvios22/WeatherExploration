using Godot;
using WeatherExploration.Source.SceneManagement;
using WeatherExploration.Source.Signals;

namespace WeatherExploration.Source.Autoload;

public partial class Global : Node {

	private SignalBus _signalBus;
	private SceneLoader _sceneLoader;
	
	public override void _EnterTree() {
		//TODO: include a logging system; have it log all inits - Global, SceneLoader, SignalBus, etc.
		GD.Print("Global Init");
		InitVariables();
	}

	private void InitVariables() {
		_signalBus = new SignalBus();
		_sceneLoader = new SceneLoader(_signalBus);
	}

	public SignalBus SignalBus => _signalBus;
	private SceneLoader SceneLoader => _sceneLoader;
}