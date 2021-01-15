using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CylinderThing : Agent
{
    public int succsessPoint = 0;
    public int testCount = 100;


    public GameObject[] goals;
    int selectedGoalId;
    public float speed,rotSpeed;    

    public override void OnEpisodeBegin()
    {
        selectedGoalId = Random.Range(0, goals.Length);
        foreach (GameObject goal in goals)
        {
            goal.GetComponent<Goal>().SetMatColor(false);
        }
        goals[selectedGoalId].GetComponent<Goal>().SetMatColor(true);

        gameObject.transform.localPosition = new Vector3(Random.Range(-1.1f, 1.1f), 0, Random.Range(-1.1f, 1.1f));
        gameObject.transform.Rotate(Vector3.up * Random.Range(0f, 360f));
        //testCount--;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(goals[selectedGoalId].transform.localPosition);
        Vector3 toPosition = (goals[selectedGoalId].transform.localPosition - transform.localPosition).normalized;
        float angleToPosition = Vector3.Angle(-transform.forward, toPosition);
        sensor.AddObservation(angleToPosition);
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var contActs = actionsOut.ContinuousActions;
        contActs[0] = Input.GetAxis("Horizontal");
        contActs[1] = Input.GetAxis("Vertical");
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuousActions = actions.ContinuousActions;
        float rot = Mathf.Clamp(continuousActions[0], -1f, 1f);
        float move = Mathf.Clamp(continuousActions[1], -0.1f, 1f);
        transform.Translate(new Vector3(0, 0, -move * Time.deltaTime * speed));
        transform.Rotate(Vector3.up * rot * Time.deltaTime * rotSpeed);
        // Penalty given each step to encourage agent to finish task quickly.
        //AddReward(-1f / MaxStep);
    }

    /*
    private void OnTriggerEnter(Collider other)
    {
        if (!triggered)
        {
            triggered = true;
            if (other.GetComponent<Renderer>().materials[0].name == "Green (Instance)")
            {
                if (testCount > 0)
                {
                    succsessPoint++;
                }
                AddReward(15);
                EndEpisode();
            }
            if (other.GetComponent<Renderer>().materials[0].name == "Red (Instance)")
            {
                AddReward(-10);
                EndEpisode();
            }
        }
    }
    */
    private void FixedUpdate()
    {
        /*Debug.Log("Distance: " + Vector3.Distance(goals[selectedGoalId].transform.localPosition, transform.localPosition));
        Debug.Log("Target position: " + goals[selectedGoalId].transform.localPosition);
        Debug.Log("Agent position: " + transform.localPosition);*/
        if(Vector3.Distance(goals[selectedGoalId].transform.localPosition, transform.localPosition) <= 0.75)
        {
            AddReward(15f);
            EndEpisode();
            return;
        }
        if (this.transform.localPosition.y <= -1)
        {
            AddReward(-10f);
            EndEpisode();
            return;
        }
    }

    /*  
    private void Update()
    {

        float rot = Input.GetAxis("Horizontal");
        float move = Input.GetAxis("Vertical");
        transform.Translate(new Vector3(0, 0, move*Time.deltaTime*speed));
        transform.Rotate(Vector3.up * rot * Time.deltaTime * rotSpeed);
    }
    */
}
