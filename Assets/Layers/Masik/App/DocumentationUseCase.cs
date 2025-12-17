using System;
using System.Collections.Generic;
using System.IO;

public class DocumentationUseCase
{

    private readonly IDocumentationLogger _documentationLogger;
    private readonly Ziphandler ziphandler = new Ziphandler();
    private
    string sessionName;

    public DocumentationUseCase(IDocumentationLogger logger)
    {

        _documentationLogger = logger;
    }

    public void loggerSetup(string dataPath)
    {
        sessionName = $"session{DateTime.Now.ToString().Replace(" ", "").Replace(":", "-")}";
        //_documentationLogger.setZipPath($"{dataPath}/{sessionName}.zip");
    }

    public void LogToJson(string message, LogLevel level, Dictionary<string, string> parameters=null)
    {
        //_documentationLogger.LogToJSON(message, level, parameters);
    }

    public void TakeScreenshot(string screenshotPath, string persistentPath)
    {
        DateTime now = DateTime.Now;
        string newScreenshotPath = Path.Combine(persistentPath, $"{sessionName}.zip/screenshot_{now.ToString().Replace(" ", "").Replace(":", "-")}.png");
        ziphandler.saveScreenshotToZip(
            Path.Combine( persistentPath, $"{sessionName}.zip"),
            File.ReadAllBytes(screenshotPath),
            $"screenshot_{now.ToString().Replace(" ", "").Replace(":", "-")}.png"
        );
        Dictionary<string, string> logData = new Dictionary<string, string>
        {
            { "screenshotPath", newScreenshotPath }
        };
        //_documentationLogger.LogToJSON("Screenshot taken", LogLevel.User, logData);

    }
}
