using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SessionView: MonoBehaviour
{
    [Inject]
    private SessionPresenter _sessionPresenter;

    [SerializeField]
    private Button streamButton;
    [SerializeField]
    private Button refreshButton;

    [SerializeField]
    private GameObject ListContent;

    [SerializeField]
    private GameObject sessionItemPrefab;


    public void Start()
    {
        streamButton.onClick.AddListener(() => { _sessionPresenter.ChangeToStream(); });
        
        refreshButton.onClick.AddListener(() => 
        {
            RefreshList();
        });
        RefreshList();
    }

    private void RefreshList()
    {
        foreach (SessionInfo session in _sessionPresenter.UpdateSessionList())
        {
            var listItem = Instantiate(sessionItemPrefab, ListContent.transform);
            Debug.Log(session.sessionName);
            var itemView = listItem.GetComponent<SessionListItemView>();
            itemView.sessionNameText.text = session.sessionName;
            itemView.viewCountText.text = session.numberOfViewers.ToString();
            itemView.joinButton.onClick.AddListener(() =>
            {
                _sessionPresenter.ChangeToView(session.sessionId);
            });
        }
    }
}
