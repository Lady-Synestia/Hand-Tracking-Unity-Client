using UnityEngine;
using HandTrackingModule.API;

public class EventManager : MonoBehaviour
{

    [Header("HandController")]
    public HandController handController;

    // Start is called before the first frame update
    void Start()
    {
        // "subscribing" the function WebsocketDataReceived() to the event DataReceivedEvent
        handController.handTrackingAPI.DataReceivedEvent += WebsocketDataReceived;
    }

    void WebsocketDataReceived(object receiver, DataReceivedEventArgs e)
    {
        handController.UpdateHandPositions(e.ModeOfDataReceived);
    }
}
