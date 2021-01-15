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

    [HideInInspector]
    public Team team;
    public int PlayerIndex;

    public VTrainField area;
    // The coefficient for the reward for colliding with a ball. Set using curriculum.
    float m_BallTouch;

    float m_Existential;
    public float LateralSpeed = 0.3f;
    public float ForwardSpeed = 1.3f;
    public float JumpForce = 5;
    public float maxVel = 3;
    [HideInInspector]
    public float timePenalty;

    [HideInInspector]
    public Rigidbody agentRb;

    BehaviorParameters m_BehaviorParameters;
    Vector3 m_Transform;


    public override void Initialize()
    {
        m_Existential = 1f / MaxStep;
        m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();

        team = (Team)m_BehaviorParameters.TeamId;

        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;
        agentRb.centerOfMass = new Vector3(0, -0.3f, 0.039f);


        var playerState = new VPlayerState
        {
            agentRb = agentRb,
            startingPos = transform.position,
            startingRot = transform.rotation,
            agentScript = this,
        };

        area.playerStates.Add(playerState);
        PlayerIndex = area.playerStates.IndexOf(playerState);
        playerState.playerIndex = PlayerIndex;

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
        sensor.AddObservation(area.ballRb.velocity);
        sensor.AddObservation(area.ball.transform.localPosition);
        Vector3 toPosition = (area.ball.transform.localPosition - transform.localPosition).normalized;
        float angleToPosition = Vector3.Angle(transform.forward, toPosition);
        sensor.AddObservation(angleToPosition);
        sensor.AddObservation(Vector3.Distance(this.transform.localPosition, area.ball.transform.localPosition));
        sensor.AddObservation(CheckGroundStatus());
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
        timePenalty -= m_Existential;
        if (CheckGroundStatus())
        {
            float clampedJumpForce = Mathf.Clamp(actionBuffers.ContinuousActions[0], 0, JumpForce);
            agentRb.AddForce(Vector3.up * clampedJumpForce, ForceMode.Impulse);
        }
    }
    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        if (agentRb != null)
            Gizmos.DrawSphere(transform.position + agentRb.centerOfMass, 0.01f);
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
            continuousActionsOut[0] = JumpForce;
        }
    }
    /// <summary>
    /// Used to provide a "kick" to the ball.
    /// </summary>
    void OnCollisionEnter(Collision c)
    {
        /* var force = k_Power * m_KickPower;
         if (position == Position.Goalie)
         {
             force = k_Power;
         }
         if (c.gameObject.CompareTag("ball"))
         {
             AddReward(.2f * m_BallTouch);
             var dir = c.contacts[0].point - transform.position;
             dir = dir.normalized;
             c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
         }*/
    }

    public override void OnEpisodeBegin()
    {
        timePenalty = 0;
        if (team == Team.A)
        {
            //transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        }
        else
        {
            //transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }
        //transform.position = m_Transform;
        agentRb.velocity = Vector3.zero;
        agentRb.angularVelocity = Vector3.zero;
        SetResetParameters();
    }

    public void SetResetParameters()
    {
        //area.ResetBall();
    }

    private void FixedUpdate()
    {
        //if (Input.GetKeyUp(KeyCode.W))
        //{
        //    if (CheckGroundStatus())
        //    {
        //        agentRb.AddForce(0, 5, 0, ForceMode.Impulse);
        //    }
        //}
        if (transform.localEulerAngles.x != 0 || transform.localEulerAngles.z != 0) transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
        float vY = agentRb.velocity.y;
        agentRb.velocity = Vector3.ClampMagnitude(agentRb.velocity, maxVel);
        agentRb.velocity = new Vector3(agentRb.velocity.x, vY, agentRb.velocity.z);
    }
}
