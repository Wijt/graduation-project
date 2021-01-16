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
}

public class VTrainField : MonoBehaviour
{
    public List<VPlayerState> playerStates = new List<VPlayerState>();
    public VBallController ballController;

    private void Awake()
    {
        foreach (VPlayer player in GetComponentsInChildren<VPlayer>())
        {
            var playerState = new VPlayerState
            {
                agentRb = player.agentRb,
                startingPos = player.transform.position,
                startingRot = player.transform.rotation,
                agentScript = player,
            };

            playerStates.Add(playerState);
            player.PlayerIndex = playerStates.IndexOf(playerState);
            playerState.playerIndex = player.PlayerIndex;
        }
    }

    void Start()
    {
        MatchReset();
    }

    public void MatchReset()
    {
        foreach (var item in playerStates)
        {
            item.agentScript.transform.position = item.startingPos;
            item.agentScript.transform.rotation = item.startingRot;
            item.agentRb.velocity = Vector3.zero;
        }
        ballController.Service();
    }

    public void EndEpsido()
    {
        foreach (var item in playerStates)
        {
            item.agentScript.EndEpisode();
        }
    }
}
