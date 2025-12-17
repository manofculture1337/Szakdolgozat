using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class AuthenticationService : IAutentication
{

    public async Task<bool> Login(string email, string password)
    {
        //string json = $"{{ \"email\": \"{email}\", \"password\": \"{password}\" }}";

        //return await SendJsonPostRequest("https://localhost:19955/api/Auth/login", json);
        return DummyLoginCheck(email, password);
    }



    private async Task<bool> SendJsonPostRequest(string URL, string json)
    {
        using (UnityWebRequest www = UnityWebRequest.Post( URL, json, "application/json"))
        {
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error While Sending: " + www.responseCode);
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public async Task<bool> Register(string email, string password,string firstName, string lastName, string role)
    {
        string json = $"{{ \"email\": \"{email}\", \"password\": \"{password}\", \"firstName\": \"{firstName}\", \"lastName\": \"{lastName}\", \"firstName\": \"{lastName}\" }}";
        return await SendJsonPostRequest("https://localhost:19955/api/Auth/register", json);
    }

    private bool DummyLoginCheck(string username, string password)
    {
        if (username != "Admin" || password != "admin")
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
