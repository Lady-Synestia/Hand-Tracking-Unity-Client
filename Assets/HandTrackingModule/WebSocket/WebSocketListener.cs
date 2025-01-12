using System;
using UnityEngine;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// hides websocket functionality from main module namespace
namespace HandTrackingModule.WebSocket
{

    // defining delegate to be called when data is received 
    public delegate void Del(string json);
    
    public class WebSocketListener
    {
        public Del DataReceivedDel;
        public bool IsActive;

        /// <summary>
        /// Semaphore code and docs: <br/>
        /// solution >  https://stackoverflow.com/a/21163280 <br/>
        /// SemaphoreSlim class > https://learn.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim?view=net-8.0
        /// </summary>
        // Semaphore is used to stop too many processes running in the thread pool
        // In this case there should only be one data receive running at once
        private readonly SemaphoreSlim _semaphore = new(1);

        /// <summary>
        /// websockets docs: <br/>
        /// websocket support > https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/websockets <br/>
        /// ClientWebSocket class > https://learn.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocket?view=net-9.0
        /// </summary>
        private readonly ClientWebSocket _ws = new();
        private readonly Uri _uri = new("ws://localhost:8765");
        private byte[] _sendBuffer;
        private readonly byte[] _receiveBuffer = new byte[4096];

        private string _jsonString;

        // common exception handling for all methods
        private static Wsrc HandleException(Exception e)
        {
            switch (e)
            {
                case WebSocketNotActiveException:
                    return Wsrc.NotActive;
                case SemaphoreFullException:
                    return Wsrc.SemaphoreFull;
                case WebSocketException:
                    Debug.LogWarning("Could not connect to the server. Check that the server is running and try again.");
                    return Wsrc.Failure;
                default:
                    Debug.LogException(e);
                    return Wsrc.Failure;
            }
        }

        /// <summary>
        /// Attempts to connect to the websocket
        /// </summary>
        /// <returns>request status code</returns>
        public async Task<Wsrc> OpenSocket()
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
            return Wsrc.Success;
        }

        /// <summary>
        /// Attempts to send message over websocket
        /// </summary>
        /// <param name="handshakeCode"></param>
        /// <returns>request status code</returns>
        public async Task<Wsrc> SendModeRequest(string handshakeCode)
        {
            try
            {
                if (!IsActive) { throw new WebSocketNotActiveException(); }

                _sendBuffer = Encoding.UTF8.GetBytes(handshakeCode);
                await _ws.SendAsync(_sendBuffer, WebSocketMessageType.Text, true, default);

                /*
                // receiving handshake confirmation
                var result = await _ws.ReceiveAsync(_receiveBuffer, default);
                string echo = Encoding.UTF8.GetString(_receiveBuffer, 0, result.Count);
                if (echo == handshakeCode)
                {
                    Debug.Log($"Handshake successful: {echo}");
                }
                else
                {
                    throw new Exception("Handshake failed");
                }
                */
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
            return Wsrc.Success;
        }

        /// <summary>
        /// Attempts to wait for data to be received from the websocket
        /// </summary>
        /// <returns>task status code</returns>
        public async Task<Wsrc> ReceiveData()
        {
            try
            {
                // semaphore timeout is set to 0
                // prevents receive requests from backing up
                if (await _semaphore.WaitAsync(0))
                {
                    try
                    {
                        if (!IsActive) { throw new WebSocketNotActiveException(); }

                        var result = await _ws.ReceiveAsync(_receiveBuffer, default);
                        _jsonString = Encoding.UTF8.GetString(_receiveBuffer, 0, result.Count);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                    if (_jsonString == "") { return Wsrc.Failure; }
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
            return Wsrc.Success;
        }

        /// <summary>
        /// Attempts to close the active connection
        /// </summary>
        /// <returns></returns>
        public async Task<Wsrc> CloseSocket()
        {
            try
            {
                if (!IsActive) { throw new WebSocketNotActiveException(); }

                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", default);
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
            return Wsrc.Success;
        }
    }
}