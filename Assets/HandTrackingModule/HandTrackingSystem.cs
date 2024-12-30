namespace HandTrackingModule
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using HandTrackingModule.Websocket;
    using Newtonsoft.Json;

    public enum Gesture
    {
        None,
        MiddleFinger,
        ThumbsUp,
        Fist,
        OpenPalm,
        Ok,
        Metal,
        WebShooter,
        Number1,
        Number2,
        Number3
    }

    public enum Orientation
    {
        None,
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

    public enum ReceiveType
    {
        Landmarks,
        Orientation,
        Gesture
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public bool RightDataReceived { get; set; }
        public bool LeftDataReceived { get; set; }
    }

    // custom exception for api data requests
    public class ReceiveTypeException : Exception
    {
        public override string Message { get; }
        public ReceiveTypeException(ReceiveType type) { Message = $"Not receiving data of type: {type}"; }
    }

    public interface IHandTracking
    {
        event EventHandler<DataReceivedEventArgs> DataReceivedEvent;
        void Activate();
        void SetReceiveTypes(ReceiveType a);
        void SetReceiveTypes(ReceiveType a, ReceiveType b);
        void SetReceiveTypes(ReceiveType a, ReceiveType b, ReceiveType c);
        Vector3 GetLandmark(string name, HandType hand);
        Vector3 GetLandmark(int i, HandType hand);
        Gesture GetGesture(HandType hand);
        Orientation GetOrientation(HandType hand);
    }

    class HandTrackingSystem : MonoBehaviour, IHandTracking
    {
        private WebsocketListener _wsListener = new();
        private Dictionary<ReceiveType, bool> _receiveTypes = new() 
        {
            { ReceiveType.Landmarks, false },
            { ReceiveType.Orientation, false },
            { ReceiveType.Gesture, false }
        };

        // data for each hand
        private Dictionary<HandType, HandData> _handsData = new()
        {
            { HandType.Right, new()},
            { HandType.Left, new()}
        };

        private bool _connectionSuccess = false;

        /// <summary>
        /// Event Delegates 
        /// EventHandler Delegate > https://learn.microsoft.com/en-us/dotnet/api/system.eventhandler?view=net-9.0
        /// Handling and Raising Events > https://learn.microsoft.com/en-us/dotnet/standard/events/
        /// </summary>
        // Event is used to notify eventManager when hand data is received
        public event EventHandler<DataReceivedEventArgs> DataReceivedEvent;

        private void Start()
        {
            // DataReceived() will be called when WSListener receives data
            _wsListener.DataReceivedDel += DataReceived;
        }

        private void Update()
        {
            // prevents ReceiveData call before connection has been established
            if (_connectionSuccess)
            {
                // ReceiveData() uses semaphores for thread control, so does not need to be awaited
                _wsListener.ReceiveData();
            }
        }

        private async void OnApplicationQuit()
        {
            WSRC status = await _wsListener.CloseSocket();
            Debug.Log($"Closed websocket connection: {status}");
        }

        private void DataReceived(string json)
        {
            Debug.Log(json);
            // initialising arguments and setting default values
            DataReceivedEventArgs args = new()
            {
                Success = true,
                RightDataReceived = false,
                LeftDataReceived = false
            };
            try
            {
                _handsData = JsonConvert.DeserializeObject<Dictionary<HandType, HandData>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, ObjectCreationHandling = ObjectCreationHandling.Reuse});

                args.RightDataReceived = CheckTypesReceived(_handsData[HandType.Right]);
                args.LeftDataReceived = CheckTypesReceived(_handsData[HandType.Left]);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                args.Success = false;
            }
            DataReceivedEvent?.Invoke(this, args);
        }

        private bool CheckTypesReceived(HandData hand)
        {
            foreach (KeyValuePair<ReceiveType, bool> kvp in _receiveTypes)
            {
                if (hand.HasValue(kvp.Key)) { return true; }
            }
            return false;
        }

        private string GenerateHandshakeCode()
        {
            char[] code = new char[4];
            int i = 1;

            // the : is so that the python socket side can discriminate between this and messages from its socket sender
            code[0] = ':';
            foreach (bool value in _receiveTypes.Values)
            {
                code[i] = value ? '1' : '0';
                i++;
            }
            // the code is a compact representation of what modes are active
            // e.g. 010 -> only points are active | 101 -> direction and gestures are active
            return new string(code);
        }

        public async void Activate()
        {
            try
            {
                if (!_receiveTypes.ContainsValue(true))
                {
                    throw new Exception("No receive mode set, websocket cannot activate");
                }
                {
                    WSRC status = await _wsListener.OpenSocket();
                    Debug.Log($"Websocket connection: {status}");
                }
                {
                    WSRC status = await _wsListener.SendModeRequest(GenerateHandshakeCode());
                    Debug.Log($"Websocket mode request sent: {status}");
                    if (status == WSRC.Success)
                    {
                        _connectionSuccess = true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        // method overloading so SetReceiveTypes can be called with 1-3 modes in any order
        public void SetReceiveTypes(ReceiveType a) { _receiveTypes[a] = true; }
        public void SetReceiveTypes(ReceiveType a, ReceiveType b) { _receiveTypes[a] = true; _receiveTypes[b] = true; }
        public void SetReceiveTypes(ReceiveType a, ReceiveType b, ReceiveType c) { _receiveTypes[a] = true; _receiveTypes[b] = true; _receiveTypes[c] = true; }


        private bool ReceiveTypeValidation(ReceiveType type, HandType hand)
        {
            try
            {
                if (!(_receiveTypes[type] && _handsData[hand].HasValue(type)))
                {
                    throw new ReceiveTypeException(type);
                }
                return true;
            }
            catch (ReceiveTypeException e)
            {
                Debug.LogWarning(e);
                return false;
            }
        }

        public Vector3 GetLandmark(string name, HandType hand = HandType.Right)
        {
            if (ReceiveTypeValidation(ReceiveType.Landmarks, hand))
            {
                return _handsData[hand].GetPoint(name);
            }
            return default;
        }
        // method overloading so GetLandmark() can be called with an index instead of a string
        public Vector3 GetLandmark(int index, HandType hand = HandType.Right)
        {
            if (ReceiveTypeValidation(ReceiveType.Landmarks, hand))
            {
                return _handsData[hand].GetPoint($"{index}");
            }
            return default;
        }

        public Gesture GetGesture(HandType hand = HandType.Right)
        {
            if (ReceiveTypeValidation(ReceiveType.Gesture, hand))
            {
                return _handsData[hand].Gesture;
            }
            return default;
        }

        public Orientation GetOrientation(HandType hand = HandType.Right)
        {
            if (ReceiveTypeValidation(ReceiveType.Orientation, hand))
            {
                return _handsData[hand].Orientation;
            }
            return default;
        }
    }

    [Serializable]
    class HandData
    {
        public Dictionary<string, Vector3> Landmarks;
        public Gesture Gesture;
        public Orientation Orientation;

        public bool HasValue(ReceiveType type)
        {
            // checking if the object is storing a value for the specified type
            // used to make sure the correct data has been received
            return type switch
            {
                ReceiveType.Landmarks => Landmarks != null,
                ReceiveType.Orientation => Orientation != Orientation.None,
                ReceiveType.Gesture => Gesture != Gesture.None,
                _ => false
            };
        }

        public Vector3 GetPoint(string key)
        {
            // only returns value if it is contained by the dictionary
            try
            {
                if (!Landmarks.ContainsKey(key))
                {
                    throw new Exception($"{key} is not a valid landmark");
                }
                return Landmarks[key];
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return default;
            }
        }
    }

}