using Unity.WebRTC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class WebRTCStreamerService
{
    private static WebRTCStreamerService _instance;
    public static WebRTCStreamerService Instance => _instance ??= new WebRTCStreamerService();

    public event Action<string> OnViewerConnected;
    public event Action<string> OnViewerDisconnected;
    public event Action<string> OnDebugMessage;

    public event Action<string, RTCSessionDescription> OnOfferCreated;
    public event Action<string, RTCIceCandidate> OnIceCandidateGenerated;

    private readonly Dictionary<string, RTCPeerConnection> _viewers = new();
    private readonly CoroutineRunner _coroutineRunner = new();

    private VideoStreamTrack _videoTrack;
    private AudioStreamTrack _audioTrack;

    private RTCConfiguration _rtcConfig;
    private bool _updateRunning = false;

    private WebRTCStreamerService()
    {
        _rtcConfig = new RTCConfiguration
        {
            iceServers = Array.Empty<RTCIceServer>()
            /*
            iceServers = new RTCIceServer[]
            {
                new RTCIceServer
                {
                    urls = new string[]
                    { "stun:stun.l.google.com:19302" }
                }
            }*/
        };

        StartUpdateLoop();
    }

    public void SetVideoTrack(VideoStreamTrack track)
    {
        _videoTrack = track;
    }

    public void SetAudioTrack(AudioStreamTrack track)
    {
        _audioTrack = track;
    }

    public void CreateConnectionForViewer(string viewerId)
    {
        OnDebugMessage?.Invoke($"Creating connection for viewer {viewerId}");
        var pc = new RTCPeerConnection(ref _rtcConfig);

        pc.OnIceCandidate += cand =>
        {
            OnIceCandidateGenerated?.Invoke(viewerId, cand);
        };

        pc.OnConnectionStateChange += state =>
        {
            if (state is RTCPeerConnectionState.Failed or RTCPeerConnectionState.Disconnected)
                RemoveViewer(viewerId);
        };

        _viewers[viewerId] = pc;

        // Add tracks
        RTCRtpSender videoSender = null;
        if (_videoTrack != null)
            videoSender = pc.AddTrack(_videoTrack);

        if (_audioTrack != null)
            pc.AddTrack(_audioTrack);

        if (videoSender != null)
        {
            var tr = pc.GetTransceivers().FirstOrDefault(t => t.Sender == videoSender);
            if (tr != null)
            {
                var caps = RTCRtpSender.GetCapabilities(TrackKind.Video);
                var h264 = Array.FindAll(caps.codecs, c => c.mimeType.ToLower().Contains("h264"));
                if (h264.Length > 0)
                {
                    var err = tr.SetCodecPreferences(h264);
                    if (err != RTCErrorType.None) OnDebugMessage?.Invoke($"SetCodecPref failed: {err}");
                    else OnDebugMessage?.Invoke($"H264 preference set for viewer {viewerId}");
                }
            }
        }

        // Start negotiation
        _coroutineRunner.StartCoroutine(CreateOfferForViewer(viewerId, pc));
        OnDebugMessage?.Invoke($"Connection created for viewer {viewerId}");
    }


    public void ReceiveAnswer(string viewerId, RTCSessionDescription answer)
    {
        OnDebugMessage?.Invoke($"Received answer from viewer {viewerId}");
        if (!_viewers.ContainsKey(viewerId)) return;

        OnDebugMessage?.Invoke($"Applying answer for viewer {viewerId}");
        var pc = _viewers[viewerId];
        _coroutineRunner.StartCoroutine(ApplyAnswer(pc, answer));
        OnViewerConnected?.Invoke(viewerId);
    }
    private IEnumerator CreateOfferForViewer(string viewerId, RTCPeerConnection pc)
    {
        OnDebugMessage?.Invoke($"Creating offer for viewer {viewerId}");
        var op = pc.CreateOffer();
        yield return op;

        if (op.IsError)
        {
            OnDebugMessage?.Invoke($"Offer error for {viewerId}: {op.Error}");
            yield break;
        }

        var desc = op.Desc;
        yield return pc.SetLocalDescription(ref desc);

        OnDebugMessage?.Invoke($"Offer created and set for viewer {viewerId}");
        OnOfferCreated?.Invoke(viewerId, desc);
    }


    private IEnumerator ApplyAnswer(RTCPeerConnection pc, RTCSessionDescription answer)
    {
        var op = pc.SetRemoteDescription(ref answer);
        yield return op;
    }

    public void AddIceCandidate(string viewerId, RTCIceCandidate cand)
    {
        if (_viewers.TryGetValue(viewerId, out var pc))
            pc.AddIceCandidate(cand);
    }

    public void RemoveViewer(string viewerId)
    {
        if (!_viewers.TryGetValue(viewerId, out var pc)) return;

        pc.Close();
        pc.Dispose();
        _viewers.Remove(viewerId);

        OnViewerDisconnected?.Invoke(viewerId);
    }

    public void StopAll()
    {
        foreach (var pc in _viewers.Values)
        {
            pc.Close();
            pc.Dispose();
        }
        _viewers.Clear();

        _updateRunning = false;
        OnDebugMessage?.Invoke("Stopped all WebRTC connections.");
    }

    private void StartUpdateLoop()
    {
        if (_updateRunning) return;

        _updateRunning = true;
        _coroutineRunner.StartCoroutine(WebRTC.Update());
    }
    public bool IsViewer(string viewerId)
    {
        return _viewers.ContainsKey(viewerId);
    }
    public List<string> GetViewerIDs()
    {
        return _viewers.Keys.ToList();
    }
}
