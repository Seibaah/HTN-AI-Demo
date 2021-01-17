using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class monster_controller : MonoBehaviour
{
    public GameObject player;
    public NavMeshAgent agent;
    public Vector3 dest = Vector3.zero;
    public GameObject world_observer;
    public world_sensors s;
    public monster_htn htn_tree;
    public AudioSource audioS;
    public GameObject pickUpPos;

    private bool planIsValid = true, taskDone = false, planDone = false,
        planDropped = false, isPathfinding = false, pickingUpObj = false, wsChange = false, recovering = false;
    private int count=0;
    private string taskN;
    private MeshRenderer rend;
    private List<Collider> objects;
    private Queue<node> tasks;
    private Queue<string> plan;
    private world_state ws;
    private GameObject projectile;
    private Collider pCollider;

    private void Start()
    {
        //initialize htn tree for the monster
        htn_tree = new monster_htn();

        audioS = GetComponent<AudioSource>();
        rend = gameObject.GetComponent<MeshRenderer>();
        rend.material.color = Color.cyan;
    }

    //update keeps track of the pathfinding status and interrupt
    private void Update()
    {
        //pathfinding to a random point
        if (isPathfinding == true && pickingUpObj == false)
        {
            HasDestinationBeenReached();
            WorldStateHasChanged(ws, s.ws);
        }
        //pathfinding to an object to pick it up
        else if (isPathfinding == true && pickingUpObj == true)
        {
            CloseToObj();
        }
    }

    //test if the object the monster wants to pick up is in close proximity
    private void CloseToObj()
    {
        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, 1f);
       
        foreach (Collider c in hitColliders)
        {
            if (c.transform.position == pCollider.transform.position)
            {
                //update task runner conditions
                taskDone = true;
                isPathfinding = false;
                pickingUpObj = false;
                break;
            }
        }
    }

    //tests a=the world_observer world state against the local saved world state. Acts as an interrupt checked by the HTNplanner
    private void WorldStateHasChanged(world_state w0, world_state wf)
    {
        if (w0.player_alive == wf.player_alive && w0.player_in_range == wf.player_in_range)
        {
            wsChange = false;
        }
        else
        {
            wsChange = true;
            taskDone = true;
        }
    }

    //Code source: https://answers.unity.com/questions/324589/how-can-i-tell-when-a-navmesh-has-reached-its-dest.html?childToView=802211#answer-802211
    private void HasDestinationBeenReached()
    {
        // Check if we've reached the destination
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    taskDone = true;
                    isPathfinding = false;
                }
            }
        }
    }

    //Top most level of the planner. Builds and runs the plan, with possibility of interrruption.
    public IEnumerator HTNplanner()
    {
        s = world_observer.GetComponent<world_sensors>();

        while (true)
        {
            BuildPlan();

            if (planIsValid == true) StartCoroutine(PlanRunner());

            //a world state change, a plan completion, a plan invalidity or plan abandonment will cause a replan. Timeout period enforced.
            yield return new WaitUntil(() => (wsChange == true || planDone == true || planIsValid == false || planDropped == true) && recovering == false);
            //reset the flags
            planDone = false;
            planIsValid = true;
            planDropped = false;
            if (projectile != null) projectile.GetComponent<Rigidbody>().isKinematic = false;

            if(player.GetComponent<player_controller>().hasWon==true || player.GetComponent<player_controller>().player_alive == false)
            {
                StopAllCoroutines();
                yield break;
            }
        }
    }

    //HTN forward plan builder
    private void BuildPlan()
    {
        node cur;
        tasks = new Queue<node>();
        plan = new Queue<string>();
        tasks.Enqueue(htn_tree.rootNode);
        while (tasks.Count > 0)
        {
            bool preConditionsMet = false;
            cur = tasks.Dequeue();

            //rootnode is a compound task we need to evaluate regardless of wolrd state
            if (cur.t == node.type.Root)    
            {
                compound_task c_cur = (compound_task)cur;
                LoadChildren(c_cur.children);
            }
            //normal compound nodes extraction and evaluation
            else if (cur.t == node.type.Compound)
            {
                compound_task c_cur = (compound_task)cur;
                preConditionsMet = EvalCompoundPreConditions(c_cur.behaviorName);

                if (preConditionsMet == true)
                {
                    //in a sequence all tasks are evaluated
                    if (c_cur.st == compound_task.subType.Sequence) 
                    {
                        LoadChildren(c_cur.children); 
                    }
                    //in the selector 1 random task is evaluated
                    else if (c_cur.st == compound_task.subType.Selector)
                    {
                        int index = Random.Range(0, c_cur.children.Count);
                        List<node> l = new List<node>();
                        l.Add(c_cur.children[index]);
                        LoadChildren(l);
                    }
                }
            }
            //primitive tasks preconditions are evaluated one last time before being added to the execution plan. A failure triggers a replan.
            else if (cur.t == node.type.Primitive)
            {
                primitive_task p_cur = (primitive_task)cur;
                preConditionsMet = EvalPrimitivePreConditions(p_cur.taskName);

                if (preConditionsMet == true)
                {
                    plan.Enqueue(p_cur.taskName);
                }
                else
                {
                    planIsValid = false;
                    break;
                }
            }
        }
        
    }  

    //evals primitive tasks preconditions against the world state. 
    private bool EvalPrimitivePreConditions(string taskName)
    {
        ws = s.ws;
        switch (taskName)
        {
            case "setNavDest":
                if (ws.player_alive == true && ws.player_in_range == false) return true;
                else return false;
            case "growl":
                if (ws.player_alive == true) return true;
                else return false;
            case "idle":
                if (ws.player_alive == true && ws.player_in_range == false) return true;
                else return false;
            case "walk":
                if (ws.player_alive == true && ws.player_in_range == false) return true;
                else return false;
            case "run":
                if (ws.player_alive == true && ws.player_in_range == false) return true;
                else return false;
            case "getAngry":
                if (ws.player_alive == true && ws.player_in_range == true) return true;
                else return false;
             case "findObj":
                if (ws.player_alive == true && ws.player_in_range == true) return true;
                else return false;
            case "navToObj":
                if (ws.player_alive == true) return true;
                else return false;
            case "pickUp":
                if (ws.player_alive == true) return true;
                else return false;
            case "recover":
                if (ws.player_alive == true) return true;
                else return false;
            case "objectThrow":
                if (ws.player_alive == true) return true;
                else return false;
            default:
                return false;
        }
    }

    //evals compound tasks preconditions against the world state. 
    private bool EvalCompoundPreConditions(string taskName)
    {
        ws = s.ws;
        switch (taskName)
        {
            case "wander":
                if (ws.player_alive == true && ws.player_in_range == false) return true;
                else return false;
            case "attack":
                if (ws.player_alive == true && ws.player_in_range == true) return true;
                else return false;
            case "navSpeed":
                if (ws.player_alive == true && ws.player_in_range == false) return true;
                else return false;
            case "findObjectToThrow":
                if (ws.player_alive == true && ws.player_in_range == true) return true;
                else return false;
            case "throw":
                if (ws.player_alive == true && ws.player_in_range == true) return true;
                else return false;
            case "superThrow":
                if (ws.player_alive == true && ws.player_in_range == true) return true;
                else return false;
            default:
                return false;
        }
    }

    //replaces a compound tasks by its selected children to be evaluated next by the plan builder
    private void LoadChildren(List<node> l)
    {
        Queue<node> q = new Queue<node>();
        foreach (node n in l) q.Enqueue(n);
        foreach (node n in tasks) q.Enqueue(n);
        tasks = q;
    }

    //reevaluates each plan entry before execution. If still valid the task is executed. Pacing enforced.
    private IEnumerator PlanRunner()
    {
        foreach (string s in plan)
        {
            if (EvalPrimitivePreConditions(s) == true)
            {
                Exec(s);
                yield return new WaitUntil(() => taskDone == true);
                taskDone = false;
            }
            else
            {
                planDropped = true;
                yield break;
            }

        }
        planDone = true;
    }

    //Calls the specific methods for each task
    private void Exec(string taskName)
    {
        taskN = taskName;
        if (taskName == "walk")
        {
            ColorChange(Color.green);
            NavSpeed(2f);
            taskDone = true;
        }
        else if (taskName == "run")
        {
            ColorChange(Color.green);
            NavSpeed(5f);
            taskDone = true;
        }
        else if (taskName == "setNavDest")
        {
            Walk();
        }
        else if (taskName == "idle")
        {
            ColorChange(Color.cyan);
            StartCoroutine(Rest());
        }
        else if (taskName == "getAngry")
        {
            ColorChange(Color.red);
            taskDone = true;
        }
        else if (taskName == "findObj")
        {
            pCollider = ChooseObj();
            taskDone = true;
        }
        else if (taskName == "navToObj")
        {
            NavToObj(pCollider);
        }
        else if (taskName == "pickUp")
        {
            PickUp(projectile);
            taskDone = true;
        }
        else if (taskName == "growl")
        {
            Growl();
            taskDone = true;
        }
        else if (taskName == "objectThrow")
        {
            Throw(projectile);
            recovering = true;
            taskDone = true;
        }
        else if (taskName == "recover")
        {
            ColorChange(Color.yellow);
            StartCoroutine(Rest());
            recovering = false;
        }

    }

    //updates navmesh components and applies a directed force to the thrown obj
    private void Throw(GameObject g)
    {
        g.GetComponent<Rigidbody>().useGravity = true;
        g.GetComponent<Rigidbody>().freezeRotation = false;
        NavMeshObstacle navObs = g.GetComponent<NavMeshObstacle>();
        navObs.carving = true;

        Vector3 u = transform.position;
        Vector3 v = player.transform.position;
        Vector3 dir = v - u;

        g.GetComponent<Rigidbody>().AddForce(dir.normalized * 500f);
    }

    //navigates closer to the object to be picked up
    private void NavToObj(Collider c)
    {
        projectile = c.gameObject;
        NavMeshObstacle navObs = projectile.GetComponent<NavMeshObstacle>();
        navObs.carving = false;

        isPathfinding = true;
        pickingUpObj = true;
        agent.SetDestination(projectile.transform.position);
    }

    //picks up an object to throw
    private void PickUp(GameObject g) 
    {
        g.GetComponent<Rigidbody>().useGravity = false;
        g.GetComponent<Rigidbody>().freezeRotation = true;
        g.GetComponent<Rigidbody>().isKinematic = false;

        projectile.transform.position = pickUpPos.transform.position;
    }

    //chooses closest valid object to throw at the player
    //Code inspired from: https://stackoverflow.com/questions/59505652/is-there-a-specific-order-to-the-colliders-that-physics-overlapsphere-returns
    private Collider ChooseObj()
    {
        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, 20f);
        Collider[] sortedColliders = hitColliders.OrderBy(c => Vector3.Distance(gameObject.transform.position, c.transform.position)).ToArray();
        foreach (Collider c in sortedColliders)
        {
            if (c.tag == "Obstacle") return c;
        }
        return null;
    } 

    //puts the planner on hold to rest. Lets the player breath a little.
    private IEnumerator Rest()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        taskDone = true;
        yield break;
    }

    //toString alternative of the active monster plan
    private string PrintQueue()
    {
        string msg = "";
        foreach (string s in plan) msg += s + "-";
        //Debug.Log(count++ + " " + msg);
        return msg;
    }

    //enables wandering around in the navmesh
    private void Walk()
    {
        isPathfinding = true;
        while (!RandomNavmeshLocation(8f)) ;
        agent.SetDestination(dest);
    }

    //sets a random destination in the navmesh to wander around
    //Code based from: https://answers.unity.com/questions/475066/how-to-get-a-random-point-on-navmesh.html
    public bool RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        dest = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            dest = hit.position;
            return true;
        }
        return false;
    }

    //sets the navigation speed of the pathfinding agent
    private void NavSpeed (float sp)
    {
        agent.speed = sp;
    }

    //plays a growl audio
    private void Growl()
    {
        audioS.PlayOneShot(audioS.clip);
    }

    //changes the AI color to better show behavior state
    private void ColorChange(Color c)
    {
        rend.material.color = c;
    }

    //updates GUI markers of interest
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 300, 200, 50), "Monster plan: " + PrintQueue());
        GUI.Label(new Rect(10, 390, 200, 50), "Monster task: " + taskN);
    }
}
