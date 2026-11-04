using System.Runtime.CompilerServices;
using UnityEngine;

public class HeadMovement : MonoBehaviour
{


    public float sensibilidad = 2.0f;
    public Transform cuerpoJugador;

    private float rotacionX = 0f;

    private Rigidbody rbJugador;





    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (cuerpoJugador != null)
        {

            rbJugador = cuerpoJugador.GetComponent<Rigidbody>();


        }
    }

    // Update is called once per frame
    void Update()
    {

        float mouseX = Input.GetAxis("Mouse X") * sensibilidad;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidad;


        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, -90, 90);
        transform.localRotation = Quaternion.Euler(rotacionX,0f,0f);

       if(rbJugador != null)
        {

            Quaternion rotacionAdicional = Quaternion.Euler(Vector3.up * mouseX);

            rbJugador.MoveRotation(rbJugador.rotation * rotacionAdicional);


        }
        else
        {
            cuerpoJugador.Rotate(Vector3.up * mouseX);
        }





    }
}
