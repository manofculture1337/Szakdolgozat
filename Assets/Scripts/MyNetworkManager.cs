using Mirror;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.CleanArchitecture.Entities;
using UnityEngine.SceneManagement;

public class MyNetworkManager : NetworkManager
{
    private List<Color> _availableColors = new List<Color>()
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta,
        Color.cyan
    };

    private Dictionary<NetworkConnectionToClient, Color> _assignedColors = new Dictionary<NetworkConnectionToClient, Color>();

    private FileSender _fileSender;

    private bool _fileSent = false;


    public override void OnStartServer()
    {
        base.OnStartServer();

        _fileSender = new FileSender();
        NetworkServer.RegisterHandler<Assets.Scripts.CleanArchitecture.Entities.FileChunkMessage>(_fileSender.OnReceiveFileChunkFromClient);
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);

        _assignedColors.Add(conn, _availableColors[0]);
        _availableColors.RemoveAt(0);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        var fileSender = FindFirstObjectByType<FileSender>();

        if (fileSender != null)
        {
            Debug.Log("Resgistering handler");
            NetworkClient.RegisterHandler<Assets.Scripts.CleanArchitecture.Entities.FileChunkMessage>(fileSender.OnReceiveChunk);
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        if (_fileSent)
        {
            //FindFirstObjectByType<FileSender>().SendFileToTarget(conn);
            _fileSender.SendFileToTarget(conn);
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        foreach (var identity in NetworkServer.spawned.Values)
        {
            if (identity == null || (identity == conn.identity && identity.connectionToClient == conn))
            {
                continue;
            }

            if (identity.connectionToClient == conn)
            {
                identity.RemoveClientAuthority();

                var handle = identity.gameObject.GetComponent<ObjectHandle>();
                if (handle != null)
                {
                    handle.ChangeOwnerColor(Color.white);
                }
            }
        }

        base.OnServerDisconnect(conn);

        _availableColors.Add(_assignedColors[conn]);
        _assignedColors.Remove(conn);
    }

    public override void OnClientConnect()
    {
        Debug.Log("Client conneted");
        base.OnClientConnect();
    }

    public override void OnClientDisconnect()
    {
        Debug.Log("Client disconnected");
        base.OnClientDisconnect();
    }

    public Color GetColor(NetworkConnectionToClient conn)
    {
        return _assignedColors[conn];
    }

    public void FileSent()
    {
        _fileSent = true;
    }

    public void ChangeSceneToStream()
    {
        //Destroy(GameObject.Find("[BuildingBlock] Camera Rig"));
        //SceneManager.LoadScene("XRScene");
    }

    public void ChangeSceneToView()
    {
        //Destroy(GameObject.Find("[BuildingBlock] Camera Rig"));
        //SceneManager.LoadScene("DispatcherScene");
    }
}