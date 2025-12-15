using UnityEngine;
using Assets.Scripts.CleanArchitecture.Domain;
using UnityEngine.UI;

public class SendButtonScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private Button _sendButton;


    private void Start()
    {
        _sendButton.onClick.AddListener(() =>
        {
            DomainManager domainManager = FindAnyObjectByType<DomainManager>();
            Debug.Log("Send button pressed");
            domainManager.sendFileUsecase.SendMesh();
        });
    }
}
