
public class RegisterUseCase
{
    private readonly IAutentication _autentication;

    public RegisterUseCase(IAutentication autentication)
    {
        _autentication = autentication;
    }

    public void Register(string username, string password, string firstName, string lastName, string role)
    {
        _autentication.Register(username, password, firstName, lastName, role);
    }
}
