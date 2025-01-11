using Unity.Services.Authentication;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Authentication))]
public class AuthenticationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Authentication authentication = (Authentication)target;

        if (GUILayout.Button("Check player id"))
        {
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        if (GUILayout.Button("Sign Out"))
        {
            authentication.SignOut();
        }
        if (GUILayout.Button("Guest"))
        {
            authentication.OnSignInAnonymouslyClicked();
        }
        if (GUILayout.Button("sign in 1"))
        {
            authentication.OnSignInClicked();
        }
        if (GUILayout.Button("sign in 2"))
        {
            authentication.OnSignIn2Clicked();
        }
    }
}
