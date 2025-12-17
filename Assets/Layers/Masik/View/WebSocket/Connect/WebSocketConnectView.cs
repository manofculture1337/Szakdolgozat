using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using WebSocketSharp;

public class WebSocketConnectView : MonoBehaviour
{
    [Inject]
    private WebSocketConnectPresenter _presenter;

    [SerializeField]
    private TMP_InputField _serverIpInputField;
    [SerializeField]
    private TMP_InputField _portInputField;
    [SerializeField]
    private Button _connectButton;
    [SerializeField]
    private GameObject _panel;
    [SerializeField]
    private Canvas _nextCanvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(_connectButton != null)
        {
            _connectButton.onClick.AddListener(OnConnectButtonClicked);
        }else
        {
            Debug.LogError("Start Server Button is not assigned in the inspector.");
        }
        if (_serverIpInputField == null)
        {
            Debug.LogError("Server IP Input Field is not assigned in the inspector.");
        }
        if (_portInputField == null)
        {
            Debug.LogError("Port Input Field is not assigned in the inspector.");
        }
        if (_panel == null)
        {
            Debug.LogError("Panel is not assigned in the inspector.");
        }
        if (_nextCanvas == null)
        {
            Debug.LogError("Next Canvas is not assigned in the inspector.");
        }
        _presenter.ConnectionStateChanged += ConnectionStateChange;
        _portInputField.characterValidation = TMP_InputField.CharacterValidation.Integer;
        _portInputField.characterLimit = 5;
    }


    private void OnConnectButtonClicked()
    {
        _portInputField.text = _presenter.Connect(GetServerIp(), GetPort()).ToString();
    }
    private void OnDestroy()
    {
        if (_connectButton != null)
        {
            _connectButton.onClick.RemoveListener(OnConnectButtonClicked);
        }
    }
    public void ShowPanel()
    {
        _panel.SetActive(true);
    }
    public void HidePanel()
    {
        _panel.SetActive(false);
        if (_nextCanvas != null)
        {
            _nextCanvas.gameObject.SetActive(true);
        }
    }
    public void ConnectionStateChange(WebSocketState state)
    {
        switch (state)
        {
            case WebSocketState.Connecting:
                _serverIpInputField.interactable = false;
                _portInputField.interactable = false;
                _connectButton.interactable = false;
                break;
            case WebSocketState.Open:
                HidePanel();
                break;
            case WebSocketState.Closed:
                _serverIpInputField.interactable = true;
                _portInputField.interactable = true;
                _connectButton.interactable = true;
                break;
            case WebSocketState.Closing:
                _serverIpInputField.interactable = false;
                _portInputField.interactable = false;
                _connectButton.interactable = false;
                break;
            default:
                _serverIpInputField.interactable = false;
                _portInputField.interactable = false;
                _connectButton.interactable = false;
                break;
        }
    }
    private string GetServerIp()
    {
        string serverIp = _serverIpInputField.text;
        if (string.IsNullOrEmpty(serverIp))
        {
            serverIp = "localhost";
            _serverIpInputField.text = serverIp;
        }
        Debug.Log("Server IP: " + serverIp);
        return serverIp;
    }
    private string GetPort()
    {
        string port = _portInputField.text;
        if (string.IsNullOrEmpty(port))
        {
            port = "8080";
            _portInputField.text = port;
        }
        Debug.Log("Server Port: " + port);
        return port;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
