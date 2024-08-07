using System.Collections.Generic;
using Godot;

namespace WeatherExploration.Source.Unit;

public partial class UnitRouteMeshController : Node3D {
    
    [Export] private MeshInstance3D _mesh;
    [Export] private Color _meshColor;

    private ImmediateMesh _immediateMesh;

    public override void _Ready() {
        _immediateMesh = _mesh.Mesh as ImmediateMesh;
    }

    //renders mesh line connecting the input worldspace points
    public void SetMeshVertices(List<Vector3> vertices) {
        if (vertices.Count < 2) {
            return;
        }
        ClearMeshVertices();
        
        _immediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
        _immediateMesh.SurfaceSetColor(_meshColor);
        
        foreach (var vertex in vertices) {
            _immediateMesh.SurfaceAddVertex(vertex);
        }
        _immediateMesh.SurfaceEnd();
    }

    public void ClearMeshVertices() {
        _immediateMesh.ClearSurfaces();
    }
}