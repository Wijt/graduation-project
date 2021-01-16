using UnityEngine;

public class VBallController : MonoBehaviour
{
    public VTrainField area;

    public enum Hit 
    {
        TeamAHit,
        TeamBHit
    }

    public Hit lastHit;

    public bool fromService;
    public VPlayer.Team serviseTo;

    public float ballStartForce = 3;
    Rigidbody ballRb;
    Vector3 ballStartingPos;

    public float maxVel;

    private void Awake()
    {
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
        float rndStartForce = ballStartForce + Random.Range(0.05f, 0.5f);
        ballRb.AddForce(((Vector3.forward * randomSign) + Vector3.up) * rndStartForce, ForceMode.Impulse);
        fromService = true;
        serviseTo = randomSign == -1 ? VPlayer.Team.A : VPlayer.Team.B;
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
            player.AddReward(0.3f);
            //Debug.Log("Hit to player.");

            lastHit = player.team == VPlayer.Team.A ? Hit.TeamAHit : Hit.TeamBHit;
            fromService = false;
        }

        if (col.gameObject.tag == "AreaA")
        {
            AddRewardToTeam(VPlayer.Team.A, -3);

            if((lastHit == Hit.TeamBHit || !fromService) || serviseTo != VPlayer.Team.A)
            {
                AddRewardToTeam(VPlayer.Team.B, 10);
            }
        }

        if (col.gameObject.tag == "AreaB")
        {
            AddRewardToTeam(VPlayer.Team.B, -3);

            if ((lastHit == Hit.TeamAHit && !fromService) || serviseTo != VPlayer.Team.B)
            {
                AddRewardToTeam(VPlayer.Team.A, 10);
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
