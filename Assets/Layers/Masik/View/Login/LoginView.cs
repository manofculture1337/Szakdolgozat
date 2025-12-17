using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class LoginView : MonoBehaviour
{
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private TMP_InputField userNameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;

    [Inject]
    private readonly LoginPresenter _loginPresenter;

    void Start()
    {
        loginButton.onClick.AddListener(() =>
        {
            _loginPresenter.Login(userNameField.text, passwordField.text);
        });
        registerButton.onClick.AddListener(() =>
        {
            registerPanel.SetActive(true);
            loginPanel.SetActive(false);
        });
    }

}
