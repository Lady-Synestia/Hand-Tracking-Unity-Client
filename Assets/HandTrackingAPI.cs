namespace HandTrackingModule
{
    using System;
    using System.Collections.Generic;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using API;

    // HandTrackingModule.API only exposes necessary features
    namespace API
    {
        public enum Gesture
        {
            FuckYou,
            ThumbsUp,
            Fist,
            OpenPalm,
            Ok,
            Metal,
            WebShooter,
            ErmAckshually,
            Victory,
            Number3,
            LmaoGottem
        }

        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        public enum HandType
        {
            Right,
            Left
        }

        public enum ReceiveMode
        {
            Points,
            Gesture,
            Direction
        }

        public class HandTrackingAPI : MonoBehaviour
        {
            /// <summary>
            /// Event Delegates 
            /// EventHandler Delegate > https://learn.microsoft.com/en-us/dotnet/api/system.eventhandler?view=net-9.0
            /// Handling and Raising Events > https://learn.microsoft.com/en-us/dotnet/standard/events/
            /// </summary>
            // Event is used to notify eventManager when hand data is received
            public event EventHandler<DataReceivedEventArgs> DataReceivedEvent;

            private WebsocketListener WSListener = new();
            private Dictionary<ReceiveMode, bool> ReceiveModes = new() {
                { ReceiveMode.Direction, false }, 
                { ReceiveMode.Points, false },
                { ReceiveMode.Gesture, false }
            };

            // data for each hand
            private HandData RHandData = new(HandType.Right);
            private HandData LHandData = new(HandType.Left);

            public async void Activate()
            {
                try
                {
                    if (!ReceiveModes.ContainsValue(true))
                    {
                        throw new Exception("No receive mode set, websocket cannot activate");
                    }
                    {
                        WSRC status = await WSListener.OpenSocket();
                        Debug.Log($"Websocket connection: {status}");
                    }
                    {
                        WSRC status = await WSListener.SendModeRequest(GenerateTypeRequestCode());
                        Debug.Log($"Websocket mode request sent: {status}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            private void Start()
            {
                // DataReceived() will be called when WSListener receives data
                WSListener.DataReceivedDel += DataReceived;
            }

            private void Update()
            {
                // string testJson = "{\"point0\": {\"x\": 0.36451587080955505, \"y\": 0.7492399215698242, \"z\": -1.5179728585223984e-09}, \"point1\": {\"x\": 0.34811195731163025, \"y\": 0.7084940671920776, \"z\": -0.05889080837368965}, \"point2\": {\"x\": 0.3175491392612457, \"y\": 0.6842957139015198, \"z\": -0.08604178577661514}, \"point3\": {\"x\": 0.28366610407829285, \"y\": 0.6672555804252625, \"z\": -0.09910319745540619}, \"point4\": {\"x\": 0.24972884356975555, \"y\": 0.6543729305267334, \"z\": -0.10958848148584366}, \"point5\": {\"x\": 0.26708173751831055, \"y\": 0.7825995683670044, \"z\": -0.07823742181062698}, \"point6\": {\"x\": 0.20796909928321838, \"y\": 0.7588865160942078, \"z\": -0.09051723033189774}, \"point7\": {\"x\": 0.18948495388031006, \"y\": 0.7131353616714478, \"z\": -0.09657265245914459}, \"point8\": {\"x\": 0.18492534756660461, \"y\": 0.6728100180625916, \"z\": -0.09973837435245514}, \"point9\": {\"x\": 0.2606772780418396, \"y\": 0.8008275628089905, \"z\": -0.0475996658205986}, \"point10\": {\"x\": 0.20537029206752777, \"y\": 0.7662889361381531, \"z\": -0.05684434249997139}, \"point11\": {\"x\": 0.18718063831329346, \"y\": 0.7139981985092163, \"z\": -0.06536299735307693}, \"point12\": {\"x\": 0.18383654952049255, \"y\": 0.6727067828178406, \"z\": -0.07237333804368973}, \"point13\": {\"x\": 0.25679683685302734, \"y\": 0.8021751046180725, \"z\": -0.017644982784986496}, \"point14\": {\"x\": 0.20753738284111023, \"y\": 0.772013247013092, \"z\": -0.021955737844109535}, \"point15\": {\"x\": 0.19017675518989563, \"y\": 0.7260228991508484, \"z\": -0.03151095658540726}, \"point16\": {\"x\": 0.1863010674715042, \"y\": 0.686863899230957, \"z\": -0.03952827677130699}, \"point17\": {\"x\": 0.2558435797691345, \"y\": 0.7944516539573669, \"z\": 0.00818195752799511}, \"point18\": {\"x\": 0.2147524505853653, \"y\": 0.7713050246238708, \"z\": 0.003776286030188203}, \"point19\": {\"x\": 0.19767005741596222, \"y\": 0.7385501265525818, \"z\": -0.003902278607711196}, \"point20\": {\"x\": 0.1897059679031372, \"y\": 0.7055431008338928, \"z\": -0.009910108521580696}}";
                // DataReceived(testJson);

                // ReceiveData() uses semaphores for thread control, so does not need to be awaited
                WSListener.ReceiveData();
            }

            private async void OnApplicationQuit()
            {
                WSRC status = await WSListener.CloseSocket();
                Debug.Log($"Closed websocket connection: {status}");
            }

            private void DataReceived(string json)
            {
                // splitting received string into array of substrings
                string[] jsonStrings = json.Trim('[', ']').Split("], [");
                
                // currently prefers right hand to left if there is only one set of hand points
                switch (jsonStrings.Length)
                {
                    case 2:
                        LHandData.SetFromJson(jsonStrings[1]);
                        goto case 1;
                    case 1:
                        RHandData.SetFromJson(jsonStrings[0]);
                        break;
                }

                // While testing, just passing the single string into the array
                // Once complete the strings will have to be extracted from the received data
                DataReceivedEventArgs args = new()
                {
                    ModeOfDataReceived = ReceiveMode.Points,
                };
                DataReceivedEvent?.Invoke(this, args);
            }

            private string GenerateTypeRequestCode()
            {
                char[] code = new char[3];
                int i = 0;
                foreach (bool value in ReceiveModes.Values)
                {
                    code[i] = value ? '1' : '0';
                    i++;
                }
                // the code is a compact representation of what modes are active
                // e.g. 010 -> only points are active | 101 -> direction and gestures are active
                return new string(code);
            }

            // method overiding so SetReceiveMode can be called with 1-3 modes in any order
            public void SetReceiveMode(ReceiveMode a) { ReceiveModes[a] = true;}
            public void SetReceiveMode(ReceiveMode a, ReceiveMode b) { ReceiveModes[a] = true; ReceiveModes[b] = true; }
            public void SetReceiveMode(ReceiveMode a, ReceiveMode b, ReceiveMode c) { ReceiveModes[a] = true; ReceiveModes[b]= true; ReceiveModes[c] = true; }

            public Vector3 GetPoint(string point, HandType hand = HandType.Right)
            {
                switch (hand)
                {
                    case HandType.Right:
                        return RHandData.GetPoint(point);
                    case HandType.Left:
                        return LHandData.GetPoint(point);
                    default:
                        return new Vector3();
                }
            }
            // method overriding so GetPoint() can be called with an index instead of a string
            public Vector3 GetPoint(int index, HandType hand = HandType.Right) { return GetPoint($"point{index}", hand); }
        }

        public class DataReceivedEventArgs : EventArgs
        {
            public ReceiveMode ModeOfDataReceived { get; set; }
        }
    }

    [Serializable]
    public struct HandPoints
    {
        public Vector3 point0;
        public Vector3 point1;
        public Vector3 point2;
        public Vector3 point3;
        public Vector3 point4;
        public Vector3 point5;
        public Vector3 point6;
        public Vector3 point7;
        public Vector3 point8;
        public Vector3 point9;
        public Vector3 point10;
        public Vector3 point11;
        public Vector3 point12;
        public Vector3 point13;
        public Vector3 point14;
        public Vector3 point15;
        public Vector3 point16;
        public Vector3 point17;
        public Vector3 point18;
        public Vector3 point19;
        public Vector3 point20;

        public void PointsFromJson(string json)
        {
            // Debug.Log(json);
            this = JsonUtility.FromJson<HandPoints>(json);
        }

        /// <summary>
        /// C# Iterators 
        /// https://learn.microsoft.com/en-us/dotnet/csharp/iterators
        /// </summary>
        // allows iteration over the struct
        public IEnumerable<Vector3> GetPoints()
        {
            yield return point0;
            yield return point1;
            yield return point2;
            yield return point3;
            yield return point4;
            yield return point5;
            yield return point6;
            yield return point7;
            yield return point8;
            yield return point9;
            yield return point10;
            yield return point11;
            yield return point12;
            yield return point13;
            yield return point14;
            yield return point15;
            yield return point16;
            yield return point17;
            yield return point18;
            yield return point19;
            yield return point20;
        }

    }

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

    public class HandData
    {
        // temp structure while refactoring
        public HandPoints handPoints;

        private Dictionary<string, Vector3> pointsDict = new();
        public HandType Hand { get; }
        public Gesture Gesture { get; }
        public Direction Direction { get; }

        public HandData(HandType hand) { Hand = hand; }

        public Vector3 GetPoint(string point)
        {
            // only returns value if it is contained by the dictionary
            if (pointsDict.ContainsKey(point)) 
            { 
                return pointsDict[point]; 
            }
            return new Vector3();
        }

        public void SetFromJson(string json)
        {
            handPoints.PointsFromJson(json);
            int n = 0;
            foreach (Vector3 point in handPoints.GetPoints())
            {
                string key = $"point{n}";
                // adds the key-value pair if key isnt already present, otherwise sets the value
                if (!pointsDict.TryAdd(key, point))
                {
                    pointsDict[key] = point;
                }
                n++;
            }
        }
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
                    // delegate call for HandTrackingAPI
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