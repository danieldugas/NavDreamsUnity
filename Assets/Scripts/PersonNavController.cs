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
        return gameObject.active;
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
