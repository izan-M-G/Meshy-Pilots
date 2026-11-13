
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class SistemaInteraccion : NetworkBehaviour
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
    public bool isAgarrado;
    public Rigidbody piezaMirada;
    public bool mostrarTextoMontar;
    public PuntoMontaje1[] puntosMontaje;

    private NetworkVariable<NetworkObjectReference> piezaSujeta =
        new NetworkVariable<NetworkObjectReference>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    private Rigidbody piezaServidor;

    private float dampingOriginal;
    private float angularDampingOriginal;
    private int capaOriginal;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {

            BuscarTextos();

            if (camaraJugador == null)
                camaraJugador = GetComponentInChildren<Camera>();

            if (camaraJugador == null)
                camaraJugador = Camera.main;

            if (camaraJugador == null)
            {
                Debug.LogError("No se encontró la cámara del jugador.");
                return;
            }

            puntosMontaje = FindObjectsByType<PuntoMontaje1>(
                FindObjectsSortMode.None);

            esconderTodosTextos();

            if (puntoDeSujecion == null)
            {
                GameObject punto = new GameObject("PuntoDeSujecion");

                puntoDeSujecion = punto.transform;
                puntoDeSujecion.SetParent(camaraJugador.transform, false);
                puntoDeSujecion.localPosition = new Vector3(0, 0, 2);
            }
        }
    }

    private void Update()
    {
        if (!IsOwner || !IsSpawned || camaraJugador == null)
            return;

        ActualizarPiezaSujeta();

        esconderTodosTextos();
        mostrarTextoMontar = false;

        if (objetoAgarrado != null)
        {
            PiezaMontaje1 pieza =
                objetoAgarrado.GetComponentInParent<PiezaMontaje1>();

            bool puedeMontar = false;

            if (pieza != null)
            {
                foreach (PuntoMontaje1 punto in puntosMontaje)
                {
                    if (punto == null)
                        continue;

                    if (punto.inTrigger &&
                        punto.piezaDetectada == pieza)
                    {
                        puedeMontar = true;
                        break;
                    }
                }
            }

            // Mostrar solamente el texto correspondiente.
            if (puedeMontar)
            {
                mostrarTexto(montarText);

                if (Input.GetKeyDown(KeyCode.G))
                {
                    foreach (PuntoMontaje1 punto in puntosMontaje)
                    {
                        if (punto != null &&
                            punto.inTrigger &&
                            punto.piezaDetectada == pieza)
                        {
                            SolicitarMontaje(pieza, punto);
                            break;
                        }
                    }
                }
            }
            else
            {
                mostrarTexto(textoInteractuarSoltar);
            }

            if (Input.GetKeyDown(KeyCode.F))
                SolicitarSoltarRpc();

            return;
        }

        Ray ray = camaraJugador.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0));

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            distanciaDeInteraccion,
            capasParaRaycast))
        {
            mirandoMontada = false;
            piezaMirada = null;
            return;
        }

        Puerta puerta =
            hit.collider.GetComponentInParent<Puerta>();

        if (puerta != null)
        {
            mostrarTexto(textoInteraccionE);

            if (Input.GetKeyDown(KeyCode.E))
                puerta.Interactuar();

            return;
        }

        Rigidbody rb =
            hit.collider.GetComponentInParent<Rigidbody>();

        if (rb == null || !rb.CompareTag("Interactuable"))
            return;

        PiezaMontaje1 piezaMiradaScript =
            rb.GetComponentInParent<PiezaMontaje1>();

        if (piezaMiradaScript == null)
            return;

        if (piezaMiradaScript.estaMontada)
        {
            mirandoMontada = true;
            piezaMirada = rb;

            mostrarTexto(desmontarText);

            if (Input.GetKeyDown(KeyCode.G))
                SolicitarDesmontajeRpc(
                    new NetworkObjectReference(
                        piezaMiradaScript.NetworkObject));
        }
        else
        {
            mirandoMontada = false;
            piezaMirada = null;

            mostrarTexto(textoInteractuar);

            if (Input.GetKeyDown(KeyCode.F))
            {
                NetworkObject netObj =
                    piezaMiradaScript.GetComponent<NetworkObject>();

                if (netObj != null)
                {
                    SolicitarAgarrarRpc(
                        new NetworkObjectReference(netObj));
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner || !IsSpawned ||
            puntoDeSujecion == null || objetoAgarrado == null)
            return;

        EnviarPosicionSujecionRpc(puntoDeSujecion.position);
    }

    private void ActualizarPiezaSujeta()
    {
        objetoAgarrado = null;
        isAgarrado = false;

        if (piezaSujeta.Value.TryGet(out NetworkObject netObj))
        {
            objetoAgarrado = netObj.GetComponent<Rigidbody>();
            isAgarrado = objetoAgarrado != null;
        }
    }

    private void SolicitarMontaje(
        PiezaMontaje1 pieza,
        PuntoMontaje1 punto)
    {
        if (pieza == null || punto == null)
            return;

        NetworkObject piezaNet =
            pieza.GetComponent<NetworkObject>();

        NetworkObject avionNet =
            punto.GetComponentInParent<NetworkObject>();

        if (piezaNet == null || avionNet == null)
        {
            Debug.LogError("Falta un NetworkObject en la pieza o el avión.");
            return;
        }

        PuntoMontaje1[] puntosAvion =
            avionNet.GetComponentsInChildren<PuntoMontaje1>(true);

        for (int i = 0; i < puntosAvion.Length; i++)
        {
            if (puntosAvion[i] == punto)
            {
                SolicitarMontajeRpc(
                    new NetworkObjectReference(piezaNet),
                    new NetworkObjectReference(avionNet),
                    i);

                return;
            }
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = true)]
    private void SolicitarAgarrarRpc(
        NetworkObjectReference referencia)
    {
        if (!referencia.TryGet(out NetworkObject netObj))
            return;

        Rigidbody rb = netObj.GetComponent<Rigidbody>();
        PiezaMontaje1 pieza = netObj.GetComponent<PiezaMontaje1>();

        if (rb == null || pieza == null ||
            pieza.estaMontada || piezaServidor != null)
            return;

        if (Vector3.Distance(transform.position, rb.position) >
            distanciaDeInteraccion + 1.5f)
            return;

        if (!pieza.IntentarSujetar(OwnerClientId))
            return;

        piezaServidor = rb;

        dampingOriginal = rb.linearDamping;
        angularDampingOriginal = rb.angularDamping;
        capaOriginal = rb.gameObject.layer;

        rb.linearDamping = 5f;
        rb.angularDamping = 5f;
        rb.gameObject.layer = 11;

        piezaSujeta.Value = referencia;
    }

    [Rpc(SendTo.Server, RequireOwnership = true)]
    private void EnviarPosicionSujecionRpc(Vector3 posicion)
    {
        if (piezaServidor == null)
            return;

        PiezaMontaje1 pieza =
            piezaServidor.GetComponent<PiezaMontaje1>();

        if (pieza == null || !pieza.LaSujeta(OwnerClientId))
            return;

        if (Vector3.Distance(transform.position, posicion) > 5f)
            return;

        Vector3 direccion =
            posicion - piezaServidor.worldCenterOfMass;

        Vector3 velocidad = direccion * velocidadAtraccion;

        piezaServidor.linearVelocity =
            Vector3.ClampMagnitude(velocidad, velocidadMaxima);
    }

    [Rpc(SendTo.Server, RequireOwnership = true)]
    private void SolicitarSoltarRpc()
    {
        SoltarEnServidor();
    }

    private void SoltarEnServidor()
    {
        if (!IsServer || piezaServidor == null)
            return;

        PiezaMontaje1 pieza =
            piezaServidor.GetComponent<PiezaMontaje1>();

        piezaServidor.gameObject.layer = capaOriginal;
        piezaServidor.linearDamping = dampingOriginal;
        piezaServidor.angularDamping = angularDampingOriginal;

        if (pieza != null)
            pieza.Liberar();

        piezaServidor = null;
        piezaSujeta.Value = default;
    }


    [Rpc(SendTo.Server, RequireOwnership = true)]
    private void SolicitarMontajeRpc(
        NetworkObjectReference referenciaPieza,
        NetworkObjectReference referenciaAvion,
        int indice)
    {
        if (!referenciaPieza.TryGet(out NetworkObject piezaNet))
            return;

        if (!referenciaAvion.TryGet(out NetworkObject avionNet))
            return;

        if (piezaServidor == null ||
            piezaNet.GetComponent<Rigidbody>() != piezaServidor)
            return;

        PiezaMontaje1 pieza =
            piezaNet.GetComponent<PiezaMontaje1>();

        if (pieza == null || !pieza.LaSujeta(OwnerClientId))
            return;

        PuntoMontaje1[] puntos =
            avionNet.GetComponentsInChildren<PuntoMontaje1>(true);

        if (indice < 0 || indice >= puntos.Length)
            return;

        PuntoMontaje1 punto = puntos[indice];

        // Restauramos los valores originales antes de montar.
        piezaServidor.linearDamping = dampingOriginal;
        piezaServidor.angularDamping = angularDampingOriginal;
        piezaServidor.gameObject.layer = capaOriginal;

        punto.MontarDesdeServidor(piezaServidor);

        if (punto.piezaInstalada == pieza)
        {
            piezaServidor = null;
            piezaSujeta.Value = default;
        }
        else
        {
            // Si el montaje falla, vuelve a la configuración
            // de objeto agarrado.
            piezaServidor.linearDamping = 5f;
            piezaServidor.angularDamping = 5f;
            piezaServidor.gameObject.layer = 11;
        }
    }


    [Rpc(SendTo.Server, RequireOwnership = true)]
    private void SolicitarDesmontajeRpc(
        NetworkObjectReference referenciaPieza)
    {
        if (!referenciaPieza.TryGet(out NetworkObject netObj))
            return;

        PiezaMontaje1 pieza =
            netObj.GetComponent<PiezaMontaje1>();

        if (pieza == null || !pieza.estaMontada)
            return;

        if (Vector3.Distance(transform.position, pieza.transform.position) >
            distanciaDeInteraccion + 1.5f)
            return;

        if (!pieza.AvionReferencia.TryGet(out NetworkObject avionNet))
            return;

        PuntoMontaje1[] puntos =
            avionNet.GetComponentsInChildren<PuntoMontaje1>(true);

        int indice = pieza.IndiceAnclaje;

        if (indice < 0 || indice >= puntos.Length)
            return;

        puntos[indice].DesmontarDesdeServidor();
    }

    public void AgarrarObjeto(Rigidbody rb)
    {
        if (!IsOwner || rb == null)
            return;

        NetworkObject netObj =
            rb.GetComponentInParent<NetworkObject>();

        if (netObj != null)
        {
            SolicitarAgarrarRpc(
                new NetworkObjectReference(netObj));
        }
    }

    public void SoltarObjeto()
    {
        if (IsOwner && IsSpawned)
            SolicitarSoltarRpc();
    }

    public void mostrarTexto(TextMeshProUGUI texto)
    {
        if (texto != null)
            texto.gameObject.SetActive(true);
    }

    public void esconderTexto(TextMeshProUGUI texto)
    {
        if (texto != null)
            texto.gameObject.SetActive(false);
    }

    private void esconderTodosTextos()
    {
        esconderTexto(textoInteractuar);
        esconderTexto(textoInteractuarSoltar);
        esconderTexto(montarText);
        esconderTexto(desmontarText);
        esconderTexto(textoInteraccionE);
    }


    private void BuscarTextos()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(
            FindObjectsSortMode.None
        );

        foreach (Canvas canvasEncontrado in canvases)
        {
            TextMeshProUGUI[] textos =
                canvasEncontrado.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (TextMeshProUGUI texto in textos)
            {
                switch (texto.name)
                {
                    case "textoInteractuar":
                        textoInteractuar = texto;
                        break;

                    case "textoInteractuarSoltar":
                        textoInteractuarSoltar = texto;
                        break;

                    case "montarText":
                        montarText = texto;
                        break;

                    case "desmontarText":
                        desmontarText = texto;
                        break;

                    case "textoInteraccionE":
                        textoInteraccionE = texto;
                        break;
                }
            }
        }

        Debug.Log("Textos asignados: " +
            (textoInteractuar != null) + " / " +
            (textoInteractuarSoltar != null) + " / " +
            (montarText != null) + " / " +
            (desmontarText != null) + " / " +
            (textoInteraccionE != null));
    }


    public override void OnNetworkDespawn()
    {
        if (IsServer)
            SoltarEnServidor();
    }

    public Rigidbody ObjetoAgarrado
    {
        get { return objetoAgarrado; }
    }
}
