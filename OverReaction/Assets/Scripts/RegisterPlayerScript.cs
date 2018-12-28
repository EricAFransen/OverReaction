using UnityEngine.UI;
using UnityEngine;

public class RegisterPlayerScript : MonoBehaviour {

	public InputField userNameinput, passwordInput; // These are set through the UI
    public Text statusField;
    public Button registerButton;

    private void Start() {
        registerButton.onClick.AddListener(RegisterPlayerButton);
    }

    public void RegisterPlayerButton() {
		Debug.Log ("Registering Player");
		new GameSparks.Api.Requests.RegistrationRequest() 
			.SetDisplayName(userNameinput.text)
			.SetPassword(passwordInput.text)
			.SetUserName(userNameinput.text)
			.Send((response) => {
				if (!response.HasErrors) {
					Debug.Log("Player Registered");
                    statusField.text = "Player successfully registered";
                    passwordInput.text = "";
				}
				else {
					Debug.Log("Player registration failed.. \n" + response.Errors.JSON.ToString());
                    statusField.text = "Player registration failed.";
				}
			});
	}

}
