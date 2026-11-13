
using UnityEngine;
using Unity.Netcode;

public class NetworkTestUI : MonoBehaviour
{
    private void OnGUI()
    {
        NetworkManager nm = NetworkManager.Singleton;

        if (nm == null)
            return;

        GUILayout.BeginArea(new Rect(20, 20, 220, 200));

        if (!nm.IsClient && !nm.IsServer)
        {
            if (GUILayout.Button("Iniciar HOST", GUILayout.Height(40)))
            {
                nm.StartHost();
            }

            if (GUILayout.Button("Iniciar CLIENT", GUILayout.Height(40)))
            {
                nm.StartClient();
            }
        }
        else
        {
            GUILayout.Label("Conectado");

            GUILayout.Label(
                nm.IsHost ? "Modo: HOST" :
                nm.IsServer ? "Modo: SERVER" :
                "Modo: CLIENT"
            );

            if (GUILayout.Button("Desconectar", GUILayout.Height(40)))
            {
                if (nm.IsHost)
                    nm.Shutdown();
                else
                    nm.Shutdown();
            }
        }

        GUILayout.EndArea();
    }
}
