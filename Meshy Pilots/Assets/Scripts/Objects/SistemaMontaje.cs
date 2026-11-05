
using UnityEngine;
using System.Collections.Generic;

public class SistemaMontaje : MonoBehaviour
{
    [Header("Raycast")]
    public Camera camaraJugador;
    public float distanciaDeInteraccion = 4f;
    public LayerMask capasPuntosMontaje;

    [Header("Deteccion")]
    public float radioDeteccionMontaje = 0.4f;

    [Header("Organizacion")]
    public Transform contenedorPiezasSueltas;
    public Transform contenedorPiezasMontadas;
    public Rigidbody rigidbodyAvion;

    // Guardamos las propiedades originales de cada Rigidbody.
    private class DatosFisicos
    {
        public float masa;
        public float linearDamping;
        public float angularDamping;
        public bool gravedad;
        public bool cinematica;
        public bool colisiones;
        public RigidbodyConstraints restricciones;
        public CollisionDetectionMode deteccionColisiones;
        public RigidbodyInterpolation interpolacion;
    }

    private Dictionary<PiezaModular, DatosFisicos> datosPiezas =
        new Dictionary<PiezaModular, DatosFisicos>();

    private PuntoMontaje DetectarPuntoMontaje()
    {
        if (camaraJugador == null)
            return null;

        Ray ray = camaraJugador.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0));

        if (Physics.SphereCast(
            ray,
            radioDeteccionMontaje,
            out RaycastHit hit,
            distanciaDeInteraccion,
            capasPuntosMontaje,
            QueryTriggerInteraction.Collide))
        {
            return hit.collider.GetComponentInParent<PuntoMontaje>();
        }

        return null;
    }

    public PuntoMontaje ObtenerPuntoMontaje()
    {
        return DetectarPuntoMontaje();
    }

    public bool IntentarMontar(Rigidbody rb, PuntoMontaje punto)
    {
        if (rb == null || punto == null)
            return false;

        if (rigidbodyAvion == null ||
            contenedorPiezasMontadas == null)
        {
            Debug.LogError(
                "Falta asignar el Rigidbody del avion o PiezasMontadas."
            );
            return false;
        }

        PiezaModular pieza = rb.GetComponent<PiezaModular>();

        if (pieza == null || !punto.PuedeAceptar(pieza))
            return false;

        Transform encajePieza = pieza.puntoDeEncaje != null
            ? pieza.puntoDeEncaje
            : pieza.transform;

        Transform anclajeSoporte = punto.anclaje != null
            ? punto.anclaje
            : punto.transform;

        // Calcular la rotacion correcta.
        Quaternion diferenciaRotacion =
            anclajeSoporte.rotation *
            Quaternion.Inverse(encajePieza.rotation);

        Quaternion nuevaRotacion =
            diferenciaRotacion * rb.transform.rotation;

        // Guardar las propiedades originales.
        DatosFisicos datos = new DatosFisicos
        {
            masa = rb.mass,
            linearDamping = rb.linearDamping,
            angularDamping = rb.angularDamping,
            gravedad = rb.useGravity,
            cinematica = rb.isKinematic,
            colisiones = rb.detectCollisions,
            restricciones = rb.constraints,
            deteccionColisiones = rb.collisionDetectionMode,
            interpolacion = rb.interpolation
        };

        datosPiezas[pieza] = datos;

        // Detener la pieza.
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        rb.isKinematic = true;
        rb.useGravity = false;

        // Colocar la pieza.
        rb.transform.rotation = nuevaRotacion;

        Vector3 diferenciaPosicion =
            anclajeSoporte.position - encajePieza.position;

        rb.transform.position += diferenciaPosicion;

        // Convertirla en hija del contenedor del avion.
        rb.transform.SetParent(contenedorPiezasMontadas, true);

        // Cambiar la capa de la pieza y todos sus hijos.
        AsignarCapaRecursiva(
            rb.gameObject,
            LayerMask.NameToLayer("PiezasMontadas")
        );

        // Registrar el montaje.
        punto.RegistrarPieza(pieza);

        // Eliminar el Rigidbody individual.
        // El collider pasara a formar parte del Rigidbody del avion.
        Destroy(rb);

        Debug.Log("Pieza integrada en el avion: " + pieza.name);

        return true;
    }

    public bool IntentarDesmontar()
    {
        PuntoMontaje punto = DetectarPuntoMontaje();

        if (punto == null || !punto.ocupado)
            return false;

        PiezaModular pieza = punto.piezaInstalada;

        if (pieza == null)
            return false;

        if (contenedorPiezasSueltas == null)
        {
            Debug.LogError("Falta asignar PiezasSueltas.");
            return false;
        }

        // Recuperar las propiedades guardadas.
        if (!datosPiezas.TryGetValue(pieza, out DatosFisicos datos))
        {
            Debug.LogError(
                "No se encontraron las propiedades fisicas de " + pieza.name
            );
            return false;
        }

        // Liberar el punto de montaje.
        punto.LiberarPunto();

        // Separar la pieza del avion.
        pieza.transform.SetParent(contenedorPiezasSueltas, true);

        // Restaurar la capa original de las piezas sueltas.
        AsignarCapaRecursiva(
            pieza.gameObject,
            LayerMask.NameToLayer("Default")
        );

        // Crear un Rigidbody independiente.
        Rigidbody nuevoRb = pieza.gameObject.AddComponent<Rigidbody>();

        // Recuperar sus propiedades.
        nuevoRb.mass = datos.masa;
        nuevoRb.linearDamping = datos.linearDamping;
        nuevoRb.angularDamping = datos.angularDamping;
        nuevoRb.useGravity = datos.gravedad;
        nuevoRb.constraints = datos.restricciones;
        nuevoRb.collisionDetectionMode = datos.deteccionColisiones;
        nuevoRb.interpolation = datos.interpolacion;
        nuevoRb.detectCollisions = datos.colisiones;

        // Restaurar el estado cinemático.
        nuevoRb.isKinematic = datos.cinematica;

        // Reiniciar velocidades.
        if (!nuevoRb.isKinematic)
        {
            nuevoRb.linearVelocity = Vector3.zero;
            nuevoRb.angularVelocity = Vector3.zero;
        }

        // Eliminar los datos guardados.
        datosPiezas.Remove(pieza);

        Debug.Log("Pieza desmontada: " + pieza.name);

        return true;
    }

    public bool PrevisualizarMontaje(
        Rigidbody rb,
        PuntoMontaje punto)
    {
        if (rb == null || punto == null)
            return false;

        PiezaModular pieza = rb.GetComponent<PiezaModular>();

        if (pieza == null || !punto.PuedeAceptar(pieza))
            return false;

        Transform encajePieza = pieza.puntoDeEncaje != null
            ? pieza.puntoDeEncaje
            : pieza.transform;

        Transform anclajeSoporte = punto.anclaje != null
            ? punto.anclaje
            : punto.transform;

        Quaternion diferenciaRotacion =
            anclajeSoporte.rotation *
            Quaternion.Inverse(encajePieza.rotation);

        rb.transform.rotation =
            diferenciaRotacion * rb.transform.rotation;

        Vector3 diferenciaPosicion =
            anclajeSoporte.position - encajePieza.position;

        rb.transform.position += diferenciaPosicion;

        return true;
    }

    private void AsignarCapaRecursiva(GameObject objeto, int capa)
    {
        if (capa < 0)
        {
            Debug.LogError("La capa indicada no existe.");
            return;
        }

        objeto.layer = capa;

        foreach (Transform hijo in objeto.transform)
        {
            AsignarCapaRecursiva(hijo.gameObject, capa);
        }
    }
}
