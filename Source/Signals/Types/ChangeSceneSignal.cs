using WeatherExploration.Source.Helper;

namespace WeatherExploration.Source.Signals.Types;

public class ChangeSceneSignal : BaseSignal {
    public SceneInfo TargetSceneInfo;
}