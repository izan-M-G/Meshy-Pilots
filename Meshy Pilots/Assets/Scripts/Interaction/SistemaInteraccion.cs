
using UnityEngine;
using TMPro;

public class SistemaInteraccion : MonoBehaviour
{
    [Header("Raycast")]
    public float distanciaDeInteraccion = 4f;
    public LayerMask capasParaRaycast;
    public Camera camaraJugador;

    [Header("Interfaz")]
    public TextMeshProUGUI textoInteractuar;

    [Header("Sujecion")]
    public Transform puntoDeSujecion;
    public float velocidadAtraccion = 12f;
    public float velocidadMaxima = 10f;

    private Rigidbody objetoAgarrado;

    private bool detectCollisionsOriginal;
    private float dampingOriginal;
    private float angularDampingOriginal;

    void Start()
    {
        if (camaraJugador == null)
            camaraJugador = Camera.main;

        if (camaraJugador == null)
        {
            enabled = false;
            return;
        }

        if (textoInteractuar != null)
            textoInteractuar.gameObject.SetActive(false);

        if (puntoDeSujecion == null)
        {
            GameObject punto = new GameObject("PuntoDeSujecion");

            puntoDeSujecion = punto.transform;
            puntoDeSujecion.SetParent(camaraJugador.transform, false);
            puntoDeSujecion.localPosition = new Vector3(0, 0, 2);
        }
    }

    void Update()
    {
        if (objetoAgarrado != null)
        {
            if (Input.GetKeyDown(KeyCode.F))
                SoltarObjeto();

            return;
        }

        Ray ray = camaraJugador.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit,
            distanciaDeInteraccion, capasParaRaycast))
        {
            Rigidbody rb = hit.collider.GetComponentInParent<Rigidbody>();

            if (rb != null && rb.CompareTag("Interactuable"))
            {
                if (textoInteractuar != null)
                    textoInteractuar.gameObject.SetActive(true);

                if (Input.GetKeyDown(KeyCode.F))
                    AgarrarObjeto(rb);
            }
            else
            {
                OcultarTexto();
            }
        }
        else
        {
            OcultarTexto();
        }
    }

    void FixedUpdate()
    {
        if (objetoAgarrado == null)
            return;

        Vector3 direccion =
            puntoDeSujecion.position -
            objetoAgarrado.worldCenterOfMass;

        Vector3 velocidad = direccion * velocidadAtraccion;

        objetoAgarrado.linearVelocity =
            Vector3.ClampMagnitude(velocidad, velocidadMaxima);
    }

    void AgarrarObjeto(Rigidbody rb)
    {
        objetoAgarrado = rb;

        detectCollisionsOriginal = rb.detectCollisions;
        dampingOriginal = rb.linearDamping;
        angularDampingOriginal = rb.angularDamping;

       

        rb.linearDamping = 5f;
        rb.angularDamping = 5f;

    }


    void SoltarObjeto()
    {
        if (objetoAgarrado == null)
            return;

        Rigidbody rb = objetoAgarrado;

        // Liberar el objeto de la sujecion
        objetoAgarrado = null;

        

        // Quitar resistencia al movimiento
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;

        
        if (textoInteractuar != null)
            textoInteractuar.gameObject.SetActive(false);

    }


    void OcultarTexto()
    {
        if (textoInteractuar != null)
            textoInteractuar.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        if (objetoAgarrado != null)
            SoltarObjeto();
    }
}
