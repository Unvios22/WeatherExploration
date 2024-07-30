using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using WeatherExploration.Source.Signals;

namespace WeatherExploration.Source.temp;

[Meta(typeof(IDependent))]
public partial class DITestNode : Node {
    public override void _Notification(int what) => this.Notify(what);
    
    [Dependency]
    public SignalBus _signalBus => this.DependOn<SignalBus>();
    
    public override void _Ready() {
        GD.Print("Test node _Ready");
    }

    public void OnResolved() {
        GD.Print("Test node OnResolved");
        if (_signalBus is not null) {
            GD.Print("Test node got SignalBus!");
        }
    }
}