using System;
using UnityEngine;
using System.Collections.Generic;

namespace HandTrackingModule
{
    /// <summary>
    /// Internal class used to store deserialised hand data
    /// </summary>
    internal class HandData
    {
        // fields are public in order for the deserialisation to work correctly
        public Dictionary<string, Vector3> Landmarks;
        public Gesture Gesture;
        public Orientation Orientation;

        /// <summary>
        /// Returns true if it is currently storing data of a particular type.
        /// Used to make sure the correct data has been received from the websocket.
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns></returns>
        public bool HasValue(ReceiveType type)
        {
            return type switch
            {
                ReceiveType.Landmarks => Landmarks != null,
                ReceiveType.Orientation => Orientation != Orientation.None,
                ReceiveType.Gesture => Gesture != Gesture.None,
                _ => false
            };
        }

        /// <summary>
        /// Gets a 'point' from the landmarks dictionary
        /// </summary>
        /// <param name="key">Dictionary key corresponding to desired landmark</param>
        /// <returns>Vector3 of the position of the point</returns>
        /// <exception cref="Exception">Thrown if an invalid key is used</exception>
        public Vector3 GetPoint(string key)
        {
            // only returns value if it is contained by the dictionary
            try
            {
                if (!Landmarks.TryGetValue(key, out Vector3 point))
                {
                    throw new Exception($"{key} is not a valid landmark");
                }
                return point;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return default;
            }
        }
    }
}