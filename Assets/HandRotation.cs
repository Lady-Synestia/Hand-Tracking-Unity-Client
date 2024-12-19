using UnityEngine;

public class HandRotation : MonoBehaviour
{
    public Transform wrist; // Unity wrist bone
    public Transform wristPosCV; // Position of wrist from OpenCV
    public Transform middleBase;
    public Transform pinkyBase;


    void Update()
    {

        // o = origin
        // a = wrist
        // b = middle base
        // c = pinky


        // vector from origin to wrist
        Vector3 OA = wristPosCV.position;

        // vector from origin to middle finger base
        Vector3 OB = middleBase.position;

        // vector from origin to pinky finger base
        Vector3 OC = pinkyBase.position;


        // vector from thumb base to middle base
        Vector3 AB = OB - OA;
        
        // vector from thumb base to pinky base
        Vector3 AC = OC - OA;


        // angle in radians about y, measured from x
        float yaw = Mathf.Tan(AC.z / AC.x);

        // angle in radians about z, measured from x
        float pitch = Mathf.Tan(AB.y / AB.x);

        // angle in radians about x, measured from z
        float roll = Mathf.Tan(AB.y / AB.z);



        Quaternion rotation = Quaternion.Euler(roll, yaw, pitch);

        wrist.rotation = rotation;

    }
}
