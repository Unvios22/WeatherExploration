using Godot;

namespace WeatherExploration.Source.Autoload;

public partial class Global : Node {

	public override void _Ready() {
		GD.Print("Global Init");
	}
}