using System;
using Godot;
using WeatherExploration.Source.Config;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Logic;

public class SimulationCSHandler {

    private SimulationSettings _simulationSettings;

    private RenderingDevice _renderingDevice;

    private readonly int _simulationRes;
    
    //TODO: add some abstraction to simplify (both by boilerplate code and by ease of use) the creation and referencing of RID's & buffers
    
    private Rid _computeShaderRid;
    private Rid _constantsBufferRid;
    private Rid _inputPressureGradientBufferRid;
    private Rid _resultPressureGradientBufferRid;
    private Rid _uniformSetRid;
    private Rid _pipelineRid;

    //update to be current with the invocation set in actual compute shader (defines amount of dispatched workgroups here)
    //TODO: also - it seems that the texture resolution now has to be a multiple of 8 for this to work corectly?
    private const int ShaderWorkgroupInvocationSize = 8;
    
    public SimulationCSHandler(SimulationSettings simulationSettings) {
        _simulationSettings = simulationSettings;
        _simulationRes = (int)_simulationSettings.TextureResolution;
        
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
        
        //TODO: standardize used data formats - e.g. some dedicated struct and parser, etc.
        var constants = new[] { _simulationRes };
        var constantsBufferByteCount = constants.Length * sizeof(uint);
        var constantsByteStream = BitConverter.GetBytes(constants[0]);
        _constantsBufferRid = _renderingDevice.StorageBufferCreate((uint)constantsBufferByteCount, constantsByteStream);
        
        var constantsUniform = new RDUniform();
        constantsUniform.UniformType  = RenderingDevice.UniformType.StorageBuffer;
        constantsUniform.Binding = 0;
        constantsUniform.AddId(_constantsBufferRid);
        
        //TODO: check whether the gradient can be set up to be populated in-shader for first run, instead of arbitrarily on CPU-side=
        var pressureGradientBufferByteCount = _simulationRes * _simulationRes * sizeof(float) * 4;
        _inputPressureGradientBufferRid = _renderingDevice.StorageBufferCreate((uint)pressureGradientBufferByteCount);

        var inputWindDataUniform = new RDUniform();
        inputWindDataUniform.UniformType = RenderingDevice.UniformType.StorageBuffer;
        inputWindDataUniform.Binding = 1;
        inputWindDataUniform.AddId(_inputPressureGradientBufferRid);
        
        _resultPressureGradientBufferRid = _renderingDevice.StorageBufferCreate((uint)pressureGradientBufferByteCount);
        
        var resultWindDataUniform = new RDUniform();
        resultWindDataUniform.UniformType = RenderingDevice.UniformType.StorageBuffer;
        resultWindDataUniform.Binding = 2;
        resultWindDataUniform.AddId(_resultPressureGradientBufferRid);
        
        _uniformSetRid = _renderingDevice.UniformSetCreate([constantsUniform, inputWindDataUniform, resultWindDataUniform], _computeShaderRid, 0);
    }
    
    private void CreatePipeline() {
        _pipelineRid = _renderingDevice.ComputePipelineCreate(_computeShaderRid);
    }
    
    public WeatherState Step(WeatherState weatherState) { 
        DispatchShaderUpdate(weatherState);
        //TODO: refactor this workflow into async/await
        //also - this library looks cool: https://github.com/Fractural/GDTask/tree/main
        return RetrieveShaderData(weatherState);
    }

    private void DispatchShaderUpdate(WeatherState weatherState) {
        //update shader data
        var pressureGradientByteStream = weatherState.PressureGradient.GetDataAsByteStream();
        _renderingDevice.BufferUpdate(_inputPressureGradientBufferRid, 0, (uint)pressureGradientByteStream.Length, pressureGradientByteStream);

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
    }
    
    private WeatherState RetrieveShaderData(WeatherState weatherState) {
        //TODO: this could potentially be done in the background and synced a few frames later; refactor & optimize
        //get the results back from the GPU
        _renderingDevice.Sync();
        
        //update the model with retrieved data
        var resultPressureGradientByteStream = _renderingDevice.BufferGetData(_resultPressureGradientBufferRid);
        weatherState.PressureGradient.SetData(resultPressureGradientByteStream);
        
        return weatherState;
    }
}