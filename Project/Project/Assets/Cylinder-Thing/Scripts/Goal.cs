using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public Material green, black;
    public void SetMatColor(bool isThisGoal)
    {
        this.GetComponent<Renderer>().material = isThisGoal ? green : black;
    }
}
