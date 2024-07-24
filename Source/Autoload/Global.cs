using Godot;

namespace WeatherExploration.Source.Autoload;

public partial class Global : Node {

	private DependencyInstaller _dependencyInstaller;

	public override void _EnterTree() {
		GD.Print("Global Init");
		InitVariables();
	}

	private void InitVariables() {
		_dependencyInstaller = new DependencyInstaller();
	}

	public DependencyInstaller GetInstaller() {
		//todo: temp only to test
		return _dependencyInstaller;
	}
}