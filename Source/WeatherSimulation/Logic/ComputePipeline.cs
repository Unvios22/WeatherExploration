using System.Collections.Generic;
using System.Linq;
using Godot;

namespace WeatherExploration.Source.WeatherSimulation.Logic;

public class FinalizedComputePipeline : IFinalizedComputePipeline{
    private RenderingDevice _renderingDevice;
    private Rid _computeShaderRid;
    private Rid _computePipelineRid;
    private Rid _uniformSetRid;
    private List<ComputeBuffer> _computeBuffers;

    private Vector3I _workGroupSize;
    
    public FinalizedComputePipeline() {
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
        //TODO: create an uniform set; assign it to buffer fields; bind the uniform set to RD list; store buffer in refs
        var bufferRid = _renderingDevice.StorageBufferCreate(storageBuffer.BufferSize, storageBuffer.BufferData);
        storageBuffer.SetBufferRID(bufferRid);

        var bufferUniform = new RDUniform();
        bufferUniform.UniformType = RenderingDevice.UniformType.StorageBuffer;
        bufferUniform.Binding = (int)storageBuffer.BufferId;
        bufferUniform.AddId(bufferRid);
        
        storageBuffer.BufferUniform = bufferUniform;
        _computeBuffers.Add(storageBuffer);
    }

    public void DeclareImageBuffer(ImageBuffer imageBuffer) {
        var textureView = new RDTextureView();
        
        //TODO: check if works as expected
        var bufferDataEnumerable = new List<byte[]>();
        bufferDataEnumerable.Add(imageBuffer.BufferData);
        var bufferDataGDArray = new Godot.Collections.Array<byte[]>(bufferDataEnumerable);
        
        //TODO: fix this egregiousness
        if (imageBuffer.BufferData.IsEmpty()) {
            bufferDataGDArray = null;
        } 
        
        var imageBufferRid = _renderingDevice.TextureCreate(imageBuffer.TextureFormat, textureView, null);
        imageBuffer.SetBufferRID(imageBufferRid);
        
        var imageBufferUniform = new RDUniform();
        imageBufferUniform.UniformType = RenderingDevice.UniformType.Image;
        imageBufferUniform.Binding = (int)imageBuffer.BufferId;
        imageBufferUniform.AddId(imageBufferRid);
        
        imageBuffer.BufferUniform = imageBufferUniform;
        _computeBuffers.Add(imageBuffer);
    }

    public void FinalizeAndBindBuffers() {
        var buffersUniformSetArray = _computeBuffers.Select(x => x.BufferUniform).ToArray();
        var buffersUniformSetGDArray = new Godot.Collections.Array<RDUniform>(buffersUniformSetArray);
        
        _uniformSetRid = _renderingDevice.UniformSetCreate(buffersUniformSetGDArray, _computeShaderRid, 0);
    }

    public void FinalizePipeline() {
        _computePipelineRid = _renderingDevice.ComputePipelineCreate(_computeShaderRid);
    }

    public void PushBuffer(ComputeBufferId bufferId, byte[] byteStream) {
        var buffer = GetBuffer(bufferId);
        buffer.BufferData = byteStream;
        PushBufferInternal(buffer);
    }

    private void PushBufferInternal(ComputeBuffer buffer) {
        if (buffer is ImageBuffer imageBuffer) {
            _renderingDevice.TextureUpdate(imageBuffer.BufferRid, 0, imageBuffer.BufferData);
        } else if (buffer is StorageBuffer storageBuffer) {
            _renderingDevice.BufferUpdate(storageBuffer.BufferRid, 0, storageBuffer.BufferSize, storageBuffer.BufferData);
        }
    }

    public byte[] FetchBuffer(ComputeBufferId bufferId) {
        var buffer = GetBuffer(bufferId);
        var fetchedBufferData = _renderingDevice.BufferGetData(buffer.BufferRid);
        buffer.BufferData = fetchedBufferData;
        return buffer.BufferData;
    }

    private ComputeBuffer GetBuffer(ComputeBufferId bufferId) {
        return _computeBuffers.FirstOrDefault(x => x.BufferId.Equals(bufferId));
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