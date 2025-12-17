public class DTOMessageWrapper
{
    public int Type { get; set; } = 999;
    public string Message { get; set; } = "";
#nullable enable
    public DTOMessageWrapper? Payload { get; set; } = null;
#nullable disable
    public static string ConvertToMessage(DTOMessageWrapper dTOMessage)
    {
        //If it is not a basic websocket message type, we will not convert the payload and put the message in the payload
        // Otherwise, we will convert it to a string
        // If the payload is null, we will set it to \0
        if (IsNotBasicWebSocketMessageType(dTOMessage.Type))
        {
            return "Type:{" + dTOMessage.Type + "}Message:{" + "Not basic message type" + "}Payload:{" + dTOMessage.Message + "}";
        }
        if (dTOMessage.Payload != null)
        {
            return "Type:{" + dTOMessage.Type + "}Message:{" + dTOMessage.Message + "}Payload:{" + ConvertToMessage(dTOMessage.Payload) + "}";
        }
        else
        {
            return "Type:{" + dTOMessage.Type + "}Message:{" + dTOMessage.Message + "}Payload:{\0}";
        }
    }
    public static DTOMessageWrapper ConvertFromMessage(string message)
    {
        // Extract the Type, Message, and Payload from the input string  
        var typeStart = message.IndexOf("Type:{") + 6;
        var typeEnd = message.IndexOf("}", typeStart);
        var typeString = message.Substring(typeStart, typeEnd - typeStart);

        var messageStart = message.IndexOf("Message:{") + 9;
        var messageEnd = message.IndexOf("}", messageStart);
        var messageContent = message.Substring(messageStart, messageEnd - messageStart);

        var payloadStart = message.IndexOf("Payload:{") + 9;
        var payloadEnd = message.LastIndexOf("}");
        var payloadContent = message.Substring(payloadStart, payloadEnd - payloadStart);

        int type = int.Parse(typeString);
        DTOMessageWrapper payload = null;
        if (!string.IsNullOrEmpty(payloadContent) && payloadContent != "\0")
        {
            // Check if the payload is a nested DTOMessageWrapper
            // If it is, recursively convert it
            // Otherwise, convert it to a string to message
            if (IsNotBasicWebSocketMessageType(type))
            {
                payload = null;
                messageContent = payloadContent;
            }
            else
            {
                payload = ConvertFromMessage(payloadContent);
            }
        }

        return new DTOMessageWrapper
        {
            Type = type,
            Message = messageContent,
            Payload = payload
        };
    }
    public static bool IsNotBasicWebSocketMessageType(int type) => type > 100;
}