namespace HandTrackingModule
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using HandTrackingModule.Websocket;
    
    /// <summary>
    /// Event Arguments for the Data Received Event.
    /// Stores flags showing if the data was received successfully,
    /// and which hands data was received for
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public bool RightDataReceived { get; set; }
        public bool LeftDataReceived { get; set; }
    }

    /// <summary>
    /// Controlling class for the module.
    /// Handles json deserialisation, and implements the IHandTracking Interface.
    /// </summary>
    internal class HandTrackingSystem : MonoBehaviour, IHandTracking
    {
        private readonly WebsocketListener _wsListener = new();

        private readonly Dictionary<ReceiveType, bool> _receiveTypes = new()
        {
            { ReceiveType.Landmarks, false },
            { ReceiveType.Orientation, false },
            { ReceiveType.Gesture, false }
        };

        /// <summary>
        /// Stores the current data for each hand.
        /// Keys are custom HandType enums.
        /// </summary>
        private Dictionary<HandType, HandData> _handsData = new()
        {
            { HandType.Right, new HandData() },
            { HandType.Left, new HandData() }
        };

        private bool _connectionSuccess = false;

        /// <summary>
        /// Event is called when data is received.
        /// Should be subscribed to in order to appropriately handle hand updates. <br/>
        /// Event Delegates docs: <br/>
        /// EventHandler Delegate > https://learn.microsoft.com/en-us/dotnet/api/system.eventhandler?view=net-9.0 <br/>
        /// Handling and Raising Events > https://learn.microsoft.com/en-us/dotnet/standard/events/
        /// </summary>
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
                _ = _wsListener.ReceiveData();
            }
        }

        private async void OnApplicationQuit()
        {
            // defaults to failure
            Wsrc status = default;
            try
            {
                // status is overwritten if successful
                status = await _wsListener.CloseSocket();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                Debug.Log($"Closed websocket connection: {status}");
            }
        }

        /// <summary>
        /// Connecting function between websocket and DataReceived Event.
        /// Handles Json deserialisation, and calls the event if successful.
        /// </summary>
        /// <param name="json">Json string received over websocket</param>
        private void DataReceived(string json)
        {
            // initialising arguments and setting default values
            DataReceivedEventArgs args = new()
            {
                Success = true,
                RightDataReceived = false,
                LeftDataReceived = false
            };
            try
            {
                // using Newtonsoft Json.NET to deserialise into a dictionary containing data for both hands
                _handsData = JsonConvert.DeserializeObject<Dictionary<HandType, HandData>>(json,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ObjectCreationHandling = ObjectCreationHandling.Reuse
                    });

                // checking that the correct data was received for each hand
                args.RightDataReceived = CheckTypesReceived(_handsData[HandType.Right]);
                args.LeftDataReceived = CheckTypesReceived(_handsData[HandType.Left]);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                args.Success = false;
            }

            // Calls any functions subscribed to the event
            DataReceivedEvent?.Invoke(this, args);
        }

        /// <summary>
        /// returns true if the types received in the json data
        /// match with the requested types
        /// </summary>
        /// <param name="hand">data stored for one hand</param>
        /// <returns></returns>
        private bool CheckTypesReceived(HandData hand)
        {
            foreach (ReceiveType key in _receiveTypes.Keys)
            {
                if (hand.HasValue(key))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Generates a 3-digit binary code to request which data types the backend API should send
        /// across the websocket. <br/>
        /// The code is a compact representation of what modes are active.
        /// e.g. 010 -> only landmarks are active | 101 -> orientation and gestures are active
        /// </summary>
        /// <returns>Code to be sent to the server</returns>
        private string GenerateHandshakeCode()
        {
            char[] code = new char[4];
            int i = 1;

            // the : is so that the backend API can discriminate between this and messages from its socket sender
            code[0] = ':';
            foreach (bool value in _receiveTypes.Values)
            {
                code[i] = value ? '1' : '0';
                i++;
            }

            return new string(code);
        }

        /// <summary>
        /// Called in order to activate the websocket connection when required.
        /// Desired data types to receive must be set before a connection can be established.
        /// </summary>
        /// <exception cref="Exception">Thrown if Activate is called before setting receive types</exception>
        public async void Activate()
        {
            try
            {
                if (!_receiveTypes.ContainsValue(true))
                {
                    throw new Exception("No receive types set, websocket cannot activate");
                }

                {
                    Wsrc status = await _wsListener.OpenSocket();
                    Debug.Log($"Websocket connection: {status}");
                }
                {
                    Wsrc status = await _wsListener.SendModeRequest(GenerateHandshakeCode());
                    Debug.Log($"Websocket mode request sent: {status}");
                    if (status == Wsrc.Success)
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
        /// <summary>
        /// Sets any of the 3 receive types. 1, 2, or 3 types can be passed in any order.
        /// </summary>
        /// <param name="a">First type parameter</param>
        public void SetReceiveTypes(ReceiveType a)
        {
            _receiveTypes[a] = true;
        }

        /// <summary>
        /// Sets any of the 3 receive types. 1, 2, or 3 types can be passed in any order.
        /// </summary>
        /// <param name="a">First type parameter</param>
        /// <param name="b">Second type parameter</param>
        public void SetReceiveTypes(ReceiveType a, ReceiveType b)
        {
            _receiveTypes[a] = true;
            _receiveTypes[b] = true;
        }

        /// <summary>
        /// Sets any of the 3 receive types. 1, 2, or 3 types can be passed in any order.
        /// </summary>
        /// <param name="a">First type parameter</param>
        /// <param name="b">Second type parameter</param>
        /// <param name="c">Third type parameter</param>
        public void SetReceiveTypes(ReceiveType a, ReceiveType b, ReceiveType c)
        {
            _receiveTypes[a] = true;
            _receiveTypes[b] = true;
            _receiveTypes[c] = true;
        }

        /// <summary>
        /// Returns true if the type of data being requested has been received
        /// </summary>
        /// <param name="type">Orientation, Gesture, or Landmarks</param>
        /// <param name="hand">Right or left hand</param>
        /// <returns></returns>
        /// <exception cref="ReceiveTypeException">Called if the requested type has not been received</exception>
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

        /// <summary>
        /// Gets the world position of a specified landmark
        /// </summary>
        /// <param name="landmark">Name of the landmark</param>
        /// <param name="hand">Right or left hand, defaults to right</param>
        /// <returns>A Vector3 representing the position of the landmark</returns>
        public Vector3 GetLandmark(string landmark, HandType hand = HandType.Right)
        {
            if (ReceiveTypeValidation(ReceiveType.Landmarks, hand))
            {
                return _handsData[hand].GetPoint(landmark);
            }

            return default;
        }

        /// <summary>
        /// Gets the world position of a specified landmark
        /// </summary>
        /// <param name="index">Index of the landmark</param>
        /// <param name="hand">Right or left hand, defaults to right</param>
        /// <returns>A Vector3 representing the position of the landmark</returns>
        public Vector3 GetLandmark(int index, HandType hand = HandType.Right)
        {
            // method overloaded so GetLandmark() can be called with an index instead of a string

            if (ReceiveTypeValidation(ReceiveType.Landmarks, hand))
            {
                // all landmark names are a string of their index
                // if the landmark names are changed only this function needs to be refactored
                return _handsData[hand].GetPoint($"{index}");
            }

            return default;
        }

        /// <summary>
        /// Gets the current Gesture of a specified hand
        /// </summary>
        /// <param name="hand">Right or left hand, defaults to right</param>
        /// <returns>Custom enum denoting the gesture</returns>
        public Gesture GetGesture(HandType hand = HandType.Right)
        {
            if (ReceiveTypeValidation(ReceiveType.Gesture, hand))
            {
                return _handsData[hand].Gesture;
            }

            return default;
        }

        /// <summary>
        /// Gets the current Orientation of a specified hand
        /// </summary>
        /// <param name="hand">Right or left hand, defaults to right</param>
        /// <returns>Custom enum denoting the orientation</returns>
        public Orientation GetOrientation(HandType hand = HandType.Right)
        {
            if (ReceiveTypeValidation(ReceiveType.Orientation, hand))
            {
                return _handsData[hand].Orientation;
            }

            return default;
        }
    }
}