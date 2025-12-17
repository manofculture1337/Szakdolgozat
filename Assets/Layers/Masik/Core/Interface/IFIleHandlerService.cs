using UnityEngine;

public interface IFileHandlerService
{
    public void SaveLog(string message);
    public void LoadLogs();
}
