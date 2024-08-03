using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

namespace WeatherExploration.Source.Unit.Logic;

[Meta(typeof(IProvide<IUnitInteractionController>))]
public partial class UnitInteractionController : Node, IProvide<IUnitInteractionController> {
    public override void _Notification(int what) => this.Notify(what);
    IUnitInteractionController IProvide<IUnitInteractionController>.Value() => (IUnitInteractionController)this;
    
    private Unit _currentlySelectedUnit;
    
    
    //todo: currently implement a simple unit manager
    
    //if unit gets clicked, select it
    
    //if cLick targets nothing, clear selection
    
    //if click is move order, tell unit to move to target
}