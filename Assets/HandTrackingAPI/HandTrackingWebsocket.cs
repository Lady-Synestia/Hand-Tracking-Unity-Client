
// hides websocket funcitonality from main module namespace
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
    public class WebsocketNotActiveException : Exception
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

    public class WebsocketListener
    {
        public Del DataReceivedDel;
        public bool isActive;

        /// <summary>
        /// Semaphore code and docs
        /// solution >  https://stackoverflow.com/a/21163280
        /// SemaphoreSlim class > https://learn.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim?view=net-8.0
        /// </summary>
        // Semaphore is used to stop too many processes running in the thread pool
        // In this case there should only be one data receival running at once
        private static SemaphoreSlim semaphore = new(1);

        /// <summary>
        /// websockets docs
        /// websocket support > https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/websockets
        /// ClientWebSocket class > https://learn.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocket?view=net-9.0
        /// </summary>
        private ClientWebSocket ws = new();
        private Uri uri = new("ws://localhost:8765");
        private byte[] sendBuffer;
        private byte[] receiveBuffer = new byte[4096];

        private string JsonString;

        // common exception handling for all methods
        private WSRC HandleException(Exception e)
        {
            switch (e)
            {
                case WebsocketNotActiveException:
                    return WSRC.NotActive;
                case SemaphoreFullException:
                    return WSRC.SemaphoreFull;
                default:
                    Debug.LogError(e);
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
                if (!isActive)
                {
                    isActive = true;
                    await ws.ConnectAsync(uri, default);
                }
            }
            catch (Exception e)
            {
                isActive = false;
                return HandleException(e);
            }
            return WSRC.Success;
        }

        /// <summary>
        /// Attempts to send message over websocket
        /// </summary>
        /// <param name="message"></param>
        /// <returns>request status code</returns>
        public async Task<WSRC> SendModeRequest(string message)
        {
            try
            {
                if (!isActive) { throw new WebsocketNotActiveException(); }

                sendBuffer = Encoding.UTF8.GetBytes(message);
                await ws.SendAsync(sendBuffer, WebSocketMessageType.Text, true, default);

                // receiving handshake confirmation
                var task = await ws.ReceiveAsync(receiveBuffer, default);
                if (task.EndOfMessage)
                {
                    string response = Encoding.UTF8.GetString(receiveBuffer);
                    Debug.Log($"Handshake successful: {response}");
                }
                else
                {
                    throw new Exception("Handshake failed");
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
                if (await semaphore.WaitAsync(0))
                {
                    try
                    {
                        if (!isActive) { throw new WebsocketNotActiveException(); }

                        var result = await ws.ReceiveAsync(receiveBuffer, default);
                        JsonString = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                    // delegate call for HandTrackingSystem
                    DataReceivedDel(JsonString);
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
                if (!isActive) { throw new WebsocketNotActiveException(); }

                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", default);
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
            return WSRC.Success;
        }
    }
}