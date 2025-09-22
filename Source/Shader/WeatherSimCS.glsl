#[compute]
#version 460

layout(local_size_x = 8, local_size_y = 8) in;

layout(std430, binding = 0) restrict readonly buffer Constants {
    uint simulationGridRes;
};

layout(r8, binding = 1) restrict readonly uniform image2D pressureTex;

layout(std430, binding = 2) restrict writeonly buffer pressureGradient {
    vec4 pressureGradientArr[];
};


uint get1DArrIndexFor2DGridPos(ivec2 gridPos){
    return gridPos.y * simulationGridRes + gridPos.x;
}

vec4 getCellGPressureGradientVal(ivec2 cellGridPos){
    //infer index in 1D array for a given2D grid position
    uint index = get1DArrIndexFor2DGridPos(cellGridPos);
    return pressureGradientArr[index];
}

void setResultPressureGradient(ivec2 cellGridPos, vec4 gradientValue){
    uint resultArrIndex = get1DArrIndexFor2DGridPos(cellGridPos);
    pressureGradientArr[resultArrIndex] = gradientValue;
}

float fetchPressureVal(ivec2 cellCoords){
    return imageLoad(pressureTex, cellCoords).r;
}

vec4 solveCellPressureGradient(ivec2 cellCoords){
    float cellPressureVal = fetchPressureVal(cellCoords);
    
    //todo optimize the lookup for derivative with "shared" and other keywords
    
    float nPressureLeftUp = fetchPressureVal(cellCoords + ivec2(-1, 1));
    float nPressureLeftMid = fetchPressureVal(cellCoords + ivec2(-1, 0));
    float nPressureLeftDown = fetchPressureVal(cellCoords + ivec2(-1, 0));
    
    float nPressureMidUp = fetchPressureVal(cellCoords + ivec2(0, 1));
    float nPressureMidDown = fetchPressureVal(cellCoords + ivec2(0, -1));
    
    float nPressureRightUp = fetchPressureVal(cellCoords + ivec2(1, 1));
    float nPressureRightMid = fetchPressureVal(cellCoords + ivec2(1, 0));
    float nPressureRightDown = fetchPressureVal(cellCoords + ivec2(1, -1));
    
    float horizontalGradient = ((nPressureRightUp + nPressureRightMid + nPressureRightDown) - (nPressureLeftUp + nPressureLeftMid + nPressureLeftDown)) / 6;
    float verticalGradient = ((nPressureLeftUp + nPressureMidUp + nPressureRightUp) - (nPressureLeftDown + nPressureMidDown + nPressureRightDown)) / 6;
    
    //todo: calculate and output the vector's magnitude also
    vec4 cellPressureGradient = vec4(horizontalGradient,verticalGradient,1,0);
    return cellPressureGradient;
}

vec4 solveCellGradientWind(vec4 cellPressureGradient){
    //should i normalize the gradient here?
    return cellPressureGradient;
}

void main() {
    ivec2 cellGridPos = ivec2(gl_GlobalInvocationID.xy);
    
    vec4 cellPressureGradient = solveCellPressureGradient(cellGridPos);
    setResultPressureGradient(cellGridPos, cellPressureGradient);
    
   vec4 cellGradientWind = solveCellGradientWind(cellPressureGradient);
}
