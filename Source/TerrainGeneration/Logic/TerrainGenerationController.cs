using Godot;

namespace WeatherExploration.Source.TerrainGeneration.Logic;

public partial class TerrainGenerationController : Node3D {

    [Export] private MeshInstance3D _terrainMesh;
    [Export] private Material _terrainMeshMaterial;
    [Export] private int _meshGridResolution;
    [Export] private float _heightScale;
    [Export] private float _uVScale;
    
    public override void _Ready() {
        GenerateTerrain();
        GD.Print("msads");
    }

    private void GenerateTerrain() {
        var noise = new FastNoiseLite();
        noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        noise.FractalOctaves = 5;
        noise.FractalLacunarity = 2;

        var surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        surfaceTool.SetMaterial(_terrainMeshMaterial);

        for (var x = 0; x < _meshGridResolution; x++) {
            for (var z = 0; z < _meshGridResolution; z++) {
                // Define the four vertices of the grid cell
                var v0 = new Vector3(x, noise.GetNoise2D(x, z) * _heightScale, z);
                var v1 = new Vector3(x + 1, noise.GetNoise2D(x + 1, z) * _heightScale, z);
                var v2 = new Vector3(x, noise.GetNoise2D(x, z + 1) * _heightScale, z + 1);
                var v3 = new Vector3(x + 1, noise.GetNoise2D(x + 1, z + 1) * _heightScale, z + 1);

                // First triangle (v0, v2, v1)
                surfaceTool.SetUV(new Vector2(x, z) / _uVScale);
                surfaceTool.AddVertex(v0);

                surfaceTool.SetUV(new Vector2(x, z + 1) / _uVScale);
                surfaceTool.AddVertex(v2);

                surfaceTool.SetUV(new Vector2(x + 1, z) / _uVScale);
                surfaceTool.AddVertex(v1);

                // Second triangle (v1, v2, v3)
                surfaceTool.SetUV(new Vector2(x + 1, z) / _uVScale);
                surfaceTool.AddVertex(v1);

                surfaceTool.SetUV(new Vector2(x, z + 1) / _uVScale);
                surfaceTool.AddVertex(v2);

                surfaceTool.SetUV(new Vector2(x + 1, z + 1) / _uVScale);
                surfaceTool.AddVertex(v3);
            }
        }
        
        surfaceTool.GenerateNormals();
        surfaceTool.GenerateTangents();
        var mesh = surfaceTool.Commit();
        _terrainMesh.Mesh = mesh;
    }
}