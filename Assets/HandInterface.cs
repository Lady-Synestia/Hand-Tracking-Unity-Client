using System;
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
        Debug.Log(json);
        this = JsonUtility.FromJson<HandPoints>(json);
    }

}

public class HandInterface : MonoBehaviour
{
    HandPoints HandPoints;

    // Multiplier for raw position data
    const int distanceMultiplier = 10;

    [Header("Hand Transforms")]
    public Hand handTransforms;

    public void UpdateHandPositions(string json)
    {
        HandPoints.PointsFromJson(json);
        handTransforms.point0.position = HandPoints.point0 * distanceMultiplier;
        handTransforms.point1.position = HandPoints.point1 * distanceMultiplier;
        handTransforms.point2.position = HandPoints.point2 * distanceMultiplier;
        handTransforms.point3.position = HandPoints.point3 * distanceMultiplier;
        handTransforms.point4.position = HandPoints.point4 * distanceMultiplier;
        handTransforms.point5.position = HandPoints.point5 * distanceMultiplier;
        handTransforms.point6.position = HandPoints.point6 * distanceMultiplier;
        handTransforms.point7.position = HandPoints.point7 * distanceMultiplier;
        handTransforms.point8.position = HandPoints.point8 * distanceMultiplier;
        handTransforms.point9.position = HandPoints.point9 * distanceMultiplier;
        handTransforms.point10.position = HandPoints.point10 * distanceMultiplier;
        handTransforms.point11.position = HandPoints.point11 * distanceMultiplier;
        handTransforms.point12.position = HandPoints.point12 * distanceMultiplier;
        handTransforms.point13.position = HandPoints.point13 * distanceMultiplier;
        handTransforms.point14.position = HandPoints.point14 * distanceMultiplier;
        handTransforms.point15.position = HandPoints.point15 * distanceMultiplier;
        handTransforms.point16.position = HandPoints.point16 * distanceMultiplier;
        handTransforms.point17.position = HandPoints.point17 * distanceMultiplier;
        handTransforms.point18.position = HandPoints.point18 * distanceMultiplier;
        handTransforms.point19.position = HandPoints.point19 * distanceMultiplier;
        handTransforms.point20.position = HandPoints.point20 * distanceMultiplier;

    }
}
