using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct HandPoints
{
    public Vector3 point0;
    public Vector3 point1;
    public Vector3 point2;
    public Vector3 point3;
    public Vector3 point4;
    public Vector3 point5;
    public Vector3 point6;
    public Vector3 point7;
    public Vector3 point8;
    public Vector3 point9;
    public Vector3 point10;
    public Vector3 point11;
    public Vector3 point12;
    public Vector3 point13;
    public Vector3 point14;
    public Vector3 point15;
    public Vector3 point16;
    public Vector3 point17;
    public Vector3 point18;
    public Vector3 point19;
    public Vector3 point20;

    public void PointsFromJson(string json)
    {
        // Debug.Log(json);
        this = JsonUtility.FromJson<HandPoints>(json);

    }

    // allows iteration over the struct
    public IEnumerable<Vector3> GetPoints()
    {
        yield return point0;
        yield return point1;
        yield return point2;
        yield return point3;
        yield return point4;
        yield return point5;
        yield return point6;
        yield return point7;
        yield return point8;
        yield return point9;
        yield return point10;
        yield return point11;
        yield return point12;
        yield return point13;
        yield return point14;
        yield return point15;
        yield return point16;
        yield return point17;
        yield return point18;
        yield return point19;
        yield return point20;
    }

}

public class HandInterface : MonoBehaviour
{
    HandPoints HandPoints;

    // Multiplier for raw position data
    // stops points from being so clustered
    // TODO: fix offset caused by multiplier
    const int distanceMultiplier = -10;

    [Header("Hand Transforms")]
    public Hand handTransforms;

    public void UpdateHandPositions(string json)
    {
        HandPoints.PointsFromJson(json);

        // iterating through the received positions to update the in-game transforms
        int index = 0;
        foreach (Vector3 point in HandPoints.GetPoints())
        {
            // Debug.Log(point);
            handTransforms.SetChildPosition(index, point * distanceMultiplier);
            index++;
        }
    }
}
