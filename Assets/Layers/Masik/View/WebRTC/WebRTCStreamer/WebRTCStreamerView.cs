using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
public class WebRTCStreamerView :MonoBehaviour
{
    [Inject]
    private WebRTCStreamerPresenter _presenter;

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
    [SerializeField]
    private Button _startStreamButton;
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
        if (_startStreamButton == null)
        {
            Debug.LogError("Start Stream Button is not assigned in the inspector.");
        }
        if (_camera == null)
        {
            Debug.LogError("Camera is not assigned in the inspector.");
        }
        _startStreamButton.interactable = false;
        _callButton.interactable = false;
        _viewerDropDown.interactable = false;
        _refreshButton.onClick.AddListener(Refresh);
        _callButton.onClick.AddListener(Call);
        _startStreamButton.onClick.AddListener(OnStartStream);
        _presenter.ConnectionStabilized += OnConnected;
        _presenter.ViewersIDsRecived += RefreshIDs;
    }

    private void OnStartStream()
    {
        _startStreamButton.interactable = false;
        _maintext.SetText("Starting Stream...");
        _presenter.StartStream(_camera);
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
            _callButton.interactable = true;
            _viewerDropDown.interactable = true;
        }
        else
        {
            _viewerDropDown.AddOptions(new List<string> { "No viewers available" });
            _callButton.interactable = false;
            _viewerDropDown.interactable = false;
        }
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

    private void OnConnected()
    {
        _maintext.SetText("Connected to:" + _presenter.GetViewerId());
        _startStreamButton.interactable = true;
    }
}