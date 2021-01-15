using UnityEngine;

public class VBallController : MonoBehaviour
{
    [HideInInspector]
    public VTrainField area;

    public int lastTouchPlayerIndex = -1;
 
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.layer == LayerMask.GetMask("player"))
        {
            col.gameObject.GetComponent<VPlayer>().AddReward(0.3f);
            lastTouchPlayerIndex = col.gameObject.GetComponent<VPlayer>().PlayerIndex;
        }

        if (col.gameObject.tag == "AreaA")
        {
            area.Won(VPlayer.Team.B, lastTouchPlayerIndex);
        }

        if (col.gameObject.tag == "AreaB")
        {
            area.Won(VPlayer.Team.A, lastTouchPlayerIndex);
        }
    }
}
