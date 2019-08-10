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
        
        Vector3 forwardInput = camForwardFlat * Input.GetAxis("Vertical");        
        Vector3 rightInput = camTransform.right * Input.GetAxis("Horizontal");
        Vector3 direction = Vector3.Normalize(forwardInput + rightInput);

        bool noInput = (direction.magnitude == 0) ? true : false;

        // -- Check for ground.
        Vector3 lowestGroundNormal = Vector3.zero; // Lowest hit point that's under slopeLimit.
        Vector3 bottom = transform.position + sphereCastOffset;
        bool onGround = false;

        // @TODO @CLEANUP: Might not need SphereCastAll anymore. A regular raycast will probably work now that the sphereCast radius has been reduced.
        //
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
            body.AddForce(friction + direction * 2500);
        }
        else
        {
            if (body.IsSleeping()) // -- Objects might more or slide out from under player after player sleeps.
            {
                print("TEST NOTIFICATION: Waking Up.");
                body.WakeUp();
            }
            
            body.AddForce(Vector3.down * 700); // -- Gravity - @TODO: Git rid of magic number.
        }
    }    
}