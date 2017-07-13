using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovNameGenerator
{
    /// <summary>
    /// A random name generator that uses a Markov process to generate the names
    /// </summary>
    public class NameGenerator
    {
        // A Dictionary to store the 
        private Dictionary<Char, Letter> letters;

        /// <summary>
        /// A constructor to create an instance of a name generator
        /// </summary>
        public NameGenerator()
        {
            this.clear();
        }

        /// <summary>
        /// A constructor to create a name generator and automatically load in names.
        /// </summary>
        /// <param name="names">An IEnumerable of names to generate names with</param>
        public NameGenerator(IEnumerable<String> names) : this()
        {
            LoadNames(names);
        }

        /// <summary>
        /// Load in names to be used to generate new names with.
        /// </summary>
        /// <param name="names">names to be loaded in</param>
        public void LoadNames(IEnumerable<String> names)
        {
            // Go through each name
            foreach (String nameString in names)
            {
                String name = nameString.ToUpper();
                // Start on the break character
                char currentChar = '\n';
                char nextChar = '\n';

                // Go through each letter of the name starting with the break character
                for (int i = 0; i < name.Length; i++)
                {
                    // Get the next char
                    if (i == name.Length - 1)
                    {
                        // There is no next character and so the next character will be the 
                        // break char
                        nextChar = '\n';
                    }
                    else
                    { 
                        // There is another character and so we set it
                        nextChar = name[i];
                    }
                    // Notify the letter that there has been another occurance
                    letters[currentChar].AddOccurance(nextChar);

                    // Move the current char ahead to the next letter
                    currentChar = nextChar;
                }
            }
        }

        /// <summary>
        /// Generates a random name based on the names loaded in.  It always ends in a newline 
        /// character
        /// </summary>
        /// <returns>A name ending with a newline character</returns>
        public String GenerateName()
        {
            // Form String
            String result = "";

            char newLetter = '\n';

            // Keep getting new letters until we get an newline character
            while (true)
            {
                // generate the next letter
                newLetter = letters[newLetter].NextLetter();

                // add it to the string builder
                result += newLetter;

                // Check if we are done
                if (newLetter.Equals('\n'))
                {
                    return result.ToString();
                }
            }

            // there is a problem if we got here.
            throw new Exception("Something went wrong while generating a name.  Somehow it broke" 
                + " out of the loop!");
        }

        /// <summary>
        /// Generates a bunch of names.
        /// </summary>
        /// <param name="NumOfNames">The number of names desired</param>
        /// <returns>An IEnumerable with a bunch of names in it</returns>
        public IEnumerable<String> GenerateNames(int NumOfNames)
        {
            List<String> result = new List<string>();

            for(int i = 0; i < NumOfNames; i++)
            {
                result.Add(GenerateName());
            }

            return result;
        }

        /// <summary>
        /// Clears this name generator.  Setting everything back to the default.
        /// </summary>
        public void clear()
        {
            letters = new Dictionary<char, Letter>();

            //Fill letters with letters
            for (int i = 65; i <= 90; i++)
            {
                Letter currentLetter = new Letter((char)i);
                letters.Add(currentLetter.Character, currentLetter);
            }

            // Add in a starting character to determine what letter to start with
            Letter BreakLetter = new Letter('\n');
            letters.Add(BreakLetter.Character, BreakLetter);
        }

        /// <summary>
        /// A private helper class to store all the info that goes along with each letter
        /// </summary>
        private class Letter
        {
            // An array of percentages.  0 represents 'a', 25 represents 'z', 26 represents '\n'
            private Percent[] percentages;
            Random rando;

            // The character that this letter represents
            public char Character
            {
                get;

                private set;
            }

            /// <summary>
            /// Constructor for creating a letter
            /// </summary>
            /// <param name="c">The character that this letter represents</param>
            public Letter(char c)
            {
                Character = c;

                percentages = new Percent[27];

                rando = new Random();

                // Fill percentages with percentages.  Each letter starts with 0% chance.
                for (int i = 0; i <= 25; i++)
                {
                    percentages[i] = new Percent();
                }

                // The ending character starts with 100% chance of being the next character.
                percentages[26] = new Percent(1, 0, 0);
            }

            /// <summary>
            /// Called for when there is another instance of this character.  Updates the totals
            /// for this letter and the character it goes to.
            /// </summary>
            /// <param name="c">The character that follows this one</param>
            public void AddOccurance(char c)
            {
                // Find the location of the next character
                int location = c - 65;

                // Go through each and update them
                for(int i = 0; i <= 26; i++)
                {
                    // If the location is the given character it is a success
                    if (i == location)
                    {
                        percentages[i].successfulEvent();

                    // Otherwise it was unsuccessful
                    } else
                    {
                        percentages[i].unsuccessfulEvent();
                    }
                }
            }

            /// <summary>
            /// Returns what letter should come after this one.  Random based on the probability
            /// distribution of this letter.
            /// </summary>
            /// <returns>The next letter of the name</returns>
            public char NextLetter()
            {
                // a randomly choosen double in [0,1], and a sum to keep track of when to return
                double RanNum = rando.NextDouble();
                double currentSum = 0;

                // Go through each entry in percentages.  If RanNum is in the current range return
                // that letter.
                for (int i = 0; i < percentages.Count() - 1; i++)
                {
                    currentSum += percentages[i].percentage;

                    if (RanNum < currentSum)
                    {
                        return (char)(i + 'A');
                    }
                }

                return '\n';
            }

            /// <summary>
            /// Internal class to act like a percentage.  The number of successfull events and 
            /// total can be changed to alter the value.
            /// </summary>
            internal class Percent
            {
                public double percentage;
                public int total;
                public int successes;

                /// <summary>
                /// Creates a percentage.  Percentage must be in [0,1], total and successes must 
                /// be positive, and successes must be less than total.  If invalid throws an
                /// ArgumentException
                /// </summary>
                /// <param name="percentage">The percentage</param>
                /// <param name="total">The total number of events</param>
                /// <param name="events">The number of successful events</param>
                internal Percent(double percentage, int total, int successes)
                {
                    // Make sure the arguments are valid
                    if (successes > total || total < 0 || 
                        successes < 0 || percentage > 1 || percentage < 0)
                    {
                        throw new ArgumentException("The arguments of this constructor are invalid");
                    }

                    // Set them if valid
                    this.percentage = percentage;
                    this.total = total;
                    this.successes = successes;
                }

                /// <summary>
                /// Creates a percentage.  Everythins starts as 0.
                /// </summary>
                internal Percent() : this(0, 0, 0)
                {
                }

                /// <summary>
                /// Increases the number of successful events.
                /// </summary>
                internal void successfulEvent()
                {
                    total++;
                    successes++;

                    recalculate();
                }

                /// <summary>
                /// Does not increase the number of successful events but does increase the total.
                /// </summary>
                internal void unsuccessfulEvent()
                {
                    total++;
                    recalculate();
                }

                /// <summary>
                /// Recalculates the value of the percentage based on the number of total events 
                /// and successes.  If there are 0 total events the probability is set to 0.
                /// </summary>
                internal void recalculate()
                {
                    if (total == 0)
                    {
                        percentage = 0;
                    } else
                    {
                        percentage = (double)successes / (double)total;
                    }
                }

                /// <summary>
                /// Clears this percentage.  Everything is set to 0.
                /// </summary>
                internal void clear()
                {
                    percentage = 0;
                    total = 0;
                    successes = 0;
                }
            }
        }
    }
}
