using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class just passes orders from the ml-agents script to the individual people in the crowd. Complex logic should be implemented in the person controller.
public class PeopleNavController : MonoBehaviour
{
    public void OnEpisodeBegin(int n_people, Vector3[] people_positions, Vector3[] people_goals)
    {
        int i = 0;
        // Get starting positions for people
        foreach (PersonNavController person in transform.GetComponentsInChildren<PersonNavController>())
        {
            if (i >= n_people)
            {
                person.RemoveFromScene();
            }
            person.AddToScene();
            person.Reset(people_positions[i], people_goals[i], 60.0f);
            i++;
        }
    }
    public void DoNavStep(EnvironmentController environmentController, float timestep)
    {
        foreach (PersonNavController person in transform.GetComponentsInChildren<PersonNavController>())
        {
            if (person.IsInScene())
            {
                person.DoNavStep(environmentController, timestep);
            }
        }
    }
}
