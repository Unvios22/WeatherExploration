using System;

namespace WeatherExploration.Source.Exceptions;

public class NoSignalListenersException : Exception {
    public NoSignalListenersException(){}
    public NoSignalListenersException(Type signalType) : base("No registered listeners found for Signal of Type: " + signalType){}
}