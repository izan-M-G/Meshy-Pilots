
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementPlayer : NetworkBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;
    public float velocidadCorrer = 7f;

    private float horizontalInput;
    private float forwardInput;

    [Header("Salto")]
    public float jumpForce = 5f;
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public float groundDistance = 0.25f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private bool puedeSaltar;
    private bool saltoSolicitado;
    private float rotacionLocal;

    private NetworkVariable<float> rotacionSolicitada =
        new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

    private HeadMovement headMovement;
    private Camera camaraJugador;
    private AudioListener audioJugador;

    private NetworkVariable<Vector2> inputMovimiento =
        new NetworkVariable<Vector2>(
            Vector2.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

    private NetworkVariable<bool> correr =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        rotacionLocal = transform.eulerAngles.y;

        if (IsOwner)
            rotacionSolicitada.Value = rotacionLocal;

        headMovement = GetComponentInChildren<HeadMovement>(true);
        camaraJugador = GetComponentInChildren<Camera>(true);
        audioJugador = GetComponentInChildren<AudioListener>(true);

        if (headMovement != null)
            headMovement.enabled = IsOwner;

        if (camaraJugador != null)
            camaraJugador.enabled = IsOwner;

        if (audioJugador != null)
            audioJugador.enabled = IsOwner;

        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        inputMovimiento.Value =
            new Vector2(horizontalInput, forwardInput);

        correr.Value = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetButtonDown("Jump"))
            SolicitarSaltoRpc();
    }

    private void FixedUpdate()
    {
        if (!IsServer || rb == null) return;

        rb.MoveRotation(
            Quaternion.Euler(0f, rotacionSolicitada.Value, 0f)
        );

        DetectarSuelo();

        Vector2 entrada = inputMovimiento.Value;

        Vector3 moveDirection =
            transform.forward * entrada.y +
            transform.right * entrada.x;

        float velocidadActual =
            correr.Value ? velocidadCorrer : speed;

        rb.linearVelocity = new Vector3(
            moveDirection.x * velocidadActual,
            rb.linearVelocity.y,
            moveDirection.z * velocidadActual
        );

        if (saltoSolicitado && puedeSaltar)
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z
            );

            rb.AddForce(
                Vector3.up * jumpForce,
                ForceMode.Impulse
            );

            saltoSolicitado = false;
            puedeSaltar = false;
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = true)]
    private void SolicitarSaltoRpc()
    {
        if (puedeSaltar)
            saltoSolicitado = true;
    }

    private void DetectarSuelo()
    {
        puedeSaltar = false;

        if (groundCheck == null) return;

        Vector3 origen =
            groundCheck.position + Vector3.up * 0.1f;

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
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(
                groundCheck.position,
                groundDistance
            );
        }
    }

    public void AplicarGiro(float giro)
    {
        if (!IsOwner) return;

        rotacionLocal += giro;
        rotacionSolicitada.Value = rotacionLocal;
    }
}
