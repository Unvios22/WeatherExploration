using System.Collections.Generic;
using Godot;

namespace WeatherExploration.Source.WeatherSimulation.Logic;

public class ComputePipelineBuilder {

    private ComputePipeline _computePipeline;
    private List<ComputeBuffer> _computeBuffers;

    public ComputePipelineBuilder BeginPipeline() {
        _computePipeline = new ComputePipeline();
        _computeBuffers = new List<ComputeBuffer>();
        return this;
    }

    public ComputePipelineBuilder SetComputeShader(RDShaderFile shaderFile) {
        _computePipeline.SetShaderFile(shaderFile);
        return this;
    }

    public ComputePipelineBuilder SetWorkGroupSize(Vector3I workGroupSize) {
        _computePipeline.SetWorkGroupSize(workGroupSize);
        return this;
    }
    
    public ComputePipelineBuilder BindImageBuffer(ComputeBufferId bufferId, RDTextureFormat textureFormat, byte[] bufferData) {
        var imageBuffer = new ImageBuffer();
        imageBuffer.BufferId = bufferId;
        imageBuffer.TextureFormat = textureFormat;
        imageBuffer.BufferData = bufferData;
        _computeBuffers.Add(imageBuffer);
        return this;
    }

    public ComputePipelineBuilder BindStorageBuffer(ComputeBufferId bufferId, uint bufferSize, byte[] bufferData) {
        var storageBuffer = new StorageBuffer();
        storageBuffer.BufferId = bufferId;
        storageBuffer.BufferSize = bufferSize;
        storageBuffer.BufferData = bufferData;
        _computeBuffers.Add(storageBuffer);
        return this;
    }

    public ComputePipelineBuilder FinalizeUniformSet() {
        foreach (var computeBuffer in _computeBuffers) {
            if (computeBuffer is StorageBuffer) {
                _computePipeline.DeclareStorageBuffer(computeBuffer as StorageBuffer);
            } else if (computeBuffer is ImageBuffer) {
                _computePipeline.DeclareImageBuffer(computeBuffer as ImageBuffer);
            }
        }
        _computePipeline.FinalizeAndBindBuffers();
        return this;
    }

    public IComputePipelineHandler FinalizePipeline() {
        _computePipeline.FinalizePipeline();
        return _computePipeline;
    }
}