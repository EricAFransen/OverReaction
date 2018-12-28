using UnityEngine;
using UnityEngine.UI;

public class AuthorizePlayerScript : MonoBehaviour {

	public InputField userNameInput, passwordInput; // These are set in the UI
    public Text statusText, loginText;
    public Button loginButton, registerButton, logoutButton, findMatchButton;

    private void Start() {
        loginButton.onClick.AddListener(AuthorizePlayerButton);
        logoutButton.onClick.AddListener(LogoutPlayerButton);
    }

    public void AuthorizePlayerButton() {
		Debug.Log("Authorizing player");
		new GameSparks.Api.Requests.AuthenticationRequest()
			.SetUserName(userNameInput.text)
			.SetPassword(passwordInput.text)
			.Send((response) => {
				if (!response.HasErrors) {
                    // Handle user authenticated
                    statusText.text = "Logged in as: " + response.DisplayName;
                    // Hide the UI elements for logging in and registering
                    userNameInput.gameObject.SetActive(false);
                    passwordInput.gameObject.SetActive(false);
                    loginButton.gameObject.SetActive(false);
                    registerButton.gameObject.SetActive(false);
                    loginText.enabled = false;
                    // Show UI elements for a logged in user
                    logoutButton.gameObject.SetActive(true);
                    findMatchButton.gameObject.SetActive(true);
				}
				else {
					Debug.Log("Error Authenticating player: " + response.Errors.JSON.ToString());
                    statusText.text = "Login failed";
				}
			});
	}

    public void LogoutPlayerButton() {
        new GameSparks.Api.Requests.EndSessionRequest()
            .Send((response) => {
                if (!response.HasErrors) {
                    // Handle user logged out
                    statusText.text = "";
                    // show the UI elements for logging in and registering
                    userNameInput.gameObject.SetActive(true);
                    passwordInput.gameObject.SetActive(true);
                    loginButton.gameObject.SetActive(true);
                    registerButton.gameObject.SetActive(true);
                    loginText.enabled = true;
                    // Hide the buttons not needed when logged out
                    logoutButton.gameObject.SetActive(false);
                    findMatchButton.gameObject.SetActive(false);
                }
                else {
                    Debug.Log("Error ending session: " + response.Errors.JSON.ToString());
                }
            });
    }
}
