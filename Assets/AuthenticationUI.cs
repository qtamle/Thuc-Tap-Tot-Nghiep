using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthenticationUI : MonoBehaviour
{
    public static Authentication Instance { get; private set; }
    public TMP_InputField username;
    public TMP_InputField password;

    public TMP_InputField usernameSignUp;
    public TMP_InputField passwordSignUp;

    public TMP_Text errorMessageText;

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

    public void OnSignUpClicked()
    {
        if (string.IsNullOrEmpty(usernameSignUp.text) || string.IsNullOrEmpty(passwordSignUp.text))
        {
            Debug.LogError("Username or Password is empty!");
            return;
        }

        _ = SignUpWithUsernamePasswordAsync(usernameSignUp.text, passwordSignUp.text);

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

    async Task SignUpWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(
                username,
                password
            );
            Debug.Log("SignUp is successful.");
            LoadShopOnlineScene();
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
            LoadShopOnlineScene();
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

    private void LoadShopOnlineScene()
    {
        SceneManager.LoadScene("Shop_Online");
    }
}
