# _**Unity Client Module for our [Hand Tracking API](https://github.com/Lady-Synestia/Hand-Tracking-API/)**_

## Usage

- Make sure any `.cs` files that use of any features from the module have `using HandTrackingModule` in them.
- Add `HandTrackingSystem.cs` to the GameObject that will control your hands.
- To access the API's interface, create a referece to `IHandTracking`.
- `SetReceiveTypes` should be called before `Activate`, otherwise the system will default to landmarks.
- Assign a function to the `DataReceivedEvent` Event, this function will be called when the API receives new data over the websocket to tell your project when to update your hands.

### Quick Setup

```cs
// Example of a HandController.cs Script

using UnityEngine;
using HandTrackingController;

public class HandController : MonoBehaviour
{
    // Declaring IHandTracking variable
    public IHandTracking HandTrackingAPI;

    private void Awake()
    {
        // Getting reference for IHandTracking
        // HandTrackingSystem.cs needs to be a component of the same GameObject as this script
        HandTrackingAPI = GetComponent<HandTrackingSystem>();
    }

    private void Start()
    {
        // Subscribes UpdateHands() to the DataReceivedEvent event: 
        // UpdateHands()
        HandTrackingAPI.DataReceivedEvent += UpdateHands;

        // Tells API to track and send gestures
        // Must be called before Activate()
        HandTrackingAPI.SetReceiveTypes(ReceiveType.Gestures);
        // Activates Websockets
        HandTrackingAPI.Activate();

    }

    // The method that subscribes to DataReceivedEvent must match its definition
    private void UpdateHands(object sender, DataReceivedEventArgs e)
    {
        // Do Something...
    }
}
```

# API Reference

## Interface Methods

### Activate:

```cs
public void IHandTracking.Activate()
```

Activates the websocket, should be used after `SetReceiveTypes`.

### SetReceiveTypes:

```cs
public void SetReceiveTypes(ReceiveType a | ReceiveType a, ReceiveType b |  ReceiveType a, ReceiveType b, ReceiveType c)
```

Takes 1-3 `ReceiveTypes` as parameters, tells the API what data to send across the websocket.

### GetLandmark:

```cs
Vector3 GetLandmark(string name, HandType hand | int index, HandType hand)
```

Takes either a `string` for the name of the Landmark, or an `integer` denoting its index.\
Optionally takes a `HandType` to request Landmark data for the left or right hand (defaults to right hand).

### GetGesture:

```cs
public Gesture GetGesture(HandType hand)
```

Optionally takes a `HandType` to request Gesture data for the left or right hand (defaults to right)

### GetOrientation:

```cs
public Orientation GetOrientation(HandType hand)
```

Optionally takes a `HandType` to request Orientation data for the left or right hand (defaults to right)

---

## Event Delegate

### DataReceivedEvent:

```cs
event EventHandler<DataReceivedEventArgs> DataReceivedEvent
```

**Parameters:**\
`object sender` The HandTrackingSystem object that made the event call.\
`DataReceivedEventArgs e` Object containing relevant event arguments.

Recommended to add this as an event in your event handling object.

Example `EventManager.cs`:
```cs
using UnityEngine;
using HandTrackingModule;
// ...

public class EventManager : MonoBehaviour
{

    // ...
    // reference to your hand controller class
    [Header("HandController")]
    public HandController handController;
    // ...

    void Start()
    {
        // ...
        // subscribing the function WebsocketDataReceived() to the event
        handController.HandTrackingAPI.DataReceivedEvent += WebsocketDataReceived;
        // ...
    }

    // ...
    // function to be called when the event is raised
    void WebsocketDataReceived(object receiver, DataReceivedEventArgs e)
    {
        if (e.Success)
        {   
            // updates hands with new data
            handController.UpdateHandPositions(e.RightDataReceived, e.LeftDataReceived);
        }
    }
    // ...
}
```


[**Using the .NET EventHandler**](https://learn.microsoft.com/en-us/dotnet/api/system.eventhandler?view=net-9.0) | [**Handling and Raising Events**](https://learn.microsoft.com/en-us/dotnet/standard/events/)

### DataReceivedEventArgs:

```cs
public class DataReceivedEventArgs : Event Args
{
    public bool Success { get; set; }
    public bool RightDataReceived { get; set; }
    public bool LeftDataReceived { get; set; }
}
```

**Fields:**\
`Success` Whether or not the Json data was received and unpacked successfully.\
`RightDataReceived` Whether or not data for the right hand was received.\
`LeftDataReceived` Whether or not data for the left hand was received.

---

## Enums

### Gesture:

```cs
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
```

Types of gesture that can be detected.

### Orientation:

```cs
public enum Orientation
{
    None,
    Up,
    Down,
    Left,
    Right
}
```

Orientations that can be detected.

### HandType:

```cs
public enum HandType
{
    Right,
    Left
}
```

Type of hand that can be detected.

### ReceiveType:

```cs
public enum ReceiveType
{
    Landmarks,
    Gesture,
    Orientation
}
```

Types of data that can be send across the websocket.

---
