using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Camera playerCamera;
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float runMultiplier = 1.5f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float gravity = 10f;
    public float mouseSensitivity = 60f;
    [SerializeField] float lookXLimit = 60f;
    float rotationX = 0f;
    Vector3 moveDirection;
    CharacterController controller;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = Camera.main;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        #region Movement

        float movementDirectionY = moveDirection.y;

        if (controller.isGrounded)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            moveDirection = (horizontalInput * transform.right) + (verticalInput * transform.forward).normalized;

            //I multiplied the moveDirection here instead of the moveSpeed later, as I thought it might multiply the gravity too otherwise
            if (Input.GetKeyDown(KeyCode.LeftShift)) moveDirection *= runMultiplier;

            if (Input.GetKeyUp(KeyCode.LeftShift)) moveDirection /= runMultiplier;

            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = jumpForce;
            }
            else
            {
                moveDirection.y = movementDirectionY;
            }
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        #endregion

        #region Rotation

        transform.Rotate(Vector3.up * mouseSensitivity * Time.deltaTime * Input.GetAxis("Mouse X"));
        
        rotationX += Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        #endregion
    }
}
