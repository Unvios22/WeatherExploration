using Godot;

namespace WeatherExploration.Source.WeatherSimulation.Logic;

public abstract class ComputeBuffer {
    public byte[] BufferData;
    public RDUniform BufferUniform;
    public Rid BufferRid { get; private set; }
    public ComputeBufferId BufferId;

    public void SetBufferRID(Rid rid) {
        BufferRid = rid;
    }
}