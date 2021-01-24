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
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        transform.eulerAngles = Vector3.zero;
        //Ball shot
         //Random.Range(0, 2) * 2 - 1 == -1 or 1
        int randomSign = Random.Range(0, 2) * 2 - 1;
       //float randomX = Random.Range(-1.3f, 1.3f);
        //float rndStartForce = ballStartForce;
        transform.localPosition = new Vector3(0, 2.85f, randomSign * 1.3f);
        //Vector3 force = ((Vector3.forward * randomSign * 3) + Vector3.up * 1).normalized * rndStartForce;
        ////Debug.Log(force);
        //ballRb.AddForce(force, ForceMode.Impulse);

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
                player.AddReward(-0.1f);
            }
            else
            {
                player.AddReward(0.1f);
                if (!player.jumpRight)
                {
                    player.AddReward(0.2f);
                    ContactPoint contact = col.contacts[0];
                    Vector3 dir = (contact.point - (contact.point + contact.normal)).normalized;
                    Debug.Log(dir);
                    ballRb.AddForce(dir * -2.5f, ForceMode.Impulse);
                    Debug.DrawLine(contact.point, contact.point + contact.normal, Color.green, 2, false);
                    if (player.team == VPlayer.Team.A)
                    {
                        if(dir.z>0) player.AddReward(-0.1f);
                    }
                    else
                    {
                        if(dir.z<0) player.AddReward(-0.1f);

                    }
                }
            }
            lastTouchId = player.PlayerIndex;
        }

        if (col.gameObject.tag == "AreaA")
        {
            area.AddRewardToTeam(VPlayer.Team.A, -0.75f);
            //Debug.Log("a ceza aldı: " + "-1");

            if (hits.Contains(Hit.TeamBHit))
            {
                area.AddRewardToTeam(VPlayer.Team.B, 1);
                //Debug.Log("b puan aldı: " + 1);
            }
        }

        if (col.gameObject.tag == "AreaB")
        {
            area.AddRewardToTeam(VPlayer.Team.B, -0.75f);
            //Debug.Log("b ceza aldı: " + "-1");

            if (hits.Contains(Hit.TeamAHit))
            {
                area.AddRewardToTeam(VPlayer.Team.A, 1);
                //Debug.Log("a puan aldı: " + "1");
            }
        }

        if (col.gameObject.name == "BetweenWall")
        {

            if (hits[hits.Count - 1] == Hit.TeamAHit)
            {
                area.AddRewardToTeam(VPlayer.Team.A, 0.4f);
            }
            else if (hits[hits.Count - 1] == Hit.TeamBHit)
            {
                area.AddRewardToTeam(VPlayer.Team.B, 0.4f);
            }
        }
        if (col.gameObject.tag == "wall")
        {

            if (hits[hits.Count - 1] == Hit.TeamAHit)
            {
                area.AddRewardToTeam(VPlayer.Team.A, -0.02f);
            }
            else if (hits[hits.Count - 1] == Hit.TeamBHit)
            {
                area.AddRewardToTeam(VPlayer.Team.B, -0.02f);
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
                col.gameObject.GetComponent<VPlayer>().AddReward(-0.1f); catchPunishment = true;
            }
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
        //Debug.Log("Match done!");
        area.EndEpsido();
        area.MatchReset();
    }
}
