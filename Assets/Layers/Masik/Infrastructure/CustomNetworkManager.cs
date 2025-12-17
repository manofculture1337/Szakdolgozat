using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager, INetworkManager
{
    List<SessionInfo> sessions = new List<SessionInfo>();
    private int idCounter = 0;

    public override void Awake()
    {
        TelepathyTransport telepathy = gameObject.AddComponent<TelepathyTransport>();
        transport = telepathy;
        Transport.active = telepathy;

        base.Awake();

        autoCreatePlayer = false;
        offlineScene = "Mirror";
        onlineScene = "AuthMenu";
    }

    public void StartServerFromInput(string ipInput, string portInput)
    {
        if (ipInput == null || portInput == null) return;

        networkAddress = ipInput;

        if (ushort.TryParse(portInput, out ushort port))
        {
            if (transport is TelepathyTransport telepathy)
                telepathy.port = port;

            StartServer();
        }
        else
        {
            Debug.LogError("Invalid port number");
        }
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        Debug.Log("A client connected to the server: " + conn.address);
    }

    public void Connect(string ipInput, string portInput)
    {
        if (ipInput == null || portInput == null) return;

        networkAddress = ipInput;

        if (ushort.TryParse(portInput, out ushort port))
        {
            if (transport is TelepathyTransport telepathy)
                telepathy.port = port;

            StartClient();
        }
        else
        {
            Debug.LogError("Invalid port number");
        }
    }

    public void Disconnect()
    {
        if (!isNetworkActive) return;

        if (mode == NetworkManagerMode.Host)
            StopHost();
        else if (mode == NetworkManagerMode.ServerOnly)
            StopServer();
        else
            StopClient();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<SessionMessage>(onHostedSession);
        NetworkServer.RegisterHandler<TargetedMessage>(OnTargetedMessageReceived);
        NetworkServer.RegisterHandler<JoinedSession>(OnJoinedSession);

        if (mode == NetworkManagerMode.ServerOnly)
        {
            Debug.Log("Dedicated server started — loading ServerScene");
            SceneManager.LoadScene("ServerScene");

        }
    }

    private void OnJoinedSession(NetworkConnectionToClient client, JoinedSession session)
    {
        sessions[session.sessionId].numberOfViewers += 1;
        sessions[session.sessionId].conenctionIds.Add(client.connectionId);
    }

    private void OnLeftSession(NetworkConnectionToClient client, JoinedSession session)
    {
        sessions[session.sessionId].numberOfViewers -= 1;
        sessions[session.sessionId].conenctionIds.Remove(client.connectionId);
    }

    public void OnTargetedMessageReceived(NetworkConnectionToClient conn, TargetedMessage message)
    {
        Debug.Log("Server: Received TargetedMessage from client: " + message.content);
        Debug.Log("Server: Finding session for connection ID " + conn.connectionId);
        SessionInfo currInfo = sessions.Find(s => s.conenctionIds.Contains(conn.connectionId));
        Debug.Log("Server: Found session " + currInfo.sessionName + " for connection ID " + conn.connectionId);
        Debug.Log("Server: Forwarding TargetedMessage to " + currInfo.conenctionIds.Count + " clients in session " + currInfo.sessionName);
        foreach (int connectionId in currInfo.conenctionIds)
        {
            if (NetworkServer.connections.TryGetValue(connectionId, out NetworkConnectionToClient targetConn))
            {
                TargetedMessage msg = new TargetedMessage
                {
                    content = message.content
                };
                targetConn.Send(msg);
            }
        }
    }

    public void HostSession(string name)
    { 
        var msg = new SessionMessage
        {
            name = name
        };
        NetworkClient.Send(msg);
    }

    public void SendTargetedMessage(string content)
    {
        var msg = new TargetedMessage
        {
            content = content
        };
        NetworkClient.Send(msg);
    }

    public void JoinSession(int id)
    {
        var msg = new JoinedSession
        {
            sessionId = id
        };
        NetworkClient.Send(msg);
    }

    private void onHostedSession(NetworkConnectionToClient client, SessionMessage message)
    {
        SessionInfo newSession = new SessionInfo
        {
            sessionId = idCounter,
            sessionName = message.name,
            numberOfViewers = 0,
            conenctionIds = new List<int> { client.connectionId }
        };
        sessions.Add(newSession);
        idCounter += 1;
        var msg = new SessionListMessage
        {
            sessions = sessions
        };
        NetworkServer.SendToAll(msg);
        //NetworkServer.connections[client.connectionId].Send(msg);
    }


    public List<SessionInfo> GetSessions()
    {
        return sessions;
    }

    private void OnClientSessionMessage(SessionListMessage msg)
    {
        Debug.Log("Client: Received SessionMessage from server.");
        sessions = msg.sessions;
        Debug.Log("Client: There are " + sessions.Count + " sessions available.");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkClient.RegisterHandler<SessionListMessage>(OnClientSessionMessage);
        NetworkClient.RegisterHandler<TargetedMessage>(OnClientTargetedMessageReceived);
    }

    private void OnClientTargetedMessageReceived(TargetedMessage message)
    {
        Debug.Log("Client: Received TargetedMessage from server: " + message.content);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        if (mode == NetworkManagerMode.ClientOnly)
        {
            SceneManager.LoadScene("Scenes/AuthMenu");
        }
    }

    public void ChangeSceneToStream()
    {
        SceneManager.LoadScene("WebRTCMultiClientStreamerScene");
    }

    public void ChangeSceneToView()
    {
        SceneManager.LoadScene("WebRTCMultiClientViewerScene");
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        if (mode == NetworkManagerMode.ClientOnly)
        {
            return;
        }

        base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
    }
}