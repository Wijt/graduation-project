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
            collision.gameObject.GetComponent<VPlayer>().AddReward(-1);
        }
    }
}
