using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FPSController : MonoBehaviour
{
    //movement settings
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float crouchSpeed = 2f;

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
    [SerializeField] private float airMovementFactor = 2f;

    //floor checking
    [Header("Floor Checking")]
    [SerializeField] private GameObject groundCheck;
    [SerializeField] private float groundDistance;
    [SerializeField] private LayerMask groundLayerMask;
    private bool isGrounded;

    //Crouching
    [Header("Chrouching Varaibles")]
    [SerializeField] private float crouchingHeight = 0.5f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private float crouchTransitionTime = 0.5f;
    private bool isCrouching;
    private bool isTransitioning;

    //standing
    [Header("Standing Varaibles")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private Vector3 standingCenter = new Vector3(0f, 2f, 0f);

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
        isGrounded = Physics.CheckSphere(groundCheck.transform.position, groundDistance, groundLayerMask);

        HandleMouse();
        HandleMovement();
        HandleCourch();
    }

    //method to handle the movement of a player
    private void HandleMovement()
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
        if (Input.GetButton("Run") && isGrounded && !isCrouching)
        {
            move *= runSpeed;
        }
        else if (isCrouching && isGrounded)
        {
            move *= crouchSpeed;
        }
        else
        {
            move *= walkSpeed;
        }

        //applying movement on ground
        if (isGrounded)
        {
            velocity.x = move.x;
            velocity.z = move.z;
        }
        //applying movement in air
        else
        {
            velocity.x += move.x * airMovementFactor * Time.deltaTime;
            velocity.z += move.z * airMovementFactor * Time.deltaTime;
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
    private void HandleMouse()
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

    //method to handle crouching
    private void HandleCourch()
    {
        //determine if player can crouch
        if (isGrounded && !isTransitioning && Input.GetButtonDown("Crouch") && !isCrouching)
        {
            StartCoroutine(CrouchStand());
        }

        //unchrouching
        if (!Input.GetButton("Crouch") && isCrouching)
        {
            StopAllCoroutines();
            
            StartCoroutine(CrouchStand());
        }
    }


    //method to handle transitioning from crouching to standing or vice-versa
    private IEnumerator CrouchStand()
    {
        //check for cieling above player while uncrouching
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
        {
            //if cieling is above player cancel co-routine
            yield break;
        }

        isTransitioning = true;

        //setting transition variables
        float timeElapsed = 0;

        float targetHeight = isCrouching ? standingHeight : crouchingHeight;
        float currentHeight = characterController.height;

        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        //moving groound check
        float groundCheckMove = isCrouching ? -1f : 1f;
        groundCheck.transform.position += new Vector3(0, groundCheckMove, 0);

        isCrouching = !isCrouching;

        //while in transition time window
        while (timeElapsed < crouchTransitionTime)
        {
            //changing height and center point of character controller
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed/crouchTransitionTime);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed/crouchTransitionTime);
            
            //increment time and wait for next frame
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        //ensuring correct final position
        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isTransitioning = false;
    }
}
