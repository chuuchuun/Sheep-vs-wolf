using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefabA;
    [SerializeField] private GameObject playerPrefabB;

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ulong clientId, int prefabId)
    {
        Debug.Log(prefabId);
        GameObject newPlayer;
        if (prefabId == 0)
            newPlayer = (GameObject)Instantiate(playerPrefabA);
        else
            newPlayer = (GameObject)Instantiate(playerPrefabB);
        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        Debug.Log("Id: " + NetworkManager.Singleton.LocalClientId);
        netObj.SpawnAsPlayerObject(clientId, true);
    }
}
