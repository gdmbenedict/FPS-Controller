using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour
{
    //movement settings
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;

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

    //other variables
    private float xRotation = 0f;

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
        HandleMouse();
        HandleMovement();
    }

    //method to handle the movement of a player
    void HandleMovement()
    {
        //getting player input;
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        characterController.Move(move * walkSpeed * Time.deltaTime);
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
