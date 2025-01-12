using System;
namespace HandTrackingModule.Websocket
{
    // custom exception for websocket listener
    internal class WebsocketNotActiveException : Exception
    {
        public override string Message { get; }
        public WebsocketNotActiveException() { Message = "Websocket not active"; }
    }

}