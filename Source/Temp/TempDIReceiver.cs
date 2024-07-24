using Godot;
using WeatherExploration.Source.Helper;

namespace WeatherExploration.Source.Temp;

public partial class TempDIReceiver : Node {

    public override void _Ready() {
        GD.Print("DI Receiver Ready");
        var global = GetAutoload.Global();
        var installer = global.GetInstaller();
        if (installer is not null) {
            GD.Print("Receiver got installer!");
        }
        else {
            GD.Print("Sad mlem noises");
        }
    }
}