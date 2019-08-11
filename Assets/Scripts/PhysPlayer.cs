using System;
using UnityEngine;

public class PhysPlayer : MonoBehaviour
{
    [SerializeField] float mouseSensitivity = 2;
    [SerializeField] float slopeLimit = 44;

    private Transform       camTransform;
    private CapsuleCollider capsule;
    private Rigidbody       playerBody;

    private Vector3 sphereCastOffset;    
    private float sphereCastDistance = 0.5f;
    private float sphereCastRadius;

    private float mouseX;
    private float mouseY;

    private int ElementBoxesLayer = 1 << 8;
    private int postProcessLayer = 1 << 30;

    private Vector3 pushBoxAssistCapsuleCastPointOffset;
    private float pushBoxAssistCapsuleCastRadius = 0.1f;
    private float pushBoxAssistCapsuleCastDistance;

    void Start()
    {
        capsule = GetComponent<CapsuleCollider>();
        playerBody = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        camTransform = transform.Find("PlayerCamera");

        mouseX = transform.rotation.eulerAngles.y;
        mouseY = camTransform.rotation.eulerAngles.x;

        // -- Bottom edge of cast sphere will start slightly above capsule bottom.
        sphereCastRadius = capsule.radius - sphereCastDistance / 2;
        float sphereCastOffsetY = -capsule.height / 2 + capsule.radius;
        sphereCastOffset = new Vector3(0, sphereCastOffsetY, 0);

        pushBoxAssistCapsuleCastPointOffset = Vector3.up * (capsule.height / 1.125f);
        pushBoxAssistCapsuleCastDistance = capsule.radius;
    }

    void Update()
    {
        Rotation(); // -- Camera movement should be as responsive as possible.
    }

    void FixedUpdate()
    {        
        Movement(); // -- Body movement should sync perfectly with physics.
        PushBoxAssist(); // -- Box pushing should sync perfectly with physics.
    }

    private void Rotation()
    {
        // @TODO: Invert Axis Option
        mouseX += Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        mouseY = Mathf.Clamp(mouseY, -89.9f, 89.9f);
        camTransform.rotation = Quaternion.Euler(mouseY, mouseX, 0f);        
    }

    private void Movement()
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
        RaycastHit[] hits = Physics.SphereCastAll(bottom, sphereCastRadius, Vector3.down, sphereCastDistance, ~postProcessLayer);

        float lowestY = Mathf.Infinity;
        foreach (RaycastHit h in hits)
        {            
            if (h.rigidbody != playerBody && Vector3.Angle(Vector3.up, h.normal) <= slopeLimit && h.point.y < lowestY)
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
            Vector3 friction = playerBody.velocity * (noInput ? -700 : -500);
            playerBody.AddForce(friction + direction * 2500);
        }
        else
        {
            if (playerBody.IsSleeping()) // -- Objects might more or slide out from under player after player sleeps.
            {
                print("TEST NOTIFICATION: Waking Up.");
                playerBody.WakeUp();
            }
            
            playerBody.AddForce(Vector3.down * 700); // -- Gravity - @TODO: Git rid of magic number.
        }
    }

    private void PushBoxAssist()
    {
        // @TODO: Requiring 0 horizontal might be a problem for joysticks.
        //        Change to only having the forward input direct the box velocity while ignoring non-forward velocity.
        if (Input.GetAxis("Vertical") == 0 || Input.GetAxis("Horizontal") != 0)
            return;

        RaycastHit boxHit;
        Vector3 point1 = transform.position + pushBoxAssistCapsuleCastPointOffset;
        Vector3 point2 = transform.position - pushBoxAssistCapsuleCastPointOffset;        
        Vector3 direction = playerBody.velocity;

        if (!Physics.CapsuleCast(point1, point2, pushBoxAssistCapsuleCastRadius, direction, out boxHit, pushBoxAssistCapsuleCastDistance, ElementBoxesLayer))
            return;

        // -- If pushing too near to an edge, don't activate PushBoxAssist. @TODO: Remove magic number.
        if ((boxHit.point - boxHit.transform.position).magnitude > 0.75f)
            return;

        Rigidbody boxBody = boxHit.collider.gameObject.GetComponent<Rigidbody>();
        Transform boxTransform = boxBody.transform;

        // -- To make pushing boxes MUCH better, lock box rotation to match slope while player is pushing the box.
        RaycastHit floorHit;
        if (Physics.Raycast(boxTransform.position, Vector3.down, out floorHit, 1.7f, ~ElementBoxesLayer))
        {            
            Vector3 hitNormal = floorHit.normal;
            Vector3 up = boxTransform.up;
            Vector3 forward = boxTransform.forward;
            Vector3 right = boxTransform.right;

            // -- Determine how much each axis vector currently aligns with hitNormal.
            float upShare = Mathf.Abs(Vector3.Dot(hitNormal, up));
            float forwardShare = Mathf.Abs(Vector3.Dot(hitNormal, forward));
            float rightShare = Mathf.Abs(Vector3.Dot(hitNormal, right));

            Vector3 rotationAxis;
            float rotationAngle;

            // -- Calculate rotationAngle and rotationAxis from axis vector closest to hitNormal.
            if (upShare >= forwardShare && upShare >= rightShare)
            {
                rotationAngle = Vector3.Angle(hitNormal, up);
                rotationAxis = Vector3.Cross(hitNormal, up);
            }
            else if (forwardShare >= upShare && forwardShare >= rightShare)
            {
                rotationAngle = Vector3.Angle(hitNormal, forward);
                rotationAxis = Vector3.Cross(hitNormal, forward);
            }
            else
            {
                rotationAngle = Vector3.Angle(hitNormal, right);
                rotationAxis = Vector3.Cross(hitNormal, right);
            }

            // -- If best axis vector is pointing away from hitNormal, recalculate.
            if (rotationAngle > 90.0f)
            {
                rotationAngle = Mathf.Abs(rotationAngle - 180.0f);
                rotationAxis = -rotationAxis;
            }

            // -- Rotate less than the full amount for a smoother transition between slopes.
            boxTransform.Rotate(rotationAxis, -rotationAngle / 4, Space.World);

            // -- Manipulate Rigidbody for smoother transitions and better physics.
            boxBody.MoveRotation(boxTransform.rotation);
            boxBody.angularVelocity = Vector3.zero;
            
            // -- Keep the box's velocity inline with player's velocity to make pushing easier.
            boxBody.velocity = boxBody.velocity.magnitude * playerBody.velocity.normalized;
        }
    }
}    