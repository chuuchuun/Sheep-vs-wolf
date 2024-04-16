using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private TextMeshProUGUI title;

    private void Awake()
    {
        title.text = "";

        serverBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
            HideButtons();
            title.text = "Server";
        });

        clientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            HideButtons();
            title.text = "Client";
        });
    }

    private void HideButtons()
    {
        serverBtn.enabled = false;
        serverBtn.gameObject.SetActive(false);
        clientBtn.enabled = false;
        clientBtn.gameObject.SetActive(false);
    }
}
