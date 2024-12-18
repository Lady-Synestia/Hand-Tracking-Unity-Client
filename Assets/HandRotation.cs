using UnityEngine;

public class HandRotation : MonoBehaviour
{
    public Transform wrist; // Unity wrist bone
    public Transform wristPosCV; // Position of wrist from OpenCV
    public Transform indexBase; // Base positions of each finger from OpenCV

    void Update()
{
    // Step 1: Calculate the direction vector from wrist to the index base
    Vector3 wristToBaseDirection = indexBase.position - wristPosCV.position;

    // Step 2: Only apply rotation if the direction vector is not zero
    if (wristToBaseDirection.sqrMagnitude > 0.0001f) // Check if the direction is not zero
    {
        wrist.rotation = Quaternion.LookRotation(wristToBaseDirection); // Align wrist to the direction of the base
    }
}
}
