using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.UIElements;

public class player : MonoBehaviour
{
    float health = 100;
    Rigidbody rb;
    bool grounded; //is the player grounded or in the air?
    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        //get attached components for easy reference
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        grounded = groundCheck(); //grounded status is set by the groundCheck function which returns a boolean every update call        
        if(grounded)
        {
            //draw ray so we can tell if player is grounded or not
            Debug.DrawRay(transform.position, Vector3.up * 3, Color.red);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            //if the jump key is down during fixedUpdate then run the jump function
            jump();
        }
        if (Input.GetKeyDown(KeyCode.E) && grounded)
        {
            //if the E key is pressed and the player isnt currently attacking then do attack function
            anim.ResetTrigger("attack");
            anim.SetTrigger("attack");
        }
        animate(); //this is where we setup the animator parameters
    }

    private void FixedUpdate()
    {
        //physics functions must be in fixedUpdate so that they are framerate independent and provide smooth motion;
        move(); // most of the player physics will be in this function
        look(); //handles rotation of the player character        
        ledgeDetect();
    }

    void ledgeDetect()
    {
        //Check for small ledges in front of player
        int counter = 0; //number of rays that hit something
        int threshold = 3; //number of hits before we decide there is a ledge
        float length = 1;        
        Vector3 start = new Vector3(-0.35f, 0f, 0.5f);
        start = transform.TransformDirection(start);
        for (float x = 0; x < 4; x++)
        {
            for (float i = 0; i < 4; i++)
            {
                Vector3 offset = transform.TransformDirection(new Vector3(i, 0, x)) * 0.2f;
                if(Physics.Raycast(transform.position + start + offset, Vector3.down, length))
                {
                    Debug.DrawRay(transform.position + start + offset, Vector3.down * length, Color.red);
                    counter++;
                }
                else
                {
                    Debug.DrawRay(transform.position + start + offset, Vector3.down * length, Color.green);
                }                
            }
        }
        if(counter>threshold)
        {
            rb.velocity += Vector3.up * 0.5f;
        }
    }

    void look()
    {
        Vector3 vec = rb.velocity; //create a vector copy of the players velocity
        vec = vec.normalized * 2; //normalize it to make it speed independent and give it a general length of 2
        vec.y = 0; //set vector's Y value to zero so the player isnt looking up or down
        if (vec != Vector3.zero)
        {
            transform.LookAt(transform.position + vec); //have the player object look at this new direction
        }
    }

    void move()
    {
        float speed = 3; //general speed of player
        float grav = 18; //strength of gravity
        float vert = rb.velocity.y; //vertical velocity must be kept track of between update calls
        float fwd = Input.GetAxis("Vertical");  //forward velocity
        float horz = Input.GetAxis("Horizontal"); //horizontal velocity
        if(Input.GetKey(KeyCode.LeftShift))
        {
            speed *= 1.6f; //if the left shift key is down then speed is increased
        }
        if(!grounded) //player is airborne
        {            
            if (vert < 0) //if player is falling then increase the rate of gravity in order to create a nicer feeling jump arc
            {
                vert -= (grav * 2) * Time.deltaTime; //incrementally add gravity to vertical velocity
            }
            else //if player is jumping or moving upward then add gravity normally
            {
                vert -= grav * Time.deltaTime;
            }
        }
        else //player is grounded
        {
            if (vert < 0) //reset vertical velocity or else we will have some wonkiness
            {
                vert = 0;
            }
        }
        vert = Mathf.Clamp(vert, -25, 100); //we need to clamp vertical velocity so it doesn't get out of hand
        Vector3 vec = new Vector3(horz * speed, vert, fwd * speed); //create the new velocity vector
        vec = Camera.main.transform.parent.transform.TransformDirection(vec); //align vector to match the camera man's rotation
        rb.velocity = vec; // assign new velocity vector
    }

    public void hurt(float dmg)
    {
        //call this from other scripts when they hurt the player
        health -= dmg;
        if(health<0)
        {
            //add game over function
        }
    }

    bool groundCheck()
    {
        float height = 2f; //general player height is close to 2 units
        RaycastHit hit; //where we store what the raycast hits
        Physics.Raycast(transform.position, -Vector3.up, out hit, height *0.6f); //raycast down from player. since raycast is from center make it a little more than half of players height
        Debug.DrawRay(transform.position, -Vector3.up * (height * 0.5f), Color.green); //draw said raycast
        if (hit.transform != null)
        {           
            return true; //if we hit something then the player is technically grounded so return true
        }
        else
        {
            return false; //if we didn't hit anything then the player is airborne so return false
        }
    }

    void jump()
    {
        float jumpForce = 10; //strength of jump
        if (grounded) //only allow jump is player is grounded
        {
            if (!anim.GetNextAnimatorStateInfo(0).IsName("Land")) //if the player is going to begin the landing animation then don't allow them to jump or else it feels weird and bad
            { //might remove landing animation
                rb.velocity = new Vector3(rb.velocity.x,jumpForce,rb.velocity.z); //set velocity to a similar vector where we can set the vertical velocity to the jump strength
            }
        }
    }

    void animate()
    {
        if(rb.velocity != Vector3.zero) //if the player is moving at all then cue the moving animation
        {
            anim.SetBool("moving", true);
        }
        else
        {
            anim.SetBool("moving", false);
        }
        anim.SetBool("airborne", !grounded); //the airborne animation is cued by the grounded variable
        anim.SetFloat("speed", rb.velocity.magnitude/1.5f);
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Coin"))
        {
            FindObjectOfType<Director>().coinAdd();
            Instantiate(Resources.Load<GameObject>("Prefabs/Poof"), other.transform.position, other.transform.rotation);
            Destroy(other.gameObject);            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.CompareTag("Enemy"))
        {
            hurt(40);
        }
    }
}
