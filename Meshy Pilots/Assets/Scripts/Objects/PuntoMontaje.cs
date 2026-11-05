using UnityEngine;

public class PuntoMontaje : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //configuracion

    public TipoPieza tipoAceptado;


    //punto encaje
    public Transform anclaje;

    //estado 
    public PiezaModular piezaInstalada;

    public bool ocupado
    {
        get { return piezaInstalada != null ; }
    }

    public bool PuedeAceptar(PiezaModular pieza)
    {
        if (pieza == null)
        {
            return false;

        }
        if (ocupado)
        {
            return false;
        }

        return pieza.tipo == tipoAceptado;

    }

    public void RegistrarPieza(PiezaModular pieza)
    {
        if (!PuedeAceptar(pieza))
            return;

        piezaInstalada = pieza;
        pieza.estaInstalada = true;
    }

    public void LiberarPunto()
    {
        if (piezaInstalada == null)
        {
            return;
        }

        piezaInstalada.estaInstalada = false;
        piezaInstalada = null;

    }
}
