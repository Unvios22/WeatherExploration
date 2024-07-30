using System;

namespace WeatherExploration.Source.Exceptions;

public class NoRegisteredSignalListenersException : Exception {
    public NoRegisteredSignalListenersException(){}
    public NoRegisteredSignalListenersException(Type signalType) : base("No registered listeners found for Signal of Type: " + signalType){}
}