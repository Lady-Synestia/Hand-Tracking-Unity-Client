using System;
namespace HandTrackingModule.WebSocket
{
    // custom exception for websocket listener
    internal class WebSocketNotActiveException : Exception
    {
        public override string Message { get; }
        public WebSocketNotActiveException() { Message = "Websocket not active"; }
    }

}