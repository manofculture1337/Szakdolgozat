using System;

public class LoginEventArgs : EventArgs
{
    public string userName;
    public string password;
    public LoginEventArgs(string userName, string password)
    {
        this.userName = userName;
        this.password = password;
    }
}
