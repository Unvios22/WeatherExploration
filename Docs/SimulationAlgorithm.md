## Weather Simulation Algorithm

### Inputs:
- RGB Texture: Air movement vector (RG) with magnitude (B)
- RGB Texture: Terrain normals vector (RG) with magnitude (slope steepness; B)
- SimulationSettings - Resolution, etc.

### Steps:
0. Have a procedurally generated mesh prepared
1. Create the terrain normals tex by sampling the mesh's normals in worldspace directly down for each cell
2. Init air movement tex (decide how to produce data)
3. Pass inputs to compute shader and init

### Algorithm for each iteration:
1. [...]
2. For each cell in air movement tex:
   1. for each of the neighbouring cells
      1. Cal