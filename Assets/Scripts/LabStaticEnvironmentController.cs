using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabStaticEnvironmentController : EnvironmentController
{
    public GameObject spawns; // The places to spawn agents / goals
    public GameObject[] moveableObjects; // The objects that can be moved
    public List<Vector3> originalPositions; // The original positions of the moveable objects
    public List<Quaternion> originalRotations; // the original rotations of the moveable objects
    public GameObject[] toggleableLights; // The lights that can be toggled
    public float lightDisableProbability = 0.1f; // The probability that each light will be disabled

    void Start()
    {
        // Get the original positions and rotations of the moveable objects
        moveableObjects = GameObject.FindGameObjectsWithTag("MoveableKozeObject");
        originalPositions = new List<Vector3>();
        originalRotations = new List<Quaternion>();
        foreach (GameObject obj in moveableObjects)
        {
            originalPositions.Add(obj.transform.position);
            originalRotations.Add(obj.transform.rotation);
        }
        // Get the toggleable lights
        toggleableLights = GameObject.FindGameObjectsWithTag("ToggleableLight");
    }

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
        
        // this is a static environment - nothing to do with objects / walls
        SampleSceneVariations();
        // fill positions and goals
        float minRobotGoalDist = 2.0f;
        float maxRobotGoalDist = 7.0f + difficulty;
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
        // This can also be removed. It allows reusing spawns as goals, otherwise we need many more spawn points
        foreach (Transform child in spawns.transform)
        {
            child.gameObject.GetComponent<SpawnPoint>().occupied = false;
        }
        for (int i = 0; i < n_people; i++)
        {
            people_goals[i] = SampleSpawn();
        }
        for (int i = 0; i < n_robots; i++)
        {
            robot_goals[i] = SampleSpawnWithinSphereShell(minRobotGoalDist, maxRobotGoalDist, robot_positions[i]);
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
        foreach (int spawn_idx in IterTools.ShuffledRange(n_spawns))
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

    private Vector3 SampleSpawnWithinSphereShell(float minRadius, float maxRadius, Vector3 center, bool occupy=false)
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
        foreach (int spawn_idx in IterTools.ShuffledRange(n_spawns))
        {
            spawn = spawns.transform.GetChild(spawn_idx);
            bool isFree = !spawn.gameObject.GetComponent<SpawnPoint>().occupied;
            bool isInOuterSphere = (spawn.position - center).magnitude < maxRadius;
            bool isOutsideInnerSphere = (spawn.position - center).magnitude > minRadius;
            bool isValid = isFree && isInOuterSphere && isOutsideInnerSphere;
            if (isValid)
            {
                found = true;
                break;
            }
        }
        if (!found)
        {
            Debug.LogError("No free spawn points in scene withing sphereshell constraints. Breaking constraints.");
            return SampleSpawn(occupy);
        }
        if (occupy) {
            spawn.gameObject.GetComponent<SpawnPoint>().occupied = true;
        }
        Vector3 pos = spawn.position;
        return pos;
    }

    private void SampleSceneVariations() {
        ShakeMoveableObjects();
        RandomToggleLights();
    }

    private void ShakeMoveableObjects() {
        // Shake them by a few centimeters, rotate a little
        for (int i = 0; i < moveableObjects.Length; i++)
        {
            GameObject obj = moveableObjects[i];
            // reset original condition
            obj.SetActive(true);
            obj.transform.position = originalPositions[i];
            obj.transform.rotation = originalRotations[i];
            // Shake, vanish, rotate
            bool vanish = Random.Range(0.0f, 1.0f) < 0.03f;
            if (vanish)
            {
                obj.SetActive(false);
            }
            float maxShake_m = 0.03f;
            obj.transform.position += new Vector3(Random.Range(-maxShake_m, maxShake_m), 0.0f, Random.Range(-maxShake_m, maxShake_m));
            obj.transform.RotateAround(obj.transform.position, Vector3.up, Random.Range(0.0f, 360.0f));
        }
    }

    private void RandomToggleLights() {
        // Toggle lights
        foreach (GameObject obj in toggleableLights)
        {
            obj.SetActive(Random.Range(0.0f, 1.0f) >= lightDisableProbability);
        }
    }

}
