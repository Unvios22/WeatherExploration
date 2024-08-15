using Godot;
using WeatherExploration.Source.Config;
using WeatherExploration.Source.WeatherSimulation.Model;

namespace WeatherExploration.Source.WeatherSimulation.Logic;

public class ShaderHandler {

    private SimulationSettings _simulationSettings;

    private RenderingDevice _renderingDevice;

    private readonly int _simulationRes;
    
    private Rid _computeShaderRid;
    private Rid _temperatureTexRid;
    private Rid _uniformSetRid;
    private Rid _pipelineRid;

    private Image _temperatureTexImage;
    
    public ShaderHandler(SimulationSettings simulationSettings) {
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
        var textureRes = _simulationSettings.TextureResolution;
        
        var temperatureTexFormat = new RDTextureFormat();
        temperatureTexFormat.Format = RenderingDevice.DataFormat.R8Unorm;
        temperatureTexFormat.Height = textureRes;
        temperatureTexFormat.Width = textureRes;
        temperatureTexFormat.UsageBits = RenderingDevice.TextureUsageBits.StorageBit |
                                  RenderingDevice.TextureUsageBits.CanUpdateBit |
                                  RenderingDevice.TextureUsageBits.CanCopyFromBit;

        var temperatureTexView = new RDTextureView();
        _temperatureTexRid = _renderingDevice.TextureCreate(temperatureTexFormat, temperatureTexView);

        var temperatureTexUniform = new RDUniform();
        temperatureTexUniform.UniformType = RenderingDevice.UniformType.Image;
        temperatureTexUniform.Binding = 0;
        
        temperatureTexUniform.AddId(_temperatureTexRid);

        _uniformSetRid = _renderingDevice.UniformSetCreate([temperatureTexUniform], _computeShaderRid, 0);
    }

    private void CreatePipeline() {
        _pipelineRid = _renderingDevice.ComputePipelineCreate(_computeShaderRid);
    }
    
    public Image Step(Image temperatureTexImage) {
       return StepComputeShader(temperatureTexImage);
    }

    private Image StepComputeShader(Image temperatureTex) {
        //update texture data
        _renderingDevice.TextureUpdate(_temperatureTexRid, 0, temperatureTex.GetData());

        //init compute list
        var computeList = _renderingDevice.ComputeListBegin();
        
        //bind compute pipeline and uniform set
        _renderingDevice.ComputeListBindComputePipeline(computeList, _pipelineRid);
        _renderingDevice.ComputeListBindUniformSet(computeList, _uniformSetRid, 0);
        
        //declare work group dispatch
        //TODO: setup proper inputs for the group sizes for optimal performance
        _renderingDevice.ComputeListDispatch(computeList, 1,1,1);
        _renderingDevice.ComputeListEnd();
        
        //submit the compute list to the GPU
        _renderingDevice.Submit();
        
        //TODO: this could potentially be done in the background and synced a few frames later; refactor & optimize
        //get the results back from the GPU
        _renderingDevice.Sync();

        //create and image from the resultant data
        var outputByteStream = _renderingDevice.TextureGetData(_temperatureTexRid, 0);
        var temperatureTexImage =
            Image.CreateFromData(_simulationRes, _simulationRes, false, Image.Format.R8, outputByteStream);
        
        return temperatureTexImage;
    }
}