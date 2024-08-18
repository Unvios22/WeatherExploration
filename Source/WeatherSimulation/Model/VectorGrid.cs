using System;
using Godot;

namespace WeatherExploration.Source.WeatherSimulation.Model;

public class VectorGrid {
    private byte[] _data;
    private Vector4[] _1DGridArray;
    private Vector4[,] _2DGridArray;

    //the resolution is assumed to be immutable past the creation of the VectorGrid
    private uint _resolution;
    
    //TODO: this class can be refactored to do with more readable code and less repetition, probably
    //also overriding [] and [,] operators from the base array type might be neat syntax sugar

    public VectorGrid(byte[] gridAsByteStream, uint resolution) {
        _data = gridAsByteStream;
        _resolution = resolution;
    }

    public VectorGrid(Vector4[] gridAs1DArray, uint resolution) {
        _1DGridArray = gridAs1DArray;
        _resolution = resolution;
    }

    public VectorGrid(Vector4[,] gridAs2DArray, uint resolution) {
        _2DGridArray = gridAs2DArray;
        _resolution = resolution;
    }

    public void SetData(byte[] gridAsByteStream) {
        _data = gridAsByteStream;
        _1DGridArray = null;
        _2DGridArray = null;
    }

    public void SetData(Vector4[] gridAs1DArray) {
        _1DGridArray = gridAs1DArray;
        _data = null;
        _2DGridArray = null;
    }

    public void SetData(Vector4[,] gridAs2DArray) {
        _2DGridArray = gridAs2DArray;
        _data = null;
        _1DGridArray = null;
    }

    public byte[] GetDataAsByteStream() {
        if (_data is not null) {
            return _data;
        }
        if (_1DGridArray is not null) {
            _data = Parse1DVectorArrayToByteStream(_1DGridArray);
            return _data;
        }
        if (_2DGridArray is not null) {
            _1DGridArray = Parse2DVectorArrayTo1DArray(_2DGridArray, _resolution);
            _data = Parse1DVectorArrayToByteStream(_1DGridArray);
            return _data;
        }
        throw new ArgumentException("No VectorGridDataFound");
    }

    public Vector4[] GetDataAs1DArray() {
        if (_1DGridArray is not null) {
            return _1DGridArray;
        }
        if (_data is not null) {
            _1DGridArray = ParseByteStreamTo1DVectorArray(_data);
            return _1DGridArray;
        }
        if (_2DGridArray is not null) {
            _1DGridArray = Parse2DVectorArrayTo1DArray(_2DGridArray, _resolution);
            return _1DGridArray;
        }
        throw new ArgumentException("No VectorGridDataFound");
    }

    public Vector4[,] GetDataAs2DArray() {
        if (_2DGridArray is not null) {
            return _2DGridArray;
        }
        if (_1DGridArray is not null) {
            _2DGridArray = Parse1DVectorArrayTo2DArray(_1DGridArray, _resolution);
            return _2DGridArray;
        }
        if (_data is not null) {
            _1DGridArray = ParseByteStreamTo1DVectorArray(_data);
            _2DGridArray = Parse1DVectorArrayTo2DArray(_1DGridArray, _resolution);
            return _2DGridArray;
        }
        throw new ArgumentException("No VectorGridDataFound");
    }

    private static Vector4[,] Parse1DVectorArrayTo2DArray(Vector4[] array1D, uint resolution) {
        var result2DArray = new Vector4[resolution, resolution];
        for (var x = 0; x < resolution; x++) {
            for (var y = 0; y < resolution; y++) {
                var index = x * resolution + y;
                result2DArray[x, y] = array1D[index];
            }
        }
        return result2DArray;
    }
    
     private static Vector4[] Parse2DVectorArrayTo1DArray(Vector4[,] array2D, uint resolution) {
        var result1DArray = new Vector4[resolution * resolution];
        for (var x = 0; x < resolution; x++) {
            for (var y = 0; y < resolution; y++) {
                var index = x * resolution + y;
                result1DArray[index] = array2D[x, y];
            }
        }
        return result1DArray;
    }
    
    private static Vector4[] ParseByteStreamTo1DVectorArray(byte[] byteStream) {
        const int sizeOfFloat = sizeof(float);
        //each vector4 has 4 floats, each float has sizeof(float) bytes
        var convertedDataArray = new Vector4[byteStream.Length / (sizeOfFloat * 4)];
        for (var i = 0; i < convertedDataArray.Length; i++) {
            var thisVector4InitialFloatIndex = i * sizeOfFloat * 4;
            
            var x = BitConverter.ToSingle(byteStream, thisVector4InitialFloatIndex);
            var y = BitConverter.ToSingle(byteStream, thisVector4InitialFloatIndex + sizeOfFloat);
            var z = BitConverter.ToSingle(byteStream, thisVector4InitialFloatIndex + sizeOfFloat * 2);
            var w = BitConverter.ToSingle(byteStream, thisVector4InitialFloatIndex + sizeOfFloat * 3);
            
            convertedDataArray[i] = new Vector4(x, y, z, w);
        }
        return convertedDataArray;
    }
    
    private static byte[] Parse1DVectorArrayToByteStream(Vector4[] array) {
        //each Vector4 has 4 floats
        var arrayAsBytes = new byte[array.Length * sizeof(float) * 4];
        
        //can't use Buffer.BlockCopy, because an array of Vector4 structs (each consisting of floats only) definitely doesn't contain primitives by c#'s standards
        //as per https://stackoverflow.com/questions/33181945/blockcopy-a-class-getting-object-must-be-an-array-of-primitives
        //life is pain

        var byteIndex = 0L;
        for (var i = 0; i < array.Length; i++) {
            var floatBytes = BitConverter.GetBytes(array[i].X);
            Array.Copy(floatBytes, 0, arrayAsBytes,byteIndex, sizeof(float));
            byteIndex+= sizeof(float);
            
            floatBytes = BitConverter.GetBytes(array[i].Y);
            Array.Copy(floatBytes, 0, arrayAsBytes,byteIndex, sizeof(float));
            byteIndex+= sizeof(float);
            
            floatBytes = BitConverter.GetBytes(array[i].Z);
            Array.Copy(floatBytes, 0, arrayAsBytes,byteIndex, sizeof(float));
            byteIndex+= sizeof(float);
            
            floatBytes = BitConverter.GetBytes(array[i].W);
            Array.Copy(floatBytes, 0, arrayAsBytes,byteIndex, sizeof(float));
            byteIndex+= sizeof(float);
        }
        return arrayAsBytes;
    }
}