using UnityEngine;
using HandTrackingModule;
using UnityEngine.Serialization;

public class HandController : MonoBehaviour
{
    [Header("Hand Transforms")]
    public Hand rightHand;
    public Hand leftHand;
    // public Transform LeftMainChild;
    // public Transform RightMainChild;

    [Header("Hand Tracking API")]
    public IHandTracking HandTrackingAPI;

    private void Awake()
    {
        HandTrackingAPI = GetComponent<HandTrackingSystem>();
    }

    private void Start()
    {
        HandTrackingAPI.SetReceiveTypes(ReceiveType.Landmarks, ReceiveType.Gesture, ReceiveType.Orientation);
        HandTrackingAPI.Activate();
    }

    public void UpdateHandPositions(bool rightDataReceived, bool leftDataReceived)
    {
        // iterating through the stored positions to update the in-game transforms       
        for (int i = 0; i < 21; i++)
        {
            if (rightDataReceived)
            {
                // getting points from api by index
                Vector3 rPoint = HandTrackingAPI.GetLandmark(i, HandType.Right);
                rightHand.SetChildPosition(i, rPoint);
            }
            if (leftDataReceived)
            {
                Vector3 lPoint = HandTrackingAPI.GetLandmark(i, HandType.Left);
                leftHand.SetChildPosition(i, lPoint);
            }
        }
    }
}
