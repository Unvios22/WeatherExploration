using System.Collections.Generic;
using Godot;
using WeatherExploration.Source.Exceptions;

namespace WeatherExploration.Source.Signals;

public class AbstractSignalBus<TBaseSignal> {
    
    //TODO & PPU: rudimentary implementation; refactor & optimize 
    
    public delegate void SignalReceiver<TSignal>(TSignal signalData) where TSignal : TBaseSignal;
    
    public void RegisterListener<TSignal>(SignalReceiver<TSignal> callback) where TSignal : TBaseSignal {
        SubscriptionsHandler<TSignal>.RegisterListener(callback);
    }
    
    public void RemoveListener<TSignal>(SignalReceiver<TSignal> callbackToRemove) where TSignal : TBaseSignal {
        SubscriptionsHandler<TSignal>.RemoveListener(callbackToRemove);
    }

    public void FireSignal<TSignal>(TSignal signal) where TSignal : TBaseSignal {
        SubscriptionsHandler<TSignal>.FireSignal(signal);
    }

    //In C# static generic classes are generated at compile-time for each possible type
    //this effectively creates a Dict<Type,...> -like structure for no additional cost (prob actually better optimized)
    private static class SubscriptionsHandler<TSignal> where TSignal : TBaseSignal {
        
        private static readonly List<SignalReceiver<TSignal>> SignalListeners = new List<SignalReceiver<TSignal>>();

        public static void RegisterListener(SignalReceiver<TSignal> callback) {
            SignalListeners.Add(callback);
        }

        public static void RemoveListener(SignalReceiver<TSignal> callbackToRemove) {
            //TODO: make sure this is enough for the handler to be collected and not cause a memory leak
            SignalListeners.RemoveAll(x => x.Equals(callbackToRemove));
        }

        public static void FireSignal(TSignal signal) {
            if (SignalListeners.Count == 0) {
                GD.PushWarning("SignalBus received sent signal of type for which there are no registered listeners: " + typeof(TSignal));
            }
            foreach (var listener in SignalListeners) {
                listener.Invoke(signal);
            }
        }
    }
}