using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class wall_spawner : MonoBehaviour
{
    public GameObject grid0, gridF, crate, rock, objSpawner;

    private int rows, cols;
    private float x0, z0, zf, xf;
    private int[,] grid;

    private enum objects
    {
        crates = 1,
        rocks = 2
    }

    //Similar to te object spawner but spawns the objects as a contiguous wall
    private void Start()
    {
        x0 = grid0.transform.position.x;
        z0 = grid0.transform.position.z;

        xf = gridF.transform.position.x;
        zf = gridF.transform.position.z;

        rows = (int)Mathf.Abs((zf - z0));
        cols = (int)Mathf.Abs((xf - x0));

        grid = new int[rows, cols];

        SpawnWall();

        SpawnObjects();
    }

    //set spawn location in discrete representation
    private void SpawnWall()
    {
        int curX = 0, curZ = 0;
        while (curX < cols && curZ < rows)
        {
            grid[curZ, curX] = UnityEngine.Random.Range(1, 3);
            int dir = UnityEngine.Random.Range(1, 4);
            if (dir == 1)
            {
                curX++;
            }
            else if (dir == 2)
            {
                curX++; curZ++;
            }
            else if (dir == 3)
            {
                curZ++;
            }
        }
    }

    //spawns the objects in the world
    private void SpawnObjects()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (grid[i, j] == 1)
                {
                    Vector3 spawn = new Vector3(-j + x0, 0.0f, 17 - i);
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
                    Vector3 spawn = new Vector3(-j + x0, 0.4f, 17 - i);
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
