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
    //public int lastTouchId = -1;

    Rigidbody ballRb;

    public float maxVel;

    //public float existinal { get => area.playerStates[0].agentScript.timePenalty; }

    private void Awake()
    {
        hits = new List<Hit>();
        ballRb = GetComponent<Rigidbody>();
        area = GetComponentInParent<VTrainField>();
    }

    public void Service()
    {
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        transform.eulerAngles = Vector3.zero;

        int randomSign = Random.Range(0, 2) * 2 - 1;
        transform.localPosition = new Vector3(0, 2.85f, randomSign * 0.70f);

        hits.Clear();
        //lastTouchId = -1;
        hits.Add(Hit.HitUnset);
    }


    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("player"))
        {
            VPlayer player = col.gameObject.GetComponent<VPlayer>();
                if (!player.jumpRight)
                {
                    //player.AddReward(0.01f);
                    ContactPoint contact = col.contacts[0];
                    ballRb.AddForce((contact.point - (contact.point + contact.normal)).normalized * player.hitForce * -2.5f, ForceMode.Impulse);
                    Debug.DrawLine(contact.point, contact.point + contact.normal, Color.green, 2, false);
                }
            //lastTouchId = player.PlayerIndex;
        }

        if (col.gameObject.tag == "AreaA")
        {
            area.AddRewardToTeam(VPlayer.Team.A, -1f);
            area.AddRewardToTeam(VPlayer.Team.B, 1/* + existinal*/);
            /*if (hits.Contains(Hit.TeamBHit))
            {
                area.AddRewardToTeam(VPlayer.Team.B, 1);
            }*/
        }

        if (col.gameObject.tag == "AreaB")
        {
            area.AddRewardToTeam(VPlayer.Team.B, -1f);
            area.AddRewardToTeam(VPlayer.Team.A, 1/* + existinal*/);
            /*if (hits.Contains(Hit.TeamAHit))
            {
                area.AddRewardToTeam(VPlayer.Team.A, 1);
            }*/
        }

        /*if (col.gameObject.name == "BetweenWall")
        {

            if (hits[hits.Count - 1] == Hit.TeamAHit)
            {
                area.AddRewardToTeam(VPlayer.Team.A, 0.02f);
            }
            else if (hits[hits.Count - 1] == Hit.TeamBHit)
            {
                area.AddRewardToTeam(VPlayer.Team.B, 0.02f);
            }
        }
        */
        if (col.gameObject.tag == "AreaA" || col.gameObject.tag == "AreaB")
        {
            EndMatch();
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
