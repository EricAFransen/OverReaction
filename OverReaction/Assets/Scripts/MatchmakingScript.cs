using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class MatchmakingScript : MonoBehaviour {

  public Text statusText;
  public Button findMatchButton;
  public RealtimeSessionInfo sessionInfo;

    private void Start() {
    findMatchButton.onClick.AddListener(StartMatchmaking);
  }

  public void StartMatchmaking() {
        Debug.Log("Beginning matchmaking");
        // MatchShortCode = The short code defined on Gamesparks. Each game mode has its own.
        // Skill = The skill ranking of the user. We dont care about it much rn so we just give everyone the same.
        new GameSparks.Api.Requests.MatchmakingRequest()
          .SetMatchShortCode("MULT_MTCH")
          .SetSkill(0) // Assume all players have 0 skill ( ͡° ͜ʖ ͡°)>⌐■-■
          .Send((response) => {
              if (!response.HasErrors) {
                statusText.text = "Searching for a match...";
                // Hide the logout button and show the cancel button
                findMatchButton.gameObject.SetActive(false);

                // Add a listeners for a success or failure callbacks
                // Failure
                GameSparks.Api.Messages.MatchNotFoundMessage.Listener += OnMatchNotFound;

                // Success
                GameSparks.Api.Messages.MatchFoundMessage.Listener += OnMatchFound;
              }
              else {
                Debug.Log("Error starting matchmaking: " + response.Errors.JSON.ToString());  
                statusText.text = "Failed to start matchmaking, please try again.";
              }
          });
  }

  private void OnMatchNotFound(GameSparks.Api.Messages.MatchNotFoundMessage message) {
    statusText.text = "No match found, please try again...";
    findMatchButton.gameObject.SetActive(true);
  }

  private void OnMatchFound(GameSparks.Api.Messages.MatchFoundMessage message){
        Debug.Log ("Match Found!...");
        StringBuilder sBuilder = new StringBuilder ();
        sBuilder.AppendLine ("Match Found...");
        sBuilder.AppendLine ("Port:" + message.Port);
        sBuilder.AppendLine ("MatchId:" + message.MatchId);
        foreach(GameSparks.Api.Messages.MatchFoundMessage._Participant player in message.Participants){
            sBuilder.AppendLine ("Player:" + player.PeerId + " User Name:" + player.DisplayName);
        }

        statusText.text = sBuilder.ToString (); // set the string to be the player-list field

        GameSparksManager.instance.startRealtimeSession(new RealtimeSessionInfo(message));
    }
}
