
using UnityEngine;
using Unity.Netcode;

public class PuntoMontaje1 : MonoBehaviour
{
    public enum TipoDeAnclaje
    {
        alaIzq, alaDer, ruedaIzq, ruedaDer, motor, helice
    }

    [System.Serializable]
    public struct rigidbodysAsociados
    {
        public PiezaMontaje1 pieza;
        public TipoDeAnclaje tiposDeAnclaje;
        public Rigidbody rigidbodys;

        public Transform padreOriginal;
        public Vector3 posicionOriginal;
        public Quaternion rotacionOriginal;

        public bool eraKinematic;
        public bool teniaGravedad;
        public bool detectabaColisiones;

        public float linearDampingOriginal;
        public float angularDampingOriginal;

        public int layerOriginal;
    }

    public TipoDeAnclaje tipoAnclaje;

    public rigidbodysAsociados[] objetosEnAnclajes =
        new rigidbodysAsociados[0];

    public Transform puntoAnclaje;
    public PiezaMontaje1 piezaDetectada;
    public PiezaMontaje1 piezaInstalada;

    public bool inTrigger;
    public Rigidbody piezaMirada;

    private NetworkObject avionNetworkObject;

    private void Awake()
    {
        avionNetworkObject =
            GetComponentInParent<NetworkObject>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PiezaMontaje1 pieza =
            other.GetComponentInParent<PiezaMontaje1>();

        if (pieza == null || pieza.estaMontada)
            return;

        if (pieza.tipoDePieza ==
            (PiezaMontaje1.TiposPieza)tipoAnclaje)
        {
            piezaDetectada = pieza;
            inTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PiezaMontaje1 pieza =
            other.GetComponentInParent<PiezaMontaje1>();

        if (pieza == null)
            return;

        if (piezaDetectada == pieza)
        {
            piezaDetectada = null;
            inTrigger = false;
        }
    }

    public void MontarDesdeServidor(Rigidbody rb)
    {
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsServer)
            return;

        if (rb == null || piezaInstalada != null ||
            puntoAnclaje == null ||
            avionNetworkObject == null)
            return;

        PiezaMontaje1 pieza =
            rb.GetComponentInParent<PiezaMontaje1>();

        if (pieza == null || pieza.estaMontada)
            return;

        if (pieza.tipoDePieza !=
            (PiezaMontaje1.TiposPieza)tipoAnclaje)
            return;

        if (piezaDetectada != pieza || !inTrigger)
            return;

        NetworkObject piezaNetworkObject =
            pieza.GetComponent<NetworkObject>();

        if (piezaNetworkObject == null)
            return;

        // Guardamos la configuración ya restaurada.
        GuardarDatos(pieza, rb);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        if (!piezaNetworkObject.TrySetParent(
            avionNetworkObject, true))
        {
            RestaurarFisicas(pieza, rb);
            EliminarDatos(pieza);
            return;
        }

        pieza.transform.SetPositionAndRotation(
            puntoAnclaje.position,
            puntoAnclaje.rotation);

        pieza.EstablecerMontadaDesdeServidor(
            true,
            ObtenerIndiceAnclaje(),
            new NetworkObjectReference(avionNetworkObject));

        piezaInstalada = pieza;
        piezaDetectada = null;
        inTrigger = false;

        Debug.Log("Pieza montada: " + pieza.name);
    }

    public void DesmontarDesdeServidor()
    {
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsServer)
            return;

        if (piezaInstalada == null)
            return;

        PiezaMontaje1 pieza = piezaInstalada;

