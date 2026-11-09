using UnityEngine;




public class PiezaMontaje1 : MonoBehaviour
{
    public enum TiposPieza
    {
        alaIzq, alaDer, ruedaIzq, ruedaDer, motor, helice
    }

    public TiposPieza tipoDePieza;


    public bool estaMontada = false;

}
