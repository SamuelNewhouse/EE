using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float runSpeed = 5;
    [SerializeField] float mouseSensitivity = 2;

    Transform           camTransform;
    CharacterController controller;

    Vector3 halfControllerHeight;
    float   groundScanDistance;
    float   mouseX = 0f;
    float   mouseY = 0f;    

    void Start()
    {
        Cursor.lockState     = CursorLockMode.Locked;
        camTransform         = transform.Find("PlayerCamera");
        controller           = GetComponent<CharacterController>();
        halfControllerHeight = new Vector3(0, controller.height / 2, 0);
        groundScanDistance   = controller.radius / 2;
    }
 
    void Update()
    {
        Rotation();
        Movement();        
    }

    void Rotation()
    {
        mouseX += Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        mouseY  = Mathf.Clamp(mouseY, -90f, 90f);
        camTransform.localRotation = Quaternion.Euler(mouseY, 0f, 0f);
        transform.rotation         = Quaternion.Euler(0f, mouseX, 0f);
    }

    void Movement()
    {        
        Vector3 bottom    = transform.position - halfControllerHeight;
        Vector3 forward   = transform.forward  * Input.GetAxis("Vertical");
        Vector3 right     = transform.right    * Input.GetAxis("Horizontal");
        Vector3 direction = Vector3.Normalize(forward + right);
        RaycastHit hit;

        if (Physics.Raycast(bottom, Vector3.down, out hit, groundScanDistance) && hit.normal != Vector3.up)
        {
            direction += Vector3.Reflect(direction, hit.normal);
            direction  = Vector3.Normalize(direction);

            if (direction.y < 0) // Running downhill.
                controller.Move(direction * runSpeed * Time.deltaTime);
            else // Running uphill.
                controller.SimpleMove(direction * runSpeed);
        }
        else // On flat ground or not on any ground.
            controller.SimpleMove(direction * runSpeed);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // no rigidbody
        if (body == null || body.isKinematic) { return; }
        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3f) { return; }
        // Calculate push direction from move direction,
        // we only push objects to the sides never up and down
        var pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        // If you know how fast your character is trying to move,
        // then you can also multiply the push velocity by that.
        // Apply the push
        body.velocity = pushDir * 2;
    }
}