using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

/// Simulated Pepper for RL training,
public class PepperRobotAgent : Agent
{
    
    public float forceMultiplier = 10;
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
    public Rigidbody BaseRBody;
    [Range(0, 1)]
    public float TestJointValue;
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
    }

    public override void OnEpisodeBegin()
    {
        int n_robots = 1;
        int n_people = people.GetMaxPeopleCount();
        int n_difficulty = 1;
        // Reset the environment
        environment.OnEpisodeBegin(n_robots, n_difficulty, n_people, out Vector3[] robot_positions, out Vector3[] robot_goals, out Vector3[] people_positions, out Vector3[] people_goals);
        // Reset people
        people.OnEpisodeBegin(n_people, people_positions, people_goals);
        // Reset robot
        this.BaseRBody.angularVelocity = Vector3.zero;
        this.BaseRBody.velocity = Vector3.zero;
        this.transform.position = robot_positions[0];
        this.Target.position = robot_goals[0];
        this.Target.gameObject.GetComponent<Renderer>().enabled = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Agent velocity
        sensor.AddObservation(BaseRBody.velocity.x);
        sensor.AddObservation(BaseRBody.velocity.z);
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float now = Time.time;
        float timestep = now - LastActionTime;
        LastActionTime = now;
        //Debug.Log(Time.time);
        //Debug.Log(timestep);
        // Actions, size = 2
        Vector3 commandVel = Vector3.zero;
        commandVel.x = actionBuffers.ContinuousActions[0];
        commandVel.z = actionBuffers.ContinuousActions[1];
        Vector3 commandRot = Vector3.zero;
        commandRot.y = actionBuffers.ContinuousActions[2];
        Vector3 required_vel_adjustment = commandVel - BaseRBody.transform.InverseTransformDirection(BaseRBody.velocity); // local frame
        Vector3 required_rot_adjustment = commandRot - BaseRBody.transform.InverseTransformDirection(BaseRBody.angularVelocity); // local frame
        Vector3 required_accel = required_vel_adjustment;
        Vector3 required_rot_accel = required_rot_adjustment;
        Vector3 required_force = required_accel * forceMultiplier;
        Vector3 required_torque = required_rot_accel * forceMultiplier;
        Vector3 required_force_wf = BaseRBody.transform.TransformDirection(required_force);
        Vector3 required_torque_wf = BaseRBody.transform.TransformDirection(required_torque);
        required_force_wf.y = 0.0f; // we can't affect vertical acceleration
        required_torque_wf.x = 0.0f; // we can't control non-vertical-axis rotation
        required_torque_wf.z = 0.0f;
        if (forceMultiplier != 0.0f) {
        BaseRBody.AddForce(required_force_wf);
        BaseRBody.AddTorque(-BaseRBody.transform.right * required_force.z * 0.2f);
        BaseRBody.AddTorque(required_torque_wf);
        }
        // Move joint, as a test
        JointSpring spring = LElbowRollParent.spring;
        spring.targetPosition = TestJointValue * -90.0f;
        LElbowRollParent.spring = spring;
        JointSpring spring2 = LShoulderPitchParent.spring;
        spring2.targetPosition = actionBuffers.ContinuousActions[3] * -90.0f;
        LShoulderPitchParent.spring = spring2;
        // Move person as a test
        people.DoNavStep(environment, timestep);
        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // Reached target
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        // Fell off platform
        else if (this.transform.localPosition.y < 0 || this.transform.localPosition.y > 5)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
{
    var continuousActionsOut = actionsOut.ContinuousActions;
    continuousActionsOut[0] = Input.GetAxis("Horizontal");
    continuousActionsOut[1] = Input.GetAxis("Vertical");
    if (Input.GetButton("Fire1"))
    {
        continuousActionsOut[2] = -1.0f;
    }
    if (Input.GetButton("Fire2"))
    {
        continuousActionsOut[2] = 1.0f;
    }
    if (Input.GetButton("Jump"))
    {
        continuousActionsOut[3] = 1.0f;
    }
}
}
