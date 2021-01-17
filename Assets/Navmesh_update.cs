using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Navmesh_update : MonoBehaviour
{
    public NavMeshSurface surface;

    //build navmesh at run time
    void Start()
    {
        surface.BuildNavMesh();
    }

}
