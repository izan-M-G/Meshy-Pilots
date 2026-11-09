
using UnityEngine;

public class Puerta : MonoBehaviour
{
    [Header("Configuración")]
    public Transform puerta;
    public float anguloAbierto = 90f;
    public float velocidad = 3f;

    [Header("Estado")]
    public bool estaAbierta = false;

    private Quaternion rotacionCerrada;
    private Quaternion rotacionAbierta;

    void Start()
    {
        if (puerta == null)
            puerta = transform;

        rotacionCerrada = puerta.localRotation;

        rotacionAbierta = rotacionCerrada *
     Quaternion.AngleAxis(anguloAbierto, Vector3.up);
    }

    void Update()
    {
        Quaternion objetivo = estaAbierta
            ? rotacionAbierta
            : rotacionCerrada;

        puerta.localRotation = Quaternion.Slerp(
            puerta.localRotation,
            objetivo,
            Time.deltaTime * velocidad
        );
    }

    public void Interactuar()
    {
        estaAbierta = !estaAbierta;
    }
}
