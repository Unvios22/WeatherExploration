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
    private Rid _temperatureTexOutRid;
    private Rid _uniformSetRid;
    private Rid _pipelineRid;

    private Image _temperatureTexImage;

    //update to be current with the invocation set in actual compute shader (defines amount of dispatched workgroups here)
    //TODO: also - it seems that the texture resolution now has to be a multiple of 8 for this to work corectly?
    private const int ShaderWorkgroupInvocationSize = 8;
    
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
        
        var temperatureTexOutFormat = new RDTextureFormat();
        temperatureTexOutFormat.Format = RenderingDevice.DataFormat.R8Unorm;
        temperatureTexOutFormat.Height = textureRes;
        temperatureTexOutFormat.Width = textureRes;
        temperatureTexOutFormat.UsageBits = RenderingDevice.TextureUsageBits.StorageBit |
                                            RenderingDevice.TextureUsageBits.CanUpdateBit |
                                            RenderingDevice.TextureUsageBits.CanCopyFromBit |
                                            RenderingDevice.TextureUsageBits.CanCopyToBit;

        var temperatureTexOutView = new RDTextureView();
        _temperatureTexOutRid = _renderingDevice.TextureCreate(temperatureTexOutFormat, temperatureTexOutView);

        var temperatureTexOutUniform = new RDUniform();
        temperatureTexOutUniform.UniformType = RenderingDevice.UniformType.Image;
        temperatureTexOutUniform.Binding = 1;
        
        temperatureTexOutUniform.AddId(_temperatureTexOutRid);
        
        _uniformSetRid = _renderingDevice.UniformSetCreate([temperatureTexUniform, temperatureTexOutUniform], _computeShaderRid, 0);
    }

    private void CreatePipeline() {
        _pipelineRid = _renderingDevice.ComputePipelineCreate(_computeShaderRid);
    }
    
    public Image Step(Image temperatureTexImage) {
       return StepComputeShader(temperatureTexImage);
    }

    private Image StepComputeShader(Image temperatureTex) {
        //update texture data
        //TODO: these have mipmaps, actually, but setting them by setMipmaps(0) in the decalrations above causes errors here?
        //I assumed that having mipmaps is always suboptimal in this case, because I don't need any interpolated values, but raw data only
        //but perhaps the base texture instance is also treated as a mipmap in and of itself?
        _renderingDevice.TextureUpdate(_temperatureTexRid, 0, temperatureTex.GetData());
        _renderingDevice.TextureClear(_temperatureTexOutRid, Colors.Black, 0, 1, 0, 1);

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
        var outputByteStream = _renderingDevice.TextureGetData(_temperatureTexOutRid, 0);
        var temperatureTexImage =
            Image.CreateFromData(_simulationRes, _simulationRes, false, Image.Format.R8, outputByteStream);
        
        return temperatureTexImage;
    }
}