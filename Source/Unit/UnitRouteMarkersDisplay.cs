using System.Collections.Generic;
using Godot;

namespace WeatherExploration.Source.Unit;

public partial class UnitRouteMarkersDisplay : Node3D {

    [Export] private  Node3D RouteMarkersHolder;
    
    private SphereMesh _minorMarkerMeshTemplate;
    private SphereMesh _waypointMarkerMeshTemplate;
    
    private const float MarkersPerWorldSpaceUnitRatio = 1.8f;

    private const float MinorMarkerRadius = 0.2f;
    private const float WaypointMarkerRadius = 0.4f;

    private List<Node> _spawnedMarkersHolder;
        
    //PPU - both the config, the calculations, and the instantiation/destroying can be optimized
    //some object pool may be better than creating/destroying objects constantly
    //also - make sure the created worldspace meshes are properly considered multiple instances of same mesh in terms of memory and GPU usage
    public override void _Ready() {
        PrepareMarkerMeshTemplate();
        _spawnedMarkersHolder = new List<Node>();
    }

    private void PrepareMarkerMeshTemplate() {
        _minorMarkerMeshTemplate = new SphereMesh();
        _minorMarkerMeshTemplate.RadialSegments = 8;
        _minorMarkerMeshTemplate.Rings = 8;
        _minorMarkerMeshTemplate.Radius = MinorMarkerRadius;
        _minorMarkerMeshTemplate.Height = MinorMarkerRadius * 2;
        
        _waypointMarkerMeshTemplate = new SphereMesh();
        _waypointMarkerMeshTemplate.RadialSegments = 8;
        _waypointMarkerMeshTemplate.Rings = 8;
        _waypointMarkerMeshTemplate.Radius = WaypointMarkerRadius;
        _waypointMarkerMeshTemplate.Height = WaypointMarkerRadius * 2;
        
        //TODO: duplicate code; refactor
        //TODO: add also custom material (prob unshaded one)
    }

    //renders mesh line connecting the input worldspace points
    //first waypoint vertex is considered to be actually the unit's position
    public void RenderUnitWaypointPath(List<Vector3> vertices) {
        if (vertices.Count < 2) {
            return;
        }
        
        ClearDisplayedMarkers();
        SpawnRouteMarkers(vertices);
    }

    private void SpawnRouteMarkers(List<Vector3> waypointVertices) {
        var markerDistanceStep = 1 / MarkersPerWorldSpaceUnitRatio;
        
        //iterating from last waypoint to make sure the markers are stable in worldspace and the "moving" end of the line
        //is on the unit's end
        for (var i = waypointVertices.Count-1; i>=1; i--) {
            //note the loop will stop after second to last element - considered to be the unit's actual position
            var currentPos = waypointVertices[i]; 
            
            SpawnWaypointMarker(currentPos);
            
            var distanceToPreviousWaypoint = currentPos.DistanceTo(waypointVertices[i - 1]);
            var vectorTowardsPreviousWaypoint = (waypointVertices[i-1] - currentPos).Normalized();
            var markersToSpawn = Mathf.FloorToInt(MarkersPerWorldSpaceUnitRatio * distanceToPreviousWaypoint);
            
            for (var j = markersToSpawn; j > 0; j--) {
                currentPos += vectorTowardsPreviousWaypoint * markerDistanceStep;
                SpawnMinorMarker(currentPos);
            }
        }
    }

    private void SpawnWaypointMarker(Vector3 position) {
        var markerNode = new MeshInstance3D();
        markerNode.Mesh = _waypointMarkerMeshTemplate;
        markerNode.Position = position;
        RouteMarkersHolder.AddChild(markerNode);
        _spawnedMarkersHolder.Add(markerNode);
    }
    
    private void SpawnMinorMarker(Vector3 position) {
        var markerNode = new MeshInstance3D();
        markerNode.Mesh = _minorMarkerMeshTemplate;
        markerNode.Position = position;
        RouteMarkersHolder.AddChild(markerNode);
        _spawnedMarkersHolder.Add(markerNode);
    }
    
    public void ClearDisplayedMarkers() {
        foreach (var spawnedNode in _spawnedMarkersHolder) {
            spawnedNode.Free();
        }
        _spawnedMarkersHolder.Clear();
    }
}