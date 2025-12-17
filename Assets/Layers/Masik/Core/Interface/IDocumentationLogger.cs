using System.Collections.Generic;

public interface IDocumentationLogger
{
    void LogToJSON(string message, LogLevel logLevel, Dictionary<string, string> addedAttachments = null);
    void setZipPath(string path);
}