using System;
using System.Runtime.InteropServices;
using Godot;
using WeatherExploration.Source.Config;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Logic;

public class ShaderHandler {

    private SimulationSettings _simulationSettings;

    private RenderingDevice _renderingDevice;

    private readonly int _simulationRes;
    
    private Rid _computeShaderRid;
    private Rid _inputWindDataBufferRid;
    private Rid _resultWindDataBufferRid;
    private Rid _uniformSetRid;
    private Rid _pipelineRid;

    private Vector4[] _windData;

    //update to be current with the invocation set in actual compute shader (defines amount of dispatched workgroups here)
    //TODO: also - it seems that the texture resolution now has to be a multiple of 8 for this to work corectly?
    private const int ShaderWorkgroupInvocationSize = 8;
    
    public ShaderHandler(SimulationSettings simulationSettings, Vector4[] windData) {
        _simulationSettings = simulationSettings;
        _simulationRes = (int)_simulationSettings.TextureResolution;
        
        _windData = windData;
        
        InitRenderingDevice();
        LoadShader();
        DeclareUniformSet();
        CreatePipeline();
    }

    private void InitRenderingDevice() {
        _renderingDevice = RenderingServer.CreateLocalRenderingDevice();
    }

    private void LoadShader() {
        var shaderFile = GD.Load<RDShaderFile>(Shaders.COMPUTE_SHADER);

        var shaderBytecode = shaderFile.GetSpirV();
        _computeShaderRid = _renderingDevice.ShaderCreateFromSpirV(shaderBytecode);
    }
    
    private void DeclareUniformSet() {

        var windDataByteStream = GetWindDataAsByteStream(_windData);
        _inputWindDataBufferRid = _renderingDevice.StorageBufferCreate((uint)windDataByteStream.Length, windDataByteStream);

        var inputWindDataUniform = new RDUniform();
        inputWindDataUniform.UniformType = RenderingDevice.UniformType.StorageBuffer;
        inputWindDataUniform.Binding = 0;
        inputWindDataUniform.AddId(_inputWindDataBufferRid);
        
        _resultWindDataBufferRid = _renderingDevice.StorageBufferCreate((uint)windDataByteStream.Length);
        
        var resultWindDataUniform = new RDUniform();
        resultWindDataUniform.UniformType = RenderingDevice.UniformType.StorageBuffer;
        resultWindDataUniform.Binding = 1;
        resultWindDataUniform.AddId(_resultWindDataBufferRid);
        
        _uniformSetRid = _renderingDevice.UniformSetCreate([inputWindDataUniform, resultWindDataUniform], _computeShaderRid, 0);
    }
    
    private void CreatePipeline() {
        _pipelineRid = _renderingDevice.ComputePipelineCreate(_computeShaderRid);
    }
    
    public Vector4[] Step() {
       return StepComputeShader();
    }

    private Vector4[] StepComputeShader() {
        
        //update shader data
        var windDataByteStream = GetWindDataAsByteStream(_windData);
        _renderingDevice.BufferUpdate(_inputWindDataBufferRid, 0, (uint)windDataByteStream.Length, windDataByteStream);

        //init compute list
        var computeList = _renderingDevice.ComputeListBegin();
        
        //bind compute pipeline and uniform set
        _renderingDevice.ComputeListBindComputePipeline(computeList, _pipelineRid);
        _renderingDevice.ComputeListBindUniformSet(computeList, _uniformSetRid, 0);
        
        //declare work group dispatch
        var groupCount = (uint) _simulationRes / ShaderWorkgroupInvocationSize;
        _renderingDevice.ComputeListDispatch(computeList, groupCount,groupCount,1);
        _renderingDevice.ComputeListEnd();
        
        //submit the compute list to the GPU
        _renderingDevice.Submit();
        
        //TODO: this could potentially be done in the background and synced a few frames later; refactor & optimize
        //get the results back from the GPU
        _renderingDevice.Sync();

        //create and image from the resultant data
        var outputByteStream = _renderingDevice.BufferGetData(_resultWindDataBufferRid);
        _windData = GetWindDataFromByteStream(outputByteStream);
        return _windData;
    }

    private byte[] GetWindDataAsByteStream(Vector4[] windData) {
        //each Vector4 has 4 floats
        var windMapInputBytes = new byte[windData.Length * sizeof(float) * 4];
        
        //can't use Buffer.BlockCopy, because an array of Vector4 structs (each consisting of floats only) definitely doesn't contain primitives by c#'s standards
        //as per https://stackoverflow.com/questions/33181945/blockcopy-a-class-getting-object-must-be-an-array-of-primitives
        //life is pain

        var byteIndex = 0L;
        for (var i = 0; i < windData.Length; i++) {
            var floatBytes = BitConverter.GetBytes(windData[i].X);
            Array.Copy(floatBytes, 0, windMapInputBytes,byteIndex, sizeof(float));
            byteIndex+= sizeof(float);
            
            floatBytes = BitConverter.GetBytes(windData[i].Y);
            Array.Copy(floatBytes, 0, windMapInputBytes,byteIndex, sizeof(float));
            byteIndex+= sizeof(float);
            
            floatBytes = BitConverter.GetBytes(windData[i].Z);
            Array.Copy(floatBytes, 0, windMapInputBytes,byteIndex, sizeof(float));
            byteIndex+= sizeof(float);
            
            floatBytes = BitConverter.GetBytes(windData[i].W);
            Array.Copy(floatBytes, 0, windMapInputBytes,byteIndex, sizeof(float));
            byteIndex+= sizeof(float);
        }
        return windMapInputBytes;
    }
    
    private Vector4[] GetWindDataFromByteStream(byte[] byteStream) {
        const int sizeOfFloat = sizeof(float);
        //each vector4 has 4 floats, each float has sizeof(float) bytes
        var convertedWindData = new Vector4[byteStream.Length / (sizeOfFloat * 4)];
        for (var i = 0; i < convertedWindData.Length; i++) {
            var thisVector4InitialFloatIndex = i * sizeOfFloat * 4;
            
            var x = BitConverter.ToSingle(byteStream, thisVector4InitialFloatIndex);
            var y = BitConverter.ToSingle(byteStream, thisVector4InitialFloatIndex + sizeOfFloat);
            var z = BitConverter.ToSingle(byteStream, thisVector4InitialFloatIndex + sizeOfFloat * 2);
            var w = BitConverter.ToSingle(byteStream, thisVector4InitialFloatIndex + sizeOfFloat * 3);
            
            convertedWindData[i] = new Vector4(x, y, z, w);
        }
        return convertedWindData;
    }
}