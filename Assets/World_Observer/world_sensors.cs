using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class world_sensors : MonoBehaviour
{
    public GameObject player, monster;
    public player_controller p;
    public monster_controller m;
    public world_state ws;
    public LayerMask layer;

    private int count=0;

    private void Start()
    {
        p = player.GetComponent<player_controller>();
        m = monster.GetComponent<monster_controller>();
        ws = new world_state();

        StartCoroutine(UpdateSensors());
    }

    //world observer holds the most up to date world state data
    IEnumerator UpdateSensors()
    {
        while (ws.player_alive)
        {            
            ws.player_alive = p.player_alive;
            ws.player_in_range = IsPlayerInRange();

            //once an initial world state is calculated we start the htn planner
            if (count == 0) StartCoroutine(m.HTNplanner());
            count++;

            yield return new WaitForSecondsRealtime(0.1f);
        }
        yield break;
    }

    //test if player is in aggro range
    private bool IsPlayerInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(monster.transform.position, 5f);
        foreach (Collider c in hitColliders)
        {
            if (c.tag == "Player") return true;
        }
        return false;
    }
}

public class world_state
{
    public bool player_alive;
    public bool player_in_range;

    public world_state()
    {
        player_alive = true;
        player_in_range = false;
    }

    public override string ToString() => $"({player_alive}, {player_in_range})";
}
