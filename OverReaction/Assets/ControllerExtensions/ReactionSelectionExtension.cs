using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class ReactionSelectionExtension : IControllerExtension
{
    public void Update(double TimeElapsed)
    {

    }

    public int SelectReaction(Simulation sim)
    {
        int selection, size;
        size = sim.GetNumReactions();
        selection = UnityEngine.Random.Range(0, size - 1);
        Debug.Log("Selected " + selection);

        
        return selection;
    }

    //0 = passed
    //-1 = failed
    public int SelectReaction(Simulation sim, GeneralModifierCard.ModifierType Type, double Modifier)
    {
        return sim.SetUpModifier(Type, Modifier);
    }
}
