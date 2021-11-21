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
    // Sensors (observations)
    public Camera FrontColorCamera;
    public int image_width = 64;
    public int image_height = 64;
    private CameraSensor cameraSensor;
    // Start is called before the first frame update
    void Start()
    {
    }

    public override void OnEpisodeBegin()
    {
        this.BaseRBody.angularVelocity = Vector3.zero;
        this.BaseRBody.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3( -2.0f, 0.0f, 0.0f);
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
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        Vector3 required_vel_adjustment = controlSignal - BaseRBody.transform.InverseTransformDirection(BaseRBody.velocity); // local frame
        Vector3 required_accel = required_vel_adjustment;
        Vector3 required_force = required_accel * forceMultiplier;
        Vector3 required_force_wf = BaseRBody.transform.TransformDirection(required_force);
        required_force_wf.y = 0.0f; // we can't affect vertical acceleration
        if (forceMultiplier != 0.0f) {
        BaseRBody.AddForce(required_force_wf);
        BaseRBody.AddTorque(-BaseRBody.transform.right * required_force.z * 0.2f);
        }
        // Move joint, as a test
        JointSpring spring = LElbowRollParent.spring;
        spring.targetPosition = TestJointValue * -90.0f;
        LElbowRollParent.spring = spring;
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
}
}
