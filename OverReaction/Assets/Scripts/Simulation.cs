using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Simulation : MonoBehaviour
{
    public enum Species
    {
        Red = 0, Blue = 1, x1 = 2, x2 = 3, x3 = 4
    };

    private readonly double TIME_SCALAR = 100.0;
    private readonly int MODIFIER_BASE = -1;

    public int[,] updateMatrix;
    private int[] liveSpecies;
    private double totalRate;
    private List<ChemicalReaction> reactions;
    private Card[] hand;
    private System.Random rand;
    private float simTime;
    public double cardTime;

    //public int Modifier;
    //public double ModifierRate;
    // List of Reactions Running (This is for when we are not the host)
    private List<RunningReaction> runningReactions;

    private GameSparksManager gameSparksManager;
    private bool iAmHost;
    private bool iAmOnline;

    public double Modifier;
    private RateModifierCard.ModifierType CurrentModifier;


    private GameObject[] reactionDisplay;
    private GameObject[] reactionDisplayText;

    //true = red, false = blue
    private bool turn;
    //0 = no win, 1 = red win, 2 = blue win
    private int win;

    //list of available chemical reactions
    List<ChemicalReaction> crList;
    List<ChemicalReaction> deck;

    //beaker
    Beaker beaker;
    private float networkUpdateRate;

    public bool GetIsRedTurn()
    {
        return turn;
    }

    public bool GetIsBlueTurn()
    {
        return !turn;
    }

    private void Start()
    {
        // get the networking info ro see if we are online or offline
        gameSparksManager = GameSparksManager.instance;
        iAmHost = gameSparksManager.iAmHost;
        iAmOnline = gameSparksManager.iAmOnline;

        initSimulation();

        updateDisplay();

        // If we are the host we send an update to the non host players
        if (iAmHost && iAmOnline) {
            StartCoroutine(sendGameState());
        }
    }

    private void initSimulation() {
        //initialize simulation
        reactions = new List<ChemicalReaction>();
        liveSpecies = new int[] { 100, 100, 30, 30, 30 };
        reactionDisplay = new GameObject[5];
        reactionDisplayText = new GameObject[5];
        rand = new System.Random();
        updateMatrix = UpdateMatrix(reactions);
        int numSpecies = liveSpecies.Length;
        runningReactions = new List<RunningReaction>();
        networkUpdateRate = 0.1f; // Makes the network update rate 33fps (1 sec / .03 sec)


        //creates the beaker contents
        beaker = new Beaker(liveSpecies);
        CurrentModifier = RateModifierCard.ModifierType.Unhandled;
        Modifier = 0;

        //Modifier = MODIFIER_BASE;



        for (int i = 0; i < reactionDisplay.Length; i++) {
            reactionDisplay[i] = GameObject.Find("RunningReact" + i);
            reactionDisplayText[i] = GameObject.Find("RunningReact3DText" + i);

            reactionDisplayText[i].GetComponent<TextMesh>().text = "";

            reactionDisplayText[i].transform.position = reactionDisplay[i].transform.position;
            reactionDisplayText[i].transform.position = new Vector3(reactionDisplayText[i].transform.position.x + 5, reactionDisplayText[i].transform.position.y, reactionDisplayText[i].transform.position.z);


            reactionDisplay[i].SetActive(false);
            reactionDisplayText[i].SetActive(false);
        }

        hand = new Card[5];
        for (int i = 0; i < 5; i++) {
            hand[i] = null;
        }

        CurrentModifier = RateModifierCard.ModifierType.Unhandled;

        //initializes the reactions 
        /*//Decremented button cards
        deck = new ChemicalReaction(rand, 1, getLiveSpecies()).getDeck(liveSpecies);
        crList = new List<ChemicalReaction>();
        for (int i = 0; i < 5; i++)
        {
            int j = rand.Next() % deck.Count;
            crList.Add(deck[j]);
            deck.RemoveAt(j);
            GameObject.Find("Option" + i.ToString()).GetComponentInChildren<Text>().text = crList[i].EquationMinimal();
        }
        */
        cardTime = Time.fixedUnscaledTime;
        turn = true;
        win = 0;
    }

    private void Update()
    {
        if ((CurrentModifier != RateModifierCard.ModifierType.Unhandled) && Input.GetMouseButtonDown(0))
        {
            GameObject b0 = GameObject.Find("RunningReact0");
            //b0.SetActive(true);
            GameObject b1 = GameObject.Find("RunningReact1");
            //b1.SetActive(true);
            GameObject b2 = GameObject.Find("RunningReact2");
            //b2.SetActive(true);
            GameObject b3 = GameObject.Find("RunningReact3");
            //b3.SetActive(true);
            GameObject b4 = GameObject.Find("RunningReact4");
            //b4.SetActive(true);
            //Physics.Raycast(ray.origin, ray.direction, 100f)
            Debug.Log("Click");

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane dPlane0 = new Plane(), dPlane1 = new Plane(), dPlane2 = new Plane(), dPlane3 = new Plane(), dPlane4 = new Plane();
            if (b0 != null)
                dPlane0 = new Plane(Vector3.forward, b0.transform.position);
            if (b1 != null)
                dPlane1 = new Plane(Vector3.forward, b1.transform.position);
            if (b2 != null)
                dPlane2 = new Plane(Vector3.forward, b2.transform.position);
            if (b3 != null)
                dPlane3 = new Plane(Vector3.forward, b3.transform.position);
            if (b4 != null)
                dPlane4 = new Plane(Vector3.forward, b4.transform.position);
            float distance = 0;
            if (dPlane0.Raycast(ray, out distance))
            {
                /*
                Debug.Log("a");
                reactions[0].multRate(ModifierRate);
                Modifier = -1;
                ModifierRate = 0;*/
                ApplyModifier(0);
            }
            else if (dPlane1.Raycast(ray, out distance))
            {
                ApplyModifier(1);
            }
            else if (dPlane2.Raycast(ray, out distance))
            {
                ApplyModifier(2);
            }
            else if (dPlane3.Raycast(ray, out distance))
            {
                ApplyModifier(3);
            }
            else if (dPlane4.Raycast(ray, out distance))
            {
                ApplyModifier(4);
            }

        }
        beaker.Update();
        

        // If we are not the host in an online game we do not want to do the simulation steps
        if (iAmHost || !iAmOnline) {
            if (NextReactionOccurs() && (win == 0)) {
                //Debug.Log("react");
                UpdateSimulation();
                bool didSimulate = StepSimulate();
                //only update if a reaction occurred
                if (didSimulate)
                {
                    beaker.updateColor(liveSpecies);
                    simTime = Time.unscaledTime;
                }
            }
            updateCardTime();
            checkWin();
        }

        updateDisplay();
        //Debug.Log(String.Join(",", new List<int>(liveSpecies).ConvertAll(i => i.ToString()).ToArray()));

        //Keep track of the last time a reaction occurred, mainly for NextReactionOccurs
        //simTime = Time.unscaledTime;
    }

    /// <summary>
    /// Returns true if another chemical reaction should occur based on the
    /// probability.
    /// </summary>
    /// <returns>True if reaction should occur, false if otherwise</returns>
    public bool NextReactionOccurs()
    {
        //Get a random number [0, 1)
        double r = rand.NextDouble();
        //How much time has passed since last reaction
        double time = Time.unscaledTime - simTime;
        //What is the probability that another reaction occurs
        //As time goes on higher chance that a reaction will occur
        double prob = 1 - Math.Exp(-totalRate * (time));
        //Debug.Log("r: " + r + ", prob: " + prob + ", time: " + time);
        //Did we choose a random number that satisfies the probability?
        //r should be evenly distributed
        return (r <= (prob/TIME_SCALAR));
    }

    public double RandTime(int totalRate)
    {
        if (totalRate <= 0)
        {
            return 0.1;
        }

        double r = rand.NextDouble();
        double ret = Math.Log(1 - r) / (-(double)totalRate);
        Debug.Log("ret: " + ret);
        return ret;
    }

    /// <summary>
    /// Updates the UpdateMatrix and TotalRate since they may have been changed since last call
    /// </summary>
    public void UpdateSimulation()
    {
        updateMatrix = UpdateMatrix(reactions);
        CalculateTotalRate(reactions);
    }

    /// <summary>
    /// Randomly selects one of the reactions to simulate, then updates the number of species in the system
    /// </summary>
    /// <returns>True if a reaction was calculated, false if otherwise</returns>
    public bool StepSimulate()
    {
        if (reactions.Count == 0) return false;
        int numSpecies = liveSpecies.Length;

        //Which reaction should occur
        int next = WeightedRandom(rand, totalRate);

        //If there are no reactions, then no reaction can occur
        if (totalRate > 0)
        {
            //If the reaction that was generated has none in the system, then can't run
            //Shouldn't happen because it would have a 0 probability.
            if (reactions[next].RateExpression() > 0)
            {
                for (int i = 0; i < liveSpecies.Length; i++)
                {
                    liveSpecies[i] = liveSpecies[i] + updateMatrix[next, i];
                }
            }

        }
        else
        {
            //Degrading all reactions mean multiple may be needed to remove
            return false;
        }
        return true;
    }

    /// <summary>
    /// Returns the total rate of the entire system.
    /// </summary>
    /// <param name="reactions">A list of the reactions in the system</param>
    /// <returns>The total rate of reaction in the entire system, equal to the sum of each reaction's rate</returns>
    void CalculateTotalRate(List<ChemicalReaction> reactions)
    {
        totalRate = 0;
        //Debug.Log("rc: "+reactions.Count);
        foreach (ChemicalReaction r in reactions)
        {
            totalRate += r.RateExpression();
            //Debug.Log("r: " + r.RateExpression());
        }
        //Debug.Log("the last rate expressiont is: " + totalRate);

        //totalRate now equlas total
    }

    /// <summary>
    /// Chooses a random number weighted on the proprotion of each reaction's rate
    /// </summary>
    /// <param name="reactions">A list of reactions in the system</param>
    /// <param name="rand">A random object to be used to generate the random weighted number</param>
    /// <param name="totalRate">The total rate of the entire system</param>
    /// <returns>A weighted random integer between 0 and number of reactions-1</returns>
    int WeightedRandom(System.Random rand, double totalRate)
    {
        double choice = 0.0;

        //Random number to find which "bucket" it falls in
        double roll = rand.NextDouble();

        for (int i = 0; i < reactions.Count; i++)
        {
            //P(reaction[i]) = rate of formation / total rate of formation
            //E.X. P(Red + Blue -> x1) = |Red|*|Blue|*k / Rtot
            choice += reactions[i].RateExpression() / totalRate;

            //Is the random number within the range of reaction i?
            //Then i is the one that occurs 
            if (roll < choice)
            {
                return i;
            }
        }

        return 0;
    }

    /// <summary>
    /// Returns the update matrix for the system's reactions
    /// </summary>
    /// <param name="reactions">List of the reactions within the system</param>
    /// <returns>A 2-dimensional matrix, the rows represent a reaction and the columns reprsent the species. 
    /// A positive number in the location row, col means the reaction index == row adds that many of the given col species.
    /// A negative number removes that many of the given speices.</returns>
    int[,] UpdateMatrix(List<ChemicalReaction> reactions)
    {
        int[,] retMatrix = new int[reactions.Count, liveSpecies.Length];

        for (int i = 0; i < reactions.Count; i++)
        {
            ChemicalReaction chem = reactions[i];
            for (int j = 0; j < chem.reactants.Length; j++)
            {
                retMatrix[i, j] -= chem.reactants[j] * chem.rate;
            }

            for (int j = 0; j < chem.products.Length; j++)
            {
                retMatrix[i, j] += chem.products[j] * chem.rate;
            }
        }
        return retMatrix;
    }

    public void AddRandomReaction()
    {
        int numSpecies = liveSpecies.Length;
        int numReactants = (rand.Next() % numSpecies) + 1;
        int numProducts = (rand.Next() % 2) + 1;
        int[] reactants = new int[numSpecies];
        int[] products = new int[numSpecies];
        for(int i = 0; i < numReactants; i++)
        {
            int reactant = rand.Next() % numSpecies;
            reactants[reactant]++;
        }
        for (int i = 0; i < numProducts; i++)
        {
            int product = rand.Next() % numSpecies;
            products[product]++;
        }
        int rate = (rand.Next() % 5)+1;
        ChemicalReaction reaction = new ChemicalReaction(reactants, products, rate, liveSpecies);
        Debug.Log("Created a new reaction:\n" + reaction.Equation()); 
        reactions.Add(reaction);
        updateMatrix = UpdateMatrix(reactions);
    }

    public void AddReaction(ChemicalReaction reaction)
    {
        simTime = Time.unscaledTime;
        reactions.Add(reaction);
        UpdateSimulation();
    }

    public void RemoveReaction(ChemicalReaction reaction)
    {
        simTime = Time.unscaledTime;
        reactions.Remove(reaction);
        UpdateSimulation();
    }

    public String ListReactions()
    {
        String list = "";
        int i = 1;
        foreach(ChemicalReaction r in reactions)
        {
            list += i.ToString() + ": " + r.EquationMinimal() + " " + string.Format("{0:0.00}", r.time) + "s\n";
            i++;
        }
        return list;
    }

    public void ChangeReactionRate(int index, int newRate)
    {
        reactions[index].rate = newRate;
    }

    public int GetNumReactions()
    {
        return reactions.Count;
    }

    public int[] getLiveSpecies()
    {
        return liveSpecies;
    }

    public void printLiveSpecies()
    {
        Debug.Log("Species Count:");
        Debug.Log("Red: " + liveSpecies[(int)Species.Red]);
        Debug.Log("Blue: " + liveSpecies[(int)Species.Blue]);
        Debug.Log("x1: " + liveSpecies[(int)Species.x1]);
        Debug.Log("x2: " + liveSpecies[(int)Species.x2]);
        Debug.Log("x3: " + liveSpecies[(int)Species.x3]);
        Debug.Log("\n");
    }

    /// <summary>
    /// Updates the text display
    /// </summary>
    public void updateDisplay()
    {
        Text[] species = new Text[5];
        species[0] = GameObject.Find("Red").GetComponent<Text>();
        species[0].text = "Red: " + liveSpecies[0];
        species[1] = GameObject.Find("Blue").GetComponent<Text>();
        species[1].text = "Blue: " + liveSpecies[1];
        species[2] = GameObject.Find("X1").GetComponent<Text>();
        species[2].text = "X1: " + liveSpecies[2];
        species[3] = GameObject.Find("X2").GetComponent<Text>();
        species[3].text = "X2: " + liveSpecies[3];
        species[4] = GameObject.Find("X3").GetComponent<Text>();
        species[4].text = "X3: " + liveSpecies[4];

        //GameObject.Find("RunningReactions").GetComponent<Text>().text = ListReactions();

        if (iAmHost || !iAmOnline) {
            /**/
            for (int i = 0; i < 5; i++) {
                //Debug.Log(i+":");
                if (reactions.Count > i) {
                    reactionDisplayText[i].GetComponent<TextMesh>().text = reactions[i].EquationMinimal() + "\n" + string.Format("{0:0.00}", reactions[i].time);
                    if (CurrentModifier != RateModifierCard.ModifierType.Unhandled) {
                        //Debug.Log("asdf");
                        reactionDisplayText[i].GetComponent<TextMesh>().color = Color.blue;
                    }
                    else {
                        reactionDisplayText[i].GetComponent<TextMesh>().color = Color.white;
                    }
                    reactionDisplay[i].SetActive(true);
                    reactionDisplayText[i].SetActive(true);

                }
                else {
                    //Debug.Log("out");
                    reactionDisplay[i].SetActive(false);
                    reactionDisplayText[i].SetActive(false);
                }
            }
        }
        else { // If we are not the host we store reactions by name and time left only
               /**/
            for (int i = 0; i < 5; i++) {
                //Debug.Log(i+":");
                if (runningReactions.Count > i) {
                    reactionDisplayText[i].GetComponent<TextMesh>().text = runningReactions[i].displayName + "\n" + string.Format("{0:0.00}", runningReactions[i].time);
                    if (Modifier != -1) {
                        //Debug.Log("asdf");
                        reactionDisplayText[i].GetComponent<TextMesh>().color = Color.blue;
                    }
                    else {
                        reactionDisplayText[i].GetComponent<TextMesh>().color = Color.white;
                    }
                    reactionDisplay[i].SetActive(true);
                    reactionDisplayText[i].SetActive(true);

                }
                else {
                    //Debug.Log("out");
                    reactionDisplay[i].SetActive(false);
                    reactionDisplayText[i].SetActive(false);
                }
            }
        }
        


        if (win != 0)
        {
            if (win == 1)
                GameObject.Find("Turn").GetComponent<Text>().text = "Red win";
            if (win == 2)
                GameObject.Find("Turn").GetComponent<Text>().text = "Blue win";
        }
        else if (turn)
        {
            GameObject.Find("Turn").GetComponent<Text>().text = "Red Turn";
        }
        else
        {
            GameObject.Find("Turn").GetComponent<Text>().text = "Blue Turn";
        }
        

    }

    public void updateCardTime()
    {
        double deltaTime = Time.fixedUnscaledTime - cardTime;
        cardTime = Time.fixedUnscaledTime;

        //pass time
        foreach(ChemicalReaction r in reactions)
        {
            r.passTime(deltaTime);
        }

        //remove reaction if expired
        for(int i= reactions.Count-1; i>=0; i--)
        {
            if (reactions[i].isExpired()) reactions.Remove(reactions[i]);
        }

        updateMatrix = UpdateMatrix(reactions);
    }

    //Currently depricated
    public void playCard(int i)
    {
        
        if (crList.Count < i) return;
        else if (crList.Count == 0) return;

        turn = !turn;

        if (GetNumReactions() >= 5)
        {
            
            return;
        }


        bool shouldRemove = false;
        for(int item = 0; item < reactions.Count; item++)
        {
            if (reactions[item].Equals(crList[i]))
                shouldRemove = true;
        }

        if(shouldRemove)
        {
            //Debug.Log("removed: " + crList[i].EquationMinimal());
            RemoveReaction(crList[i]);
        }
        else
        {
            AddReaction(crList[i]);
        }

        crList.RemoveAt(i);

        if (deck.Count == 0)
        {
            for(int j=0; j<5; j++)
            {
                if(j < crList.Count)
                    GameObject.Find("Option" + j.ToString()).GetComponentInChildren<Text>().text = crList[j].EquationMinimal();
                else
                    GameObject.Find("Option" + j.ToString()).GetComponentInChildren<Text>().text = "";
            }
            if (crList.Count == 0)
            {
                if (liveSpecies[0] > liveSpecies[1]) win = 1;
                else win = 2;
            }
            
            return;
        }
        //crList[i] = new ChemicalReaction(rand, 1, liveSpecies);
        int t = rand.Next(deck.Count);
        crList.Add(deck[t]);
        deck.RemoveAt(t);

        //GameObject.Find("Option" + i.ToString()).GetComponentInChildren<Text>().text = crList[i].EquationMinimal();

        

        for (int j = 0; j < 5; j++)
        {
            if (crList.Count>j)
                GameObject.Find("Option" + j.ToString()).GetComponentInChildren<Text>().text = crList[j].EquationMinimal();
            else
                GameObject.Find("Option" + j.ToString()).GetComponentInChildren<Text>().text = "";
        }

        //New reactions, so update the simulation
        UpdateSimulation();
    }

    //stores references to cards - never used
    public int AddToHand(Card c)
    {
        //TODO - delete if I don't end up using this
        for(int i=0; i<hand.Length; i++)
        {
            if(hand[i] != null)
            {
                hand[i] = c;
                return 0;
            }
        }

        //Hand full
        return -1;
    }


    public void checkWin()
    {
        if ((liveSpecies[0] >= 170) && (win == 0))
        {
            win = 1;
        }
        else if ((liveSpecies[1] >= 170) && (win == 0))
        {
            win = 2;
        }
        //Temporary, prevents winning for testing purposes 
        win = 0;
    }

    /// <summary>
    /// Sets the liveSpecies array of the simulation to the given array.
    /// </summary>
    /// <param name="newSpecies">An array list that liveSpecies' elements will be set to</param>
    /// <returns>true if it sets liveSpecies to be equal to newSpecies, false otherwise</returns>
    public bool SetLiveSpecies(int[] newSpecies)
    {
        if (newSpecies.Length == liveSpecies.Length)
        {
            for (int i = 0; i < liveSpecies.Length; i++)
            {
                liveSpecies[i] = newSpecies[i];
            }
        }
        return false;
    }
    public void setSpecies(int[] newQuantities) {
        liveSpecies = newQuantities;
    }

    public void setRunningReactions(List<RunningReaction> runningReactions) {
        this.runningReactions = runningReactions;
    }

    private IEnumerator sendGameState() {
        if (iAmHost) {
            gameSparksManager.updateGameState(liveSpecies, reactions);
        }

        yield return new WaitForSeconds(networkUpdateRate);
        StartCoroutine(sendGameState());
    }

    public ChemicalReaction[] GetReactions()
    {
        return reactions.ToArray();
    }

    public ChemicalReaction RemoveReaction(int index)
    {
        if (index < 0 || index >= reactions.Count) return null;
        ChemicalReaction reaction = reactions[index];
        RemoveReaction(reaction);
        return reaction;
    }

    public int GetReactionIndex(ChemicalReaction reaction)
    {
        return reactions.IndexOf(reaction);
    }

    public int SetUpModifier(RateModifierCard.ModifierType Type, double m)
    {
        Debug.Log("Modifier played");
        if (CurrentModifier == RateModifierCard.ModifierType.Unhandled)
        {
            Debug.Log("Valid Modifier");
            CurrentModifier = Type;
            Modifier = m;
            return 0;
        }
        
        //Modifier already prepared
        return -1;
    }

    public int ApplyModifier(int i)
    {
        switch (CurrentModifier)
        {
            case RateModifierCard.ModifierType.ReactionRate:
                Debug.Log("Rate change, Modifier: " + Modifier);
                reactions[i].rate = (int)(reactions[i].rate * Modifier);
                
                break;
            case RateModifierCard.ModifierType.ReactionTime:
                Debug.Log("Time change, Modifier: " + Modifier);
                reactions[i].time = reactions[i].time * Modifier;
                break;
            case RateModifierCard.ModifierType.ReactionReactants:
                Debug.Log("Reactants change, Modifier: " + Modifier);
                for (int j = 0; j < reactions[i].reactants.Length; j++)
                    reactions[i].reactants[j] = (int)(reactions[i].reactants[j] * Modifier);
                break;
            case RateModifierCard.ModifierType.ReactionProducts:
                Debug.Log("Products change, Modifier: " + Modifier);
                for (int j = 0; j < reactions[i].products.Length; j++)
                    reactions[i].products[j] = (int)(reactions[i].products[j] * Modifier);
                break;
            default:
                //do nothing
                break;
        }
        CurrentModifier = RateModifierCard.ModifierType.Unhandled;
        return 0;
    }
}

