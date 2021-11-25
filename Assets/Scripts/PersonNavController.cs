using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This controls all the behavior of a single person agent. The only external input are the possible goal locations (environment dependent),
// The person should sample possible goals, select one and choose its own behavior and path.
public class PersonNavController : MonoBehaviour
{
    public GameObject currentGoal;
    public float waitAtGoalTimeRemaining;
    public static float kGoalReachedDist = 0.1f;

    void Start()
    {
        // Add capsule colliders to important limbs and trunk (compromise between accuracy and precision)
        // This is specific to rocketbox joint chain and will fail if gameobject hierarcy is different
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

    // Called by the ML-Agents environment to tell the person to reset itself
    // duration is the time in seconds that the goal remains valid, even if reached (used to give idle tasks)
    public void Reset(Vector3 position, Vector3 goal)
    {
        Destroy(currentGoal);
        currentGoal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        currentGoal.GetComponent<Collider>().enabled = false;
        currentGoal.GetComponent<Renderer>().enabled = false;
        currentGoal.transform.position = goal;
        waitAtGoalTimeRemaining = SampleGoalWaitTime();
        transform.position = position;
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
        this.transform.Translate(new Vector3(0.0f, 0.0f, 0.01f));
        this.transform.Rotate(new Vector3(0.0f, 0.2f, 0.0f));
        this.GetComponent<Animator>().SetFloat("Speed", 0.1f);

        // if goal is reached, ask for a new one or wait for a while
        if ((currentGoal.transform.position - this.transform.position).magnitude < kGoalReachedDist) {
            waitAtGoalTimeRemaining -= timestep;
            if (waitAtGoalTimeRemaining <= 0.0f)
            {
                currentGoal.transform.position = environmentController.SamplePersonGoal(this.transform.position);
                waitAtGoalTimeRemaining = SampleGoalWaitTime();
            }
        }
        
    }

    private float SampleGoalWaitTime()
    {
        return Random.Range(0.0f, 60.0f);
    }
    
}
