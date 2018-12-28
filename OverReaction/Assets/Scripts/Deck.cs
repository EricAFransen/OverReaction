using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour {

    // Determines whether the deck is bottomless or not
    public bool InfiniteCards = false;
    // Determines whether we run ParserTest or not
    public bool RunTest = false;
    // The prefab that all the cards are based off of
    public GameObject BaseCard;
    // The prefab each text box is based off of
    public GameObject TextBox;

    public bool IsHost = true;

    // The stack that holds all the indexes of the cards to play
    private Stack<int> Indexes;
    // The Card controller for the game
    private ICardController cardController;
    
	// Use this for initialization
	void Start () {
        // TODO: Check if the player is the host

        // Get the card controller from the camera
        cardController = Camera.main.GetComponent<MainController>().CardController;
        // Check if the card controller has been initialized
        cardController.Init();
        // Run the test if true
        if (RunTest) {
            CardFactory Parser = new CardFactory();
            Parser.AutoAddParsers();
            if (!ParserTest.Test(Parser))
                Debug.LogError("ParseTest failed!");
            else
                Debug.Log("Parse test passed!");
        }
        // Initialize the stack
        Indexes = new Stack<int>();
        if (GameSparksManager.instance.iAmHost) {
            // Get the number of cards from the controller
            int max = cardController.GetNumberOfCards();
            // Initialize a list to shuffle
            List<int> temp = new List<int>();
            // Add the indexes of all the cards
            for (int i = 0; i < max; i++) {
                temp.Add(i);
            }
            // Grab the count from the list
            int count = temp.Count;
            // Get the last index
            int last = count - 1;
            // Iterate through the list
            for (int i = 0; i < last; ++i) {
                // Do an in-place shuffle
                int r = Random.Range(i, count);
                int tmp = temp[i];
                temp[i] = temp[r];
                temp[r] = tmp;
            }
            // Add all the indexes to a stack
            foreach (int i in temp) {
                Indexes.Push(i);
            }
        }
        else {
            StartCoroutine(checkForInitializedDeck());
        }
    
    }

    private IEnumerator checkForInitializedDeck() {
        // We use this boolean since we cant yield in a catch statement
        bool failed = false;
        try {
            // Get the number of cards from the controller
            int max = cardController.GetNumberOfCards();
            // Initialize a list to shuffle
            List<int> temp = new List<int>();
            // Add the indexes of all the cards
            for (int i = 0; i < max; i++) {
                temp.Add(i);
            }
            // Grab the count from the list
            int count = temp.Count;
            // Get the last index
            int last = count - 1;
            // Iterate through the list
            for (int i = 0; i < last; ++i) {
                // Do an in-place shuffle
                int r = UnityEngine.Random.Range(i, count);
                int tmp = temp[i];
                temp[i] = temp[r];
                temp[r] = tmp;
            }
            // Add all the indexes to a stack
            foreach (int i in temp) {
                Indexes.Push(i);
            }
        }
        catch (System.NullReferenceException e) {
            // We havent recieved the deck yet, yield and check again
            failed = true;
            Debug.Log("Deck not yet recieved, exception: " + e.ToString());
        }
        // If we failed, wait and check again.
        if (failed) {
            yield return new WaitForSeconds(.25f);
            StartCoroutine(checkForInitializedDeck());
        }
        
    }

    // Called whenever you click on the deck
    void OnMouseUpAsButton()
    {
        // Set the index to a marker value
        int index = -1;
        // Check if the deck is empty
        if (Indexes.Count > 0)
        {
            // Grab the top value and remove it from the stack
            index = Indexes.Pop();
        } else if (InfiniteCards)
        {
            // If we have a bottomless deck, generate a random number
            index = Random.Range(0, cardController.GetNumberOfCards());
        }
        // Make sure the index isn't the marker symbol
        if(index >= 0)
        {
            // Create a copy of the base card
            GameObject card = Object.Instantiate(BaseCard);
            // Set the index in the script
            card.GetComponent<DragDrop>().index = index;

            BoxCollider collider = card.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.06f, 0.01f, 0.07f);
            // Create a copy of the text box
            GameObject Obj = Object.Instantiate(TextBox);
            // Get the actual text from the text box
            TextMesh tm = Obj.GetComponent<TextMesh>();
            // Set the text on the text box
            tm.text = cardController.GetCardDescription(index);
            // Set the text size
            tm.characterSize = 0.1f;
            // Set the font size
            tm.fontSize = 100;
            // Set the parent of the text box to the card
            Obj.transform.parent = card.transform;
            // Adjust the position and the rotation of the text box
            Vector3 position = new Vector3(2.5f, 5, 0);
            Vector3 Euler = new Vector3(0, 180, 0);
            Quaternion rotation = Quaternion.Euler(Euler);
            Obj.transform.SetPositionAndRotation(position, rotation);
            Obj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        }
    }

    public void AddCard(int index)
    {
        // Push the index onto the stack
        Indexes.Push(index);
    }

    public int[] GetIndexes()
    {
        // Return the stack of indexes as an array
        return Indexes.ToArray();
    }

    public void Clear()
    {
        // Clear the indexes
        Indexes.Clear();
    }
}
