using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using WebSocketSharp;

public class ConnectView : MonoBehaviour
{
    [SerializeField] private TMP_InputField IPField;
    [SerializeField] private TMP_InputField PortField;
    [SerializeField] private GameObject onlinePanel;
    [SerializeField] private Button connectButton;
    [SerializeField] private Button hostButton;

    [SerializeField] private GameObject changePanel;
    [SerializeField] private Button onlineButton;
    [SerializeField] private Button offlineButton;
    
    

    [Inject]
    private readonly ConnectPresenter _connectPresenter;

    //private INetworkManager _networkManager;

    [Inject]
    private readonly IObjectResolver _resolver;

    void Start()
    {
        connectButton.onClick.AddListener(() =>
        {
            string ipAddress;
            if (string.IsNullOrWhiteSpace(IPField.text))
            {
                ipAddress = "localhost";
            }else
            {
                ipAddress = IPField.text;
            }
            _connectPresenter.Connect(ipAddress, PortField.text);
        });
        hostButton.onClick.AddListener(() =>
        {
            string ipAddress;
            if (string.IsNullOrWhiteSpace(IPField.text))
            {
                ipAddress = "localhost";
            }
            else
            {
                ipAddress = IPField.text;
            }
            Debug.Log("Starting host at IP: " + ipAddress + " Port: " + PortField.text);
            _connectPresenter.StartHost(ipAddress, PortField.text);
        });
        onlineButton.onClick.AddListener(() =>
        {
            changePanel.SetActive(false);
            onlinePanel.SetActive(true);
            //_networkManager = _resolver.Resolve<INetworkManager>();
            _connectPresenter.SetToOnline(/*_networkManager*/);
        });
        offlineButton.onClick.AddListener(() =>
        {
            _connectPresenter.ChangeToOfflineScene();
        });


    }

    public void ConnectionStateChange(WebSocketState state)
    {
        switch (state)
        {
            case WebSocketState.Connecting:
                IPField.interactable = false;
                PortField.interactable = false;
                connectButton.interactable = false;
                break;
            case WebSocketState.Closed:
                IPField.interactable = true;
                PortField.interactable = true;
                connectButton.interactable = true;
                break;
            case WebSocketState.Closing:
                IPField.interactable = false;
                PortField.interactable = false;
                connectButton.interactable = false;
                break;
            default:
                IPField.interactable = false;
                PortField.interactable = false;
                connectButton.interactable = false;
                break;
        }
    }
}
