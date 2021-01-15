using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VBall : MonoBehaviour
{
    Vector3 startingPos;
    // Start is called before the first frame update
    void Start()
    {
        startingPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.position = startingPos;   
            transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    private void OnCollisionEnter(Collision c)
    {
        if (c.transform.tag == "Player")
        {
            ContactPoint contact = c.contacts[0];
            transform.GetComponent<Rigidbody>().AddForce((contact.point-(contact.point + contact.normal)).normalized*-250);
            Debug.DrawLine(contact.point, contact.point + contact.normal, Color.green, 2, false);
        }
    }
}
