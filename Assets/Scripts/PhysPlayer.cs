using System;
using UnityEngine;

public class PhysPlayer : MonoBehaviour
{
    [SerializeField] float runSpeed = 5;
    [SerializeField] float mouseSensitivity = 2;

    Transform       camTransform;
    CapsuleCollider capsule;
    Rigidbody       body;    

    Vector3 sphereCastOffset;
    float groundScanDistance;
    float mouseX = 0f;
    float mouseY = 0f;
    
    Rigidbody pushingRigidbody = null;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        camTransform = transform.Find("PlayerCamera");
        capsule = GetComponent<CapsuleCollider>();
        body = GetComponent<Rigidbody>();
        sphereCastOffset = new Vector3(0, -capsule.height / 2 + capsule.radius + .05f, 0);
        groundScanDistance = 0.1f;
        mouseX = transform.rotation.eulerAngles.y;
        mouseY = camTransform.rotation.eulerAngles.x;
    }

    void Update()
    {
        Rotation();
    }

    void FixedUpdate()
    {        
        Movement();
    }

    void Rotation()
    {
        mouseX += Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        mouseY = Mathf.Clamp(mouseY, -89f, 89f);
        camTransform.rotation = Quaternion.Euler(mouseY, mouseX, 0f);        
    }

    void Movement()
    {
        Vector3 forward = camTransform.forward * Input.GetAxis("Vertical");
        forward.y = 0; // -- Restrict to xz plane to prevent moving up or down in space.

        Vector3 right = camTransform.right * Input.GetAxis("Horizontal");
        Vector3 direction = Vector3.Normalize(forward + right);

        bool noInput = (direction.magnitude == 0) ? true : false;
        
        // -- Check for ground.
        Vector3 bottom = transform.position + sphereCastOffset;
        RaycastHit hit;        
        bool onGround = Physics.SphereCast(bottom, capsule.radius, Vector3.down, out hit, groundScanDistance);

        if (onGround)
        {
            if (noInput && body.velocity.magnitude < 0.2f)
            {
                body.velocity = Vector3.zero;
                body.Sleep();
                return;
            }

            // -- Create vector parallel to slope
            if (hit.normal != Vector3.up)
                direction = Vector3.Normalize(direction + Vector3.Reflect(direction, hit.normal));

            // -- Apply friction and force
            Vector3 friction = body.velocity * (noInput ? -2400 : -500);

            body.AddForce(friction + direction * 2400);
        }

        /*
        if (pushingRigidbody)
        {
            //pushingRigidbody.AddForce(direction * 700);
        }
        // -- Instantly stop pushing an object if forward input ends.
        if (Input.GetAxis("Vertical") <= 0)
            pushingRigidbody = null;
        */
    }

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody otherBody = collision.rigidbody;
        if (Input.GetAxis("Vertical") <= 0 || otherBody == null || otherBody.isKinematic)
            return;

        // Only push object if it collides within a certain vertical distance of player center.
        Vector3 ownPosition = transform.position;        
        Vector3 closestPoint = collision.collider.ClosestPoint(ownPosition);        
        if (Mathf.Abs(closestPoint.y - ownPosition.y) < 0.3)
            pushingRigidbody = otherBody;        
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.rigidbody == pushingRigidbody)
            pushingRigidbody = null;
    }
}