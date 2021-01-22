using UnityEngine;
using System.Collections.Generic;

public class VBallController : MonoBehaviour
{
    public VTrainField area;

    public enum Hit 
    {
        HitUnset,
        TeamAHit,
        TeamBHit
    }

    public List<Hit> hits;
    public int lastHitId = -1;
    public float ballStartForce = 3;
    Rigidbody ballRb;
    Vector3 ballStartingPos;

    public float maxVel;

    public float existinal { get => area.playerStates[0].agentScript.timePenalty; }

    private void Awake()
    {
        hits = new List<Hit>();
        ballRb = GetComponent<Rigidbody>();
        ballStartingPos = transform.position;

        area = GetComponentInParent<VTrainField>();

        ballStartingPos = transform.position;
    }

    public void Service()
    {
        transform.position = ballStartingPos;
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        //Ball shot
        // Random.Range(0,2)*2-1   == -1 or 1
        int randomSign = Random.Range(0, 2) * 2 - 1;
        //float randomX = Random.Range(-1.3f, 1.3f);
        //float rndStartForce = ballStartForce + Random.Range(0.05f, 0.5f);
        transform.localPosition = new Vector3(0, 1.5f, randomSign * 0.7f);
        //ballRb.AddForce(((Vector3.forward * randomSign) + Vector3.up) * rndStartForce, ForceMode.Impulse);

        hits.Clear();
        hits.Add(Hit.HitUnset);
        lastHitId = -1;
    }

    void AddRewardToTeam(VPlayer.Team team, float reward)
    {
        foreach (var playerStates in area.playerStates)
        {
            if (playerStates.agentScript.team != team)
            {
                playerStates.agentScript.AddReward(reward);
            }
        }
    }


    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("player"))
        {
            VPlayer player = col.gameObject.GetComponent<VPlayer>();
            player.AddReward(1f);
            //Debug.Log("Hit to player.");
            lastHitId = player.PlayerIndex;
            Hit hit = player.team == VPlayer.Team.A ? Hit.TeamAHit : Hit.TeamBHit;
            hits.Add(hit);
        }

        if (col.gameObject.tag == "AreaA")
        {
            AddRewardToTeam(VPlayer.Team.A, -10 * (1 + existinal));
            //Debug.Log("a ceza aldı");

            if (hits.Contains(Hit.TeamBHit))
            {
                AddRewardToTeam(VPlayer.Team.B, 10 * (1 + existinal));
                //Debug.Log("b puan aldı");
            }
        }

        if (col.gameObject.tag == "AreaB")
        {
            AddRewardToTeam(VPlayer.Team.B, -10 * (1 + existinal));
            //Debug.Log("b ceza aldı");

            if (hits.Contains(Hit.TeamAHit))
            {
                AddRewardToTeam(VPlayer.Team.A, 10 * (1 + existinal));
                //Debug.Log("a puan aldı");

            }
        }

        if (col.gameObject.name == "BetweenWall")
        {
            if (lastHitId != -1)
            {
                area.playerStates[lastHitId].agentScript.AddReward(2);
            }
        }

        if(col.gameObject.tag == "AreaA" || col.gameObject.tag == "AreaB")
        {
            area.EndEpsido();
            area.MatchReset();
        }
    }
    private void FixedUpdate()
    {
        var rgV = ballRb.velocity;
        ballRb.velocity = new Vector3(Mathf.Clamp(rgV.x, -maxVel, maxVel), Mathf.Clamp(rgV.y, -maxVel, maxVel), Mathf.Clamp(rgV.z, -maxVel, maxVel));
    }
}
