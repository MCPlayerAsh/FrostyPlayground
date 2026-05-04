using NewEditor.Data;
using NewEditor.Data.NARCTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewEditor.Forms
{
    public partial class RandomMovesEditor : Form
    {
        public RandomMovesEditor()
        {
            InitializeComponent();
        }

        private void ApplyRandomMoves(object sender, EventArgs e)
        {
            Random random = rngSeedNumberBox.Value != 0 ? new Random((int)rngSeedNumberBox.Value) : new Random();

            List<MoveDataEntry> moveData = MainEditor.moveDataNarc.moves;
            var moveNames = MainEditor.textNarc.textFiles[VersionConstants.MoveNameTextFileID].text;
            var validMoves = Enumerable.Range(1, moveData.Count - 1)
                .Where(id => IsEligibleMoveForRandomizer(id, moveNames))
                .ToList();
            var validStatusMoves = validMoves.Where(id => moveData[id].damageType == 0).ToList();
            var validNonStatusMoves = validMoves.Where(id => moveData[id].damageType != 0 && moveData[id].category != 9).ToList();
            if (validMoves.Count == 0 || validStatusMoves.Count == 0 || validNonStatusMoves.Count == 0)
            {
                MessageBox.Show("No valid move pool found after filtering placeholders (DontUse/Empty/Pound).");
                return;
            }

            foreach (PokemonEntry pk in MainEditor.pokemonDataNarc.pokemon)
            {
                LevelUpMoveset l = pk.levelUpMoves;

                if (l == null || l.moves == null) continue;

                l.moves.Clear();

                int move = PickMove(validNonStatusMoves, moveData, pk, random, true, (int)stabRatioNumberBox.Value);
                l.moves.Add(new LevelUpMoveSlot((short)move, 1));

                move = validStatusMoves[random.Next(validStatusMoves.Count)];
                l.moves.Add(new LevelUpMoveSlot((short)move, 1));

                for (int i = 1 + (int)moveSpacingNumberBox.Value; i <= 100 && l.moves.Count < totalMovesNumberBox.Value; i += (int)moveSpacingNumberBox.Value)
                {
                    move = PickMove(validMoves, moveData, pk, random, false, (int)stabRatioNumberBox.Value);
                    l.moves.Add(new LevelUpMoveSlot((short)move, (short)i));
                }

                pk.ApplyData();
            }
        }

        static bool IsEligibleMoveForRandomizer(int moveId, IReadOnlyList<string> moveNames)
        {
            if (moveId >= 560 && moveId <= 679) return false; // reserved DontUse range
            if (moveId >= 680)
            {
                string name = moveId < moveNames.Count ? moveNames[moveId] : string.Empty;
                if (string.Equals(name, "Empty", StringComparison.OrdinalIgnoreCase)) return false;
                if (string.Equals(name, "Pound", StringComparison.OrdinalIgnoreCase)) return false; // legacy placeholder fallback
            }
            return true;
        }

        static int PickMove(List<int> pool, List<MoveDataEntry> moveData, PokemonEntry pk, Random random, bool requireDamaging, int stabRatio)
        {
            if (pool.Count == 0) return 1;

            var picks = new List<int>(pool);
            for (int tries = 0; tries < 200; tries++)
            {
                int move = picks[random.Next(picks.Count)];
                var mv = moveData[move];
                if (mv.category == 9) continue;
                if (requireDamaging && mv.damageType == 0) continue;

                bool needsStab = mv.damageType != 0 && random.Next(100) < stabRatio;
                if (needsStab && !(mv.element == pk.type1 || mv.element == pk.type2)) continue;
                return move;
            }

            return picks[random.Next(picks.Count)];
        }
    }
}
