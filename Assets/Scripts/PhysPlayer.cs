using UnityEngine;

public class PhysPlayer : MonoBehaviour
{
    [SerializeField] float runSpeed = 5;
    [SerializeField] float mouseSensitivity = 2;

    Transform       camTransform;
    CapsuleCollider capsule;
    Rigidbody       body;    

    Vector3 halfHeight;
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
        halfHeight = new Vector3(0, capsule.height / 2, 0);
        groundScanDistance = capsule.radius / 2;
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
        mouseY = Mathf.Clamp(mouseY, -90f, 90f);
        camTransform.localRotation = Quaternion.Euler(mouseY, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, mouseX, 0f);
    }

    void Movement()
    {
        Vector3 forward = transform.forward * Input.GetAxis("Vertical");
        Vector3 right = transform.right * Input.GetAxis("Horizontal");
        Vector3 direction = Vector3.Normalize(forward + right);

        // Instantly stop pushing an object if forward input ends.
        if (Input.GetAxis("Vertical") <= 0)
            pushingRigidbody = null;

        if (direction.magnitude == 0)
            return;

        // Always align player motion parallel to slopes.
        Vector3 bottom = transform.position - halfHeight;        
        RaycastHit hit;        
        if (Physics.Raycast(bottom, Vector3.down, out hit, groundScanDistance) && hit.normal != Vector3.up)
            direction = Vector3.Normalize(direction + Vector3.Reflect(direction, hit.normal));

        Vector3 baseMove = direction * runSpeed * Time.deltaTime;

        body.MovePosition(transform.position + baseMove);

        if(pushingRigidbody)
            pushingRigidbody.velocity = baseMove * 50;
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