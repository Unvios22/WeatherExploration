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
    
    private MultiMesh _minorMarkerMultimesh;
    private MultiMesh _waypointMarkerMultimesh;
    
    public override void _Ready() {
        PrepareMarkerMeshTemplate();
        PrepareMultimeshes();
        InstantiateMultimeshInstanceNodes();
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
    
    private void PrepareMultimeshes() {
        _minorMarkerMultimesh = new MultiMesh();
        _minorMarkerMultimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        _minorMarkerMultimesh.SetMesh(_minorMarkerMeshTemplate);
        
        _waypointMarkerMultimesh = new MultiMesh();
        _waypointMarkerMultimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        _waypointMarkerMultimesh.SetMesh(_waypointMarkerMeshTemplate);
    }

    private void InstantiateMultimeshInstanceNodes() {
        var waypointMarkerMultimeshInstance = new MultiMeshInstance3D();
        waypointMarkerMultimeshInstance.Multimesh = _waypointMarkerMultimesh;
        RouteMarkersHolder.AddChild(waypointMarkerMultimeshInstance);
        
        var minorMarkerMultimeshInstance = new MultiMeshInstance3D();
        minorMarkerMultimeshInstance.Multimesh = _minorMarkerMultimesh;
        RouteMarkersHolder.AddChild(minorMarkerMultimeshInstance);
    }

    //renders line of mesh markers connecting the input worldspace points
    //first waypoint vertex is considered to be actually the unit's position
    public void RenderUnitWaypointPath(List<Vector3> vertices) {
        if (vertices.Count < 2) {
            return;
        }
        SpawnRouteMarkers(vertices);
    }
    
    private void SpawnRouteMarkers(List<Vector3> waypointVertices) {
        var markerDistanceStep = 1 / MarkersPerWorldSpaceUnitRatio;
        
        _waypointMarkerMultimesh.InstanceCount = waypointVertices.Count - 1;
        
        var waypointMarkerIndex = 0;
        var minorMarkerCount = 0;
        //PPU: using a cached list here is necessary because the amount of minor markers to spawn is known only after the loop
        //and only then can it be set as instanceCount in the multimesh (assigning instanceCount multiple times inside 
        //the loop clears the previously created instances)
        var minorMarkersToSpawn = new List<Vector3>();
        
        //iterating from last waypoint to make sure the markers are stable in worldspace and the "moving" end of the line
        //is on the unit's end
        for (var i = waypointVertices.Count-1; i>=1; i--) {
            //note the loop will stop after second to last element - considered to be the unit's actual position
            var currentPos = waypointVertices[i]; 
            
            SpawnWaypointMarker(waypointMarkerIndex, currentPos);
            waypointMarkerIndex++;
            
            var distanceToPreviousWaypoint = currentPos.DistanceTo(waypointVertices[i - 1]);
            var vectorTowardsPreviousWaypoint = (waypointVertices[i-1] - currentPos).Normalized();
            var markersToSpawnForThisWaypoint = Mathf.FloorToInt(MarkersPerWorldSpaceUnitRatio * distanceToPreviousWaypoint);
            
            minorMarkerCount += markersToSpawnForThisWaypoint;
            
            //prepare a list of worldspace positons for minor markers to spawn at
            for (var j = markersToSpawnForThisWaypoint; j > 0; j--) {
                currentPos += vectorTowardsPreviousWaypoint * markerDistanceStep;
                minorMarkersToSpawn.Add(currentPos);
            }
        }
        
        _minorMarkerMultimesh.InstanceCount = minorMarkerCount;
        
        //spawn the minor markers
        var minorMarkerIndex = 0;
        foreach (var markerPos in minorMarkersToSpawn) {
            SpawnMinorMarker(minorMarkerIndex, markerPos);
            minorMarkerIndex++;
        }
    }

    private void SpawnWaypointMarker(int meshInstanceId, Vector3 instancePos) {
        var instanceTransform = new Transform3D(Basis.Identity, instancePos);
        _waypointMarkerMultimesh.SetInstanceTransform(meshInstanceId, instanceTransform);
    }
    
    private void SpawnMinorMarker(int meshInstanceId, Vector3 instancePos) {
        var instanceTransform = new Transform3D(Basis.Identity, instancePos);
        _minorMarkerMultimesh.SetInstanceTransform(meshInstanceId, instanceTransform);
    }
    
    public void ClearDisplayedMarkers() {
        _waypointMarkerMultimesh.InstanceCount = 0;
        _minorMarkerMultimesh.InstanceCount = 0;
    }
}