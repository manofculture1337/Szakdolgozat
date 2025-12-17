using TMPro;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class WebRTCViewerView : MonoBehaviour
{
    [Inject]
    private WebRTCViewerPresenter _presenter;

    [SerializeField]
    private RawImage _videoStream;
    [SerializeField]
    private TMP_Text _mainText;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_videoStream == null)
        {
            Debug.LogError("Video Stream is not assigned in the inspector.");
        }
        if (_mainText == null)
        {
            Debug.LogError("Main Text is not assigned in the inspector.");
        }
        if (_presenter != null)
        {
            _presenter.VideoStreamReceived += OnVideoStreamReceived;
        }
        else
        {
            Debug.LogError("WebRTC Viewer Presenter instance is null.");
        }

    }

    private void OnVideoStreamReceived(VideoStreamTrack videoStreamTrack)
    {
        if (videoStreamTrack != null)
        {
            videoStreamTrack.OnVideoReceived += OnVideoReceived;
            _mainText.SetText("Video Stream Received");
        }
        else
        {
            Debug.LogError("Video Stream is not assigned in the inspector.");
        }
    }
    private void OnVideoReceived(Texture texture)
    {
        if (_videoStream != null)
        {
            _videoStream.texture = texture;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
