using Mirror;
using TMPro;
using UnityEngine;


public class ObjectHandle : NetworkBehaviour
{
    public TextMeshProUGUI listItem;

    private ControlObject _controller;

    [SyncVar (hook = nameof(OnOwnerChanged))]
    private Color _syncOwnerColor;

    [SyncVar (hook = nameof(OnNameChanged))]
    private string _syncObjectName;

    [SyncVar (hook = nameof(OnObjectColorChanged))]
    private Color _syncObjectColor;


    private void Awake()
    {
        _controller = FindFirstObjectByType<ControlObject>();
        _controller.RegisterObject(gameObject);
    }

    private void Start()
    {
        _controller.TrySelectObject(gameObject);
    }

    public void RequestAuthority()
    {
        if (isServer)
        {
            return;
        }

        CmdRequestAuthority();
    }

    [Command (requiresAuthority = false)]
    private void CmdRequestAuthority(NetworkConnectionToClient sender = null)
    {
        if (netIdentity.connectionToClient == sender)
        {
            TargetChangeOwnerColor(sender, NetworkManager.singleton.gameObject.GetComponent<MyNetworkManager>().GetColor(sender));
            TargetSelectObject(sender);

            return;
        }
        else if (netIdentity.connectionToClient != null)
        {
            return;
        }

        netIdentity.AssignClientAuthority(sender);

        TargetChangeOwnerColor(sender, NetworkManager.singleton.gameObject.GetComponent<MyNetworkManager>().GetColor(sender));
        TargetSelectObject(sender);
    }

    public void ReleaseAuthority()
    {
        CmdReleaseAuthority();
    }

    [Command (requiresAuthority = false)]
    private void CmdReleaseAuthority(NetworkConnectionToClient sender = null)
    {
        if (netIdentity.connectionToClient != connectionToClient)
        {
            return;
        }

        netIdentity.RemoveClientAuthority();

        ChangeOwnerColor(Color.white);
    }

    [TargetRpc]
    private void TargetChangeOwnerColor(NetworkConnectionToClient target, Color newColor)
    {
        ChangeOwnerColor(newColor);
    }

    [TargetRpc]
    private void TargetSelectObject(NetworkConnectionToClient target)
    {
        _controller.SelectObject(gameObject);
    }

    public void ChangeOwnerColor(Color newColor)
    {
        Color oldColor = _syncOwnerColor;
        _syncOwnerColor = newColor;
        OnOwnerChanged(oldColor, _syncOwnerColor);
    }

    private void OnOwnerChanged(Color oldColor, Color newColor)
    {
        RecolorListItem(newColor);
        RecolorOutline(newColor);
    }

    private void RecolorListItem(Color newColor)
    {
        listItem.GetComponent<GameObjectListItemHandle>().SetColor(newColor);
    }

    private void RecolorOutline(Color newColor)
    {
        if (newColor == Color.white)
        {
            Outline[] outlines = gameObject.GetComponentsInChildren<Outline>();
            foreach (Outline outline in outlines)
            {
                outline.OutlineMode = Outline.Mode.OutlineHidden;
            }
        }
        else
        {
            Outline[] outlines = gameObject.GetComponentsInChildren<Outline>();
            foreach (Outline outline in outlines)
            {
                outline.OutlineMode = Outline.Mode.OutlineAll;
                outline.OutlineColor = newColor;
            }
        }
    }

    [TargetRpc]
    public void TargetChangeName(NetworkConnectionToClient target, string newName)
    {
        ChangeName(newName);
    }

    public void ChangeName(string newName)
    {
        string oldName = _syncObjectName;
        _syncObjectName = newName;
        OnNameChanged(oldName, newName);
    }

    private void OnNameChanged(string oldName, string newName)
    {
        gameObject.name = newName;
        listItem.text = newName;
    }

    public void SetObjectColor(Color newColor)
    {
        Color oldColor = _syncObjectColor;
        _syncObjectColor = newColor;
        OnObjectColorChanged(oldColor, newColor);
    }

    private void OnObjectColorChanged(Color oldColor, Color newColor)
    {
        if (TryGetComponent<LineRenderer>(out var line))
        {
            line.startColor = newColor;
            line.endColor = newColor;
        }
        else
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material.color = newColor;
            }
        }
    }

    public void SetListItem(TextMeshProUGUI item)
    {
        listItem = item;
    }

    private void OnDestroy()
    {
        _controller.DeregisterObject(gameObject);
    }
}