using System.Collections.Generic;
using System.Linq;
using WeatherExploration.Source.Unit.Model;

namespace WeatherExploration.Source.Unit;

public class UnitDisplayController {
    
    public void DrawUnitWaypoints(Queue<UnitWaypoint> waypoints) {
        var waypointsVectorArray = waypoints.Select(x => x.Position).ToArray();
    }
}