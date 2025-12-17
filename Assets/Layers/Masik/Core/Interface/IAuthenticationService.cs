using System.Threading.Tasks;

public interface IAutentication
{
    public Task<bool> Login(string username, string password);
    public Task<bool> Register(string email, string password, string firstName, string lastName, string role);
}
