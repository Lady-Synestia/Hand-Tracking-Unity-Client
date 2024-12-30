// hides websocket functionality from main module namespace
namespace HandTrackingModule.Websocket
{
    using System;
    using UnityEngine;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    // defining delegate to be called when data is received 
    public delegate void Del(string json);

    // defining custom exception for websocket listener
    class WebsocketNotActiveException : Exception
    {
        public override string Message { get; }
        public WebsocketNotActiveException() { Message = "Websocket not active"; }

    }

    /// <summary>
    /// Websocket Return Codes
    /// </summary>
    public enum WSRC
    {
        Success,
        Failure,
        SemaphoreFull,
        NotActive
    }

    class WebsocketListener
    {
        public Del DataReceivedDel;
        public bool IsActive;

        /// <summary>
        /// Semaphore code and docs
        /// solution >  https://stackoverflow.com/a/21163280
        /// SemaphoreSlim class > https://learn.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim?view=net-8.0
        /// </summary>
        // Semaphore is used to stop too many processes running in the thread pool
        // In this case there should only be one data receival running at once
        private static SemaphoreSlim _semaphore = new(1);

        /// <summary>
        /// websockets docs
        /// websocket support > https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/websockets
        /// ClientWebSocket class > https://learn.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocket?view=net-9.0
        /// </summary>
        private ClientWebSocket _ws = new();
        private Uri _uri = new("ws://localhost:8765");
        private byte[] _sendBuffer;
        private byte[] _receiveBuffer = new byte[4096];

        private string _jsonString;

        // common exception handling for all methods
        private static WSRC HandleException(Exception e)
        {
            switch (e)
            {
                case WebsocketNotActiveException:
                    return WSRC.NotActive;
                case SemaphoreFullException:
                    return WSRC.SemaphoreFull;
                default:
                    Debug.LogException(e);
                    return WSRC.Failure;
            }
        }

        /// <summary>
        /// Attempts to connect to the websocket
        /// </summary>
        /// <returns>request status code</returns>
        public async Task<WSRC> OpenSocket()
        {

            try
            {
                if (!IsActive)
                {
                    IsActive = true;
                    await _ws.ConnectAsync(_uri, default);
                }
            }
            catch (Exception e)
            {
                IsActive = false;
                return HandleException(e);
            }
            return WSRC.Success;
        }

        /// <summary>
        /// Attempts to send message over websocket
        /// </summary>
        /// <param name="message"></param>
        /// <returns>request status code</returns>
        public async Task<WSRC> SendModeRequest(string handshakeCode)
        {
            try
            {
                if (!IsActive) { throw new WebsocketNotActiveException(); }

                _sendBuffer = Encoding.UTF8.GetBytes(handshakeCode);
                await _ws.SendAsync(_sendBuffer, WebSocketMessageType.Text, true, default);

                // receiving handshake confirmation
                var result = await _ws.ReceiveAsync(_receiveBuffer, default);
                string echo = Encoding.UTF8.GetString(_receiveBuffer, 0, result.Count);
                if (echo == handshakeCode)
                {
                    Debug.Log($"Handshake successful: {echo}");
                }
                else
                {
                    // throw new Exception("Handshake failed");
                }
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
            return WSRC.Success;
        }

        /// <summary>
        /// Attempts to wait for data to be received from the websocket
        /// </summary>
        /// <returns>task status code</returns>
        public async Task<WSRC> ReceiveData()
        {
            try
            {
                // semaphore timeout is set to 0
                // prevents receive requests from backing up
                if (await _semaphore.WaitAsync(0))
                {
                    try
                    {
                        if (!IsActive) { throw new WebsocketNotActiveException(); }

                        var result = await _ws.ReceiveAsync(_receiveBuffer, default);
                        _jsonString = Encoding.UTF8.GetString(_receiveBuffer, 0, result.Count);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                    // delegate call for HandTrackingSystem
                    DataReceivedDel(_jsonString);
                }
                else
                {
                    throw new SemaphoreFullException();
                }
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
            return WSRC.Success;
        }

        /// <summary>
        /// Attempts to close the active connection
        /// </summary>
        /// <returns></returns>
        public async Task<WSRC> CloseSocket()
        {
            try
            {
                if (!IsActive) { throw new WebsocketNotActiveException(); }

                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", default);
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
            return WSRC.Success;
        }
    }
}