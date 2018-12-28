using GameSparks.Api.Messages;
using GameSparks.Api.Responses;
using GameSparks.Core;
using GameSparks.RT;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// This class is to enforce that the GameSparksManager is a singleton
public class GameSparksManager : MonoBehaviour {

    // The GameSparksManager singleton
    public static GameSparksManager instance = null;
    private GameSparksRTUnity gameSparksRTUnity;
    private RealtimeSessionInfo sessionInfo;
    public Button playOfflineButton;
    private Simulation simulation;
    public Text playerListText;
    public bool iAmHost{ get; set; }
    public bool iAmOnline { get; set; }

	void Awake() {
		// if there is no instance then we will make one
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(this.gameObject);
            iAmHost = false;
            iAmOnline = false;
        }
		// if there is already a GameSparkManager then destroy this one.
		else {
      if (playerListText != null && GameSparksManager.instance.iAmOnline) {
        // if this is true we just made the sim scene and need to pass this in
        GameSparksManager.instance.setPlayerListText(playerListText);
      }
			Destroy(this.gameObject);
		}
	}

    private void Start() {
        if (playOfflineButton != null)
        playOfflineButton.onClick.AddListener(startOfflineGame);
    }

    public void startRealtimeSession(RealtimeSessionInfo info) {
        Debug.Log("GSM| Creating New RT Session Instance...");
        sessionInfo = info;
        gameSparksRTUnity = this.gameObject.AddComponent<GameSparksRTUnity>(); // Adds the RT script to the game
        
        GSRequestData mockedResponse = new GSRequestData()
                                            .AddNumber("port", (double) info.getPortID())
                                            .AddString("host", info.getHostURL())
                                            .AddString("accessToken", info.getAccessToken()); // construct a dataset from the game-details

        FindMatchResponse response = new FindMatchResponse(mockedResponse); // create a match-response from that data and pass it into the game-config
        
        gameSparksRTUnity.Configure(response,
            (peerId) => { OnPlayerConnectedToGame(peerId); },
            (peerId) => { OnPlayerDisconnected(peerId); },
            (ready) => { OnRTReady(ready); },
            (packet) => { OnPacketReceived(packet); });
        gameSparksRTUnity.Connect(); // when the config is set, connect the game

    }

    private void OnPlayerConnectedToGame(int peerId) {
        Debug.Log("GSM| Player Connected, " + peerId);
    }

    private void OnPlayerDisconnected(int peerId) {
        Debug.Log("GSM| Player Disconnected, " + peerId);
    }

    private void OnRTReady(bool isReady) {
        if (isReady) {
            Debug.Log("GSM| RT Session Connected...");
        }

        // Ask server if you are the game host
        gameSparksRTUnity.SendData(10, GameSparksRT.DeliveryIntent.RELIABLE, null);
        Debug.Log("Sent packet 10");
    }

    private void OnPacketReceived(RTPacket packet) {
        Debug.Log("Got packet: " + packet.OpCode);
        if (packet.OpCode == 11) {
            handleHostRecieved(packet);
        } else if (packet.OpCode == 21) {
            handleCardPlayRequestResponse(packet);
        } else if (packet.OpCode == 24) {
            handleHostPlayCard(packet);
        } else if (packet.OpCode == 26) {
            handleInitialDeckRecieved(packet);
        } else if (packet.OpCode == 100) {
            handleUpdateGameState(packet);
        }
    }

    private void handleInitialDeckRecieved(RTPacket packet) {
        Dictionary<int, string> deck = new Dictionary<int, string>();
        uint offset = 1;
        RTData data = packet.Data;
        int deckSize = (int)data.GetInt(offset++);
        // For each card in the deck
        for (int i = 0; i<deckSize; i++) {
            string cardDisplay = data.GetString(offset++);
            int index = (int)data.GetInt(offset++);
            deck.Add(index, cardDisplay);
        }

        CardController.Controller.setDeck(deck);
    }

    // Applies a given card to the simulation 
    // This type of packet will only ever be recieved by the host
    private void handleHostPlayCard(RTPacket packet) {
        RTData data = packet.Data;  
        int cardIndex = (int) data.GetInt(1);
        string peerId = data.GetString(2);
        Debug.Log("Request for host to play card: " + cardIndex + " for user: " + peerId);
        // Play the card
        CardController.Controller.PlayCard(peerId, cardIndex);
    }

    // This response will handle a reponse after trying to play a card
    // if you are the host, and we get true back. Then we apply it
    private void handleCardPlayRequestResponse(RTPacket packet) {
        if (packet.Data.GetInt(1) == 1) {
            if (iAmHost) {
                // Might not end up using. Host asking to play a card sends a 24 like every non host player if the card is playable
            }
            // If we are not the host, we dont need to do anything. The UI will be updated once the host applies it
        } else { // You cannot play the card
            // Add ui response to alert the user
        }
    }

    private void handleHostRecieved(RTPacket packet) {
        // Recieved host
        iAmHost = packet.Data.GetInt(1) == gameSparksRTUnity.PeerId;
        iAmOnline = true; // Online game starting

        if (iAmHost) {
            Debug.Log("I am the host");
        }
        else {
            Debug.Log("I am the client");
        }

        SceneManager.LoadScene("SimulationScene");
        
    }

    public GameSparksRTUnity getRTSession() {
        return this.gameSparksRTUnity;
    }

    public RealtimeSessionInfo getSessionInfo() {
        return this.sessionInfo;
    }

    // Used when we make the simulation scene to pass in a reference to the playerlist in the sim scene
    // We do this since this singleton is created in the login scene so we cant access it when starting
    public void setPlayerListText(Text newPLText) {
      playerListText = newPLText;
      
      // Populate the player list on the UI
      string playerListString = "";
      foreach (RealtimePlayer player in getSessionInfo().getPlayers()) {
        playerListString += player.getDisplayName() + "\n";     
      }

      playerListText.text = playerListString;
    }

    public void canPlayCard(int pos) {
        Debug.Log("Sending playCardRequest for card: " + pos);
        RTData rtData = new RTData();
        rtData.SetInt(1, pos);
        gameSparksRTUnity.SendData(20, GameSparksRT.DeliveryIntent.UNRELIABLE, rtData);
    }

    // Serialize all players decks and send them out. See network docs for serialization patterns
    public void sendInitialDeck(Dictionary<string, Dictionary<int, Card>> cards) {
        Debug.Log("Sending the initial deck states");
        RTData rTData = new RTData();
        uint offset = 1;
        rTData.SetInt(offset++, cards.Count);

        foreach (KeyValuePair<string, Dictionary<int, Card>> playerPair in cards) {
            // For each pair in the deck of a specific player
            rTData.SetInt(offset++, playerPair.Value.Count); // Set the first int to tell the length of a deck
            rTData.SetString(offset++, playerPair.Key); // add the players peerID to preface their deck
            foreach (KeyValuePair<int, Card> deckPair in playerPair.Value) {
                rTData.SetString(offset++, deckPair.Value.ToString()); // add the cards displayString
                rTData.SetInt(offset++, deckPair.Key); // Add the cards position
            }
        }
        Debug.Log("Sent initial deck states");
        gameSparksRTUnity.SendData(25, GameSparksRT.DeliveryIntent.RELIABLE, rTData);
        
    }

    public void updateGameState(int[] quantities, List<ChemicalReaction> activeReactions) {
        RTData rtData = new RTData();
        uint i = 1; // keeps track of how many things weve added
        for (int k = 0; k < quantities.Length; k++) {
            rtData.SetInt(i++, quantities[k]);
        }
        foreach (ChemicalReaction reaction in activeReactions) {
            rtData.SetString(i++, reaction.EquationMinimal());
            rtData.SetDouble(i++, reaction.time);
            Debug.Log(reaction.time + " " + reaction.ToString());
        }
        gameSparksRTUnity.SendData(100, GameSparksRT.DeliveryIntent.UNRELIABLE_SEQUENCED, rtData);
    }

    private void handleUpdateGameState(RTPacket packet) {
        // Check if sim is null then get it 
        if (simulation == null) {
            simulation = Camera.main.GetComponent<Simulation>();
        }

        int[] speciesCount = new int[5]; // Adjust the size depending on number of species
        List<RunningReaction> currentReactions = new List<RunningReaction>();
        uint offset = 1; // Tracks the current offset of int values
        RTData data = packet.Data;
        // Populate the specied counts, use of turnary because we need to cast the packet's nullable ints to generic ints
        for (int i = 0; i < speciesCount.Length; i++) {
            speciesCount[i] = data.GetInt(offset++) ?? default(int);
        }
        Debug.Log("Species Recieved: "+String.Join(",", new List<int>(speciesCount).ConvertAll(i => i.ToString()).ToArray()));
        simulation.setSpecies(speciesCount);

        // Now we need to populate the ongoing reactions to display on the ui
        while (data.GetString(offset) != null) {
            string reactionName = data.GetString(offset++);
            double reactionTime = data.GetDouble(offset++) ?? default(double); // Need to cast from a nullable double to a generic double
            currentReactions.Add(new RunningReaction(reactionName, reactionTime));
        }
        // TODO: use the reactionList to update the UI
        simulation.setRunningReactions(currentReactions);
    }

    // Parses one Serialized Chemical Reaction from the data, starting at an offset
    // Returns the reaction and the new offset after parsing.
    private ReactionWithOffset parseReaction(uint givenOffset, RTData data) {
        uint offset = givenOffset;
        int[] givenReactants = new int[5];
        int[] givenProducts = new int[5];
        int[] givenSpecies = { 0, 0, 0, 0, 0 }; // We do not pass in the species
        int givenRate = 0;
        // Copy the next 5 elements into an array for the reactants.
        // We also need to do the ternary since the packet data are nullable ints, so we need to make them generic ints
        for (int i = 0; i < givenReactants.Length; i++) {
            givenReactants[i] = data.GetInt(offset++) ?? default(int);
        }

        // Do the same for the products
        for (int i = 0; i < givenProducts.Length; i++) {
            givenProducts[i] = data.GetInt(offset++) ?? default(int);
        }

        // Get the Rate
        givenRate = data.GetInt(offset++) ?? default(int);

        return new ReactionWithOffset(offset, new ChemicalReaction(givenReactants, givenProducts, givenRate, givenSpecies)); 
    }

    public void startOfflineGame() {
        iAmOnline = false;
        iAmHost = true;
        SceneManager.LoadScene("simulationScene");
    }
}

