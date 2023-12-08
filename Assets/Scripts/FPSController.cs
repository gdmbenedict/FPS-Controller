using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour
{
    //movement settings
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;

    //mouse settings
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensativity = 100f;
    [SerializeField] private float maxDown = -90f;
    [SerializeField] private float maxUp = 90f;

    //components
    [Header("Components")]
    [SerializeField] private GameObject playerCamera;
    private CharacterController characterController;
    private Transform playerBody;

    //jumping
    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 2f;

    //floor checking
    [Header("Floor Checking")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance;
    [SerializeField] private LayerMask groundLayerMask;
    private bool isGrounded;

    //external variables
    [Header("External Variables")]
    [SerializeField] private float gravity = -9.81f;

    //other variables
    private float xRotation = 0f;
    [SerializeField]  private Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        //get components
        characterController = GetComponent<CharacterController>(); 
        playerBody = GetComponent<Transform>();

        //locks cursor to screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //check to see if grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayerMask);

        HandleMouse();
        HandleMovement();
    }

    //method to handle the movement of a player
    void HandleMovement()
    {
        //resetting velocity if grounded
        if (isGrounded && velocity.y < 0)
        {
            //not zero to make sure that player actually reaches the ground
            velocity.y = -2f;
        }

        //getting player input;
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //getting unit vector for direction
        Vector3 move = Vector3.ClampMagnitude(transform.right * x + transform.forward * z, 1.0f);

        //determine movement speed
        if (Input.GetButton("Run"))
        {
            move *= runSpeed;
        }
        else
        {
            move *= walkSpeed;
        }

        //applying speed
        if (isGrounded)
        {
            velocity.x = move.x;
            velocity.z = move.z;
        }

        //applying gravity
        velocity.y += gravity * Time.deltaTime;
        
        //jumping
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        //moving character
        characterController.Move(velocity * Time.deltaTime);

    }

    //method that handles mouse inputs
    void HandleMouse()
    {
        //getting mouse inputs
        float mouseX = Input.GetAxis("Mouse X") * mouseSensativity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensativity * Time.deltaTime;

        //rotate the player horixontally with mouse
        playerBody.Rotate(Vector3.up * mouseX);

        //rotate camera vertically with mouse
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, maxDown, maxUp);

        playerCamera.GetComponent<Transform>().localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
