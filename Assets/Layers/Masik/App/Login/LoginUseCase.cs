
using System.Threading.Tasks;

public class LoginUseCase
{
    private readonly IAutentication _autentication;
    private readonly IDocumentationLogger _documentationLogger;

    public LoginUseCase(IAutentication autentication, IDocumentationLogger documentationLogger)
    {
        _autentication = autentication;
        _documentationLogger = documentationLogger;
    }

    public async Task<bool> Login(string email, string password)
    {
        if (await _autentication.Login(email, password))
        {
            //_documentationLogger.LogToJSON("Login successful for user: " + email, LogLevel.User);
            return true;
        }
        else
        {
            //_documentationLogger.LogToJSON("Login failed for user: " + email, LogLevel.User);
            return false;
        }
    }

}
