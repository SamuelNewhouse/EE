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

    Vector3 lastPushDirection = Vector3.zero;

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

        if (direction.magnitude == 0)
            return;        

        Vector3 bottom = transform.position - halfHeight;        
        RaycastHit hit;

        if (Physics.Raycast(bottom, Vector3.down, out hit, groundScanDistance) && hit.normal != Vector3.up)
            direction = Vector3.Normalize(direction + Vector3.Reflect(direction, hit.normal));

        body.MovePosition(transform.position + direction * runSpeed * Time.deltaTime);

        lastPushDirection = direction;
    }

    // @TODO: Track one object to push at a time using OnCollisionEnter and OnCollisionExit.
    //   Place the code in Movement() to ensure perfect lock with physics simulation.
    //   Right now, it's based on a frame-by-frame update.
    //

    
    void OnCollisionStay(Collision collision)
    {        
        Rigidbody otherBody = collision.rigidbody;
        if (Input.GetAxis("Vertical") > 0 && otherBody != null && !otherBody.isKinematic)
            otherBody.velocity = lastPushDirection * runSpeed * Time.deltaTime * 50;
    }

    void OnCollisionExit(Collision collision)
    {
        Rigidbody otherBody = collision.rigidbody;
        if (otherBody != null && !otherBody.isKinematic)
            otherBody.velocity = lastPushDirection * runSpeed * Time.deltaTime * 10;
    }
}