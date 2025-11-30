using Mirror;
using UnityEngine;
using UnityEngine.UI;


public class ConnectToServer : MonoBehaviour
{
    [SerializeField]
    private Button _joinButton;


    private void Start()
    {
        _joinButton.onClick.AddListener(() => 
        {
            NetworkManager.singleton.networkAddress = "localhost";
            NetworkManager.singleton.StartClient();
            _joinButton.interactable = false;
        });
    }
}
