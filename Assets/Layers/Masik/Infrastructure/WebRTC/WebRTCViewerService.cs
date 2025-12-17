using Unity.WebRTC;
using System;
using System.Collections;
public class WebRTCViewerService
{
    private static WebRTCViewerService _instance;
    public static WebRTCViewerService Instance => _instance ??= new WebRTCViewerService();

    public event Action<VideoStreamTrack> OnVideoReceived;
    public event Action<AudioStreamTrack> OnAudioReceived;
    public event Action<string> OnDebugMessage;
    public event Action OnDisconnected;

    public event Action<RTCIceCandidate> OnLocalIceCandidate;
    public event Action<RTCSessionDescription> OnLocalAnswerCreated;

    private RTCPeerConnection _peer;
    private readonly CoroutineRunner _coroutineRunner = new();
    private RTCConfiguration _rtcConfig;
    private bool _updateRunning;

    private WebRTCViewerService()
    {

        _rtcConfig = new RTCConfiguration
        {
            iceServers = Array.Empty<RTCIceServer>()
            /*
            iceServers = new RTCIceServer[]
            {
                new RTCIceServer
                {
                    urls = new [] { "stun:stun.l.google.com:19302" }
                }
            }*/
        };

        StartUpdateLoop();
    }


    public void CreateConnection()
    {
        _peer = new RTCPeerConnection(ref _rtcConfig);

        _peer.OnTrack += e =>
        {
            if (e.Track is VideoStreamTrack v) OnVideoReceived?.Invoke(v);
            if (e.Track is AudioStreamTrack a) OnAudioReceived?.Invoke(a);
        };

        _peer.OnIceCandidate += cand =>
        {
            OnLocalIceCandidate?.Invoke(cand);
        };
        _peer.OnConnectionStateChange += state =>
        {
            OnDebugMessage?.Invoke("Connection state changed: " + state);
            if(state == RTCPeerConnectionState.Failed)
            {
                Disconnect();
                CreateConnection();
            }
        };
    }

    public void ReceiveOffer(RTCSessionDescription offer)
    {
        _coroutineRunner.StartCoroutine(HandleOffer(offer));
    }

    private IEnumerator HandleOffer(RTCSessionDescription offer)
    {
        OnDebugMessage?.Invoke("Received offer, creating answer...");
        var op = _peer.SetRemoteDescription(ref offer);
        yield return op;

        var answer = _peer.CreateAnswer();
        yield return answer;

        var desc = answer.Desc;
        yield return _peer.SetLocalDescription(ref desc);
        OnDebugMessage?.Invoke("Answer created and set as local description.");
        OnLocalAnswerCreated?.Invoke(desc);
    }


    public void AddRemoteIceCandidate(RTCIceCandidate candidate)
    {
        _peer.AddIceCandidate(candidate);
    }


    public void Disconnect()
    {
        OnDebugMessage?.Invoke("Disconnecting peer...");
        _peer?.Close();
        _peer?.Dispose();
        _peer = null;
        OnDisconnected?.Invoke();
    }

    public void StopAll()
    {
        Disconnect();
        _updateRunning = false;
        OnDebugMessage?.Invoke("Stopped all WebRTC connections.");
    }


    private void StartUpdateLoop()
    {
        if (_updateRunning) return;

        _updateRunning = true;
        _coroutineRunner.StartCoroutine(WebRTC.Update());
    }
}
