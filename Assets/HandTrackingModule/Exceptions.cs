using System;

namespace HandTrackingModule
{
    /// <summary>
    /// Custom exception for backend API data requests.
    /// Thrown if the data type requested has not been received from the Backend API
    /// </summary>
    internal class ReceiveTypeException : Exception
    {
        public override string Message { get; }
        public ReceiveTypeException(ReceiveType type) { Message = $"Not receiving data of type: {type}"; }
    }
}