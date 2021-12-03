using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject camera;
    [SerializeField] float x, y, mouseX, mouseY, rotationOnX;
    [SerializeField] float moveSpeed, jumpForce;
    [Range(0, 1)]
    public float counterForce;
    [Range(0, 300)]
    public float rotSpeed;
    [SerializeField] private Transform player, orientation;
    [SerializeField] LayerMask ground;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void MyInput()
    {
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
    }

    // Update is called once per frame
    void Update()
    {
        MyInput();
        //This section of my code actually controls the camera movement
        float horiz = mouseX * rotSpeed * Time.deltaTime;
        float vert = mouseY * rotSpeed * Time.deltaTime;

        // How to control camera's y-axis movement
        rotationOnX -= mouseY;
        rotationOnX = Mathf.Clamp(rotationOnX, -90f, 90f);
        camera.transform.localEulerAngles = new Vector3(rotationOnX, 0f, 0f);

        //How to control the camera's x-axis movement
        player.Rotate(Vector3.up * horiz);

        // This section of my code is used to control the player's actual movement
        Vector3 forceToMove = ((x * orientation.right) + (y * orientation.forward)) * moveSpeed;
        rb.AddForce(forceToMove);
        Vector3 counterMovement = forceToMove * (-counterForce);
        if (rb.velocity.magnitude > 0)
        {
            rb.AddForce(counterMovement);
        }

    }
}
