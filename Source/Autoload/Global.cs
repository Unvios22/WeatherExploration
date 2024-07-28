using Godot;
using WeatherExploration.Source.Config;
using WeatherExploration.Source.Helper;
using WeatherExploration.Source.Signals;

namespace WeatherExploration.Source.Autoload;

public partial class Global : Node {

	private SignalBus<BaseSignal> _signalBus;
	private SceneLoader _sceneLoader;
	
	public override void _EnterTree() {
		//TODO: include a logging system; have it log all inits - Global, SceneLoader, SignalBus, etc.
		GD.Print("Global Init");
		InitVariables();
	}

	private void InitVariables() {
		_signalBus = new SignalBus<BaseSignal>();
		_sceneLoader = new SceneLoader();
	}

	public SignalBus<BaseSignal> SignalBus => _signalBus;
	private SceneLoader SceneLoader => _sceneLoader;
}