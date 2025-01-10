using System;
using Godot;
using WeatherExploration.Source.Config;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Logic;

public class SimulationComputeHandler {

    private SimulationSettings _simulationSettings;
    private readonly int _simulationRes;
    
    private IComputePipelineHandler _computePipeline;

    //update to be current with the invocation set in actual compute shader (defines amount of dispatched workgroups here)
    //TODO: also - it seems that the texture resolution now has to be a multiple of 8 for this to work correctly?
    private const int ShaderWorkgroupInvocationSize = 8;
    
    public SimulationComputeHandler(SimulationSettings simulationSettings) {
        _simulationSettings = simulationSettings;
        _simulationRes = (int)_simulationSettings.TextureResolution;

        SetupComputePipeline();
    }

    private void SetupComputePipeline() {
        var computeShader = LoadComputeShader();
        
        var groupCount = _simulationRes / ShaderWorkgroupInvocationSize;
        
        var computePipelineBuilder = new ComputePipelineBuilder();

        var constantsArray = new[] { _simulationRes };
        
        var pressureTexFormat = new RDTextureFormat();
        pressureTexFormat.Format = RenderingDevice.DataFormat.R8Unorm;
        pressureTexFormat.Height = _simulationSettings.TextureResolution;
        pressureTexFormat.Width = _simulationSettings.TextureResolution;
        pressureTexFormat.UsageBits = RenderingDevice.TextureUsageBits.StorageBit |
                                      RenderingDevice.TextureUsageBits.CanUpdateBit |
                                      RenderingDevice.TextureUsageBits.CanCopyFromBit;
        
        var pressureGradientBufferByteCount = _simulationRes * _simulationRes * sizeof(float) * 4;
        
        _computePipeline = computePipelineBuilder
            .BeginPipeline()
            .SetComputeShader(computeShader)
            .SetWorkGroupSize(new Vector3I(groupCount, groupCount, 1))
            .BindStorageBuffer(ComputeBufferId.Constants, (uint)constantsArray.Length * sizeof(uint), BitConverter.GetBytes(constantsArray[0]))
            //note buffer data is empty here
            .BindImageBuffer(ComputeBufferId.PressureTex, pressureTexFormat, null)
            //note buffer data is empty here
            .BindStorageBuffer(ComputeBufferId.PressureGradient, (uint)pressureGradientBufferByteCount, null)
            .FinalizeUniformSet()
            .FinalizePipeline();
    }

    private RDShaderFile LoadComputeShader() {
        return GD.Load<RDShaderFile>(Shaders.SIMULATION_COMPUTE_SHADER);
    }
    
    public WeatherState StepSim(WeatherState weatherState) { 
        DispatchShaderUpdate(weatherState);
        
        //TODO: refactor this workflow into async/await
        //also - this library looks cool: https://github.com/Fractural/GDTask/tree/main
        _computePipeline.Dispatch();
        _computePipeline.Sync();
        
        return RetrieveShaderData(weatherState);
    }

    private void DispatchShaderUpdate(WeatherState weatherState) {
        //update shader data
        var pressureTexByteStream = weatherState.PressureTex.GetData();
        _computePipeline.PushBuffer(ComputeBufferId.PressureTex, pressureTexByteStream);
    }
    
    private WeatherState RetrieveShaderData(WeatherState weatherState) {
        //update the model with retrieved data
        var resultPressureGradientStream = _computePipeline.FetchBuffer(ComputeBufferId.PressureGradient);
        weatherState.PressureGradient.SetData(resultPressureGradientStream);
        
        return weatherState;
    }
}