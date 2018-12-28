// ====================================================================================================
//
// Cloud Code for ONE_VS_ONE_MATCH, write your code here to customize the GameSparks platform.
//
// For details of the GameSparks Cloud Code API see https://docs.gchrome-extension://mpognobbkildjkofajifpdfhcoklimli/components/startpage/startpage.html?section=Speed-dials&activeSpeedDialIndex=0amesparks.com/
//
// @Author 
// Jess Walters (Jesswalters53@gmail.com)
// ====================================================================================================

// TODO: Switch from turn based to interval based

var hostId = 0;
var isHostTurn = false;
var decks = {};

// this function is called when a player connects to the RT session //
RTSession.onPlayerConnect(function (player) {
    RTSession.getLogger().debug("Player Connected");
    // construct a new account details request with the ID of the player that just connected //
    RTSession.newRequest().createAccountDetailsRequest().setPlayerId(player.getPlayerId()).send(function (response) {
        // construct a new RTData object with the player's display name //
        var rtDataToSend = RTSession.newData().setString(1, response.displayName);
        rtDataToSend.setString(2, player.getPeerId());
        // send the display name to all the players //
        RTSession.newPacket().setOpCode(1).setData(rtDataToSend).send();
    });
});

// handle opcode 10: getHostID
RTSession.onPacket(10, function (packet) {
    RTSession.getLogger().debug("Recieved packet 10");
    var rtData = RTSession.newData(); // create a new RTData object
    if (hostId == 0) { // If no host so far, make this player the host
        hostId = packet.getSender().getPeerId();
        isHostTurn = true; // Set the initial turn to the host
    }
    // Set the retun data as the hostID
    rtData.setNumber(1, hostId);
    // Create a packet to return with response code 11: HostIDResponse
    RTSession.newPacket().setOpCode(11).setTargetPeers([packet.getSender().getPeerId()]).setData(rtData).send();
    RTSession.getLogger().debug("Responded with packet 11");
    });

// OpCode 20 = PlayCardRequest
    RTSession.onPacket(20, function (packet) {
        RTSession.getLogger().debug("Recieved Card Play Request from player");
        var rtData = RTSession.newData();
        rtData.setNumber(1, 0); // Default response as false
        rtData.setNumber(2, packet.getData().getNumber(1)); // Set the 2 index to give the given card pos
        
        var deck = decks[packet.getSender().getPeerId()];
        for (i = 0; i < deck.length; i++) {
            if (deck[i] == packet.getData().getNumber(1)) { // If the card is in the deck then we approve it
                rtData.setNumber(1, 1);
                deck[i] = "";
                // We need to also send another packet to the host that tells it that the opponent is playing this card
                var toHostData = RTSession.newData();
                toHostData.setNumber(1, packet.getData().getNumber(1)); // pass the pos of the card to the host
                toHostData.setString(2, packet.getSender().getPeerId()); // Send the peer id so the host knows who sent it
                RTSession.newPacket().setOpCode(24).setData(toHostData).setTargetPeers([hostId]).send();
            }
        }
        
        decks[packet.getSender().getPeerId()] = deck;
        RTSession.newPacket().setOpCode(21).setTargetPeers([packet.getSender().getPeerId()]).setData(rtData).send();
    });

// Opcode 25 handles the host sending out the initial card decks to the rest of the players.
// We parse the decks and send out the appropriate decks to their players.
    RTSession.onPacket(25, function (packet) {
        RTSession.getLogger().debug("Recieved Initial decks");
        var data = packet.getData();
        var offset = 1;
        var numPlayers = data.getNumber(offset++);
        for (i = 0; i < numPlayers; i++) {
            // Player data
            // Offset used for the packer we are sending to the player
            var playerOffset = 1;
            var playerDataDeck = RTSession.newData();
            var playerCardList = [];
            
            var deckSize = data.getNumber(offset++);
            playerDataDeck.setNumber(playerOffset++, deckSize);
            
            var peerID = data.getString(offset++)
            
            for (j = 0; j < deckSize; j++) {
                // Per card
                var cardString = data.getString(offset++);
                var index = data.getNumber(offset++);
                
                // Add index to list of unplayed cards for the player
                playerCardList.push(index);
                
                // Set data to send to the player
                playerDataDeck.setString(playerOffset++, cardString);
                playerDataDeck.setNumber(playerOffset++, index);
            }
            
            // Store the players card list to the dict for all decks
            decks[peerID] = playerCardList;
            
            // Send the player their deck unless they are host
            if (!isHost(peerID)) {
                RTSession.newPacket().setOpCode(26).setTargetPeers([peerID]).setData(playerDataDeck).send();
                RTSession.getLogger().debug("Sent deck to player: " + peerID);
            }
        }
    });

function isHost(packet) {
    return hostId == packet.getSender().getPeerId();
}

function isHost(peerID){
    // Parse to an int to compare
    return hostId == parseInt(peerID, 10);
}
// a new setInterval callback function //
//RTSession.setInterval(function(){
//    // get the current server-time in milliseconds //
//    var rtData = RTSession.newData().setString(1, new Date().getUTCMilliseconds());
//    RTSession.newPacket().setOpCode(101).setData(rtData).send(); // send the packet to all players
//}, 1000); // set the time interval to be 1 second
