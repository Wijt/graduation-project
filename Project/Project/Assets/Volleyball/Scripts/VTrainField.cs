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
        List<Vector3> positions = FourRandomPos();
        for (int i = 0; i < playerStates.Count; i++)
        {
            playerStates[i].agentRb.transform.position = positions[i];
        }
        ballController.Service();
    }

    public List<Vector3> FourRandomPos()
    {
        int team = -1;
        List<Vector3> positions = new List<Vector3>();
        while (true)
        {

            if (positions.Count >= 4) break;
            if (positions.Count >= 2) team = 1;

            bool good = true;

            float randomX = Random.Range(-1.25f, 1.25f);
            float randomZ = Random.Range(0.25f, 2.72f) * team;
            Vector3 randomPos = new Vector3(randomX, 0.5f, randomZ);

            foreach (Vector3 pos in positions)
                if (Vector3.Distance(pos, randomPos) <= 0.4f) good = false;

            if (good) positions.Add(randomPos);
        }
        return positions;
    }

    public void EndEpsido()
    {
        foreach (var item in playerStates)
        {
            item.agentScript.EndEpisode();
        }
    }
    public void AddRewardToTeam(VPlayer.Team team, float reward)
    {
        foreach (var playerStates in playerStates)
        {
            if (playerStates.agentScript.team != team)
            {
                playerStates.agentScript.AddReward(reward);
            }
        }
    }
}
