
using UnityEngine;

public class TriggerDesmontajePieza : MonoBehaviour
{
    public bool playerInRangePoint1 = false;
    public PlayerTrigger playerTrigger;

    private void OnTriggerEnter(Collider other)
    {
        PlayerTrigger jugador = other.GetComponentInParent<PlayerTrigger>();

        if (jugador == null)
            return;

        playerTrigger = jugador;
        playerInRangePoint1 = true;

        Debug.Log("Jugador ENTRA en rango");
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerTrigger jugador = other.GetComponentInParent<PlayerTrigger>();

        if (jugador == null)
            return;

        playerInRangePoint1 = false;
        playerTrigger = null;

        Debug.Log("Jugador SALE del rango");
    }
}
