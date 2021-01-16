using UnityEngine;

public class VBallController : MonoBehaviour
{
    public VTrainField area;

    public enum Hit 
    {
        HitUnset,
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

    public float existinal { get => area.playerStates[0].agentScript.timePenalty; }

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
        float randomX = Random.Range(-1.3f, 1.3f);
        float rndStartForce = ballStartForce + Random.Range(0.05f, 0.5f);
        transform.localPosition = (Vector3.right * randomX) + (Vector3.up * 1.5f);
        ballRb.AddForce(((Vector3.forward * randomSign) + Vector3.up) * rndStartForce, ForceMode.Impulse);
        fromService = true;
        serviseTo = randomSign == -1 ? VPlayer.Team.A : VPlayer.Team.B;

        lastHit = Hit.HitUnset;
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

            lastHit = player.team == VPlayer.Team.A ? Hit.TeamAHit : Hit.TeamBHit;
            fromService = false;
        }

        if (col.gameObject.tag == "AreaA")
        {
            AddRewardToTeam(VPlayer.Team.A, -10 * (1 + existinal));

            if ((lastHit == Hit.TeamBHit || !fromService) || serviseTo != VPlayer.Team.A)
            {
                AddRewardToTeam(VPlayer.Team.B, 10 * (1 + existinal));
                Debug.Log("b puan aldı a ceza aldı");
            }
        }

        if (col.gameObject.tag == "AreaB")
        {
            AddRewardToTeam(VPlayer.Team.B, -10 * (1 + existinal));
            if(serviseTo == VPlayer.Team.B)
            {
            }
            else
            {
                AddRewardToTeam(VPlayer.Team.A, 10 * (1 + existinal));
                Debug.Log("a puan aldı b ceza aldı");
            }
            if ((lastHit == Hit.TeamAHit && !fromService) || (lastHit == Hit.HitUnset))
            {
                AddRewardToTeam(VPlayer.Team.A, 10 * (1 + existinal));
                Debug.Log("a puan aldı b ceza aldı");
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
