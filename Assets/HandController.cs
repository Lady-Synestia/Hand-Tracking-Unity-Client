using UnityEngine;
using HandTrackingModule.API;

public class HandController : MonoBehaviour
{
    [Header("Hand Transforms")]
    public Hand RightHand;
    public Hand LeftHand;

    [Header("HandTrackingAPI")]
    public HandTrackingAPI handTrackingAPI;

    private void Start()
    {
        handTrackingAPI.SetReceiveMode(ReceiveMode.Points);
        handTrackingAPI.Activate();
    }

    public void UpdateHandPositions(ReceiveMode mode)
    {
        if (mode == ReceiveMode.Points)
        {
            // iterating through the stored positions to update the in-game transforms
            for (int i = 0; i < 21; i++) 
            {
                // getting points from api by index
                var rpoint = handTrackingAPI.GetPoint(i, HandType.Right);
                var lpoint = handTrackingAPI.GetPoint(i, HandType.Left);
                // Debug.Log(point);
                RightHand.SetChildPosition(i, rpoint);
                LeftHand.SetChildPosition(i, lpoint);
            }
        }
    }
}
