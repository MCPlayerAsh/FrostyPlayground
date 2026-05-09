namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Curated starter lists when no <c>customnames.rncn</c> exists yet (UPR-style parity; not a full JAR extract).</summary>
    internal static class FvxCustomNamesDefaults
    {
        internal static void ApplyTo(FvxCustomNamesSet set)
        {
            if (set == null) return;
            AddUnique(set.TrainerNames, TrainerNames);
            AddUnique(set.TrainerClasses, TrainerClasses);
            AddUnique(set.DoublesTrainerNames, DoublesTrainerNames);
            AddUnique(set.DoublesTrainerClasses, DoublesTrainerClasses);
            AddUnique(set.PokemonNicknames, PokemonNicknames);
        }

        static void AddUnique(System.Collections.Generic.List<string> target, string[] lines)
        {
            foreach (var s in lines)
            {
                var t = (s ?? "").Trim();
                if (t.Length == 0) continue;
                if (!target.Contains(t)) target.Add(t);
            }
        }

        static readonly string[] TrainerNames =
        {
            "Alex", "Jordan", "Sam", "Taylor", "Casey", "Riley", "Morgan", "Quinn", "Avery", "Jamie",
            "Chris", "Drew", "Blake", "Skyler", "Rowan", "Emery", "Reese", "Parker", "Hayden", "Logan",
            "Ash", "Misty", "Brock", "Dawn", "Iris", "Cilan", "Bianca", "Cheren", "Hugh", "Rosa",
            "Nate", "Hilda", "Hilbert", "Ethan", "Lyra", "Lucas", "Barry", "Paul", "Gary", "Silver"
        };

        static readonly string[] TrainerClasses =
        {
            "Youngster", "Lass", "School Kid", "Rich Boy", "Lady", "Backpacker", "Ace Trainer", "Ranger",
            "Scientist", "Worker", "Hiker", "Fisherman", "Nursery Aide", "Preschooler", "Smasher",
            "Linebacker", "Dancer", "Clerk", "Doctor", "Nurse", "Pilot", "Striker", "Baker", "Depot Agent"
        };

        static readonly string[] DoublesTrainerNames =
        {
            "Amy & Ben", "Chloe & Dan", "Eva & Frank", "Gina & Hal", "Ivy & Jake", "Kate & Leo",
            "Mia & Nick", "Olivia & Pete", "Quinn & Ray", "Sara & Tom", "Uma & Vic", "Wren & Zach",
            "Avery & Blake", "Drew & Ellis", "Finley & Gray", "Harper & Jules", "Kelly & Lane"
        };

        static readonly string[] DoublesTrainerClasses =
        {
            "Twins", "Couple", "Sis and Bro", "Senior and Junior", "Team", "The Battle Couple",
            "Double Team", "Ace Duo", "Backpacker Duo", "Ranger Duo", "Worker Duo", "Hiker Duo"
        };

        static readonly string[] PokemonNicknames =
        {
            "Pip", "Noodle", "Beans", "Muffin", "Pickle", "Tofu", "Waffle", "Biscuit", "Pebble", "Sprout",
            "Zippy", "Flicker", "Breeze", "Comet", "Echo", "Jinx", "Kiwi", "Mochi", "Nimbus", "OnyxJr",
            "Pixel", "Quark", "Riff", "Scout", "Tango", "Uni", "Vex", "Wisp", "Yoyo", "Zest",
            "Ace", "Bolt", "Chip", "Dash", "Elm", "Fern", "Glim", "Halo", "Ivy", "Jade"
        };
    }
}
