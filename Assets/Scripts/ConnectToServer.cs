using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ConnectToServer : MonoBehaviour
{
    [SerializeField]
    private Button _joinButton;

    [SerializeField]
    private TMP_InputField inputField;

    private void Start()
    {
        _joinButton.onClick.AddListener(() => 
        {
            Debug.Log("Clicked join");
            NetworkManager.singleton.networkAddress = inputField.text ?? "localhost";
            Debug.Log("Address set to: " + NetworkManager.singleton.networkAddress);
            try
            { 
                NetworkManager.singleton.StartClient();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Couldn't join, reason: " + e.Message);
            }
            Debug.Log("Joined");
        });
    }
}
