namespace HandTrackingModule
{
    using System;
    using UnityEngine;
    
    /// <summary>
    /// Interface for the API.
    /// Should be instantiated to interact with the system.
    /// </summary>
    public interface IHandTracking
    {
        /// <summary>
        /// Event is called when data is received.
        /// Should be subscribed to in order to appropriately handle hand updates. <br/>
        /// </summary>
        event EventHandler<DataReceivedEventArgs> DataReceivedEvent;
        
        /// <summary>
        /// Called in order to activate the websocket connection when required.
        /// Desired data types to receive must be set before a connection can be established.
        /// </summary>
        /// <exception cref="Exception">Thrown if Activate is called before setting receive types</exception>
        void Activate();
        
        /// <summary>
        /// Sets any of the 3 receive types. 1, 2, or 3 types can be passed in any order.
        /// </summary>
        /// <param name="a">First type parameter</param>
        void SetReceiveTypes(ReceiveType a);
        
        /// <summary>
        /// Sets any of the 3 receive types. 1, 2, or 3 types can be passed in any order.
        /// </summary>
        /// <param name="a">First type parameter</param>
        /// <param name="b">Second type parameter</param>
        void SetReceiveTypes(ReceiveType a, ReceiveType b);
        
        /// <summary>
        /// Sets any of the 3 receive types. 1, 2, or 3 types can be passed in any order.
        /// </summary>
        /// <param name="a">First type parameter</param>
        /// <param name="b">Second type parameter</param>
        /// <param name="c">Third type parameter</param>
        void SetReceiveTypes(ReceiveType a, ReceiveType b, ReceiveType c);
        
        /// <summary>
        /// Gets the world position of a specified landmark
        /// </summary>
        /// <param name="landmark">Name of the landmark</param>
        /// <param name="hand">Right or left hand, defaults to right</param>
        /// <returns>A Vector3 representing the position of the landmark</returns>
        Vector3 GetLandmark(string landmark, HandType hand);
        
        /// <summary>
        /// Gets the world position of a specified landmark
        /// </summary>
        /// <param name="i">Index of the landmark</param>
        /// <param name="hand">Right or left hand, defaults to right</param>
        /// <returns>A Vector3 representing the position of the landmark</returns>
        Vector3 GetLandmark(int i, HandType hand);
        
        /// <summary>
        /// Gets the current Gesture of a specified hand
        /// </summary>
        /// <param name="hand">Right or left hand, defaults to right</param>
        /// <returns>Custom enum denoting the gesture</returns>
        /// 
        Gesture GetGesture(HandType hand);
        
        /// <summary>
        /// Gets the current Orientation of a specified hand
        /// </summary>
        /// <param name="hand">Right or left hand, defaults to right</param>
        /// <returns>Custom enum denoting the orientation</returns>
        Orientation GetOrientation(HandType hand);
    }
}