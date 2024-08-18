#[compute]
#version 450

layout(local_size_x = 8, local_size_y = 8) in;

layout(std430, binding = 0) restrict readonly buffer windDirectionMap {
    vec4 windData[];
};

layout(std430, binding = 1) restrict writeonly buffer windDirectionMapResult {
    vec4 resultWindData[];
};

const int simulationGridRes = 4;

//vec4 solveNeighborInfluence(vec2 baseCellVector, float baseCellMagnitude, ivec2 neighborCoords){
//    vec4 neighborAmount = imageLoad(airMovementTex, neighborCoords);
//
//    vec2 neighborVector = vec2(neighborAmount.x, neighborAmount.y);
//    float neighborMagnitude = neighborAmount.z;
//    
//    float vectorDot = dot(baseCellVector, neighborVector);
//
//    if(vectorDot < 0){
//
//        float effectIntensity = vectorDot * -1;
//        vec2 resultVector = normalize(baseCellVector + (neighborVector * effectIntensity));
//        float resultMagnitude = baseCellMagnitude - (neighborMagnitude * effectIntensity);
//        return vec4(resultVector.x, resultVector.y, resultMagnitude, 0);
//
//    } else{
//        return vec4(0,1,0,0);
//    }
//}

int get1DArrIndexFor2DGridPos(ivec2 gridPos){
    return gridPos.y * simulationGridRes + gridPos.x;
}

vec4 getCellWindData(ivec2 cellGridPos){
    //infer index in 1D array for a given2D grid position
    int index = get1DArrIndexFor2DGridPos(cellGridPos);
    return windData[index];
    
}

void setResultCellWindData(ivec2 cellGridPos, vec4 windData){
    int resultArrIndex = get1DArrIndexFor2DGridPos(cellGridPos);
    resultWindData[resultArrIndex] = windData;
}

void solveCellWindChange(ivec2 cellCoords){
    vec4 cellWindData = getCellWindData(cellCoords);
    setResultCellWindData(cellCoords, cellWindData);
    
    
//    vec4 cellAmount = imageLoad(airMovementTex, cellCoords);
//    
//    vec2 airMovementVector = vec2(cellAmount.x, cellAmount.y);
//    float airMovementMagnitude = cellAmount.z;
//    
//    vec4 cellAmountOut = vec4(0,0,0,1);
//
//    cellAmountOut += solveNeighborInfluence(airMovementVector, airMovementMagnitude, ivec2(-1, -1));
//    cellAmountOut += solveNeighborInfluence(airMovementVector, airMovementMagnitude, ivec2(-1, 0));
//    cellAmountOut += solveNeighborInfluence(airMovementVector, airMovementMagnitude, ivec2(-1, 1));
//    
//    cellAmountOut += solveNeighborInfluence(airMovementVector, airMovementMagnitude, ivec2(0, -1));
//    cellAmountOut += solveNeighborInfluence(airMovementVector, airMovementMagnitude, ivec2(0, 1));
//    
//    cellAmountOut += solveNeighborInfluence(airMovementVector, airMovementMagnitude, ivec2(1, -1));
//    cellAmountOut += solveNeighborInfluence(airMovementVector, airMovementMagnitude, ivec2(1, 0));
//    cellAmountOut += solveNeighborInfluence(airMovementVector, airMovementMagnitude, ivec2(1, 1));
}



void main() {
    ivec2 cellGridPos = ivec2(gl_GlobalInvocationID.xy);
    solveCellWindChange(cellGridPos);
}
