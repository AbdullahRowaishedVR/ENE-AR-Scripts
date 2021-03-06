﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geolocation : MonoBehaviour
{
    private UserInterface UI;

    private float longitude, latitude;

    public float Longitude { get => longitude; set => longitude = value; }
    public float Latitude { get => latitude; set => latitude = value; }

    private void Awake()
    {
        UI = GameObject.Find("UserInterface").GetComponent<UserInterface>();
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        IEnumerator returnValue = LocationInit();
        //DEBUG
        //latitude = 26.329872f;
        //longitude = 50.109161f;
        return returnValue;
    }

    private IEnumerator LocationInit()
    {
        if(!Input.location.isEnabledByUser) {
            yield break;
        }

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            UI.UpdateGPS("Timed out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            UI.UpdateGPS("Unable to determine device location");
            yield break;
        }
        else
        {
            UI.UpdateGPS((Latitude = Input.location.lastData.latitude) + ", " + (Longitude = Input.location.lastData.longitude));
        }

        Input.location.Stop();

        
        yield return null;
    }

}



