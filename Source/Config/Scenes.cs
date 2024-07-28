using WeatherExploration.Source.Helper;

namespace WeatherExploration.Source.Config;

public static class Scenes {
    //config file containing scene filenames
    public static readonly SceneInfo MAIN_MENU = new SceneInfo { Name = nameof(MAIN_MENU), Path = "res://Scenes/MainMenu.tscn" };
    public static readonly SceneInfo DI_TEST = new SceneInfo { Name = nameof(DI_TEST), Path = "res://Scenes/DITest.tscn" };
    
    //Adding INITIAL_SCENE as a variable to checks against with SceneLoader
    //The initial scene is ran by the engine and thus the SceneLoader doesn't have it's data by default; and it's not to be ran again
    //TODO: can this workflow be improved?
    public static readonly SceneInfo INITIAL_SCENE = new SceneInfo { Name = nameof(INITIAL_SCENE), Path = null };
} 