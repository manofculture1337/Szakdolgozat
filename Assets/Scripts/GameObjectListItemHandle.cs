using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GameObjectListItemHandle : MonoBehaviour
{
    private GameObject _gameObject;

    private ControlObject _controller;


    private void Start()
    {
        _controller = FindFirstObjectByType<ControlObject>();

        GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            _controller.SetIntentional();
            _controller.TrySelectObject(_gameObject);
        });
    }

    public void SetGameObject(GameObject obj)
    {
        _gameObject = obj;
        GetComponentInChildren<TextMeshProUGUI>().text = obj.name;
    }

    public void SetColor(Color col)
    {
        GetComponentInChildren<TextMeshProUGUI>().color = col;
    }
}