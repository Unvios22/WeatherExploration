using System.Collections.Generic;
using System.Linq;
using Godot;

namespace WeatherExploration.Source.WeatherSimulation.Logic;

public class ComputePipeline : IFinalizedComputePipeline{
    private RenderingDevice _renderingDevice;
    private Rid _computeShaderRid;
    private Rid _computePipelineRid;
    private Rid _uniformSetRid;
    private List<ComputeBuffer> _computeBuffers;

    private Vector3I _workGroupSize;
    
    public ComputePipeline() {
        _renderingDevice = RenderingServer.CreateLocalRenderingDevice();
        _computeBuffers = new List<ComputeBuffer>();
    }

    public void SetWorkGroupSize(Vector3I workGroupCount) {
        _workGroupSize = workGroupCount;
    }
    
    public void SetShaderFile(RDShaderFile shaderFile) {
        var shaderBytecode = shaderFile.GetSpirV();
        _computeShaderRid = _renderingDevice.ShaderCreateFromSpirV(shaderBytecode);
    }

    public void DeclareStorageBuffer(StorageBuffer storageBuffer) {
        var bufferRid = _renderingDevice.StorageBufferCreate(storageBuffer.BufferSize, storageBuffer.Data);
        storageBuffer.Rid = bufferRid;

        var bufferUniform = new RDUniform();
        bufferUniform.UniformType = RenderingDevice.UniformType.StorageBuffer;
        bufferUniform.Binding = (int)storageBuffer.Id;
        bufferUniform.AddId(bufferRid);
        
        storageBuffer.Uniform = bufferUniform;
        _computeBuffers.Add(storageBuffer);
    }

    public void DeclareImageBuffer(ImageBuffer imageBuffer) {
        var textureView = new RDTextureView();
        
        //TODO: check if works as expected
        var bufferDataEnumerable = new List<byte[]>();
        bufferDataEnumerable.Add(imageBuffer.Data);
        var bufferDataGDArray = new Godot.Collections.Array<byte[]>(bufferDataEnumerable);
        
        //TODO: fix this egregiousness
        if (imageBuffer.Data.IsEmpty()) {
            bufferDataGDArray = null;
        } 
        
        var imageBufferRid = _renderingDevice.TextureCreate(imageBuffer.TextureFormat, textureView, null);
        imageBuffer.Rid = imageBufferRid;
        
        var imageBufferUniform = new RDUniform();
        imageBufferUniform.UniformType = RenderingDevice.UniformType.Image;
        imageBufferUniform.Binding = (int)imageBuffer.Id;
        imageBufferUniform.AddId(imageBufferRid);
        
        imageBuffer.Uniform = imageBufferUniform;
        _computeBuffers.Add(imageBuffer);
    }

    public void FinalizeAndBindBuffers() {
        var buffersUniformSetArray = _computeBuffers.Select(x => x.Uniform).ToArray();
        var buffersUniformSetGDArray = new Godot.Collections.Array<RDUniform>(buffersUniformSetArray);
        
        _uniformSetRid = _renderingDevice.UniformSetCreate(buffersUniformSetGDArray, _computeShaderRid, 0);
    }

    public void FinalizePipeline() {
        _computePipelineRid = _renderingDevice.ComputePipelineCreate(_computeShaderRid);
    }

    public void PushBuffer(ComputeBufferId bufferId, byte[] byteStream) {
        var buffer = GetBuffer(bufferId);
        buffer.Data = byteStream;
        PushBufferInternal(buffer);
    }

    private void PushBufferInternal(ComputeBuffer buffer) {
        if (buffer is ImageBuffer imageBuffer) {
            _renderingDevice.TextureUpdate(imageBuffer.Rid, 0, imageBuffer.Data);
        } else if (buffer is StorageBuffer storageBuffer) {
            _renderingDevice.BufferUpdate(storageBuffer.Rid, 0, storageBuffer.BufferSize, storageBuffer.Data);
        }
    }

    public byte[] FetchBuffer(ComputeBufferId bufferId) {
        var buffer = GetBuffer(bufferId);
        var fetchedBufferData = buffer switch {
            ImageBuffer => _renderingDevice.TextureGetData(buffer.Rid, 0),
            StorageBuffer => _renderingDevice.BufferGetData(buffer.Rid),
            _ => []
        };
        buffer.Data = fetchedBufferData;
        return buffer.Data;
    }

    private ComputeBuffer GetBuffer(ComputeBufferId bufferId) {
        return _computeBuffers.FirstOrDefault(x => x.Id.Equals(bufferId));
    }

    public void Dispatch() {
        //init compute list
        var computeList = _renderingDevice.ComputeListBegin();
        
        //bind compute pipeline and uniform set
        _renderingDevice.ComputeListBindComputePipeline(computeList, _computePipelineRid);
        _renderingDevice.ComputeListBindUniformSet(computeList, _uniformSetRid, 0);
        
        _renderingDevice.ComputeListDispatch(computeList, (uint)_workGroupSize.X,(uint)_workGroupSize.Y,(uint)_workGroupSize.Z);
        _renderingDevice.ComputeListEnd();
        
        //submit the compute list to the GPU
        _renderingDevice.Submit();
    }
    
    public void Sync() {
        //TODO: this could potentially be done in the background and synced a few frames later; refactor & optimize
        //get the results back from the GPU
        _renderingDevice.Sync();
    }
}