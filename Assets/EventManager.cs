using UnityEngine;
using HandTrackingModule;

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
        if (e.success)
        {
            handController.UpdateHandPositions();
        }
    }
}
