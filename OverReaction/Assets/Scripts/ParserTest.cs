﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

class ParserTest
{
    public static bool Test(CardFactory factory)
    {
        Debug.Log("Running Parser Test..."); // Make sure we know the test is being run
        bool result = true; // How we tell if the test passes
        string c1s = "0,1,"; // Default card, cost = 1
        string c2s = "1,1,3,1,1,0,1,2,0,0,"; // ReactionCard, cost = 1, numSpecies = 3, rate = 1, reactants = 1,0,1, products = 2,0,0
        string c3s = "1,1,1,1,1,1,"; // Reaction card, cost = 1, numSpecies = 1, rate = 1, reactants = 1, products = 1
        string c4s = "1,2,3,4,5,6,7,8,9,10,"; // Reaction card, cost = 2, numSpecies = 3, reactants = 4,5,6, products = 7,8,9
        string c5s = "2,1,3,4,"; // RateModifier card, cost = 1, numerator = 3, denominator = 4
        ICard c1 = factory.ParseLine(c1s); // Parse the first string
        if (!c1.GetCSV().Equals(c1s)) // Check if the resulting data string matches the original
        {
            Debug.LogWarning("c1 did not match c1s\nc1s: " + c1s + "\nc1: " + c1.GetCSV());
            result = false;
        }
        ICard c2 = factory.ParseLine(c2s); // Parse the second string
        if (!c2.GetCSV().Equals(c2s)) { // Make sure the resulting data string matches the original
            Debug.LogWarning("c2 did not match c2s\nc2s: " + c2s + "\nc2: " + c2.GetCSV());
            result = false;
        }
        ICard c3 = factory.ParseLine(c3s); // More of the same
        if (!c3.GetCSV().Equals(c3s))
        {
            Debug.LogWarning("c3 did not match c3s\nc3s: " + c3s + "\nc3: " + c3.GetCSV());
            result = false;
        }
        ICard c4 = factory.ParseLine(c4s); // More of the same
        if (!c4.GetCSV().Equals(c4s))
        {
            Debug.LogWarning("c4 did not match c4s\nc4s: " + c4s + "\nc4: " + c4.GetCSV());
            result = false;
        }
        ICard c5 = factory.ParseLine(c5s); // More of the same
        if (!c5.GetCSV().Equals(c5s))
        {
            Debug.LogWarning("c5 did not match c5s\nc5s: " + c5s + "\nc5: " + c5.GetCSV());
            result = false;
        }
        string[] strings = { c1s, c2s, c3s, c4s, c5s }; // Put all the source strings in an array
        string deckString = string.Join("\n", strings); // Combine all the strings with newline characters
        List<ICard> cards = factory.ParseDeck(deckString, false); // Parse an entire deck
        for(int i = 0; i < strings.Length; i++) // Go through the list
        {
            string source = strings[strings.Length - (i + 1)]; // Find the source
            string parsed = cards[i].GetCSV(); // Find the card
            if(!source.Equals(parsed)) // Check if they match
            {
                Debug.LogWarning("Card did not match\nParsed: " + parsed + "\nSource: " + source);
                result = false;
            }
        }
        return result; // return the result
    }
}

