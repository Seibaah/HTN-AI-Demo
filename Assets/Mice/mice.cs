using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mice : MonoBehaviour
{
    public List<Vector3> targets;
    public float max_speed=3f, max_force=2f;

    private GameObject player;
    private bool targetReached = false;
    private Vector3 targetV;
    private Vector3 currentVelocity;
    private Vector3 dir;
    Vector3 collisAvoidDir;
    Vector3 collisAvoidF;
    private Rigidbody rb;
    private SphereCollider sc;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        targets = new List<Vector3>();

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Obstacle"))
        {
            targets.Add(g.transform.position);
        }

        rb = gameObject.AddComponent<Rigidbody>();
        rb.freezeRotation = true;
        sc = gameObject.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        gameObject.GetComponent<MeshRenderer>().material.color = Color.red;

        StartCoroutine(GetNewTarget());
    }

    //Avoidance behvior inspired from: https://github.com/SebLague/Boids/blob/master/Assets/Scripts/Boid.cs
    private void Update()
    {
        if (targetReached == false)
        {
            transform.Translate(dir = Seek(targetV) * Time.deltaTime);

            if (HeadedForCollision(9, 0.2f))
            {
                collisAvoidDir = ObstacleRays();
                collisAvoidF = SteerTowards(collisAvoidDir) * 2f;
            }
        }
        //avoid player, monster and other mice
        if (HeadedForCollision(10, 2f))
        {
            collisAvoidDir = ObstacleRays();
            collisAvoidF = SteerTowards(collisAvoidDir) * 2f;
        }
        if (HeadedForCollision(11, 2f))
        {
            collisAvoidDir = ObstacleRays();
            collisAvoidF = SteerTowards(collisAvoidDir) * 2f;
        }
        if (HeadedForCollision(13, 0.5f))
        {
            collisAvoidDir = ObstacleRays();
            collisAvoidF = SteerTowards(collisAvoidDir) * 2f;
        }
        //stop when near target
        if (Vector3.Distance(gameObject.transform.position, targetV) < 2f)
        {
            currentVelocity = Vector3.zero;
            targetReached = true;
        }
    }

    //Based: https://github.com/SebLague/Boids/blob/master/Assets/Scripts/Boid.cs
    private Vector3 SteerTowards(Vector3 v)
    {
        Vector3 v2 = v.normalized * max_speed - currentVelocity;
        return Vector3.ClampMagnitude(v, max_force);
    }

    //raycast in a circle to find a free direction to steer towards
    //Based on: https://github.com/SebLague/Boids/blob/master/Assets/Scripts/Boid.cs
    private Vector3 ObstacleRays()
    {
        Vector3[] rayDirs = circularRaycast.dirs;

        for (int i =0; i <rayDirs.Length; i++)
        {
            Vector3 d = gameObject.transform.TransformDirection(rayDirs[i]);
            Ray r = new Ray(gameObject.transform.position, d);
            if (!Physics.Raycast(r, 1f, 9))
            {
                return d;
            }
        }

        return dir;
    }

    //Raycasts in fron to check for collision
    //Based on: https://github.com/SebLague/Boids/blob/master/Assets/Scripts/Boid.cs
    private bool HeadedForCollision(int n, float r)
    {
        RaycastHit hit;
        if (Physics.SphereCast(gameObject.transform.position, r, dir, out hit, n))
        {
            return true;
        }
        return false;
    }

    //Based on: https://www.askforgametask.com/tutorial/steering-behaviors-seek/
    private Vector3 Seek(Vector3 v)
    {
        Vector3 desiredVel = v - gameObject.transform.position;
        desiredVel.Normalize();
        desiredVel *= max_speed;

        Vector3 steeringF = desiredVel - rb.velocity;

        if (steeringF.sqrMagnitude > max_speed * max_force)
        {
            steeringF.Normalize();
            steeringF *= max_force;
        }

        return steeringF;
    }

    //Coroutine to assign a new seek target
    private IEnumerator GetNewTarget()
    {
        while (true)
        {
            int n = Random.Range(0, targets.Count);
            targetV = targets[n];
            yield return new WaitForSecondsRealtime(2f);
            targetReached = false;
            if (player.GetComponent<player_controller>().hasWon == true || player.GetComponent<player_controller>().player_alive == false)
            {
                StopAllCoroutines();
            }
        }
    }

    //mice are destroyed by throwable objects in movement or the monster stomping on them
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Monster")
        {
            Destroy(gameObject);
        }
        if (other.gameObject.tag == "Obstacle" && other.gameObject.GetComponent<Rigidbody>().velocity != Vector3.zero)
        {
            Destroy(gameObject);
        }
    }
}


