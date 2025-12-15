using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.CleanArchitecture.Domain;

public class ExportButton : MonoBehaviour
{
    [SerializeField]
    private Button _exportButton;


    private void Start()
    {
        _exportButton.onClick.AddListener(() =>
        {
            DomainManager domainManager = FindAnyObjectByType<DomainManager>();
            domainManager.roomMeshExporter.ExtractAndExportMesh();
        });
    }
}
