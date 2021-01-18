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
    public int lastTouchId = -1;

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
        //Ball shot
        // Random.Range(0,2)*2-1   == -1 or 1
        int randomSign = Random.Range(0, 2) * 2 - 1;
        float randomX = Random.Range(-1.3f, 1.3f);
        float rndStartForce = ballStartForce;
        transform.localPosition = (Vector3.right * randomX) + (Vector3.up * 2f);
        Vector3 force = ((Vector3.forward * randomSign * 3) + Vector3.up * 1).normalized * rndStartForce;
        ////Debug.Log(force);
        ballRb.AddForce(force, ForceMode.Impulse);

        hits.Clear();
        lastTouchId = -1;
        hits.Add(Hit.HitUnset);
    }



    public float maxCatchTime = 0.3f;
    float catchTimer;
    bool catchPunishment = false;
    void OnCollisionEnter(Collision col)
    {
        ////Debug.Log(area.playerStates[0].agentScript.StepCount);
        if (col.gameObject.layer == LayerMask.NameToLayer("player"))
        {
            catchTimer = 0;
            catchPunishment = false;
            //Debug.Log("Hit to player.");
            VPlayer player = col.gameObject.GetComponent<VPlayer>();
            if (lastTouchId == player.PlayerIndex)
            {
                //Debug.Log("Double Hit");
                player.AddReward(-1f);
            }
            else
            {
                player.AddReward(1f);
                Hit hit = player.team == VPlayer.Team.A ? Hit.TeamAHit : Hit.TeamBHit;
                hits.Add(hit);
            }
            lastTouchId = player.PlayerIndex;
        }

        if (col.gameObject.tag == "AreaA")
        {
            area.AddRewardToTeam(VPlayer.Team.A, -10 * ((1 + existinal)/2));
            //Debug.Log("a ceza aldı: "+ (- 10 * ((1 + existinal) / 2)));

            if (hits.Contains(Hit.TeamBHit))
            {
                area.AddRewardToTeam(VPlayer.Team.B, 10 * (1 + existinal));
                //Debug.Log("b puan aldı: " + (10 * (1 + existinal)));
            }
        }

        if (col.gameObject.tag == "AreaB")
        {
            area.AddRewardToTeam(VPlayer.Team.B, -10 * ((1 + existinal) / 2));
            //Debug.Log("b ceza aldı: " + (-10 * ((1 + existinal) / 2)));
     
            if (hits.Contains(Hit.TeamAHit))
            {
                area.AddRewardToTeam(VPlayer.Team.A, 10 * (1 + existinal));
                //Debug.Log("a puan aldı: " + (10 * (1 + existinal)));
            }
        }

        if (col.gameObject.name == "BetweenWall")
        {

            if (hits[hits.Count - 1] == Hit.TeamAHit)
            {
                area.AddRewardToTeam(VPlayer.Team.A, 1);
            }else if (hits[hits.Count - 1] == Hit.TeamBHit)
            {
                area.AddRewardToTeam(VPlayer.Team.B, 1);
            }
        }

        if (col.gameObject.tag == "AreaA" || col.gameObject.tag == "AreaB")
        {
            EndMatch();
        }
    }

    private void OnCollisionStay(Collision col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("player"))
        {
            catchTimer += Time.deltaTime;
            //Debug.Log("tutuş" + catchTimer + catchPunishment);
            if (catchTimer >= maxCatchTime && !catchPunishment)
            {
                //Debug.Log("Oyuncu ceza aldı. Aşırı tutuş.");
                col.gameObject.GetComponent<VPlayer>().AddReward(-1); catchPunishment = true; }
        }
    }

    private void FixedUpdate()
    {
        var rgV = ballRb.velocity;
        ballRb.velocity = new Vector3(Mathf.Clamp(rgV.x, -maxVel, maxVel), Mathf.Clamp(rgV.y, -maxVel, maxVel), Mathf.Clamp(rgV.z, -maxVel, maxVel));
        if (Input.GetKeyUp(KeyCode.O))
        {
            EndMatch();
        }            
    }

    void EndMatch()
    {
        area.EndEpsido();
        area.MatchReset();
    }
}
