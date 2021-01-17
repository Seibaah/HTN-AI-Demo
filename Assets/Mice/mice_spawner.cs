using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mice_spawner : MonoBehaviour
{
    public int n;
    public List<GameObject> miceList;

    private void Start()
    {
        miceList = new List<GameObject>();

        SpawnMice();
    }

    //Spawn mice
    private void SpawnMice()
    {
        for (int i=0; i<n; i++)
        {
            GameObject m = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            m.name = "mouse " + i;
            m.transform.position = new Vector3(-11f, 0f, 16f);
            m.transform.parent = gameObject.transform;
            m.layer = 13;
            m.AddComponent<mice>();
            miceList.Add(m);
        }
    }
}

//Code inspired from: https://github.com/SebLague/Boids/blob/master/Assets/Scripts/BoidHelper.cs
public static class circularRaycast
{
    const int viewDirs = 24;
    public static readonly Vector3[] dirs;

    static circularRaycast()
    {
        dirs = new Vector3[circularRaycast.viewDirs];

        float angleIncrement = Mathf.PI * 15 / 180;

        for (int i=0; i< viewDirs; i++)
        {
            float azimuth = angleIncrement * i;
            dirs[i] = new Vector3(Mathf.Cos(azimuth), 0, Mathf.Sin(azimuth));
        }
    }
}
