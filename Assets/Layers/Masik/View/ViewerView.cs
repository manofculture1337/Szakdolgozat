using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class ViewerView : MonoBehaviour
{
    [SerializeField]
    private Button sendMessageButton;

    [Inject]
    private ViewerPresenter presenter;

    void Start()
    {
        sendMessageButton.onClick.AddListener(() => presenter.SendMessage());
    }
}
