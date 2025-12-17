using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class RegisterView : MonoBehaviour
{
    [SerializeField]
    private GameObject registerPanel;
    [SerializeField]
    private GameObject loginPanel;

    [SerializeField]
    private Button registerButton;

    [SerializeField]
    private Button changeToLoginButton;

    [SerializeField]
    private TMP_InputField emailField;
    [SerializeField]
    private TMP_InputField roleField;
    [SerializeField]
    private TMP_InputField passwordField;
    [SerializeField]
    private TMP_InputField confirmPasswordField;
    [SerializeField]
    private TMP_InputField firstNameField;
    [SerializeField]
    private TMP_InputField lastNameField;


    [Inject]
    private readonly LoginPresenter _loginPresenter;

    void Start()
    {
        changeToLoginButton.onClick.AddListener(() =>
        {
            loginPanel.SetActive(true);
            registerPanel.SetActive(false);
        });
        registerButton.onClick.AddListener(() =>
        {
            if(passwordField.text.Equals(confirmPasswordField.text))
            _loginPresenter.Register(
                emailField.text,
                passwordField.text,
                firstNameField.text,
                lastNameField.text,
                roleField.text
            );
        });
    }
}
