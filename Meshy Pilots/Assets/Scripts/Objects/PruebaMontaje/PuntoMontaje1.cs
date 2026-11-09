using Unity.VisualScripting;
using UnityEngine;





public class PuntoMontaje1 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

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

        [Header("Transformación original")]
        public Transform padreOriginal;
        public Vector3 posicionOriginal;
        public Quaternion rotacionOriginal;

        [Header("Estado físico original")]
        public bool eraKinematic;
        public bool teniaGravedad;
        public bool detectabaColisiones;
        public Vector3 velocidadOriginal;
        public Vector3 velocidadAngularOriginal;
        public float linearDampingOriginal;
        public float angularDampingOriginal;
    }

    public TipoDeAnclaje tipoAnclaje;

    public rigidbodysAsociados[] objetosEnAnclajes;

    public Transform puntoAnclaje;
    public PiezaMontaje1 piezaDetectada;

    public PiezaMontaje1 piezaInstalada;

    public bool inTrigger;

    public Rigidbody piezaMirada;

    public SistemaInteraccion sistemaInteraccion;



    private void OnTriggerEnter(Collider other)
    {



        PiezaMontaje1 piezaMontaje1 = other.GetComponentInParent<PiezaMontaje1>();


        if(piezaMontaje1 == null)
        {
            return;
        }

        if (piezaMontaje1.estaMontada)
        {
            return;
        }



        if (piezaMontaje1.tipoDePieza == (PiezaMontaje1.TiposPieza)tipoAnclaje )
        {

            piezaDetectada = piezaMontaje1;
            inTrigger = true;
        }

        Debug.Log("PIEZA DETECTADA EN: " + gameObject.name);






    }

    private void OnTriggerExit(Collider other)
    {

        PiezaMontaje1 pieza = other.GetComponentInParent<PiezaMontaje1>();

        if(pieza == null || pieza.estaMontada)
        {
            return; 
        }

        if(piezaDetectada == pieza)
        {
            piezaDetectada = null;
            inTrigger = false;

        }



    }



    void Start()
    {

        sistemaInteraccion = sistemaInteraccion.GetComponent<SistemaInteraccion>();



    }

    // Update is called once per frame
    void Update()
    {


        if (piezaDetectada != null &&
        sistemaInteraccion.isAgarrado &&
        sistemaInteraccion.objetoAgarrado != null)
        {
            PiezaMontaje1 piezaAgarrada =
                sistemaInteraccion.objetoAgarrado.GetComponentInParent<PiezaMontaje1>();

            if (piezaAgarrada == piezaDetectada)
            {
                sistemaInteraccion.mostrarTextoMontar = true;
            }
        }

        if (!Input.GetKeyDown(KeyCode.G)) return;


        if (piezaInstalada != null &&
        sistemaInteraccion.mirandoMontada &&
        sistemaInteraccion.piezaMirada != null &&
        objetosEnAnclajes.Length > 0)
        {
            if (piezaInstalada.GetComponent<Rigidbody>() == sistemaInteraccion.piezaMirada)
            {
                DesmontarPieza();
            }
        }

        if (piezaDetectada != null &&
        sistemaInteraccion.isAgarrado &&
        sistemaInteraccion.objetoAgarrado != null)
        {
            PiezaMontaje1 piezaAgarrada =
                sistemaInteraccion.objetoAgarrado.GetComponentInParent<PiezaMontaje1>();

            if (piezaAgarrada == piezaDetectada &&
                Input.GetKeyDown(KeyCode.G))
            {
                MontarPieza();
                sistemaInteraccion.SoltarObjeto();
            }
        }


    }


    public void MontarPieza()
    {
        if (piezaDetectada == null || sistemaInteraccion.objetoAgarrado == null)
            return;

        PiezaMontaje1 piezaAgarrada =
            sistemaInteraccion.objetoAgarrado.GetComponentInParent<PiezaMontaje1>();

        // Comprobar que la pieza del trigger es la que tenemos agarrada.
        if (piezaAgarrada != piezaDetectada)
            return;

        if (piezaAgarrada.tipoDePieza != (PiezaMontaje1.TiposPieza)tipoAnclaje)
            return;

        if (puntoAnclaje == null)
            return;

        PiezaMontaje1 piezaMontaje1 = piezaAgarrada;

        Rigidbody rb = piezaMontaje1.GetComponentInParent<Rigidbody>();

        if (rb == null)
            return;

        GuardarDatos(piezaMontaje1, rb);

        piezaMontaje1.transform.SetPositionAndRotation(
            puntoAnclaje.position,
            puntoAnclaje.rotation
        );

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        piezaMontaje1.transform.SetParent(transform, true);

        piezaMontaje1.estaMontada = true;
        piezaInstalada = piezaMontaje1;
        piezaDetectada = null;
        inTrigger = false;
    }



    public void DesmontarPieza()
    {
        if (piezaInstalada == null)
            return;

        Rigidbody rbBuscado = sistemaInteraccion.piezaMirada;

        // Buscamos los datos de la pieza instalada.
        for (int i = 0; i < objetosEnAnclajes.Length; i++)
        {
            rigidbodysAsociados datos = objetosEnAnclajes[i];

            if (datos.rigidbodys != rbBuscado)
                continue;

            PiezaMontaje1 pieza = datos.pieza;
            Rigidbody rb = datos.rigidbodys;

            // Separamos la pieza del avión manteniendo su posición actual.
            pieza.transform.SetParent(null, true);

            // Recuperamos su configuración física original.
            rb.detectCollisions = datos.detectabaColisiones;
            rb.isKinematic = datos.eraKinematic;
            rb.useGravity = datos.teniaGravedad;

            rb.linearDamping = datos.linearDampingOriginal;
            rb.angularDamping = datos.angularDampingOriginal;

            rb.linearVelocity = datos.velocidadOriginal;
            rb.angularVelocity = datos.velocidadAngularOriginal;

            pieza.estaMontada = false;

            piezaInstalada = null;
            piezaMirada = null;

            // Eliminamos el registro utilizado.
            objetosEnAnclajes[i] = default;

            Debug.Log("Pieza desmontada: " + pieza.name);

            return;
        }

        Debug.LogWarning("No se encontraron los datos del Rigidbody de la pieza.");
    }

    private void GuardarDatos(PiezaMontaje1 pieza, Rigidbody rb)
    {
        // Evitar guardar dos veces la misma pieza.
        for (int i = 0; i < objetosEnAnclajes.Length; i++)
        {
            if (objetosEnAnclajes[i].rigidbodys == rb)
                return;
        }

        rigidbodysAsociados datos = new rigidbodysAsociados();

        datos.pieza = pieza;
        datos.tiposDeAnclaje = tipoAnclaje;
        datos.rigidbodys = rb;

        // Guardamos la transformación antes de montarla.
        datos.padreOriginal = pieza.transform.parent;
        datos.posicionOriginal = pieza.transform.position;
        datos.rotacionOriginal = pieza.transform.rotation;

        // Guardamos el estado físico original.
        datos.eraKinematic = rb.isKinematic;
        datos.teniaGravedad = rb.useGravity;
        datos.detectabaColisiones = rb.detectCollisions;
        datos.velocidadOriginal = rb.linearVelocity;
        datos.velocidadAngularOriginal = rb.angularVelocity;
        datos.linearDampingOriginal = rb.linearDamping;
        datos.angularDampingOriginal = rb.angularDamping;

        // Ampliamos el array para guardar el nuevo registro.
        System.Array.Resize(ref objetosEnAnclajes, objetosEnAnclajes.Length + 1);

        // Guardamos el struct en el array.
        objetosEnAnclajes[objetosEnAnclajes.Length - 1] = datos;

        Debug.Log("Datos guardados: " + pieza.name);
    }


}
