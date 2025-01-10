namespace WeatherExploration.Source.WeatherSimulation.Logic;

public interface IComputePipelineHandler {
    public void PushBuffer(ComputeBufferId bufferId, byte[] byteStream);
    public byte[] FetchBuffer(ComputeBufferId bufferId);
    public void Dispatch();
    public void Sync();
}