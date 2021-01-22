using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;

public class VPlayer : Agent
{
    // Note that that the detectable tags are different for the blue and purple teams. The order is
    // * ball
    // * own goal
    // * opposing goal
    // * wall
    // * own teammate
    // * opposing player
    public enum Team
    {
        A = 0,
        B = 1
    }

    public Team team;
    public int PlayerIndex;

    public VTrainField area;
    public VBallController ballController;

    float existinal;
    public float LateralSpeed = 0.3f;
    public float ForwardSpeed = 1.3f;
    public float JumpForce = 50;
    public float maxVel = 3;
    [HideInInspector]
    public float timePenalty;

    [HideInInspector]
    public Rigidbody agentRb;
    [HideInInspector]
    public Rigidbody ballRb;

    BehaviorParameters m_BehaviorParameters;

    int invert;

    public override void Initialize()
    {
        existinal = 1f / MaxStep;
        m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();

        team = (Team)m_BehaviorParameters.TeamId;

        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;
        agentRb.centerOfMass = new Vector3(0, -0.3f, 0.039f);

        ballRb = ballController.GetComponent<Rigidbody>();

    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;


        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];

        switch (forwardAxis)
        {
            case 1:
                dirToGo = transform.forward * ForwardSpeed;
                break;
            case 2:
                dirToGo = transform.forward * -ForwardSpeed;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                dirToGo = transform.right * LateralSpeed;
                break;
            case 2:
                dirToGo = transform.right * -LateralSpeed;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = Vector3.up * -1f;
                break;
            case 2:
                rotateDir = Vector3.up * 1f;
                break;
        }

        transform.Rotate(rotateDir * Time.deltaTime * 100f);
        agentRb.AddForce(dirToGo * Time.deltaTime, ForceMode.VelocityChange);
    }

    bool CheckGroundStatus()
    {
        RaycastHit hit;
        Ray landingRay = new Ray(transform.position, Vector3.down);
        Debug.DrawRay(transform.position, Vector3.down * .45f);

        return Physics.Raycast(landingRay, out hit, .45f, LayerMask.GetMask("trainfield"));
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(ballRb.velocity.x * invert);
        sensor.AddObservation(ballRb.velocity.y);
        sensor.AddObservation(ballRb.velocity.z* invert);

        sensor.AddObservation(ballRb.transform.localPosition.x * invert);
        sensor.AddObservation(ballRb.transform.localPosition.y);
        sensor.AddObservation(ballRb.transform.localPosition.z * invert);

        sensor.AddObservation(Vector3.Distance(transform.localPosition, ballRb.transform.localPosition));

        sensor.AddObservation(CheckGroundStatus());


        foreach (var playerStates in area.transform.GetComponentsInChildren<VPlayer>()) {
            Transform playerPos = playerStates.agentRb.transform;
            Rigidbody playerRb = playerStates.agentRb;

            sensor.AddObservation(playerPos.transform.localPosition.x * invert);
            sensor.AddObservation(playerPos.transform.localPosition.y);
            sensor.AddObservation(playerPos.transform.localPosition.z * invert);

            sensor.AddObservation(playerRb.velocity.x * invert);
            sensor.AddObservation(playerRb.velocity.y);
            sensor.AddObservation(playerRb.velocity.z * invert);
        }

        sensor.AddObservation(transform.rotation.y);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
        timePenalty -= existinal;
        var continuousActions = actionBuffers.ContinuousActions;
        if (CheckGroundStatus())
            if(continuousActions[0]>0)
                agentRb.AddForce(Vector3.up * continuousActions[0] * JumpForce, ForceMode.Impulse);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        var continuousActionsOut = actionsOut.ContinuousActions;

        continuousActionsOut.Clear();
        discreteActionsOut.Clear();

        //forward
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        //rotate
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[2] = 2;
        }
        //right
        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[1] = 2;
        }

        //jump
        if (Input.GetKey(KeyCode.Space))
        {
            continuousActionsOut[0] = 1;
        }
    }
 
    public override void OnEpisodeBegin()
    {
        invert = (int)team == 0 ? 1 : -1;
        timePenalty = 0;

        agentRb.velocity = Vector3.zero;
        agentRb.angularVelocity = Vector3.zero;
    }


    private void FixedUpdate()
    {
        if (transform.localEulerAngles.x != 0 || transform.localEulerAngles.z != 0)
        {
            //transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + 0.1f, transform.localPosition.z);
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
        }

        var rgV = agentRb.velocity;
        agentRb.velocity = new Vector3(Mathf.Clamp(rgV.x, -maxVel, maxVel), rgV.y, Mathf.Clamp(rgV.z, -maxVel, maxVel));
    }

    public void ExistinalPunishment()
    {
        AddReward(10 * (1 + timePenalty));
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag=="Net")
        {
            AddReward(-1);
        }   
    }
}
