using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MovementPlayer : MonoBehaviour
{


    //movimiento , constantes;
    public float speed = 5f;
    private float horizontalInput;
    private float forwardInput;

    public float velocidadCorrer = 7f;



    //salto
    [Header("Salto")]
    public float jumpForce = 5f;
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public float groundDistance = 0.25f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private bool puedeSaltar;
    private bool saltoSolicitado;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {

        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        DetectarSuelo();


        if (Input.GetButtonDown("Jump") && puedeSaltar)
        {
            saltoSolicitado = true;
            puedeSaltar = false;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && puedeSaltar)
        {
            speed = 7f;
        }
        speed = Input.GetKey(KeyCode.LeftShift)
            ? velocidadCorrer
            : 5f;


    }

    void FixedUpdate()
    {
        Vector3 moveDirection =
            transform.forward * forwardInput +
            transform.right * horizontalInput;

        rb.linearVelocity = new Vector3(
            moveDirection.x * speed,
            rb.linearVelocity.y,
            moveDirection.z * speed
        );

        if (saltoSolicitado)
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z
            );

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            saltoSolicitado = false;
        }
    }

    void DetectarSuelo()
    {
        puedeSaltar = false;

        Vector3 origen = groundCheck.position + Vector3.up * 0.1f;

        if (Physics.SphereCast(
            origen,
            groundRadius,
            Vector3.down,
            out RaycastHit hit,
            groundDistance,
            ~0,
            QueryTriggerInteraction.Ignore))
        {
            

            Transform objeto = hit.collider.transform;
            bool esInteractuable = false;

            while (objeto != null)
            {
                if (objeto.CompareTag("Interactuable"))
                {
                    esInteractuable = true;
                    break;
                }

                objeto = objeto.parent;
            }

            puedeSaltar = !esInteractuable;

        }
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
