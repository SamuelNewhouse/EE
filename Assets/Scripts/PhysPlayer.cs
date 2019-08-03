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
    float sphereCastDistance = 0.1f;
    float mouseX = 0f;
    float mouseY = 0f;
    
    Rigidbody pushingRigidbody = null;

    void Start()
    {
        capsule = GetComponent<CapsuleCollider>();
        body = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        camTransform = transform.Find("PlayerCamera");

        mouseX = transform.rotation.eulerAngles.y;
        mouseY = camTransform.rotation.eulerAngles.x;

        // Bottom edge of cast sphere will start slightly above capsule bottom.
        float sphereCastOffsetY = -capsule.height / 2 + capsule.radius + .05f;
        sphereCastOffset = new Vector3(0, sphereCastOffsetY, 0);
    }

    void Update()
    {
        Rotation(); // Camera movement should be as responsive as possible.
    }

    void FixedUpdate()
    {        
        Movement(); // Body movement should sync perfectly with physics.
    }

    void Rotation()
    {
        mouseX += Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        mouseY = Mathf.Clamp(mouseY, -89.9f, 89.9f);
        camTransform.rotation = Quaternion.Euler(mouseY, mouseX, 0f);        
    }

    void Movement()
    {
        // -- When getting forward input multiplier from camera rotation, we need to remove up/down rotation and re-normalize.
        Vector3 camForwardFlat = camTransform.forward;
        camForwardFlat.y = 0; // -- Restrict to xz plane to prevent moving up or down in space.
        camForwardFlat = Vector3.Normalize(camForwardFlat);
        
        Vector3 forward = camForwardFlat * Input.GetAxis("Vertical");        
        Vector3 right = camTransform.right * Input.GetAxis("Horizontal");
        Vector3 direction = Vector3.Normalize(forward + right);

        bool noInput = (direction.magnitude == 0) ? true : false;

        // -- Check for ground.
        RaycastHit hit;
        Vector3 bottom = transform.position + sphereCastOffset;
        bool onGround = Physics.SphereCast(bottom, capsule.radius, Vector3.down, out hit, sphereCastDistance);

        if (onGround)
        {
            if (noInput && body.velocity.magnitude < 0.2f)
            {
                body.velocity = Vector3.zero;
                body.Sleep();
                return;
            }

            // -- Create vector parallel to ground slope
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