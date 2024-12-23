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
        handController.HandTrackingAPI.DataReceivedEvent += WebsocketDataReceived;
    }

    void WebsocketDataReceived(object receiver, DataReceivedEventArgs e)
    {
        if (e.Success)
        {
            handController.UpdateHandPositions(e.RightDataReceived, e.LeftDataReceived);
        }
    }
}