public class RealtimeSessionInfo {
    private string hostURL;
    private string accessToken;
    private int portID;
    private string matchID;
    private List<RealtimePlayer> players = new List<RealtimePlayer>();

    /// Creates a new RTSession object which is held until a new RT session is created
    public RealtimeSessionInfo(MatchFoundMessage message) {
        portID = (int) message.Port;
        hostURL = message.Host;
        accessToken = message.AccessToken;
        matchID = message.MatchId;
        // we loop through each participant and get their peerId and display name
        // Even though we only support 1v1 matches this allows us to not need to change anything to scale up
        foreach (MatchFoundMessage._Participant p in message.Participants) {
            players.Add(new RealtimePlayer(p.DisplayName, p.Id, p.PeerId + ""));
        }
    }

    public string getHostURL() {return this.hostURL;}

    public string getAccessToken() {return this.accessToken;}

    public int getPortID() {return this.portID;}

    public string getMatchID() {return this.matchID;}

    public List<RealtimePlayer> getPlayers() {return this.players;}
}

public class RealtimePlayer {
    private string displayName;
    private string id;
    private string peerId;
    public bool isOnline;

    public RealtimePlayer(string displayName, string id, string peerId) {
        this.displayName = displayName;
        this.id = id;
        this.peerId = peerId;
    }

    public string getDisplayName() {return this.displayName;}

    public string getID() {return this.id;}

    public string getPeerID() {return this.peerId;}
}

// This POJO? POCO? POCSO? is used so we can pass back a reaction we parsed from the data, and the offset after we parsed it
public class ReactionWithOffset {
    public uint offset { get; set; }
    public ChemicalReaction reaction { get; set; }

    public ReactionWithOffset(uint offset, ChemicalReaction reaction) {
        this.offset = offset;
        this.reaction = reaction;
    }
}

// This POCSO? is used to keep the display info for all currently running reactions
public class RunningReaction {
    public string displayName { get; set; }
    public double time { get; set; }

    public RunningReaction(string displayName, double time) {
        this.displayName = displayName;
        this.time = time;
    }
}
