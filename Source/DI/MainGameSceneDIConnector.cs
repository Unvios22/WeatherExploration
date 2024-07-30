using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using WeatherExploration.Source.Helper;
using WeatherExploration.Source.Signals;

namespace WeatherExploration.Source.DI;

[Meta(typeof(IProvider), typeof(IAutoOn))]
public partial class MainGameSceneDIConnector : SceneDIConnector, IProvide<SignalBus> {
    public override void _Notification(int what) => this.Notify(what);
    
    private SignalBus _signalBus;

    public override void _EnterTree() {
        GD.Print(nameof(MainGameSceneDIConnector) + " init");
        GetDIReferences();
    }
    
    private void GetDIReferences() {
        var global = GetAutoload.Global();
        _signalBus = global.SignalBus;
    }
    
    SignalBus IProvide<SignalBus>.Value() => _signalBus;
    
    public void OnReady() => this.Provide();
}