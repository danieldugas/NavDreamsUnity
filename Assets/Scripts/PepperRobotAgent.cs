using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.AI; // used to refer to NavMeshObstacle class

/// Simulated Pepper for RL training,
public class PepperRobotAgent : Agent
{
    public bool DEBUG = false;
    public bool DEBUGOBS = false;
    public int currentDifficulty = 0;
    
    [Tooltip("Higher force multiplier = faster acceleration to desired vel.")]
    public float forceMultiplier = 10.0f;
    public float torqueMultiplier = 10.0f;
    public float velMultiplier = 1.0f;
    public float rotvelMultiplier = 1.0f;
    public float goalRadius = 1.42f;
    public float maxDamage = 0.5f; // measured as sum of impact velocity [m/s]
    public Transform Target;

    // Effectors (actions): Joint angles, force applied to base rigid body
    public HingeJoint HeadYawParent;
    public HingeJoint HeadPitchParent;
    public HingeJoint LShoulderPitchParent;
    public HingeJoint LShoulderRollParent;
    public HingeJoint LElbowYawParent;
    public HingeJoint LElbowRollParent;
    public HingeJoint LWristYawParent;
    public HingeJoint LHandParent;
    public HingeJoint RShoulderPitchParent;
    public HingeJoint RShoulderRollParent;
    public HingeJoint RElbowYawParent;
    public HingeJoint RElbowRollParent;
    public HingeJoint RWristYawParent;
    public HingeJoint RHandParent;
    public HingeJoint HipRollParent;
    public HingeJoint HipPitchParent;
    public HingeJoint KneePitchParent;
    [Tooltip("The robot's tibia, base movement is transferred to this rigidbody as a force.")]
    public Rigidbody BaseRBody;
    [Tooltip("A transform which will be moved to wherever the base is, but kept pointing up and on y=0 plane.")]
    public Transform BaseFootprint;
    // People
    public PeopleNavController people;
    public EnvironmentController environment;
    // Sensors (observations)
    public Camera FrontColorCamera;
    public int image_width = 64;
    public int image_height = 64;

