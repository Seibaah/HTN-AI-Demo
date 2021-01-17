using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class exit : MonoBehaviour
{
    public player_controller p;

    //tests for exit with the treasure chest
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && p.hasTreasure == true)
        {
            p.hasWon = true;
            p.canPlay = false;
        }
    }
}
