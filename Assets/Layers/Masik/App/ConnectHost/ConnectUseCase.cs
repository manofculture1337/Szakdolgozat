using Mirror;
using UnityEngine;

public class ConnectUseCase
{
    //private INetworkManager _networkManager;
    private MyNetworkManager myNetworkManager;

    public void Connect(string ip, string port, GameObject gameObject)
    {
        TelepathyTransport telepathy = gameObject.AddComponent<TelepathyTransport>();
        if (ushort.TryParse(port, out ushort portout))
        {
            telepathy.port = portout;
            myNetworkManager.transport = telepathy;
            
        }
        myNetworkManager.networkAddress = ip;
        myNetworkManager.StartClient();
        //_networkManager.Connect(ip, port);
    }
    public void StartHost(string ip, string port)
    {
        //_networkManager.StartServerFromInput(ip, port);
    }

    public void SetToOnline(/*INetworkManager networkManager*/)
    {
        //_networkManager = networkManager;
    }
}