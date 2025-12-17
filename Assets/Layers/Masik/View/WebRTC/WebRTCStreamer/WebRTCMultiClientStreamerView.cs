using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class WebRTCMultiClientStreamerView : MonoBehaviour
{
    [Inject]
    private WebRTCMultiClientStreamerPresenter _presenter;

    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private TMP_Text _maintext;
    [SerializeField]
    private Button _refreshButton;
    [SerializeField]
    private TMP_Dropdown _viewerDropDown;
    [SerializeField]
    private Button _callButton;
    void Start()
    {
        if (_maintext == null)
        {
            Debug.LogError("Main Text is not assigned in the inspector.");
        }
        if (_refreshButton == null)
        {
            Debug.LogError("Refresh Button is not assigned in the inspector.");
        }
        if (_viewerDropDown == null)
        {
            Debug.LogError("Viewer Drop Down is not assigned in the inspector.");
        }
        if (_callButton == null)
        {
            Debug.LogError("Call Button is not assigned in the inspector.");
        }
        if (_camera == null)
        {
            Debug.LogError("Camera is not assigned in the inspector.");
        }
        _callButton.interactable = false;
        _viewerDropDown.interactable = false;
        _refreshButton.onClick.AddListener(Refresh);
        _callButton.onClick.AddListener(Call);
        _presenter.ViewersIDsRecived += RefreshIDs;
        _presenter.SetStream(_camera);
        _presenter.OnViewerConnected += OnConnected;
        _presenter.OnViewerDisconnected += OnDisconnected;
        _presenter.OnPairingFailed += OnPairingFailed;
        _viewerDropDown.onValueChanged.AddListener(NewSelected);
    }

    private void NewSelected(int arg0)
    {
        var selectedViewerId = _viewerDropDown.options[arg0].text;
        if(_presenter.IsConnectedTo(selectedViewerId))
        {
            _callButton.GetComponentInChildren<TextMeshProUGUI>().text="Disconnect";
        }
        else
        {
            _callButton.GetComponentInChildren<TextMeshProUGUI>().text = "Connect";
        }
    }

    private void Refresh()
    {
        _viewerDropDown.ClearOptions();
        _viewerDropDown.AddOptions(new List<string> { "Refreshing..." });
        _callButton.interactable = false;
        _viewerDropDown.interactable = false;
        _presenter.Refresh();
    }
    private void RefreshIDs()
    {
        _viewerDropDown.ClearOptions();
        var viewerIDs = _presenter.GetViewerIDs();
        if (viewerIDs != null && viewerIDs.Count > 0)
        {
            var stringIDs = viewerIDs.ConvertAll(id => id.ToString());
            _viewerDropDown.AddOptions(stringIDs);
            _viewerDropDown.value = 0;
            _viewerDropDown.RefreshShownValue();
            _callButton.interactable = true;
            _viewerDropDown.interactable = true;
        }
        else
        {
            _viewerDropDown.AddOptions(new List<string> { "No viewers available" });
            _viewerDropDown.value = 0;
            _viewerDropDown.RefreshShownValue();
            _callButton.interactable = false;
            _viewerDropDown.interactable = false;
        }
        _refreshButton.interactable = true;
        NewSelected(_viewerDropDown.value);
    }
    private void Call()
    {
        _callButton.interactable = false;
        _refreshButton.interactable = false;
        _viewerDropDown.interactable = false;
        _maintext.SetText("Pairing up...");
        var selectedViewerId = _viewerDropDown.options[_viewerDropDown.value].text;
        _presenter.Call(selectedViewerId);
    }
    private void OnConnected(string id)
    {
        _maintext.SetText($"Connected to {id}!");
        Refresh();
    }
    private void OnDisconnected(string id)
    {
        _maintext.SetText($"Disconnected from {id}.");
        Refresh();
    }
    private void OnPairingFailed(string id)
    {
        _maintext.SetText($"Pairing with {id} failed.");
        Refresh();
    }
}
