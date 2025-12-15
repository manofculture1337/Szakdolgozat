using UnityEngine;
using Mirror;
using Assets.Scripts.CleanArchitecture.Domain;

public class NetworkedObjectControl : NetworkBehaviour
{
    [SerializeField]
    private GameObject _exclamationMarkPrefab;

    [SerializeField]
    private GameObject _questionMarkPrefab;

    [SerializeField]
    private GameObject _arrowPrefab;

    [SerializeField]
    private GameObject _flagPrefab;

    [SerializeField]
    private GameObject _freeLinePrefab;

    private GameObjectType _nextObjectType;


    [Command]
    public void CmdNewObject(NetworkConnectionToClient sender = null)
    {
        GameObject obj = Instantiate(TypeToPrefab());
        NetworkServer.Spawn(obj, sender);
        obj.GetComponent<ObjectHandle>().TargetChangeName(sender, _nextObjectType.ToString());

        //RpcSynchronize(obj);
    }

    [ClientRpc]
    private void RpcSynchronize(GameObject obj)
    {
        var domainManager = GameObject.FindFirstObjectByType<DomainManager>();

        if (domainManager)
        {
            domainManager.offsetUsecases?.AddObjectToAnchor(obj);
        }
    }

    [Command]
    public void CmdDestroyObject(GameObject obj)
    {
        NetworkServer.Destroy(obj);
    }

    [Command (requiresAuthority = false)]
    public void CmdNewPointForFreeLine(GameObject freeLineObj, Vector3 point)
    {
        RpcNewPointForFreeLine(freeLineObj, point);
        NewPointForFreeLine(freeLineObj, point);
    }

    [ClientRpc]
    private void RpcNewPointForFreeLine(GameObject freeLineObj, Vector3 point)
    {
        NewPointForFreeLine(freeLineObj, point);
    }

    private void NewPointForFreeLine(GameObject freeLineObj, Vector3 point)
    {
        LineRenderer line = freeLineObj.GetComponent<LineRenderer>();
        line.positionCount += 1;
        line.SetPosition(line.positionCount - 1, point);
    }

    [Command]
    public void CmdSetNextObjectType(GameObjectType type)
    {
        _nextObjectType = type;
    }

    private GameObject TypeToPrefab()
    {
        switch (_nextObjectType)
        {
            case GameObjectType.ExclamationMark:
                return _exclamationMarkPrefab;
            case GameObjectType.QuestionMark:
                return _questionMarkPrefab;
            case GameObjectType.Arrow:
                return _arrowPrefab;
            case GameObjectType.Flag:
                return _flagPrefab;
            default:
                return _freeLinePrefab;
        }
    }
}