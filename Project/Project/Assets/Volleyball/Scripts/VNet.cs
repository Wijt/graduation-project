using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VNet : MonoBehaviour
{
    public VTrainField area;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("player"))
        {
            VPlayer player = collision.gameObject.GetComponent<VPlayer>();
            if (player.team == VPlayer.Team.A) area.ballController.BWon();
            if (player.team == VPlayer.Team.B) area.ballController.AWon();
        }
    }
}
