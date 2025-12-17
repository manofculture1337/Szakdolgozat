using Unity.WebRTC;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

public delegate void VideostreamHandler(VideoStreamTrack videoStreamTrack);
public delegate void AudioStreamHandler(AudioStreamTrack audioStreamTrack);

public class WebRTCService
{
    private WebRTCMessageHandlerService _webRTCMessageHandler;

    private ulong _maxBitrate = 2_000_000; // in bps (2 Mbps)  
    private float _downscaleFactor = 1.5f; // Downscaling factor for resolution  
    private int stateLogInterval = 5; // Log state every X seconds  

    public event DoneEventHandler Connected;
    public event VideostreamHandler OnVideoStreamReceived;
    public event AudioStreamHandler OnAudioStreamReceived;
    public event DisplayMessageHandler DisplayDebugMessage;

    private readonly CoroutineRunner _coroutineRunner = new CoroutineRunner(); // Add a helper class to manage coroutines  

    //Maybe make modell for it
    private RTCPeerConnection _peerConnection;
    private RTCRtpSender _videoSender;
    public RTCSignalingState SignalingState => _peerConnection.SignalingState;
    //-end

    private static WebRTCService _instance;
    public WebRTCService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new WebRTCService();
            }
            return _instance;
        }
    }
    public void SetServices(WebRTCMessageHandlerService webRTCMessageHandler)
    {
        if (_instance == null)
        {
            _instance = new WebRTCService();
        }
        if(_instance._webRTCMessageHandler == null)
        {
            _instance._webRTCMessageHandler = webRTCMessageHandler;
            _instance.InstanceStartService();
        }
    }
    public void SendVideoTrack(VideoStreamTrack videoStreamTrack) => _instance.InstanceSendVideoTrack(videoStreamTrack);

    public void SendAudioTrack(AudioStreamTrack audioStreamTrack) => _instance.InstanceSendAudioTrack(audioStreamTrack);
    public void Negotiate() => _instance.InstanceNegotiate();
    public void RTCSDpRecived(RTCSessionDescription sdp) => _instance.InstanceRTCSDpRecived(sdp);
    public void IceCanditetRecived(RTCIceCandidate iceCandidate) => _instance.InstanceIceCanditetRecived(iceCandidate);
    public void ListenConnectionDone(System.Action listener)
    {
        _instance.Connected += delegate
        {
            listener();
        };
    }
    private void InstanceStartService()
    {
        _coroutineRunner.StartCoroutine(WebRTC.Update());
        ConfigureRTC();
        //_coroutineRunner.StartCoroutine(LogStatsCoroutine());
    }

    private IEnumerator LogStatsCoroutine()
    {
        while (true)
        {
            if (_peerConnection != null)
            {
                var op = _peerConnection.GetStats();
                yield return op; // Wait until IsDone is true  

                if (op.IsError)
                {

                }
                else
                {
                    // Or iterate for detailed logging:  
                    foreach (var stat in op.Value.Stats.Values)
                    {
                        DisplayDebugMessage?.Invoke($"Type: {stat.Type}, ID: {stat.Id}, Timestamp: {stat.Timestamp}");
                        foreach (var pair in stat.Dict)
                        {
                            DisplayDebugMessage?.Invoke($"{pair.Key}: {pair.Value}");
                        }
                    }
                }
            }
            yield return new WaitForSeconds(stateLogInterval); // Log every X seconds  
        }
    }

    private void ConfigureRTC()
    {
        var config = new RTCConfiguration
        {
            iceServers = new RTCIceServer[]
            {
               new RTCIceServer
               {
                   urls = new string[]
                   {  
                       // Google Stun server  
                       "stun:stun.l.google.com:19302"
                   },
               }
            },
        };

        _peerConnection = new RTCPeerConnection(ref config);
        _peerConnection.OnNegotiationNeeded += Negotiate;
        _peerConnection.OnConnectionStateChange += OnConnectionStateChange;
        _peerConnection.OnIceCandidate += OnIceCandidate;
        _peerConnection.OnTrack += OnTrack;
    }

    private void OnTrack(RTCTrackEvent e)
    {
        if (e.Track is VideoStreamTrack)
        {
            DisplayDebugMessage?.Invoke("Video track received");
            var videoTrack = e.Track as VideoStreamTrack;
            OnVideoStreamReceived?.Invoke(videoTrack);
        }
        else if (e.Track is AudioStreamTrack)
        {
            DisplayDebugMessage?.Invoke("Audio track received");
            var audioTrack = e.Track as AudioStreamTrack;
            OnAudioStreamReceived?.Invoke(audioTrack);
        }
    }

    private void InstanceSendVideoTrack(VideoStreamTrack videoStreamTrack)
    {
        if (videoStreamTrack == null)
        {
            DisplayDebugMessage?.Invoke("Video stream track is null");
            return;
        }
        //Remove the old video track if it exists  
        if (_videoSender != null)
            _peerConnection.RemoveTrack(_videoSender);

        _videoSender = _peerConnection.AddTrack(videoStreamTrack);
        if (_videoSender == null)
        {
            DisplayDebugMessage?.Invoke("Failed to add video stream track");
            return;
        }
        // Improve performance   
        // 1. by limiting the bitrate  
        //    Set the max bitrate for the video track to 1 Mbps  
        else
        {
            var parameters = _videoSender.GetParameters();
            foreach (var encoding in parameters.encodings)
            {
                encoding.maxBitrate = _maxBitrate;
                DisplayDebugMessage?.Invoke($"Set max bitrate to {_maxBitrate} bps");
                encoding.maxFramerate = 60;
                encoding.scaleResolutionDownBy = _downscaleFactor;
            }
            _videoSender.SetParameters(parameters);
        }

        // After adding the video track  
        var videoTransceiver = _peerConnection
            .GetTransceivers()
            .FirstOrDefault(t => t.Sender.Track == _videoSender.Track && t.Sender.Track.Kind == TrackKind.Video);

        if (videoTransceiver != null)
        {
            var videoCapabilities = RTCRtpSender.GetCapabilities(TrackKind.Video);
            var h264Codecs = videoCapabilities.codecs
                .Where(codec => codec.mimeType.ToLower().Contains("h264"))
                .ToArray();

            if (h264Codecs.Length > 0)
            {
                var error = videoTransceiver.SetCodecPreferences(h264Codecs);
                if (error != RTCErrorType.None)
                    DisplayDebugMessage?.Invoke($"Failed to set codec preferences: {error}");
            }
        }
    }

    private void InstanceSendAudioTrack(AudioStreamTrack audioStreamTrack)
    {//Maybe remove the old track  
        if (audioStreamTrack == null)
        {
            DisplayDebugMessage?.Invoke("Audio stream track is null");
            return;
        }
        var sender = _peerConnection.AddTrack(audioStreamTrack);
        if (sender == null)
        {
            DisplayDebugMessage?.Invoke("Failed to add audio stream track");
            return;
        }
    }

    private void OnConnectionStateChange(RTCPeerConnectionState state)
    {
        DisplayDebugMessage?.Invoke("Connection state changed: " + state);
    }

    private void InstanceNegotiate()
    {
        _coroutineRunner.StartCoroutine(OnNegotiationNeeded());
    }

    private IEnumerator OnNegotiationNeeded()
    {
        var offer = _peerConnection.CreateOffer();
        yield return offer;
        if (offer.IsError)
        {
            DisplayDebugMessage?.Invoke("Error creating offer: " + offer.Error);
            yield break;
        }
        var description = offer.Desc;
        yield return _peerConnection.SetLocalDescription(ref description);
        _webRTCMessageHandler.SendSDPMessage(description);
    }

    private IEnumerator OnOfferRecived(RTCSessionDescription offer)
    {
        DisplayDebugMessage?.Invoke("WebRTCOfferRecived");
        var remotedesc = _peerConnection.SetRemoteDescription(ref offer);
        yield return remotedesc;
        if (remotedesc.IsError)
        {   
            DisplayDebugMessage?.Invoke("Error setting remote description: " + remotedesc.Error);
            yield break;
        }
        var answer = _peerConnection.CreateAnswer();
        yield return answer;
        if (answer.IsError)
        {
            DisplayDebugMessage?.Invoke("Error creating answer: " + answer.Error);
            yield break;
        }
        var description = answer.Desc;
        yield return _peerConnection.SetLocalDescription(ref description);
        _webRTCMessageHandler.SendSDPMessage(description);
        Connected?.Invoke();
    }

    private IEnumerator OnAnswerRecived(RTCSessionDescription answer)
    {
        DisplayDebugMessage?.Invoke("WebRTAnswerRecived");
        yield return _peerConnection.SetRemoteDescription(ref answer);
        Connected?.Invoke();
    }

    public void OnIceCandidate(RTCIceCandidate candidate)
    {
        if (candidate == null)
        {
            DisplayDebugMessage?.Invoke("ICE candidate is null");
            //throw new ArgumentNullException("ICE candidate is null");
            return;
        }
        _webRTCMessageHandler.SendICEMessage(candidate);
    }

    private void InstanceIceCanditetRecived(RTCIceCandidate iceCandidate)
    {
        if (iceCandidate == null)
        {
            DisplayDebugMessage?.Invoke("ICE candidate is null");
            //throw new ArgumentNullException("ICE candidate is null");
            return;
        }
        _peerConnection.AddIceCandidate(iceCandidate);
    }

    private void InstanceRTCSDpRecived(RTCSessionDescription sdp)
    {
        switch (sdp.type)
        {
            case RTCSdpType.Offer:
                DisplayDebugMessage?.Invoke("Received offer");
                _coroutineRunner.StartCoroutine(OnOfferRecived(sdp));
                break;
            case RTCSdpType.Answer:
                DisplayDebugMessage?.Invoke("Received answer");
                _coroutineRunner.StartCoroutine(OnAnswerRecived(sdp));
                break;
            default:
                DisplayDebugMessage?.Invoke("Unknown SDP type: " + sdp.type);
                //throw new ArgumentException("Unknown SDP type: " + sdp.type);
                return;
        }
    }
}

// Helper class to manage coroutines without MonoBehaviour  
public class CoroutineRunner
{
    private readonly TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();

    public void StartCoroutine(IEnumerator routine)
    {
        Task.Factory.StartNew(() => RunCoroutine(routine), CancellationToken.None, TaskCreationOptions.None, _scheduler);
    }

    private async void RunCoroutine(IEnumerator routine)
    {
        while (routine.MoveNext())
        {
            if (routine.Current is WaitForSeconds wait)
            {
                await Task.Delay(TimeSpan.FromSeconds(wait.Seconds));
            }
            else
            {
                await Task.Yield();
            }
        }
    }
}

// Custom WaitForSeconds implementation  
public class WaitForSeconds
{
    public float Seconds { get; }

    public WaitForSeconds(float seconds)
    {
        Seconds = seconds;
    }
}
