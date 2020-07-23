using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class enemy : MonoBehaviour
{
    NavMeshAgent nav;
    Rigidbody rb;
    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        nav.destination = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //patrol();
        move();
    }

    void patrol()
    {
        if(Vector3.Distance(transform.position,nav.destination)<2)
        {
            float x = UnityEngine.Random.Range(-10, 10);
            float z = UnityEngine.Random.Range(-10, 10);
            Vector3 vec = new Vector3(x, transform.position.y, z);
            nav.destination = vec;
        }
    }

    void move()
    {
        if(nav.destination!=null && nav.enabled == true)
        {
            float speed = 2;
            Vector3 dir = (nav.steeringTarget - transform.position).normalized;
            transform.LookAt(nav.steeringTarget);
            rb.velocity = transform.forward * speed;
        }
    }

    void die()
    {
        nav.enabled = false;
        anim.SetTrigger("dead");
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name.ToUpper());
        //if collide with player fist
        if(other.name.ToUpper() == "FIST")
        {
            die();
            float intensity = 10;
            Vector3 dir = other.transform.parent.transform.forward;
            rb.velocity = dir * intensity;
            Debug.DrawRay(other.transform.position, dir, Color.magenta, 10);
        }
    }
}
