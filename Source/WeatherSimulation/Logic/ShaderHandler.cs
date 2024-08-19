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

    private VectorGrid _pressureGradient;

    //update to be current with the invocation set in actual compute shader (defines amount of dispatched workgroups here)
    //TODO: also - it seems that the texture resolution now has to be a multiple of 8 for this to work corectly?
    private const int ShaderWorkgroupInvocationSize = 8;
    
    public ShaderHandler(SimulationSettings simulationSettings, VectorGrid pressureGradient) {
        _simulationSettings = simulationSettings;
        _simulationRes = (int)_simulationSettings.TextureResolution;
        
        _pressureGradient = pressureGradient;
        
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
        var pressureGradientByteStream = _pressureGradient.GetDataAsByteStream();
        _inputWindDataBufferRid = _renderingDevice.StorageBufferCreate((uint)pressureGradientByteStream.Length, pressureGradientByteStream);

        var inputWindDataUniform = new RDUniform();
        inputWindDataUniform.UniformType = RenderingDevice.UniformType.StorageBuffer;
        inputWindDataUniform.Binding = 0;
        inputWindDataUniform.AddId(_inputWindDataBufferRid);
        
        _resultWindDataBufferRid = _renderingDevice.StorageBufferCreate((uint)pressureGradientByteStream.Length);
        
        var resultWindDataUniform = new RDUniform();
        resultWindDataUniform.UniformType = RenderingDevice.UniformType.StorageBuffer;
        resultWindDataUniform.Binding = 1;
        resultWindDataUniform.AddId(_resultWindDataBufferRid);
        
        _uniformSetRid = _renderingDevice.UniformSetCreate([inputWindDataUniform, resultWindDataUniform], _computeShaderRid, 0);
    }
    
    private void CreatePipeline() {
        _pipelineRid = _renderingDevice.ComputePipelineCreate(_computeShaderRid);
    }
    
    public VectorGrid Step() {
       return StepComputeShader();
    }

    private VectorGrid StepComputeShader() {
        
        //update shader data
        var windDataByteStream = _pressureGradient.GetDataAsByteStream();
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
        _pressureGradient.SetData(outputByteStream);
        return _pressureGradient;
    }
}