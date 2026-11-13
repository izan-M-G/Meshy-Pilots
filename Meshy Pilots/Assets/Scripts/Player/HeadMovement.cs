
using UnityEngine;

public class HeadMovement : MonoBehaviour
{
    public float sensibilidad = 2f;

    private float rotacionX = 0f;
    private float rotacionYLocal;

    private MovementPlayer movimientoJugador;

    void Start()
    {
        movimientoJugador =
            GetComponentInParent<MovementPlayer>();

        // Guardamos la orientación inicial de la cámara.
        rotacionYLocal = transform.eulerAngles.y;
    }

    void Update()
    {
        if (movimientoJugador == null)
            return;

        float mouseX =
            Input.GetAxis("Mouse X") * sensibilidad;

        float mouseY =
            Input.GetAxis("Mouse Y") * sensibilidad;

        // Giro vertical local.
        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, -90f, 90f);

        // Giro horizontal local e inmediato.
        rotacionYLocal += mouseX;

        transform.rotation = Quaternion.Euler(
            rotacionX,
            rotacionYLocal,
            0f
        );

        // Enviamos también el giro al servidor
        // para sincronizar el cuerpo del jugador.
        movimientoJugador.AplicarGiro(mouseX);
    }
}
