
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

    [Header("Sistema de montaje")]
    public SistemaMontaje sistemaMontaje;

    private Rigidbody objetoAgarrado;
    private bool previsualizando = false;

    private float dampingOriginal;
    private float angularDampingOriginal;
    private bool gravedadOriginal;


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
        // CASO 1: Llevamos una pieza
        if (objetoAgarrado != null)
        {
            PuntoMontaje punto = null;

            if (sistemaMontaje != null)
                punto = sistemaMontaje.ObtenerPuntoMontaje();

            PiezaModular pieza =
                objetoAgarrado.GetComponent<PiezaModular>();

            bool puedeColocar = false;

            if (punto != null && pieza != null &&
                punto.PuedeAceptar(pieza))
            {
                if (!previsualizando)
                {
                    if (!objetoAgarrado.isKinematic)
                    {
                        objetoAgarrado.linearVelocity = Vector3.zero;
                        objetoAgarrado.angularVelocity = Vector3.zero;
                    }

                    objetoAgarrado.isKinematic = true;
                    objetoAgarrado.useGravity = false;
                }

                puedeColocar = sistemaMontaje.PrevisualizarMontaje(
                    objetoAgarrado, punto);

                previsualizando = puedeColocar;

                if (puedeColocar)
                    MostrarTexto("F - Colocar pieza");
                else
                    MostrarTexto("F - Soltar pieza");
            }
            else
            {
                if (previsualizando)
                {
                    objetoAgarrado.isKinematic = false;
                    objetoAgarrado.useGravity = gravedadOriginal;

                    objetoAgarrado.linearVelocity = Vector3.zero;
                    objetoAgarrado.angularVelocity = Vector3.zero;

                    previsualizando = false;
                }

                if (punto != null)
                    MostrarTexto("Soporte incompatible");
                else
                    MostrarTexto("F - Soltar pieza");
            }

            // Instalar o soltar
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (puedeColocar)
                {
                    bool instalada =
                        sistemaMontaje.IntentarMontar(
                            objetoAgarrado, punto);

                    if (instalada)
                    {
                        objetoAgarrado = null;
                        previsualizando = false;
                        OcultarTexto();
                    }
                    else
                    {
                        SoltarObjeto();
                    }
                }
                else
                {
                    SoltarObjeto();
                }
            }

            return;
        }


        // CASO 2: No llevamos ninguna pieza

        Ray ray = camaraJugador.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            distanciaDeInteraccion,
            capasParaRaycast,
            QueryTriggerInteraction.Ignore))
        {
            PiezaModular pieza =
                hit.collider.GetComponentInParent<PiezaModular>();

            // Pieza instalada
            if (pieza != null && pieza.estaInstalada)
            {
                MostrarTexto("F - Desmontar pieza");

                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (sistemaMontaje != null)
                        sistemaMontaje.IntentarDesmontar();
                }

                return;
            }

            // Pieza suelta
            Rigidbody rb =
                hit.collider.GetComponentInParent<Rigidbody>();

            if (rb != null && rb.CompareTag("Interactuable"))
            {
                MostrarTexto("F - Recoger pieza");

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

        if (previsualizando || objetoAgarrado.isKinematic)
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

        previsualizando = false;

        dampingOriginal = rb.linearDamping;
        angularDampingOriginal = rb.angularDamping;
        gravedadOriginal = rb.useGravity;

        rb.isKinematic = false;
        rb.useGravity = false;

        rb.linearDamping = 5f;
        rb.angularDamping = 5f;
    }


    void SoltarObjeto()
    {
        if (objetoAgarrado == null)
            return;

        Rigidbody rb = objetoAgarrado;

        objetoAgarrado = null;
        previsualizando = false;

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.linearDamping = dampingOriginal;
        rb.angularDamping = angularDampingOriginal;

        OcultarTexto();
    }


    void MostrarTexto(string mensaje)
    {
        if (textoInteractuar != null)
        {
            textoInteractuar.text = mensaje;
            textoInteractuar.gameObject.SetActive(true);
        }
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
