using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is the base class for all static environments
// It exposes methods to be called by the ML-Agents script
// for setting up the environment at the start of a new episode,
// for getting robot starting positions, and agent starting positions and goals
public abstract class EnvironmentController : MonoBehaviour
{
    // sets up the environment and creates a spawn/goal constellation (all positions in world frame)
    abstract public void OnEpisodeBegin(int n_robots,
                                        int difficulty,
                                        int n_people,
                                        out Vector3[] robot_positions,
                                        out Vector3[] robot_goals,
                                        out Vector3[] people_positions,
                                        out Vector3[] people_goals);
    // Used by person nav controllers to find reachable goals
    abstract public Vector3 SamplePersonGoal(Vector3 person_position);
    // Used by robot agent script to find reachable goals
    abstract public Vector3 SampleRobotGoal(Vector3 robot_position);
}
