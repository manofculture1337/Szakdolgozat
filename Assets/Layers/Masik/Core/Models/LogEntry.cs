using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class LogEntry
{
    public string timestamp;
    public string message;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public LogLevel level;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, string> attachments;
}

public enum LogLevel
{
    System,
    User
}
