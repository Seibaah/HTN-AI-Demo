using Packages.Rider.Editor.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_controller : MonoBehaviour
{
    public bool canPlay = true, hasTreasure = false, hasWon = false;
    public int health = 2;
    public float speed;
    public bool player_alive = true;
    public GameObject shield;
    public CharacterController controller;
    public Transform groundCheck;
    public LayerMask groundMask, rigidMask;

    private int energy = 10;
    private float gravity = -900f, groundDistance = 0.2f;
    private bool isGrounded;
    private Vector3 velocity;

    private void Start()
    {
        StartCoroutine("ShieldEnergy");
    }

    /*FPS camera script. Credits to Brackeys at https://www.youtube.com/watch?v=_QajrabyTJc 
     Slight modifications were made to interrupt user input on win or loss*/
    private void Update()
    {
        if (Physics.CheckSphere(groundCheck.position, groundDistance, groundMask) ||
            Physics.CheckSphere(groundCheck.position, groundDistance, rigidMask)) {
            isGrounded = true;
        }
        else isGrounded = false;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
        }

        
        if (canPlay == true)
        {
            // player movement - forward, backward, left, right
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Vector3 move = transform.right * 1.2f * x + transform.forward * z * 1.2f;

            controller.Move(move * speed * Time.deltaTime);

            velocity.y += 1.5f * gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && energy > 0)
            {
                shield.SetActive(!shield.activeSelf);
            }

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject.tag == "Treasure")
                    {
                        hasTreasure = true;
                        Destroy(hit.collider.gameObject);
                    }
                }
            }
        }
    }

    //manages shield ressource
    IEnumerator ShieldEnergy()
    {
        while (true)
        {
            if (shield.activeSelf)
            {
                energy--;
            }

            if (energy == 0)
            {
                shield.SetActive(false);
                yield break;
            }

            yield return new WaitForSecondsRealtime(1.0f);
        }
    }

    //player stats
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 150, 50), "Shield energy: " + energy.ToString());
        GUI.Label(new Rect(10, 30, 150, 50), "Shield active: " + shield.activeSelf.ToString());
        GUI.Label(new Rect(10, 50, 150, 50), "Health: " + health.ToString());
    }

    //collision with thrown obj
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            if (shield.activeSelf == true)
            {
                Destroy(collision.gameObject);
            }
            else
            {
                health--;
                if (health < 1)
                {
                    player_alive = false;
                    canPlay = false;
                    StopAllCoroutines();
                }
            }
        }
    }
}
