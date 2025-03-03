using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class Authentication : MonoBehaviour
{
    public static Authentication Instance { get; private set; }
    public TMP_InputField username;
    public TMP_InputField password;

    public TMP_Text errorMessageText;

    async void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void ShowError(string message)
    {
        Debug.LogError(message);
        // Hiển thị thông báo lỗi trên UI, ví dụ: gán cho một TextMeshPro UI Text
        errorMessageText.text = message;
    }

    public void CheckPlayerId()
    {
        errorMessageText.text = AuthenticationService.Instance.PlayerId;
        //Debug.Log($"PlayerId = {AuthenticationService.Instance.PlayerId}");
    }

    public void OnSignInAnonymouslyClicked()
    {
        _ = SignInAnonymouslyAsync();
    }

    public void OnSignUpClicked()
    {
        if (string.IsNullOrEmpty(username.text) || string.IsNullOrEmpty(password.text))
        {
            Debug.LogError("Username or Password is empty!");
            return;
        }

        _ = SignUpWithUsernamePasswordAsync(username.text, password.text);

        CheckPlayerId();
    }

    public void OnSignInClicked()
    {
        if (string.IsNullOrEmpty(username.text) || string.IsNullOrEmpty(password.text))
        {
            ShowError("Username or Password is empty");
            return;
        }

        _ = SignInWithUsernamePasswordAsync(username.text, password.text);
        CheckPlayerId();
    }

    public void OnSignIn1Clicked()
    {
        // if (string.IsNullOrEmpty(username.text) || string.IsNullOrEmpty(password.text))
        // {
        //     ShowError("Username or Password is empty");
        //     return;
        // }
        string usernameTest = "Abc";
        string passwordTest = "Abc1234.";

        _ = SignInWithUsernamePasswordAsync(usernameTest, passwordTest);
        CheckPlayerId();
    }

    public void OnSignIn2Clicked()
    {
        // if (string.IsNullOrEmpty(username.text) || string.IsNullOrEmpty(password.text))
        // {
        //     ShowError("Username or Password is empty");
        //     return;
        // }
        string usernameTest = "Abcd";
        string passwordTest = "Abc1234.";

        _ = SignInWithUsernamePasswordAsync(usernameTest, passwordTest);
        CheckPlayerId();
    }

    public void OnUpdatePasswordUpdateClicked(string currentPassword, string newPassword)
    {
        _ = UpdatePasswordAsync(currentPassword, newPassword);
    }

    public void SignOut()
    {
        try
        {
            AuthenticationService.Instance.SignOut(true);
            Debug.Log("Sign out anonymously succeeded!");

            //Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (AuthenticationException ex)
        {
            ShowError($"Authentication Error: {ex.Message}");
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            ShowError($"Request Failed: {ex.Message}");
            Debug.LogException(ex);
        }
        CheckPlayerId();
    }

    async Task SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            //Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            CheckPlayerId();
        }
        catch (AuthenticationException ex)
        {
            ShowError($"Authentication Error: {ex.Message}");
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            ShowError($"Request Failed: {ex.Message}");
            Debug.LogException(ex);
        }
    }

    async Task SignUpWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(
                username,
                password
            );
            Debug.Log("SignUp is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    async Task SignInWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(
                username,
                password
            );
            CheckPlayerId();
            Debug.Log("SignIn is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    async Task AddUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.AddUsernamePasswordAsync(username, password);
            Debug.Log("Username and password added.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    async Task UpdatePasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            await AuthenticationService.Instance.UpdatePasswordAsync(currentPassword, newPassword);
            Debug.Log("Password updated.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }
}
