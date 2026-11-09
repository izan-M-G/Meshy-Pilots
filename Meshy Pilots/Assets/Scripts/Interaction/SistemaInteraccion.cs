
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

    public TextMeshProUGUI textoInteractuarSoltar;

    public TextMeshProUGUI montarText;

    public TextMeshProUGUI desmontarText;

    public TextMeshProUGUI textoInteraccionE;

    [Header("Sujecion")]
    public Transform puntoDeSujecion;
    public float velocidadAtraccion = 12f;
    public float velocidadMaxima = 10f;

    public PuntoMontaje1 montajeClass;
    public Rigidbody objetoAgarrado;

    public bool mirandoMontada;

    private bool detectCollisionsOriginal;
    private float dampingOriginal;
    private float angularDampingOriginal;

    public bool isAgarrado = false;

    public Rigidbody piezaMirada;

    public bool mostrarTextoMontar;

    public PuntoMontaje1[] puntosMontaje;

    void Start()
    {
        if (puntosMontaje == null || puntosMontaje.Length == 0)
        {
            puntosMontaje = FindObjectsByType<PuntoMontaje1>(FindObjectsSortMode.None);
        }

        if (camaraJugador == null)
            camaraJugador = Camera.main;

        if (camaraJugador == null)
        {
            enabled = false;
            return;
        }

        if (montajeClass == null)
        {
            montajeClass = FindFirstObjectByType<PuntoMontaje1>();
        }

        if (textoInteractuar != null)
            textoInteractuar.gameObject.SetActive(false);

        if (montarText != null)
            montarText.gameObject.SetActive(false);

        if (textoInteractuarSoltar != null)
            textoInteractuarSoltar.gameObject.SetActive(false);

        if (desmontarText != null)
        {
            desmontarText.gameObject.SetActive(false);
        }

        if (textoInteraccionE != null)
        {
            textoInteraccionE.gameObject.SetActive(false);
        }

        if (puntoDeSujecion == null)
        {
            GameObject punto = new GameObject("PuntoDeSujecion");

            puntoDeSujecion = punto.transform;
            puntoDeSujecion.SetParent(camaraJugador.transform, false);
            puntoDeSujecion.localPosition = new Vector3(0, 0, 2);
        }
    }

    void esconderTodosTextos()
    {
        if (textoInteractuarSoltar != null)
        {
            textoInteractuarSoltar.gameObject.SetActive(false);
        }

        if (montarText != null)
        {
            montarText.gameObject.SetActive(false);
        }

        if (textoInteractuar != null)
        {
            textoInteractuar.gameObject.SetActive(false);
        }

        if (desmontarText != null)
            desmontarText.gameObject.SetActive(false);

        if (textoInteraccionE != null)
        {
            textoInteraccionE.gameObject.SetActive(false);
        }
    }

    public void mostrarTexto(TextMeshProUGUI texto)
    {
        if (texto != null)
        {
            texto.gameObject.SetActive(true);
        }
    }

    public void esconderTexto(TextMeshProUGUI texto)
    {
        if (texto != null)
        {
            texto.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        esconderTodosTextos();

        mostrarTextoMontar = false;

        esconderTodosTextos();

        if (objetoAgarrado != null)
        {
            esconderTexto(montarText);
            esconderTexto(textoInteractuar);

            mostrarTexto(textoInteractuarSoltar);

            bool puedeMontar = false;

            PiezaMontaje1 piezaAgarrada =
                objetoAgarrado.GetComponentInParent<PiezaMontaje1>();

            if (piezaAgarrada != null)
            {
                foreach (PuntoMontaje1 punto in puntosMontaje)
                {
                    if (punto == null) continue;

                    if (punto.inTrigger &&
                        punto.piezaDetectada == piezaAgarrada)
                    {
                        puedeMontar = true;
                        break;
                    }
                }
            }

            if (puedeMontar)
            {
                esconderTexto(textoInteractuar);
                esconderTexto(textoInteractuarSoltar);
                mostrarTexto(montarText);
            }

            // F = soltar la pieza
            if (Input.GetKeyDown(KeyCode.F))
            {
                SoltarObjeto();
            }

            return;
        }

        Ray ray = camaraJugador.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit,
            distanciaDeInteraccion, capasParaRaycast))
        {
            // Comprobar primero si estamos mirando una puerta.
            Puerta puerta = hit.collider.gameObject.GetComponentInParent<Puerta>();

            if (puerta != null)
            {
                mirandoMontada = false;
                piezaMirada = null;

                mostrarTexto(textoInteraccionE);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    puerta.Interactuar();
                }

                return;
            }

            Rigidbody rb = hit.collider.GetComponentInParent<Rigidbody>();

            if (rb != null && rb.CompareTag("Interactuable"))
            {
                if (rb.GetComponentInParent<PiezaMontaje1>().estaMontada)
                {
                    mirandoMontada = true;

                    piezaMirada = rb;

                    esconderTexto(textoInteractuar);

                    esconderTexto(textoInteractuarSoltar);

                    mostrarTexto(desmontarText);
                }
                else
                {
                    piezaMirada = null;
                    mirandoMontada = false;

                    esconderTexto(montarText);

                    esconderTexto(textoInteractuarSoltar);

                    mostrarTexto(textoInteractuar);

                    if (Input.GetKeyDown(KeyCode.F) && !rb.GetComponentInParent<PiezaMontaje1>().estaMontada)
                        AgarrarObjeto(rb);
                }
            }
            else
            {
                mirandoMontada = false;
                piezaMirada = null;
            }
        }
        else
        {
            mirandoMontada = false;
            piezaMirada = null;
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

    public void AgarrarObjeto(Rigidbody rb)
    {
        isAgarrado = true;
        esconderTexto(textoInteractuar);
        mostrarTexto(textoInteractuarSoltar);

        objetoAgarrado = rb;

        detectCollisionsOriginal = rb.detectCollisions;
        dampingOriginal = rb.linearDamping;
        angularDampingOriginal = rb.angularDamping;

        rb.linearDamping = 5f;
        rb.angularDamping = 5f;
    }

    public void SoltarObjeto()
    {
        isAgarrado = false;
        if (objetoAgarrado == null)
            return;

        Rigidbody rb = objetoAgarrado;

        // Liberar el objeto de la sujecion
        objetoAgarrado = null;

        // Quitar resistencia al movimiento
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;

        esconderTodosTextos();
    }

    void OnDisable()
    {
        if (objetoAgarrado != null)
            SoltarObjeto();
    }

    public Rigidbody ObjetoAgarrado
    {
        get { return objetoAgarrado; }
    }
}
