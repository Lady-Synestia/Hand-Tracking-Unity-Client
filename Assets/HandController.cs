using UnityEngine;
using HandTrackingModule;

public class HandController : MonoBehaviour
{
    [Header("Hand Transforms")]
    public Hand RightHand;
    public Hand LeftHand;

    [Header("Hand Tracking API")]
    public HandTrackingSystem HandTrackingAPI;

    private void Start()
    {
        HandTrackingAPI.SetReceiveMode(ReceiveType.Landmarks);
        HandTrackingAPI.Activate();
    }

    public void UpdateHandPositions()
    {
        // iterating through the stored positions to update the in-game transforms
        for (int i = 0; i < 21; i++) 
        {
            // getting points from api by index
            Vector3 rpoint = HandTrackingAPI.GetLandmark(i, HandType.Right);
            Vector3 lpoint = HandTrackingAPI.GetLandmark(i, HandType.Left);

            // Debug.Log(point);
            RightHand.SetChildPosition(i, rpoint);
            LeftHand.SetChildPosition(i, lpoint);
        } 
    }
}
