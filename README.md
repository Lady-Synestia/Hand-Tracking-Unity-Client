# Unity Client Project for our [Hand Tracking API](https://github.com/Lady-Synestia/Hand-Tracking-API/)

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
        // Getting reference for IHandTracking]
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

## Module Definitions

### **Functions**

#### *Activate*:

```cs
public void IHandTracking.Activate()
```

Activates the websocket, should be used after `SetReceiveTypes`.

#### *SetReceiveTypes*:

```cs
public void SetReceiveTypes(ReceiveType a | ReceiveType a, ReceiveType b |  ReceiveType a, ReceiveType b, ReceiveType c)
```

Takes 1-3 `ReceiveTypes` as parameters, tells the API what data to send across the websocket.

#### *GetLandmark*:

```cs
Vector3 GetLandmark(string name, HandType hand | int index, HandType hand)
```

Takes either a `string` for the name of the Landmark, or an `integer` denoting its index.<br>
Also takes a `HandType` to request data for the left or right hand.

---

### **Event Delegate**

#### *DataReceivedEvent*:

```cs
event EventHandler<DataReceivedEventArgs> DataReceivedEvent
```

**Parameters:**<br>
`object sender` The HandTrackingSystem object that made the event call.<br>
`DataReceivedEventArgs e` Object containing relevant event arguments.

Recommended to add this as an event in your event handling object.

#### *DataReceivedEventArgs*:

```cs
public class DataReceivedEventArgs : Event Args
{
    public bool Success { get; set; }
    public bool RightDataReceived { get; set; }
    public bool LeftDataReceived { get; set; }
}
```

**Fields:**
`Success` Whether or not the Json data was received and unpacked successfully.
`RightDataReceived` Whether or not data for the right hand was received.
`LeftDataReceived` Whether or not data for the left hand was received.

---

### **Enums**

#### *Gesture:*

```cs
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
```

Types of gesture that can be detected.

#### *Direction:*

```cs
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}
```

Directions that can be detected.

#### *HandType:*

```cs
public enum HandType
{
    Right,
    Left
}
```

Type of hand that can be detected.

#### *ReceiveType:*

```cs
public enum ReceiveType
{
    Landmarks,
    Gesture,
    Direction
}
```

Types of data that can be send across the websocket.


## TODO

#### Chloe:
- Refactor Json deserialisation for updated Json format

#### Ruby:
- Fix current rig movement implementation
- Come up with a way of moving the hands based on gestures and direction
