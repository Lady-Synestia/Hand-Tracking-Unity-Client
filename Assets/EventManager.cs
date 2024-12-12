using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [Header("Websocket Listener Script")]
    public WebSocketListener wsListener;

    [Header("HandController")]
    public HandController handController;

    // Start is called before the first frame update
    void Start()
    {
        // "subscribing" the function WebsocketDataReceived() to the event HandPointsChanged
        wsListener.HandPointsChanged += WebsocketDataReceived;
    }

    void WebsocketDataReceived(object receiver, HandPointsChangedEventArgs e)
    {
        handController.UpdateHandPositions(e.JsonHandStrings);
    }
}
