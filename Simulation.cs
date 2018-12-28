using System;
using System.Collections.Generic;
using System.Threading;

namespace Overreaction
{
    class Simulation
    {
        public enum Species
        {
            Red = 0, Blue = 1, x1 = 2, x2 = 3, x3 = 4
        };

        readonly double STD_DEV = 2.0;

        public int[,] updateMatrix;
        private int[] liveSpecies;
        private Random rand;
        private List<ChemicalReaction> reactions;

        public Simulation(int[] species)
        {
            reactions = new List<ChemicalReaction>();

            //Red, blue, x1, x2, x3
            //int[] species = { 10, 10, 0, 0, 0 };
            liveSpecies = species;
            ChemicalReaction.InitReactions(reactions, liveSpecies);
            int numSpecies = liveSpecies.Length;
            rand = new Random();

            updateMatrix = UpdateMatrix(reactions);
        }

        public void updateSimulation()
        {
            updateReactions();
            updateMatrix = UpdateMatrix(reactions);
        }

        public void updateReactions()
        {
            //TODO
        }

        public double randTime(int totalRate)
        {
            double i1 = 1.0 - rand.NextDouble();
            double i2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(i1) * Math.Sin(2.0 * Math.PI * i2));
            return (1 / totalRate) + STD_DEV * randStdNormal;
        }

        /// <summary>
        /// Randomly selects one of the reactions to simulate, then updates the number of species in the system
        /// </summary>
        /// <returns>True if a reaction was calculated, false if otherwise</returns>
        public bool StepSimulate()
        {
            int numSpecies = liveSpecies;

            int totalRate = CalculateTotalRate(reactions);
            if(totalRate > 0)
            {
                int next = WeightedRandom(reactions, rand, totalRate);
                if (reactions[next].RateExpression() > 0)
                {

                    double time = randTime(totalRate);
                    Thread.Sleep(time);
                    for (int i = 0; i < liveSpecies.Length; i++)
                    {
                        liveSpecies[i] = liveSpecies[i] + updateMatrix[next, i];
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Simulates multiple reactions starting from the call of the method, to the end time
        /// Note: does not run in realtime
        /// </summary>
        /// <param name="endTime">How long reactions should be simulated for</param>
        /// <returns>True if reactions were simulated, false if otherwise</returns>
        public bool TimeSimulate(double endTime)
        {
            int numSpecies = liveSpecies.Length;

            updateMatrix = UpdateMatrix(reactions);
            double time = 0.0;

            /*
            Console.WriteLine("Species Count:");
            Console.WriteLine("Red: " + liveSpecies[(int)Species.Red]);
            Console.WriteLine("Blue: " + liveSpecies[(int)Species.Blue]);
            Console.WriteLine("x1: " + liveSpecies[(int)Species.x1]);
            Console.WriteLine("x2: " + liveSpecies[(int)Species.x2]);
            Console.WriteLine("x3: " + liveSpecies[(int)Species.x3]);
            Console.WriteLine();
            */

            while (time < endTime)
            {
                int totalRate = CalculateTotalRate(reactions);
                if (totalRate != 0)
                {
                    int next = WeightedRandom(reactions, rand, totalRate);
                    if (reactions[next].RateExpression() > 0)
                    {
                        time += randTime(totalRate);
                        for (int i = 0; i < SPECIES_NUM; i++)
                        {
                            liveSpecies[i] = liveSpecies[i] + updateMatrix[next, i];
                        }

                        /*
                        Console.WriteLine("At time " + time + " ms the reaction: " + reactions[next].Equation() + " occurred");
                        Console.WriteLine("Species Count:");
                        Console.WriteLine("Red: " + liveSpecies[(int)Species.Red]);
                        Console.WriteLine("Blue: " + liveSpecies[(int)Species.Blue]);
                        Console.WriteLine("x1: " + liveSpecies[(int)Species.x1]);
                        Console.WriteLine("x2: " + liveSpecies[(int)Species.x2]);
                        Console.WriteLine("x3: " + liveSpecies[(int)Species.x3]);
                        Console.WriteLine();
                        */
                    }
                }
                else
                {
                    Console.WriteLine("No more reactions are possible.");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the total rate of the entire system.
        /// </summary>
        /// <param name="reactions">A list of the reactions in the system</param>
        /// <returns>The total rate of reaction in the entire system, equal to the sum of each reaction's rate</returns>
        int CalculateTotalRate(List<ChemicalReaction> reactions)
        {
            int totalRate = 0;

            foreach (ChemicalReaction r in reactions)
            {
                totalRate += r.RateExpression();
            }

            return totalRate;
        }

        /// <summary>
        /// Chooses a random number weighted on the proprotion of each reaction's rate
        /// </summary>
        /// <param name="reactions">A list of reactions in the system</param>
        /// <param name="rand">A random object to be used to generate the random weighted number</param>
        /// <param name="totalRate">The total rate of the entire system</param>
        /// <returns>A weighted random integer between 0 and number of reactions-1</returns>
        int WeightedRandom(List<ChemicalReaction> reactions, Random rand, double totalRate)
        {
            double choice = 0.0;

            double roll = rand.NextDouble();

            for (int i = 0; i < reactions.Count; i++)
            {
                //P(reaction[i]) = rate of formation / total rate of formation
                //E.X. P(Red + Blue -> x1) = |Red|*|Blue|*k / Rtot
                choice += (reactions[i].RateExpression() / totalRate);

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
            int[,] retMatrix = new int[reactions.Count, SPECIES_NUM];

            for (int i = 0; i < reactions.Count; i++)
            {
                ChemicalReaction chem = reactions[i];
                for (int j = 0; j < chem.Reactants.Length; j++)
                {
                    retMatrix[i, j] -= chem.Reactants[j] * chem.Rate;
                }

                for (int j = 0; j < chem.Products.Length; j++)
                {
                    retMatrix[i, j] += chem.Products[j] * chem.Rate;
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
            Console.Out.WriteLine("Created a new reaction:\n" + reaction.Equation());
            reactions.Add(reaction);
            updateMatrix = UpdateMatrix(reactions);
        }
        public void AddReaction(ChemicalReaction reaction)
        {
            reactions.Add(reaction);
            updateMatrix = UpdateMatrix(reactions);
        }
        public String ListReactions()
        {
            String list = "";
            for(int i = 0; i < reactions.Count; i++)
            {
                ChemicalReaction reaction = reactions[i];
                list += i.ToString() + ": " + reaction.Equation() + "\n";
            }
            return list;
        }

        public void ChangeReactionRate(int index, int newRate)
        {
            reactions[index].Rate = newRate;
        }

        public int GetNumReactions()
        {
            return reactions.Count;
        }
    }
}
