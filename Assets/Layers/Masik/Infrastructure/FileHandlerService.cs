using System;
using System.IO;
using UnityEngine;

public class FileHandlerService : IFileHandlerService
{
    private readonly string logPath = Application.persistentDataPath + "/log";
    DateTime localDate;

    public void SaveLog(string message)
    {
        localDate = DateTime.Now;
        string savePath = logPath + localDate.ToString("yyyy-MM-dd") + ".txt";
        message= localDate.ToString("HH:mm:ss") + " - " + message;
        if (File.Exists(savePath))
        {
            File.AppendAllText(savePath, message + "\n");
        }
        else
        {
            File.WriteAllText(savePath, message + "\n");
        }
    }

    public void LoadLogs()
    {
        if (Directory.Exists(Application.persistentDataPath + "/Logs"))
        {
            DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath + "/Logs");
            FileInfo[] info = dir.GetFiles("*.txt");
            foreach (FileInfo f in info)
            {
                Debug.Log(f.Name);
            }
        }
        else
        {
            Debug.Log("No log files found.");
        }
    }

}
