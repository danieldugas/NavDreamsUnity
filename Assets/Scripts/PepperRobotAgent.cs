using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

/// Simulated Pepper for RL training,
public class PepperRobotAgent : Agent
{
    
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
    // Sensors (observations)
    public Camera FrontColorCamera;
    // Start is called before the first frame update
    void Start()
    {
    }

    public override void OnEpisodeBegin()
    {
        this.BaseRBody.angularVelocity = Vector3.zero;
        this.BaseRBody.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3( 0, 0.5f, 0);
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
}
