#[compute]
#version 460

layout(local_size_x = 8, local_size_y = 8) in;

layout(r8, binding = 0) restrict readonly uniform image2D temperatureTex;
layout(r8, binding = 1) restrict uniform image2D temperatureTexOut;

const float DissipationPercentage = 0.6;

void processCell(ivec2 cellCoords){

    vec4 cellAmount = imageLoad(temperatureTex, cellCoords);
    vec4 amountToDisperse = cellAmount * DissipationPercentage;
    vec4 remainingCellVal = cellAmount - amountToDisperse;

    imageStore(temperatureTexOut, cellCoords, cellAmount * 1.1);
    
//    atomicAdd(imageStore(temperatureTexOut, cellCoords, remainingCellVal));
//
//    vec4 amountPerNeighbour = amountToDisperse / 8;
//    
//    imageAtomicAdd(temperatureTexOut, cellCoords + ivec2(-1, -1), amountPerNeighbour);
//    imageAtomicAdd(temperatureTexOut, cellCoords + ivec2(-1, 0), amountPerNeighbour);
//    imageAtomicAdd(temperatureTexOut, cellCoords + ivec2(-1, 1), amountPerNeighbour);
//
//    imageAtomicAdd(temperatureTexOut, cellCoords + ivec2(0, -1), amountPerNeighbour);
//    imageAtomicAdd(temperatureTexOut, cellCoords + ivec2(0, 1), amountPerNeighbour);
//
//    imageAtomicAdd(temperatureTexOut, cellCoords + ivec2(1, -1), amountPerNeighbour);
//    imageAtomicAdd(temperatureTexOut, cellCoords + ivec2(1, 0), amountPerNeighbour);
//    imageAtomicAdd(temperatureTexOut, cellCoords + ivec2(1, 1), amountPerNeighbour);
}

void main() {
    ivec2 coords = ivec2(gl_GlobalInvocationID.xy);
    processCell(coords);
}
