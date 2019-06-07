using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class PlayerControls : MonoBehaviour
{
    private float uTime = 0.0f;
    public float speed = 10.0f;
    public float gravity = 20.0f;
    public float maxVelocityChange = 10.0f;
    public float grip = 0.75f;
    public bool canJump = true;
    public float jumpHeight = 2.0f;
    private bool grounded = false;
    private bool jumped = false;
    private Rigidbody thisRB;

    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    CursorLockMode wantedMode;

    private void Start()
    {
        
    }

    void Awake()
    {
        thisRB = GetComponent<Rigidbody>();
        thisRB.freezeRotation = true;
        thisRB.useGravity = false;
        Cursor.lockState = wantedMode = CursorLockMode.Locked;
    }

    void FixedUpdate()
    {
        // Set/Release cursor on keypress
        if (Input.GetKeyDown(KeyCode.Escape))
            Cursor.lockState = wantedMode = CursorLockMode.None;
        if (Input.GetKeyDown(KeyCode.L))
            Cursor.lockState = wantedMode = CursorLockMode.Locked;
        // Calculate how fast we should be moving
        Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal") * (1f + Input.GetAxis("Sprint")), 0, Input.GetAxis("Vertical") * (1f + Input.GetAxis("Sprint")));
        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= speed;

        // Apply a force that attempts to reach our target velocity
        Vector3 velocity = thisRB.velocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;
        if (grounded)
        {
            thisRB.AddForce(velocityChange, ForceMode.VelocityChange);
            jumped = false;
            // Jump
            if (canJump && Input.GetButton("Jump"))
            {
                thisRB.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
                jumped = true;
            }
        }
        else
        { 
            thisRB.AddForce(velocityChange*.05f, ForceMode.VelocityChange);
        }

        yaw += speedH * Input.GetAxis("Mouse X");
        pitch -= speedV * Input.GetAxis("Mouse Y");
        if (pitch < -90) pitch = -90;
        if (pitch > 90) pitch = 90;


        transform.eulerAngles = new Vector3(0.0f, yaw, 0.0f);
        transform.GetChild(0).transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

        // We apply gravity manually for more tuning control
        thisRB.AddForce(new Vector3(0, -gravity * thisRB.mass, 0));

        grounded = false;
    }

    void OnCollisionStay()
    {
        grounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
    }

    private void OnTriggerStay(Collider other)
    {
        if(!grounded && !jumped)
        {
            thisRB.AddForce(new Vector3(0, -50 * thisRB.mass, 0));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        jumped = true;
    }

    float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }
}