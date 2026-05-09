using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// Mirrors UPR-FVX <see cref="Gen5Constants.tagTrainersBW"/> /
    /// <c>tagTrainersBW2</c> (trainer indices are UPR <b>1-based</b> where noted).
    /// </summary>
    internal static class Gen5UpTrainerTags
    {
        /// <summary>Build per-trainer tags; <paramref name="nameId"/> matches <see cref="NARCTypes.TrainerEntry.nameID"/> (0-based).</summary>
        public static string[] BuildTagsForRom(int trainerCount)
        {
            var tags = new string[trainerCount];
            if (MainEditor.RomType == RomType.BW2)
                TagBw2(tags);
            else if (MainEditor.RomType == RomType.BW1)
                TagBw(tags);
            return tags;
        }

        static void Set1(string[] tags, int oneBased, string tag)
        {
            if (oneBased < 1 || oneBased > tags.Length) return;
            tags[oneBased - 1] = tag;
        }

        static void SetN(string[] tags, string tag, params int[] oneBasedIndices)
        {
            foreach (int n in oneBasedIndices)
                Set1(tags, n, tag);
        }

        static void RivalTriplet(string[] tags, string prefix, int middleIndex0Based)
        {
            if (middleIndex0Based <= 0 || middleIndex0Based >= tags.Length - 1) return;
            tags[middleIndex0Based - 1] = prefix + "-0";
            tags[middleIndex0Based] = prefix + "-1";
            tags[middleIndex0Based + 1] = prefix + "-2";
        }

        /// <summary>Port of <c>Gen5Constants.tagTrainersBW</c>.</summary>
        static void TagBw(string[] tags)
        {
            SetN(tags, "GYM11", 0x09, 0x0A);
            SetN(tags, "GYM2", 0x56, 0x57, 0x58);
            SetN(tags, "GYM3", 0xC4, 0xC6, 0xC7, 0xC8);
            SetN(tags, "GYM4", 0x42, 0x43, 0x44, 0x45);
            SetN(tags, "GYM5", 0xC9, 0xCA, 0xCB, 0x5F, 0xA8);
            SetN(tags, "GYM6", 0x7D, 0x7F, 0x80, 0x46, 0x47);
            SetN(tags, "GYM7", 0xD7, 0xD8, 0xD9, 0xD4, 0xD5, 0xD6);
            SetN(tags, "GYM8", 0x109, 0x10A, 0x10F, 0x10E, 0x110, 0x10B, 0x113, 0x112);

            Set1(tags, 0x0C, "GYM1-LEADER");
            Set1(tags, 0x0B, "GYM9-LEADER");
            Set1(tags, 0x0D, "GYM10-LEADER");
            Set1(tags, 0x15, "GYM2-LEADER");
            Set1(tags, 0x16, "GYM3-LEADER");
            Set1(tags, 0x17, "GYM4-LEADER");
            Set1(tags, 0x18, "GYM5-LEADER");
            Set1(tags, 0x19, "GYM6-LEADER");
            Set1(tags, 0x83, "GYM7-LEADER");
            Set1(tags, 0x84, "GYM8-LEADER");
            Set1(tags, 0x85, "GYM8-LEADER");

            Set1(tags, 0xE4, "ELITE1");
            Set1(tags, 0xE6, "ELITE2");
            Set1(tags, 0xE7, "ELITE3");
            Set1(tags, 0xE5, "ELITE4");

            Set1(tags, 0x233, "ELITE1");
            Set1(tags, 0x235, "ELITE2");
            Set1(tags, 0x236, "ELITE3");
            Set1(tags, 0x234, "ELITE4");
            Set1(tags, 0x197, "CHAMPION");

            Set1(tags, 0x21E, "UBER");
            Set1(tags, 0x237, "UBER");
            Set1(tags, 0xE8, "UBER");
            Set1(tags, 0x24A, "UBER");
            Set1(tags, 0x24B, "UBER");

            RivalTriplet(tags, "RIVAL1", 0x35);
            RivalTriplet(tags, "RIVAL2", 0x11F);
            RivalTriplet(tags, "RIVAL3", 0x38);
            RivalTriplet(tags, "RIVAL4", 0x193);
            RivalTriplet(tags, "RIVAL5", 0x5A);
            RivalTriplet(tags, "RIVAL6", 0x21B);
            RivalTriplet(tags, "RIVAL7", 0x24C);
            RivalTriplet(tags, "RIVAL8", 0x24F);

            RivalTriplet(tags, "FRIEND1", 0x3B);
            RivalTriplet(tags, "FRIEND2", 0x1F2);
            RivalTriplet(tags, "FRIEND3", 0x1FB);
            RivalTriplet(tags, "FRIEND4", 0x1EB);
            RivalTriplet(tags, "FRIEND5", 0x1EE);
            RivalTriplet(tags, "FRIEND6", 0x252);

            Set1(tags, 64, "NOTSTRONG");
            SetN(tags, "STRONG", 65, 89, 218);
        }

        /// <summary>Port of <c>Gen5Constants.tagTrainersBW2</c>.</summary>
        static void TagBw2(string[] tags)
        {
            SetN(tags, "GYM1", 0xab, 0xac);
            SetN(tags, "GYM2", 0xb2, 0xb3);
            SetN(tags, "GYM3", 0x2de, 0x2df, 0x2e0, 0x2e1);
            SetN(tags, "GYM4", 0x26d, 0x94, 0xcf, 0xd0, 0xd1);
            SetN(tags, "GYM5", 0x13f, 0x140, 0x141, 0x142, 0x143, 0x144, 0x145);
            SetN(tags, "GYM6", 0x95, 0x96, 0x97, 0x98, 0x14c);
            SetN(tags, "GYM7", 0x17d, 0x17e, 0x17f, 0x180, 0x181);
            SetN(tags, "GYM8", 0x15e, 0x15f, 0x160, 0x161, 0x162, 0x163);

            SetN(tags, "GYM1-LEADER", 0x9c, 0x2fc);
            SetN(tags, "GYM2-LEADER", 0x9d, 0x2fd);
            SetN(tags, "GYM3-LEADER", 0x9a, 0x2fe);
            SetN(tags, "GYM4-LEADER", 0x99, 0x2ff);
            SetN(tags, "GYM5-LEADER", 0x9e, 0x300);
            SetN(tags, "GYM6-LEADER", 0x9b, 0x301);
            SetN(tags, "GYM7-LEADER", 0x9f, 0x302);
            SetN(tags, "GYM8-LEADER", 0xa0, 0x303);

            SetN(tags, "ELITE1", 0x26, 0x304, 0x8f, 0x309);
            SetN(tags, "ELITE2", 0x28, 0x305, 0x91, 0x30a);
            SetN(tags, "ELITE3", 0x29, 0x307, 0x92, 0x30c);
            SetN(tags, "ELITE4", 0x27, 0x306, 0x90, 0x30b);
            SetN(tags, "CHAMPION", 0x155, 0x308, 0x218, 0x30d);

            RivalTriplet(tags, "RIVAL1", 0xa1);
            RivalTriplet(tags, "RIVAL2", 0xa6);
            RivalTriplet(tags, "RIVAL3", 0x24c);
            RivalTriplet(tags, "RIVAL4", 0x170);
            RivalTriplet(tags, "RIVAL5", 0x17a);
            RivalTriplet(tags, "RIVAL6", 0x2bd);
            RivalTriplet(tags, "RIVAL7", 0x31a);
            RivalTriplet(tags, "RIVAL8", 0x2ac);
            RivalTriplet(tags, "RIVAL9", 0x2b5);
            RivalTriplet(tags, "RIVAL10", 0x2b8);

            RivalTriplet(tags, "FRIEND2", 0x168);
            RivalTriplet(tags, "FRIEND2", 0x16b);

            SetN(tags, "GYM1", 0x173, 0x278);
            Set1(tags, 0x32E, "GYM1-NOTSTRONG");

            Set1(tags, 0x1f0, "GYM9-LEADER");
            Set1(tags, 0x1ee, "GYM10-LEADER");
            Set1(tags, 0x1ef, "GYM11-LEADER");

            SetN(tags, "THEMED:ZINZOLIN-STRONG", 0x2c0, 0x248, 0x15b, 0x1f1);
            SetN(tags, "THEMED:COLRESS-STRONG", 0x166, 0x158, 0x32d);
            Set1(tags, 0x32f, "THEMED:COLRESS-STRONG-NOTSTRONG");
            SetN(tags, "THEMED:SHADOW1", 0x247, 0x15c, 0x2af);
            SetN(tags, "THEMED:SHADOW2", 0x1f2, 0x2b0);
            SetN(tags, "THEMED:SHADOW3", 0x1f3, 0x2b1);

            Set1(tags, 0x246, "UBER");
            Set1(tags, 0x1c8, "UBER");
            Set1(tags, 0xca, "UBER");
            Set1(tags, 0xc9, "UBER");
            Set1(tags, 0x5, "UBER");
            Set1(tags, 0x6, "UBER");
            Set1(tags, 0x30e, "UBER");
            Set1(tags, 0x30f, "UBER");
            Set1(tags, 0x310, "UBER");
            Set1(tags, 0x311, "UBER");
            Set1(tags, 0x159, "UBER");
            Set1(tags, 0x8c, "UBER");
            Set1(tags, 0x24f, "UBER");
        }
    }
}
