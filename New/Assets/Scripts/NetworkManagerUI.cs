using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;

public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private Button sheepBtn;
    [SerializeField] private Button wolfBtn;

    [SerializeField] private PlayerSpawner spawner;

    [SerializeField] private TextMeshProUGUI title;

    private void Awake()
    {
        title.text = "";

        sheepBtn.gameObject.SetActive(false);
        wolfBtn.gameObject.SetActive(false);

        serverBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
            HideFirstButtons();
            title.text = "Server";
        });

        clientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            HideFirstButtons();
            ShowSecondButtons();
            title.text = "Client";
        });

        sheepBtn.onClick.AddListener(() =>
        {
            if (NetworkManager.IsConnectedClient)
            {
                spawner.SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, 0);
                title.text = "Sheep";
                HideSecondButtons();
            }
        });

        wolfBtn.onClick.AddListener(() =>
        {
            if (NetworkManager.IsConnectedClient)
            {
                spawner.SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, 1);
                title.text = "Wolf";
                HideSecondButtons();
            }
        });
    }

    private void HideFirstButtons()
    {
        serverBtn.gameObject.SetActive(false);
        clientBtn.gameObject.SetActive(false);
    }

    private void ShowSecondButtons()
    {
        sheepBtn.gameObject.SetActive(true);
        wolfBtn.gameObject.SetActive(true);
    }

    private void HideSecondButtons()
    {
        sheepBtn.gameObject.SetActive(false);
        wolfBtn.gameObject.SetActive(false);
    }
}