        for (int i = 0; i < objetosEnAnclajes.Length; i++)
        {
            rigidbodysAsociados datos = objetosEnAnclajes[i];

            if (datos.pieza != pieza)
                continue;

            Rigidbody rb = datos.rigidbodys;

            NetworkObject piezaNetworkObject =
                pieza.GetComponent<NetworkObject>();

            if (rb == null || piezaNetworkObject == null)
                return;

            if (!piezaNetworkObject.TryRemoveParent(true))
            {
                Debug.LogError(
                    "No se pudo separar la pieza del avión.");
                return;
            }

            if (datos.padreOriginal != null)
            {
                pieza.transform.SetParent(
                    datos.padreOriginal, true);
            }

            // Conservamos la posición actual del anclaje.
            // No devolvemos la pieza a su posición inicial.
            pieza.transform.SetPositionAndRotation(
                pieza.transform.position,
                pieza.transform.rotation);

            RestaurarFisicas(datos, rb);

            pieza.EstablecerMontadaDesdeServidor(false);

            piezaInstalada = null;
            piezaMirada = null;
            piezaDetectada = null;
            inTrigger = false;

            EliminarDatos(pieza);

            Debug.Log("Pieza desmontada: " + pieza.name);
            return;
        }

        Debug.LogWarning(
            "No se encontraron los datos originales de la pieza.");
    }

    private void RestaurarFisicas(
        PiezaMontaje1 pieza, Rigidbody rb)
    {
        for (int i = 0; i < objetosEnAnclajes.Length; i++)
        {
            if (objetosEnAnclajes[i].pieza == pieza)
            {
                RestaurarFisicas(objetosEnAnclajes[i], rb);
                return;
            }
        }
    }

    private void RestaurarFisicas(
        rigidbodysAsociados datos, Rigidbody rb)
    {
        rb.isKinematic = datos.eraKinematic;
        rb.useGravity = datos.teniaGravedad;
        rb.detectCollisions = datos.detectabaColisiones;

        rb.linearDamping = datos.linearDampingOriginal;
        rb.angularDamping = datos.angularDampingOriginal;

        rb.gameObject.layer = datos.layerOriginal;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private int ObtenerIndiceAnclaje()
    {
        if (avionNetworkObject == null)
            return -1;

        PuntoMontaje1[] puntos =
            avionNetworkObject.GetComponentsInChildren<PuntoMontaje1>(true);

        for (int i = 0; i < puntos.Length; i++)
        {
            if (puntos[i] == this)
                return i;
        }

        return -1;
    }

    private void GuardarDatos(
        PiezaMontaje1 pieza, Rigidbody rb)
    {
        rigidbodysAsociados datos =
            new rigidbodysAsociados();

        datos.pieza = pieza;
        datos.tiposDeAnclaje = tipoAnclaje;
        datos.rigidbodys = rb;

        datos.padreOriginal = pieza.transform.parent;
        datos.posicionOriginal = pieza.transform.position;
        datos.rotacionOriginal = pieza.transform.rotation;

        datos.eraKinematic = rb.isKinematic;
        datos.teniaGravedad = rb.useGravity;
        datos.detectabaColisiones = rb.detectCollisions;

        datos.linearDampingOriginal = rb.linearDamping;
        datos.angularDampingOriginal = rb.angularDamping;

        datos.layerOriginal = rb.gameObject.layer;

        System.Array.Resize(
            ref objetosEnAnclajes,
            objetosEnAnclajes.Length + 1);

        objetosEnAnclajes[
            objetosEnAnclajes.Length - 1] = datos;
    }

    private void EliminarDatos(PiezaMontaje1 pieza)
    {
        for (int i = 0; i < objetosEnAnclajes.Length; i++)
        {
            if (objetosEnAnclajes[i].pieza != pieza)
                continue;

            var nuevaLista =
                new rigidbodysAsociados[
                    objetosEnAnclajes.Length - 1];

            int indiceNuevo = 0;

            for (int j = 0; j < objetosEnAnclajes.Length; j++)
            {
                if (j == i)
                    continue;

                nuevaLista[indiceNuevo] =
                    objetosEnAnclajes[j];

                indiceNuevo++;
            }

            objetosEnAnclajes = nuevaLista;
            return;
        }
    }
}
