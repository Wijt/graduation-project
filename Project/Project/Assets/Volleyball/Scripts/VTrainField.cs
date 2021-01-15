using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VPlayerState
{
    public int playerIndex;
    public Rigidbody agentRb;
    public Vector3 startingPos;
    public Quaternion startingRot;
    public VPlayer agentScript;
    public float ballPosReward;
}

public class VTrainField : MonoBehaviour
{
    public GameObject ball;
    [HideInInspector]
    public Rigidbody ballRb;

    VBallController m_BallController;
    public List<VPlayerState> playerStates = new List<VPlayerState>();

    [HideInInspector]
    public Vector3 ballStartingPos;
    public float ballStartForce = 3;

    void Awake()
    {
        ballRb = ball.GetComponent<Rigidbody>();
        m_BallController = ball.GetComponent<VBallController>();
        m_BallController.area = this;
        ballStartingPos = this.ball.transform.position;
        MatchReset();
    }

    public void Won(VPlayer.Team scoredTeam, int lastTouchPlayerIndex)
    {
       foreach (var ps in playerStates)
        {
            if (ps.agentScript.team == scoredTeam)
            {
                if (lastTouchPlayerIndex != -1)
                {
                    if (playerStates[lastTouchPlayerIndex].agentScript.team == scoredTeam)
                        ps.agentScript.AddReward(5 + ps.agentScript.timePenalty);
                }
               
            }
            else
            {
                ps.agentScript.AddReward(-1);
            }
        }
        MatchReset();
    }

    public void MatchReset()
    {
        ball.transform.position = ballStartingPos;
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        m_BallController.lastTouchPlayerIndex = -1;

        foreach (var item in playerStates)
        {
            item.agentScript.EndEpisode();
            item.agentScript.transform.position = item.startingPos;
            item.agentScript.transform.rotation = item.startingRot;
            item.agentRb.velocity = Vector3.zero;
        }


        //Ball shot
        // Random.Range(0,2)*2-1   == -1 or 1
        int randomSign = Random.Range(0, 2) * 2 - 1;
        float rndStartForce = ballStartForce + Random.Range(-1f, 1f);
        ballRb.AddForce(((Vector3.forward * randomSign * 2) + (Vector3.up * 4)) * rndStartForce, ForceMode.Impulse);
    }
}
