using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MovementPlayer : MonoBehaviour
{


    //movimiento , constantes;
    public float speed = 5f;
    private float horizontalInput;
    private float forwardInput;





    //salto

    public float jumpForce = 5f;

    public LayerMask groundLayer;



    //deteccion de el suelo
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    private bool isGorunded;


    //definicion de el rigid body
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {


        isGorunded = Physics.CheckSphere(groundCheck.position, groundDistance,groundLayer);


        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Jump") && isGorunded)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && isGorunded)
        {
            speed = 7f;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) && isGorunded)
        {
            speed = 5.0f;
        }


    }

    void FixedUpdate()
    {
        Vector3 moveDirection = transform.forward * forwardInput + transform.right * horizontalInput;
        rb.linearVelocity = new Vector3(moveDirection.x * speed, rb.linearVelocity.y, moveDirection.z * speed);
    }

    void Jump()
    {

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }


    private void OnDrawGizmosSelected()
    {

        if(groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(groundCheck.position, groundDistance);
        }


    }

}
