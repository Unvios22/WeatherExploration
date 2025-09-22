using Godot;

namespace WeatherExploration.Source.WeatherSimulation.Logic;

public abstract class ComputeBuffer {
    public byte[] Data;
    public RDUniform Uniform;
    public Rid Rid;
    public ComputeBufferId Id;
    //TODO: refactor to be immutable (save for BufferData); same for inheriting types
}