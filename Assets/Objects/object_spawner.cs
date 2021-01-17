using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class object_spawner : MonoBehaviour
{
    public int numOfRocks, numOfCrates;
    public GameObject grid0, gridF, crate, rock, objSpawner;

    private int rows, cols;
    private float x0, z0, zf, xf;
    private int[,] grid;
    private enum objects
    {
        crates=1,
        rocks=2
    }

    //this script spawns a defined number of objects in an area defined by grid0 and gridF
    private void Start()
    {
        x0 = grid0.transform.position.x;
        z0 = grid0.transform.position.z;

        xf = gridF.transform.position.x;       
        zf = gridF.transform.position.z;

        rows = (int)Mathf.Abs((zf - z0));
        cols = (int)Mathf.Abs((xf - x0));

        grid = new int[rows, cols];

        SetObjectsSpawn(crate, numOfCrates, (int)objects.crates);
        SetObjectsSpawn(rock, numOfRocks, (int)objects.rocks);

        SpawnObjects();
    }

    //a discrete representation of the area is used to set the objects random spawn
    private void SetObjectsSpawn(GameObject o, int n, int code)
    {
        while (n>0)
        {
            int x = Random.Range(0, cols);
            int z = Random.Range(0, rows);

            if (grid[z, x] != 0)
            {
                continue;
            }
            else
            {
                grid[z, x] = code;
            }

            n--;
        }
    }

    //Using the discrete representation rocks and crates are spawned
    private void SpawnObjects()
    {
        for (int i=0; i<rows; i++)
        {
            for (int j=0; j<cols; j++)
            {
                if (grid[i, j] == 1)
                {
                    Vector3 spawn = new Vector3(-j + x0, 0.0f, i);
                    GameObject o = Instantiate(crate, spawn, Quaternion.identity);
                    o.transform.parent = objSpawner.transform;
                    o.AddComponent<throwable_crate>();
                    o.name = "Crate";
                    o.tag = "Obstacle";
                    o.layer = 9;
                    NavMeshObstacle navObs = o.AddComponent<NavMeshObstacle>();
                    navObs.carving = true;
                    navObs.carveOnlyStationary = false;
                    navObs.size = new Vector3(0.75f, 0.65f, 0.75f);
                    navObs.center += new Vector3(0.0f, 0.32f, 0.0f);
                }
                if (grid[i, j] == 2)
                {
                    Vector3 spawn = new Vector3(-j + x0, 0.4f, i);
                    GameObject o = Instantiate(rock, spawn, Quaternion.identity);
                    o.transform.parent = objSpawner.transform;
                    o.AddComponent<throwable_rock>();
                    o.name = "Rock";
                    o.tag = "Obstacle";
                    o.layer = 9;
                    NavMeshObstacle navObs = o.AddComponent<NavMeshObstacle>();
                    navObs.carving = true;
                    navObs.carveOnlyStationary = false;
                    navObs.size = new Vector3(0.8f, 0.8f, 0.8f);
                }
            }
        }
    }
}
