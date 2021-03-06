﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatureDraw : MonoBehaviour
{
    [Tooltip("Database of Geometries")]
    public FeatureData features;

    [Tooltip("Geolocation Script")]
    public Geolocation geolocation;

    [Tooltip("Segment Pool")]
    public SegmentPooling segmentPool;

    public void DrawFeatures()
    {
        foreach (GIS_Feature feature in features.database.geometries)
        {
            if (feature is GIS_Feature.GIS_LineString)
            {

                GIS_Feature.GIS_LineString fiber_cable = (GIS_Feature.GIS_LineString) feature;
                GameObject cable = CreateFiberCable(fiber_cable.featureName, fiber_cable.points[0], geolocation);


                bool hasNearPoints = false; //Does this fiber cable fall within the 'goldilock zone'


                for (int i = 0; i < fiber_cable.points.Length - 1; i++)
                {
                    GIS_Feature.GIS_Point firstPoint, secondPoint;
                    float length, angle; //length and angle between two points
                    float[] midpoint;

                    firstPoint = fiber_cable.points[i];
                    secondPoint = fiber_cable.points[i + 1];
                    length = EstimateLength(firstPoint.coordinates, secondPoint.coordinates);
                    midpoint = EstimateMidpoint(firstPoint.coordinates, secondPoint.coordinates);
                    angle = EstimateAngle(firstPoint.coordinates, secondPoint.coordinates);

                    if (length < 500f && !hasNearPoints) {
                        hasNearPoints = true;
                    }

                    CreateFiberSegment(midpoint, length, angle, cable, i+1);
                }

                if (!hasNearPoints)
                {
                    if (cable.activeSelf)
                    {
                        cable.SetActive(false);
                    }
                }
                else
                {
                    if (!cable.activeSelf)
                    {
                        cable.SetActive(true);
                    }
                }
            }
        }
    }

    private float[] EstimateMidpoint(GIS_Feature.Coordinates coordinates1, GIS_Feature.Coordinates coordinates2)
    {
        float mpLat, mpLong, mpLatAct, mpLongAct;
        mpLat = (coordinates2.Latitude + coordinates1.Latitude) / 2;
        mpLong = (coordinates2.Longitude + coordinates1.Longitude) / 2;
        mpLatAct = 111139 * (mpLat - geolocation.Latitude);
        mpLongAct = 111139 * (mpLong - geolocation.Longitude);

        //TODO reference midpoint (In GPS coordinates format) to User GPS location and convert to Unity units.
        return new float[] {mpLatAct, mpLongAct};
    }

    /// <summary>
    /// Creates an empty object with the name 
    /// </summary>
    /// <param name="gIS_Point"></param>
    /// <param name="geolocation"></param>
    /// <returns></returns>
    private GameObject CreateFiberCable(string featureName, GIS_Feature.GIS_Point gIS_Point, Geolocation geolocation)
    {
        float y, x, a;
        y = (gIS_Point.coordinates.Latitude - geolocation.Latitude) * 111139;
        x = (gIS_Point.coordinates.Longitude - geolocation.Longitude) * 111139;
        a = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        GameObject cable = new GameObject(featureName);
        cable.transform.SetParent(transform);
        cable.transform.position = new Vector3(x, 0f, y);
        cable.transform.Rotate(Vector3.up*a);

        return cable;
    }

    private void CreateFiberSegment(float[] midpoint, float length, float angle, GameObject cable, int segmentNo)
    {
        GameObject segment = segmentPool.PoolSegment();

        segment.name = cable.name + ".Segment" + segmentNo;
        segment.transform.parent = cable.transform;

        segment.transform.position = new Vector3(midpoint[1], -4f, midpoint[0]);
        segment.transform.Rotate(Vector3.forward * angle);
        segment.transform.localScale += Vector3.up * length;
    }

    /// <summary>
    /// Uses pythagorean theory to determine length between two XY GIS_Points in planar space.
    /// </summary>
    /// <param name="coordinates1"></param>
    /// <param name="coordinates2"></param>
    /// <returns></returns>
    private float EstimateLength(GIS_Feature.Coordinates coordinates1, GIS_Feature.Coordinates coordinates2)
    {
        return 111139 * Mathf.Sqrt(Mathf.Pow(coordinates2.Latitude - coordinates1.Latitude, 2) + Mathf.Pow(coordinates2.Longitude - coordinates1.Longitude, 2));
    }
    /// <summary>
    /// Uses pythagorean theory to determine the angle between two XY GIS_Points in planar space
    /// </summary>
    /// <param name="coordinates1"></param>
    /// <param name="coordinates2"></param>
    /// <returns></returns>
    private float EstimateAngle(GIS_Feature.Coordinates coordinates1, GIS_Feature.Coordinates coordinates2)
    {
        return Mathf.Rad2Deg * Mathf.Atan2(coordinates2.Latitude - coordinates1.Latitude, coordinates2.Longitude - coordinates1.Longitude);
    }
}
