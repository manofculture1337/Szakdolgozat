using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

public class DocumentationLogger : IDocumentationLogger
{
    private string zipPath = null;
    DateTime localDate;
    List<LogEntry> logEntries = new List<LogEntry>();

    public void setZipPath(string path)
    {
        localDate = DateTime.Now;
        Debug.Log($"Setting zip path to: {path}");
        zipPath = path;
        logEntries.Clear();
        logEntries.Add(new LogEntry
        {
            timestamp = localDate.ToString("yyyy-MM-dd HH:mm:ss"),
            message = "Documentation started",
            level= LogLevel.System,
            attachments = null,
        });
        if (File.Exists(zipPath) == false)
        {
            using (FileStream zipToCreate = new FileStream(zipPath, FileMode.Create))
            {
                using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
                {
                    ZipArchiveEntry logEntry = archive.CreateEntry("log.json");

                    var jsonLog = JsonConvert.SerializeObject(logEntries, Formatting.Indented);
                    using (StreamWriter writer = new StreamWriter(logEntry.Open()))
                    {
                        writer.WriteLine(jsonLog);
                    }
                }
            }
        }
    }

    public void LogToJSON(string message, LogLevel logLevel, Dictionary<string, string> addedAttachments = null)
    {
        localDate = DateTime.Now;

        using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Open))
        {
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
            {

                ZipArchiveEntry logEntry;

                logEntry = archive.GetEntry("log.json");
                string existingContent;
                using (StreamReader reader = new StreamReader(logEntry.Open()))
                {
                    existingContent = reader.ReadToEnd();
                }

                logEntries=JsonConvert.DeserializeObject<List<LogEntry>>(existingContent);

                logEntries.Add(new LogEntry
                {
                    timestamp = localDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    message = message,
                    level = logLevel,
                    attachments = addedAttachments,
                });

                var jsonLog = JsonConvert.SerializeObject(logEntries, Formatting.Indented);
                using (StreamWriter writer = new StreamWriter(logEntry.Open()))
                {
                    writer.WriteLine(jsonLog);
                    
                }
            }
        }

    }

}
