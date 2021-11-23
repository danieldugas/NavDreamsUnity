using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LabStaticEnvironmentController : EnvironmentController
{
    public GameObject spawns; // The places to spawn agents / goals
    override public void OnEpisodeBegin(int n_robots,
                                        int difficulty,
                                        int n_people,
                                        out Vector3[] robot_positions,
                                        out Vector3[] robot_goals,
                                        out Vector3[] people_positions,
                                        out Vector3[] people_goals)
    {
        // hide and reset spawn points
        foreach (Transform child in spawns.transform)
        { 
            child.gameObject.GetComponent<Renderer>().enabled = false;
            child.gameObject.GetComponent<Collider>().enabled = false;
            child.gameObject.GetComponent<SpawnPoint>().occupied = false;
        }
        
        // this is a static environment - nothing to do
        robot_positions = new Vector3[n_robots];
        robot_goals = new Vector3[n_robots];
        people_positions = new Vector3[n_people];
        people_goals = new Vector3[n_people];
        // fill positions using spawn
        for (int i = 0; i < n_people; i++)
        {
            people_positions[i] = SampleSpawn(true);  
        }
        for (int i = 0; i < n_robots; i++)
        {
            robot_positions[i] = SampleSpawn(true);
        }
        for (int i = 0; i < n_people; i++)
        {
            people_goals[i] = SampleSpawn();
        }
        for (int i = 0; i < n_robots; i++)
        {
            robot_goals[i] = SampleSpawn();
        }

    }
    override public Vector3 SamplePersonGoal(Vector3 person_position)
    {
        return SampleSpawn();
    }
    override public Vector3 SampleRobotGoal(Vector3 robot_position)
    {
        return SampleSpawn();
    }

    private Vector3 SampleSpawn(bool occupy=false)
    {
        // list all children of the spawns object
        // and pick one at random
        int n_spawns = spawns.transform.childCount;
        if (n_spawns == 0) {
            Debug.LogError("No spawn points in scene");
            return Vector3.zero;
        }
        bool found = false;
        Transform spawn = null;
        foreach (int spawn_idx in ShuffledRange(n_spawns))
        {
            spawn = spawns.transform.GetChild(spawn_idx);
            if (!spawn.gameObject.GetComponent<SpawnPoint>().occupied)
            {
                found = true;
                break;
            }
        }
        if (!found)
        {
            Debug.LogError("No free spawn points in scene");
            return Vector3.zero;
        }
        if (occupy) {
            spawn.gameObject.GetComponent<SpawnPoint>().occupied = true;
        }
        Vector3 pos = spawn.position;
        return pos;
    }

    private static List<int> ShuffledRange(int max) {
        List<int> list = new List<int>();
        for (int i = 0; i < max; i++) {
            list.Add(i);
        }
        for (int i = list.Count; i > 1; i -= 1)
        {
            int j = Random.Range(0, i); 
            int temp = list[j];
            list[j] = list[i - 1];
            list[i - 1] = temp;
        }
        return list;
    }

}
