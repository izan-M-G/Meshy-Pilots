using UnityEngine;


public enum TipoPieza
{
    Motor,Helice,Ruedas,Alas,Mandos
}







public class PiezaModular : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    //identificacion 
    public TipoPieza tipo;

    //puntos
    public Transform puntoDeEncaje;


    //estado

    public bool estaInstalada = false;




    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}