    private CameraSensor cameraSensor;
    private float LastActionTime = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        // Creat damage monitor for each collider
        foreach (Rigidbody cl in GetComponentsInChildren<Rigidbody>())
        {
            ColliderDamageMonitor dm = cl.gameObject.AddComponent<ColliderDamageMonitor>() as ColliderDamageMonitor;
            dm.parent = gameObject;
            dm.DEBUG = DEBUG;
        }
    }

    public override void OnEpisodeBegin()
    {
        int n_robots = 1;
        int n_people = Mathf.Clamp(currentDifficulty, 0, people.GetMaxPeopleCount());
        // Reset the environment
        environment.OnEpisodeBegin(n_robots, currentDifficulty, n_people, out Vector3[] robot_positions, out Vector3[] robot_goals, out Vector3[] people_positions, out Vector3[] people_goals);
        // Reset people
        people.OnEpisodeBegin(n_people, people_positions, people_goals);
        // Reset robot
        // Reset damage monitor for each collider
        foreach (ColliderDamageMonitor dm in GetComponentsInChildren<ColliderDamageMonitor>())
        {
            dm.damage = 0.0f;
        }
        // make all children rigidbodies kinematic
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
        }
        this.BaseRBody.isKinematic = true;
        this.BaseRBody.transform.position = robot_positions[0] + Vector3.up * 0.3f;
        this.BaseRBody.transform.rotation = Quaternion.identity;
        this.BaseRBody.isKinematic = false;
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = false;
        }
        this.BaseRBody.angularVelocity = Vector3.zero;
        this.BaseRBody.velocity = Vector3.zero;
        this.Target.position = robot_goals[0];
        this.Target.gameObject.GetComponent<Renderer>().enabled = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Move base footprint to wherever the x, z coordinates of the tibia are
        Vector3 pos = this.BaseRBody.transform.position;
        pos.y = 0;
        this.BaseFootprint.position = pos;
        // projecting robot forward in horizontal plane to align basefootprint yaw still has to be done.
        // until then, it is unused.

        // Get goal in base frame
        Vector3 goal_in_base = this.BaseRBody.transform.InverseTransformPoint(this.Target.position);
        float goal_in_forward = goal_in_base.z;
        float goal_in_left = -goal_in_base.x;
        float vel_in_forward = BaseRBody.velocity.z;
        float vel_in_left = -BaseRBody.velocity.x;
        float vel_in_trigtop = -BaseRBody.angularVelocity.y;
        if (DEBUGOBS)
            Debug.Log("goal_in_forward: " + goal_in_forward + " goal_in_left: " + goal_in_left + " vel_in_forward: " + vel_in_forward + " vel_in_left: " + vel_in_left + " vel_in_trigtop: " + vel_in_trigtop);

        // Target and Agent positions
        sensor.AddObservation(goal_in_forward);
        sensor.AddObservation(goal_in_left);

        // Agent velocity
        sensor.AddObservation(vel_in_forward);
        sensor.AddObservation(vel_in_left);
        sensor.AddObservation(vel_in_trigtop);
        sensor.AddObservation(currentDifficulty);
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float now = Time.time;
        float timestep = now - LastActionTime;
        LastActionTime = now;
        //Debug.Log(Time.time);
        //Debug.Log(timestep);
        // Actions, size = 2
        Vector3 commandVel = Vector3.zero; // commanded velocity in base unity frame
        commandVel.x = -actionBuffers.ContinuousActions[1];
        commandVel.z = actionBuffers.ContinuousActions[0];
        Vector3 commandRot = Vector3.zero;
        commandRot.y = -actionBuffers.ContinuousActions[2];

        // invulnerable if stopped (otherwise people unfairly hurt the robot)
        bool isMoving = commandVel.magnitude > 0.05f || commandRot.magnitude > 0.05f;
        foreach (ColliderDamageMonitor dm in GetComponentsInChildren<ColliderDamageMonitor>())
        {
            if (isMoving)
            {
                dm.ignoreDamage = false;
            } else {
                dm.ignoreDamage = true;
            }
        }
         NavMeshObstacle nmo = BaseRBody.gameObject.GetComponent<NavMeshObstacle>();
        if (isMoving)
        {
            // people will avoid the robot less when it's moving
            nmo.radius = 0.1f;
            nmo.carving = false;
        } else {
            // people will try to avoid the robot more generously when it stops
            nmo.radius = 0.3f;
            nmo.carving = true;
        }

        // Apply command velocity control as a force
        Vector3 required_vel_adjustment = commandVel * velMultiplier - BaseRBody.transform.InverseTransformDirection(BaseRBody.velocity); // local frame
        Vector3 required_rot_adjustment = commandRot * rotvelMultiplier - BaseRBody.transform.InverseTransformDirection(BaseRBody.angularVelocity); // local frame
        Vector3 required_accel = required_vel_adjustment;
        Vector3 required_rot_accel = required_rot_adjustment;
        Vector3 required_force = required_accel * forceMultiplier;
        Vector3 required_torque = required_rot_accel * torqueMultiplier;
        Vector3 required_force_wf = BaseRBody.transform.TransformDirection(required_force);
        Vector3 required_torque_wf = BaseRBody.transform.TransformDirection(required_torque);
        required_force_wf.y = 0.0f; // we can't affect vertical acceleration
        required_torque_wf.x = 0.0f; // we can't control non-vertical-axis rotation
        required_torque_wf.z = 0.0f;
        BaseRBody.AddForce(required_force_wf);
        BaseRBody.AddTorque(-BaseRBody.transform.right * required_force.z * 0.2f);
        BaseRBody.AddTorque(required_torque_wf);
        // Move joint, as a test
        SetJointTarget(LShoulderPitchParent, (1.0f - actionBuffers.ContinuousActions[3]) * 90.0f);
        SetJointTarget(RShoulderPitchParent, (1.0f - actionBuffers.ContinuousActions[3]) * 90.0f);
        // Change difficulty if required
        if (actionBuffers.ContinuousActions[4] != 0.0f)
        {
            if (DEBUG)
                Debug.Log("Difficulty change requested");
            currentDifficulty = Mathf.Clamp(
                Mathf.RoundToInt(actionBuffers.ContinuousActions[4] * 50), 0, 50);
        }
        // Move people
        people.DoNavStep(environment, timestep);
        // Rewards
        float distanceToTarget = Vector3.Distance(BaseRBody.transform.position, Target.position);

        // Reached target
        if (distanceToTarget < goalRadius)
        {
            if (DEBUG)
                Debug.Log("Reached target");
            SetReward(100.0f);
            currentDifficulty = Mathf.Clamp(currentDifficulty + 1, 0, 50);
            EndEpisode();
        }

        // Fell off platform
        else if (BaseRBody.transform.localPosition.y < 0 || BaseRBody.transform.localPosition.y > 5)
        {
            if (DEBUG)
                Debug.Log("invalid height");
            currentDifficulty = Mathf.Clamp(currentDifficulty - 1, 0, 50);
            EndEpisode();
        }

        // Toppled over
        else if (BaseRBody.transform.up.y < 0.5f)
        {
            if (DEBUG)
                Debug.Log("Toppled over");
            currentDifficulty = Mathf.Clamp(currentDifficulty - 1, 0, 50);
            EndEpisode();
        }

        foreach (ColliderDamageMonitor dm in GetComponentsInChildren<ColliderDamageMonitor>())
        {
            if (dm.damage > maxDamage)
            {
                if (DEBUG)
                    Debug.Log("Maximum damage from " + dm.gameObject.name);
                currentDifficulty = Mathf.Clamp(currentDifficulty - 1, 0, 50);
                EndEpisode();
            }
        }
    }

    private void SetJointTarget(HingeJoint joint, float target)
    {
        JointSpring spring2 = joint.spring;
        spring2.targetPosition = target;
        joint.spring = spring2;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
{
    var continuousActionsOut = actionsOut.ContinuousActions;
    continuousActionsOut[1] = -Input.GetAxis("Horizontal");
    continuousActionsOut[0] = Input.GetAxis("Vertical");
    if (Input.GetButton("Fire1"))
    {
        continuousActionsOut[2] = 1.0f;
    }
    if (Input.GetButton("Fire2"))
    {
        continuousActionsOut[2] = -1.0f;
    }
    if (Input.GetButton("Jump"))
    {
        continuousActionsOut[3] = 1.0f;
    }
}
}
