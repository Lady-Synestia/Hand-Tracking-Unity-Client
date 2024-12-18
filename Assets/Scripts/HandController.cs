using UnityEngine;
using HandTrackingModule;

public class HandController : MonoBehaviour
{
    [Header("Hand Transforms")]
    public Hand RightHand;
    public Hand LeftHand;
    public Transform LeftMainChild;
    public Transform RightMainChild;

    [Header("Hand Tracking API")]
    public IHandTracking HandTrackingAPI;

    private void Awake()
    {
        HandTrackingAPI = GetComponent<HandTrackingSystem>();
    }

    private void Start()
    {
        HandTrackingAPI.SetReceiveTypes(ReceiveType.Landmarks);
        HandTrackingAPI.Activate();
    }

    public void UpdateHandPositions(bool RightDataReceived, bool LeftDataReceived)
    {
        // iterating through the stored positions to update the in-game transforms       
        for (int i = 0; i < 21; i++)
        {
            if (RightDataReceived)
            {
                // getting points from api by index
                Vector3 rpoint = HandTrackingAPI.GetLandmark(i, HandType.Right);
                // Debug.Log(point);
                RightHand.SetChildPosition(i, rpoint);
            }
            if (LeftDataReceived)
            {
                Vector3 lpoint = HandTrackingAPI.GetLandmark(i, HandType.Left);
                LeftHand.SetChildPosition(i, lpoint);
            }
        }
    }
}
