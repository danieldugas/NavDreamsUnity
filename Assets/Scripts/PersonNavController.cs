using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This controls all the behavior of a single person agent. The only external input are the possible goal locations (environment dependent),
// The person should sample possible goals, select one and choose its own behavior and path.
public class PersonNavController : MonoBehaviour
{
    [Header("Debug Attributes (Leave unchanged)")]
    [Tooltip("Will be created if set to None")]
    public GameObject currentGoal;
    public float waitAtGoalTimeRemaining;
    public float kGoalReachedDist = 1.0f; // vertical distance included!
    public bool Waiting = false; // If true, the person is waiting at a goal.

    void Start()
    {
        // Add navmeshagent
        UnityEngine.AI.NavMeshAgent nma = this.gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>() as UnityEngine.AI.NavMeshAgent;
        nma.radius = 0.3f;
        nma.height = 1.8f;
        nma.speed = 1.0f;
        // Add capsule colliders to important limbs and trunk (compromise between accuracy and precision)
        // This is specific to rocketbox joint chain and will fail if gameobject hierarcy is different
        try {
        Transform bip = transform.Find("Bip01");
        Transform lforearm = transform.Find("Bip01").Find("Bip01 Pelvis").Find("Bip01 Spine").Find("Bip01 Spine1").Find("Bip01 Spine2").Find("Bip01 L Clavicle").Find("Bip01 L UpperArm").Find("Bip01 L Forearm");
        Transform lcalf = transform.Find("Bip01").Find("Bip01 Pelvis").Find("Bip01 L Thigh").Find("Bip01 L Calf");
        Transform lfoot = lcalf.Find("Bip01 L Foot");
        Transform rforearm = transform.Find("Bip01").Find("Bip01 Pelvis").Find("Bip01 Spine").Find("Bip01 Spine1").Find("Bip01 Spine2").Find("Bip01 R Clavicle").Find("Bip01 R UpperArm").Find("Bip01 R Forearm");
        Transform rcalf = transform.Find("Bip01").Find("Bip01 Pelvis").Find("Bip01 R Thigh").Find("Bip01 R Calf");
        Transform rfoot = rcalf.Find("Bip01 R Foot");
        CapsuleCollider ccBip = bip.gameObject.AddComponent<CapsuleCollider>() as CapsuleCollider;
        ccBip.center = new Vector3(0.0f, 0.0f, 0.3f);
        ccBip.radius = 0.15f;
        ccBip.height = 1.0f;
        ccBip.direction = 2;
        CapsuleCollider ccLForearm = lforearm.gameObject.AddComponent<CapsuleCollider>() as CapsuleCollider;
        ccLForearm.center = new Vector3(-0.15f, 0.0f, 0.0f);
        ccLForearm.radius = 0.03f;
        ccLForearm.height = 0.5f;
        ccLForearm.direction = 0;
        CapsuleCollider ccLCalf = lcalf.gameObject.AddComponent<CapsuleCollider>() as CapsuleCollider;
        ccLCalf.center = new Vector3(-0.2f, 0.0f, 0.0f);
        ccLCalf.radius = 0.06f;
        ccLCalf.height = 0.5f;
        ccLCalf.direction = 0;
        CapsuleCollider ccLFoot = lfoot.gameObject.AddComponent<CapsuleCollider>() as CapsuleCollider;
        ccLFoot.radius = 0.05f;
        ccLFoot.center = new Vector3(-0.1f, 0.1f, 0.0f);
        ccLFoot.height = 0.2f;
        ccLFoot.direction = 1;
        CapsuleCollider ccRForearm = rforearm.gameObject.AddComponent<CapsuleCollider>() as CapsuleCollider;
        ccRForearm.center = new Vector3(-0.15f, 0.0f, 0.0f);
        ccRForearm.radius = 0.03f;
        ccRForearm.height = 0.5f;
        ccRForearm.direction = 0;
        CapsuleCollider ccRCalf = rcalf.gameObject.AddComponent<CapsuleCollider>() as CapsuleCollider;
        ccRCalf.center = new Vector3(-0.2f, 0.0f, 0.0f);
        ccRCalf.radius = 0.06f;
        ccRCalf.height = 0.5f;
        ccRCalf.direction = 0;
        CapsuleCollider ccRFoot = rfoot.gameObject.AddComponent<CapsuleCollider>() as CapsuleCollider;
        ccRFoot.radius = 0.05f;
        ccRFoot.center = new Vector3(-0.1f, 0.1f, 0.0f);
        ccRFoot.height = 0.2f;
        ccRFoot.direction = 1;
        }
        // catch null reference except
        catch (System.NullReferenceException e) {
            Debug.Log("Error: " + e.Message);
            this.transform.SetParent(null);
        }
    }

    // Called by the ML-Agents environment to tell the person to reset itself
    // duration is the time in seconds that the goal remains valid, even if reached (used to give idle tasks)
    public void Reset(Vector3 position, Vector3 goal)
    {
        // Update own position
        transform.position = position;
        // Create goal object if it does not exist
        if (currentGoal == null)
        {
            currentGoal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            currentGoal.GetComponent<Collider>().enabled = false;
            currentGoal.GetComponent<Renderer>().enabled = false;
        }
        //
        UpdateGoal(goal);
        
    }
    public void RemoveFromScene()
    {
        gameObject.SetActive(false);
    }
    public void AddToScene()
    {
        gameObject.SetActive(true);
    }
    public bool IsInScene()
    {
        return gameObject.activeInHierarchy;
    }
    public void DoNavStep(EnvironmentController environmentController, float timestep)
    {
        // NavMeshAgent takes care of actual movement, we just set destinations when necessary
        // check if goal is reached
        if ((currentGoal.transform.position - this.transform.position).magnitude < kGoalReachedDist) {
            waitAtGoalTimeRemaining -= timestep;
            if (waitAtGoalTimeRemaining <= 0.0f)
            {
                // Ask for a new goal
                Vector3 goal = environmentController.SamplePersonGoal(this.transform.position);
                UpdateGoal(goal);
            } else {
                if (!Waiting) {
                    // Wait for a while (enable wait animation)
                    UnityEngine.AI.NavMeshAgent nma = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    nma.speed = 0.0f;
                    this.GetComponent<Animator>().SetFloat("Speed", 0.0f);
                    this.GetComponent<Animator>().SetInteger("AnimationType", Random.Range(1, 4));
                    Waiting = true;
                }
            }
        }
        
    }

    // takes the new goal position and samples a wait time and speed, then directs the navmeshagent and animator 
    private void UpdateGoal(Vector3 goal)
    {
        Waiting = false;
        currentGoal.transform.position = goal;
        waitAtGoalTimeRemaining = SampleGoalWaitTime();
        // Update navmesh agent
        UnityEngine.AI.NavMeshAgent nma = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
        nma.destination = currentGoal.transform.position;
        nma.speed = SampleSpeed();
        // update animation to walking
        this.GetComponent<Animator>().SetFloat("Speed", nma.speed);
    }

    // returns a random speed within the range of possible agent speeds
    private float SampleSpeed()
    {
        return Random.Range(0.5f, 1.0f);
    }

    // returns a random wait time at goal within the range of possible wait times
    private float SampleGoalWaitTime()
    {
        return Random.Range(0.0f, 60.0f);
    }
    
}
