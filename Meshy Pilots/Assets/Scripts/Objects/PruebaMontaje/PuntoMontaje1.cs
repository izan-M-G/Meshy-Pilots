using Unity.VisualScripting;
using UnityEngine;





public class PuntoMontaje1 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public Transform puntoAnclaje;
    public PiezaMontaje1 piezaDetectada;

    public PiezaMontaje1 piezaInstalada;

    public bool inTrigger;



    public SistemaInteraccion sistemaInteraccion;

    public TriggerDesmontajePieza triggerDesmontajePieza;

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

        piezaDetectada = piezaMontaje1;
        inTrigger = true;


        

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
        triggerDesmontajePieza = triggerDesmontajePieza.GetComponent<TriggerDesmontajePieza>();



    }

    // Update is called once per frame
    void Update()
    {

        

        if (!Input.GetKeyDown(KeyCode.G)) return;


        if(piezaInstalada != null)
        {
            
            DesmontarPieza();

        }

        if(piezaDetectada != null)
        {
            MontarPieza();
            sistemaInteraccion.SoltarObjeto();

        }


    }


    public void MontarPieza()
    {


        if(puntoAnclaje == null)
        {
            return;
        }

        PiezaMontaje1 piezaMontaje1 = piezaDetectada;

        Rigidbody rb = piezaDetectada.GetComponentInParent<Rigidbody>();

        piezaMontaje1.transform.SetPositionAndRotation(puntoAnclaje.position, puntoAnclaje.rotation);

        if(rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }


        piezaMontaje1.transform.SetParent(transform, true);



        piezaMontaje1.estaMontada = true;
        piezaInstalada = piezaMontaje1;
        piezaDetectada = null;

    
    
    }



    public void DesmontarPieza()
    {


      

        if (triggerDesmontajePieza == null ||
            !triggerDesmontajePieza.playerInRangePoint1)
        {
            return;
        }



        if (triggerDesmontajePieza.playerInRangePoint1)
        {
            if (piezaInstalada == null)
            {
                return;
            }

            PiezaMontaje1 pieza = piezaInstalada;

            pieza.transform.SetParent(null, true);


            Rigidbody rb = pieza.GetComponentInParent<Rigidbody>();

            if (rb != null)
            {
                rb.detectCollisions = true;
                rb.isKinematic = false;
                rb.useGravity = true;

            }

            pieza.estaMontada = false;
            piezaInstalada = null;



        }



    }




}
