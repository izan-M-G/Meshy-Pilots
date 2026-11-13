
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PiezaMontaje1 : NetworkBehaviour
{
    public enum TiposPieza
    {
        alaIzq, alaDer, ruedaIzq, ruedaDer, motor, helice
    }

    public TiposPieza tipoDePieza;

    private NetworkVariable<bool> estadoMontada =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    private NetworkVariable<ulong> jugadorQueLaSujeta =
        new NetworkVariable<ulong>(
            ulong.MaxValue,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    private NetworkVariable<int> indiceAnclaje =
        new NetworkVariable<int>(
            -1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    private NetworkVariable<NetworkObjectReference> avionReferencia =
        new NetworkVariable<NetworkObjectReference>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public bool estaMontada => estadoMontada.Value;
    public int IndiceAnclaje => indiceAnclaje.Value;
    public NetworkObjectReference AvionReferencia => avionReferencia.Value;

    public override void OnNetworkSpawn()
    {
        estadoMontada.OnValueChanged += CambiarEstadoMontaje;
    }

    public override void OnNetworkDespawn()
    {
        estadoMontada.OnValueChanged -= CambiarEstadoMontaje;
    }

    private void CambiarEstadoMontaje(bool anterior, bool nuevo)
    {
        // El estado se consulta directamente desde la NetworkVariable.
    }

    public bool IntentarSujetar(ulong jugadorId)
    {
        if (!IsServer || estaMontada)
            return false;

        if (jugadorQueLaSujeta.Value != ulong.MaxValue)
            return false;

        jugadorQueLaSujeta.Value = jugadorId;
        return true;
    }

    public bool LaSujeta(ulong jugadorId)
    {
        return jugadorQueLaSujeta.Value == jugadorId;
    }

    public void Liberar()
    {
        if (!IsServer)
            return;

        jugadorQueLaSujeta.Value = ulong.MaxValue;
    }

    public void EstablecerMontadaDesdeServidor(
        bool montada,
        int indice = -1,
        NetworkObjectReference avion = default)
    {
        if (!IsServer)
            return;

        estadoMontada.Value = montada;
        indiceAnclaje.Value = montada ? indice : -1;
        avionReferencia.Value = montada ? avion : default;

        if (montada)
            jugadorQueLaSujeta.Value = ulong.MaxValue;
    }
}
