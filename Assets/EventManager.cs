using UnityEngine;

public class EventManager : MonoBehaviour
{
    [Header("Websocket Listener Script")]
    public HandTrackingAPI handTrackingAPI;

    [Header("HandController")]
    public HandController handController;

    // Start is called before the first frame update
    void Start()
    {
        // "subscribing" the function WebsocketDataReceived() to the event HandPointsChanged
        handTrackingAPI.HandPointsChanged += WebsocketDataReceived;
    }

    void WebsocketDataReceived(object receiver, HandPointsChangedEventArgs e)
    {
        handController.UpdateHandPositions(e.ModeOfDataReceived);
    }
}
