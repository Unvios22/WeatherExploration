using System;

namespace WeatherExploration.Source.Signals;

public class SignalBus<TBaseSignal> : IDisposable {
    
    public SignalBus() {
        
        //TODO
    }

    public Delegate RegisterListener<TSignal>(Delegate callback) where TSignal : TBaseSignal {
        //TODO
        throw new NotImplementedException();
    }
    
    public Delegate RemoveListener<TSignal>(Delegate callback) where TSignal : TBaseSignal {
        //TODO
        throw new NotImplementedException();
    }

    public void FireSignal<TSignal>(TSignal Signal) where TSignal : TBaseSignal {
        //TODO
        throw new NotImplementedException();
    }
    
    public void Dispose() {
        // TODO release managed resources here
    }
}