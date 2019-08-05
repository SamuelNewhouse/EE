using System;
using UnityEngine;

public class PhysPlayer : MonoBehaviour
{
    [SerializeField] float mouseSensitivity = 2;
    [SerializeField] float slopeLimit = 44;

    Transform       camTransform;
    CapsuleCollider capsule;
    Rigidbody       body;

    Vector3 sphereCastOffset;    
    float sphereCastDistance = 0.5f;
    float sphereCastRadius;

    float mouseX;
    float mouseY;
    
    // Rigidbody pushingRigidbody = null;

    void Start()
    {
        capsule = GetComponent<CapsuleCollider>();
        body = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        camTransform = transform.Find("PlayerCamera");

        mouseX = transform.rotation.eulerAngles.y;
        mouseY = camTransform.rotation.eulerAngles.x;

        // -- Bottom edge of cast sphere will start slightly above capsule bottom.
        sphereCastRadius = capsule.radius - sphereCastDistance / 2;
        float sphereCastOffsetY = -capsule.height / 2 + capsule.radius;
        sphereCastOffset = new Vector3(0, sphereCastOffsetY, 0);
    }

    void Update()
    {
        Rotation(); // -- Camera movement should be as responsive as possible.
    }

    void FixedUpdate()
    {        
        Movement(); // -- Body movement should sync perfectly with physics.
    }

    void Rotation()
    {
        // @TODO: Invert Axis Option
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
        Vector3 lowestGroundNormal = Vector3.zero; // Lowest hit point that's under slopeLimit.
        Vector3 bottom = transform.position + sphereCastOffset;
        bool onGround = false; //Physics.SphereCast(bottom, capsule.radius, Vector3.down, out hit, sphereCastDistance);

        // -- Need to SphereCastAll because the player might be "on" a very steep slope when they should really be considered on a
        //    lesser slope that's further below them. Anything over the slopeLimit will be ignored for determining onGround.
        RaycastHit[] hits = Physics.SphereCastAll(bottom, sphereCastRadius, Vector3.down, sphereCastDistance);

        float lowestY = Mathf.Infinity;
        foreach (RaycastHit h in hits)
        {            
            if (h.rigidbody != body && Vector3.Angle(Vector3.up, h.normal) <= slopeLimit && h.point.y < lowestY)
            {
                onGround = true;
                lowestY = h.point.y;
                lowestGroundNormal = h.normal;                
            }
        }

        if (onGround)
        {
            // -- Create vector parallel to ground slope
            if (lowestGroundNormal != Vector3.up)
                direction = Vector3.Normalize(direction + Vector3.Reflect(direction, lowestGroundNormal));

            // -- Apply friction and force
            // @TODO: Git rid of magic numbers.
            Vector3 friction = body.velocity * (noInput ? -700 : -500);
            body.AddForce(friction + direction * 1900);
        }
        else
        {
            if (body.IsSleeping()) // -- Objects can slide out from under player after player sleeps.
            {
                print("Waking Up.");
                body.WakeUp();
            }
            
            body.AddForce(Vector3.down * 700); // -- Gravity
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

    /*
    void OnCollisionEnter(Collision collision)
    {
        Rigidbody otherBody = collision.rigidbody;
        if (Input.GetAxis("Vertical") <= 0 || otherBody == null || otherBody.isKinematic)
            return;

        // -- Only push object if it collides within a certain vertical distance of player center.
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
    */
}