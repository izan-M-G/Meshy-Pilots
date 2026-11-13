
using UnityEngine;
using Unity.Netcode;

public class Puerta : NetworkBehaviour
{
    [Header("Configuración")]
    public Transform puerta;
    public float anguloAbierto = 90f;
    public float velocidad = 3f;

    [Header("Estado")]
    public bool estaAbierta = false;

    private Quaternion rotacionCerrada;
    private Quaternion rotacionAbierta;

    private NetworkVariable<bool> estadoPuerta =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private void Start()
    {
        if (puerta == null)
            puerta = transform;

        rotacionCerrada = puerta.localRotation;

        rotacionAbierta = rotacionCerrada *
            Quaternion.AngleAxis(anguloAbierto, Vector3.up);

        estaAbierta = estadoPuerta.Value;
    }

    public override void OnNetworkSpawn()
    {
        estadoPuerta.OnValueChanged += ActualizarEstado;

        estaAbierta = estadoPuerta.Value;
    }

    public override void OnNetworkDespawn()
    {
        estadoPuerta.OnValueChanged -= ActualizarEstado;
    }

    private void Update()
    {
        if (puerta == null)
            return;

        estaAbierta = estadoPuerta.Value;

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
        if (!IsSpawned)
            return;

        SolicitarCambiarEstadoRpc();
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void SolicitarCambiarEstadoRpc()
    {
        estadoPuerta.Value = !estadoPuerta.Value;
    }

    private void ActualizarEstado(bool anterior, bool nuevo)
    {
        estaAbierta = nuevo;
    }
}
