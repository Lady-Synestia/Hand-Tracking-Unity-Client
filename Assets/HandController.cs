using UnityEngine;
using HandTrackingModule.API;

public class HandController : MonoBehaviour
{
    [Header("Hand Transforms")]
    public Hand handTransforms;

    [Header("HandTrackingAPI")]
    public HandTrackingAPI handTrackingAPI;

    private void Start()
    {
        handTrackingAPI.SetReceiveMode(ReceiveMode.Points);
    }

    public void UpdateHandPositions(ReceiveMode mode)
    {
        if (mode == ReceiveMode.Points)
        {
            // iterating through the stored positions to update the in-game transforms
            for (int i = 0; i < 21; i++) 
            {
                // getting point from api by index
                var point = handTrackingAPI.GetPoint(index: i);
                // Debug.Log(point);
                handTransforms.SetChildPosition(i, point);
            }
        }
    }
}
