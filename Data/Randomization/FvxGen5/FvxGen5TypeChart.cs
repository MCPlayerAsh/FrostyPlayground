using System.Collections.Generic;
using NewEditor.Data;
using NewEditor.Data.Randomization.GeneShuffle;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// Reads the in-ROM Gen 5 type chart from the battle overlay (UPR-style 8/4/2/0 factors by default).
    /// Row = defending type, column = attacking type. Custom tables are supported: stride 17 vs 18 is inferred structurally
    /// (diagonal coherence + standard-factor density), and nonstandard cell values use neutral-from-diagonal for triangle logic.
    /// </summary>
    internal static class FvxGen5TypeChart
    {
        public const byte FactorSuperEffective = 8;
        public const byte FactorNeutral = 4;
        public const byte FactorResisted = 2;
        public const byte FactorImmune = 0;

        /// <summary>Gen 5 primary type ids for the classic starter triangle.</summary>
        public const byte Fire = 9;
        public const byte Water = 10;
        public const byte Grass = 11;

        /// <summary>
        /// Raw bytes used for the type chart at <see cref="FvxGen5RomLayout.TypeChartOffsetInBattleOvl"/>.
        /// When the battle overlay is stored <b>decompressed</b> in memory, this is the live in-memory overlay byte list
        /// (same object the Type Chart Editor edits), so randomizers see unsaved-in-disk edits.
        /// When still BLZ-compressed, returns a decoded snapshot (edit the overlay via File Explorer → decompress first).
        /// </summary>
        static IList<byte> GetBattleOverlayChartBytes(bool bw2, out string error)
        {
            error = null;
            var fs = MainEditor.fileSystem;
            int idx = FvxGen5RomLayout.BattleOverlayIndex(bw2);
            if (fs?.overlays == null || idx < 0 || idx >= fs.overlays.Count)
            {
                error = "Battle overlay not available.";
                return null;
            }

            var raw = fs.overlays[idx];
            bool compressed = idx < fs.y9.entries.Count && fs.y9.entries[idx].compressed;
            if (compressed)
            {
                byte[] dec = BLZDecoder.BLZ_DecodePub(raw.ToArray());
                if (dec == null)
                {
                    error = "Battle overlay could not be decompressed.";
                    return null;
                }

                return dec;
            }

            return raw;
        }

        /// <summary>Vanilla chart uses <b>17</b> bytes per row; Fairy Vpatch extends to <b>18</b> bytes per row at the same base offset.</summary>
        const int VanillaChartSide = 17;

        /// <summary>Fairy extended chart side (17 types → 18×18 matrix).</summary>
        const int FairyChartSide = 18;

        /// <summary>
        /// How often the main diagonal matches its mode (same-type cells). Wrong row stride reads off-diagonal garbage and scores lower.
        /// Works for custom charts: does not assume vanilla matchups.
        /// </summary>
        static int DiagonalCoherenceScore(IList<byte> ov, int b, int stride, int side)
        {
            if (b < 0 || side <= 0)
                return -1;
            int last = b + (side - 1) * stride + (side - 1);
            if (last < 0 || last >= ov.Count)
                return -1;

            var counts = new Dictionary<byte, int>();
            for (int i = 0; i < side; i++)
            {
                byte v = ov[b + i * stride + i];
                counts[v] = counts.TryGetValue(v, out int c) ? c + 1 : 1;
            }

            int best = 0;
            foreach (var kv in counts)
            {
                if (kv.Value > best)
                    best = kv.Value;
            }

            return best;
        }

        /// <summary>Count of cells in the side×side block that use standard UPR factors (0/2/4/8). Custom charts may score low on both strides equally.</summary>
        static int StandardFactorCellCount(IList<byte> ov, int b, int stride, int side)
        {
            if (b < 0 || side <= 0)
                return -1;
            int last = b + (side - 1) * stride + (side - 1);
            if (last < 0 || last >= ov.Count)
                return -1;

            int n = 0;
            for (int def = 0; def < side; def++)
            {
                for (int atk = 0; atk < side; atk++)
                {
                    byte v = ov[b + def * stride + atk];
                    if (v == FactorImmune || v == FactorResisted || v == FactorNeutral || v == FactorSuperEffective)
                        n++;
                }
            }

            return n;
        }

        /// <summary>
        /// Bytes per row in the overlay matrix at <paramref name="chartBaseOffset"/>.
        /// Uses structural hints so custom effectiveness tables work; vanilla matchup bytes are not assumed.
        /// </summary>
        internal static int DetectBattleOverlayChartStride(IList<byte> ov, int chartBaseOffset)
        {
            int hint = GeneShuffleFairyPatch.TypeChartListsFairy() ? 17 : 16;
            return DetectBattleOverlayChartStride(ov, chartBaseOffset, hint);
        }

        /// <summary>
        /// <paramref name="maxTypeInclusive"/> is the highest type id in use (16 = 17 types, 17 = 18 types / Fairy).
        /// </summary>
        internal static int DetectBattleOverlayChartStride(IList<byte> ov, int chartBaseOffset, int maxTypeInclusive)
        {
            if (ov == null || chartBaseOffset < 0)
                return VanillaChartSide;
            if (ov.Count < chartBaseOffset + VanillaChartSide * VanillaChartSide)
                return VanillaChartSide;

            bool roomFor18 = ov.Count >= chartBaseOffset + FairyChartSide * FairyChartSide;

            if (maxTypeInclusive >= 17 && roomFor18)
                return FairyChartSide;

            if (GeneShuffleFairyPatch.TypeChartListsFairy() && roomFor18)
                return FairyChartSide;

            if (!roomFor18)
                return VanillaChartSide;

            int d17 = DiagonalCoherenceScore(ov, chartBaseOffset, VanillaChartSide, VanillaChartSide);
            int d18 = DiagonalCoherenceScore(ov, chartBaseOffset, FairyChartSide, FairyChartSide);
            int s17 = StandardFactorCellCount(ov, chartBaseOffset, VanillaChartSide, VanillaChartSide);
            int s18 = StandardFactorCellCount(ov, chartBaseOffset, FairyChartSide, FairyChartSide);

            long score17 = (long)d17 * 10000 + s17;
            long score18 = (long)d18 * 10000 + s18;

            if (score18 > score17)
                return FairyChartSide;
            if (score17 > score18)
                return VanillaChartSide;

            if (maxTypeInclusive >= 17)
                return FairyChartSide;
            if (GeneShuffleFairyPatch.TypeChartListsFairy())
                return FairyChartSide;
            return VanillaChartSide;
        }

        public static bool TryReadChart(bool bw2, int maxTypeInclusive, out byte[] chart, out string error)
        {
            chart = null;
            error = null;
            int n = maxTypeInclusive + 1;
            if (n <= 0 || n > 32)
            {
                error = "Invalid type chart size.";
                return false;
            }

            var ov = GetBattleOverlayChartBytes(bw2, out error);
            if (ov == null)
                return false;

            int baseOff = FvxGen5RomLayout.TypeChartOffsetInBattleOvl(bw2);

            int stride = DetectBattleOverlayChartStride(ov, baseOff, maxTypeInclusive);
            int physicalSide = stride == FairyChartSide ? FairyChartSide : VanillaChartSide;

            int lastIndex = baseOff + (physicalSide - 1) * stride + (physicalSide - 1);
            if (ov.Count <= lastIndex)
            {
                error = "Type chart read exceeds overlay size (battle overlay too small or decompress the battle overlay).";
                return false;
            }

            if (n >= FairyChartSide && stride < FairyChartSide)
            {
                error =
                    "The battle overlay still has 17-byte chart rows, but Fairy-type (18 types) is active. Apply the Fairy Vpatch, decompress overlay 93/167 if needed, or uncheck Include Fairy-type.";
                return false;
            }

            chart = new byte[n * n];

            for (int def = 0; def < n; def++)
            {
                for (int atk = 0; atk < n; atk++)
                {
                    if (def < physicalSide && atk < physicalSide)
                        chart[def * n + atk] = ov[baseOff + def * stride + atk];
                    else
                        chart[def * n + atk] = FactorNeutral;
                }
            }

            return true;
        }

        /// <summary>
        /// Cached interpretation of the ROM chart for starter triangles: vanilla charts use factor <see cref="FactorSuperEffective"/> (8);
        /// charts with other byte values infer “super-effective” as strictly above the neutral tier taken from same-type diagonal cells.
        /// </summary>
        internal readonly struct TypeTriangleEvalContext
        {
            public readonly bool StrictGen5FactorsOnly;
            public readonly byte NeutralDiagonalFactor;

            public static TypeTriangleEvalContext Create(byte[] chart, int maxTypeInclusive)
                => new TypeTriangleEvalContext(ChartUsesOnlyStandardFactors(chart),
                    InferNeutralFactorFromDiagonal(chart, maxTypeInclusive));

            TypeTriangleEvalContext(bool strictGen5FactorsOnly, byte neutralDiagonalFactor)
            {
                StrictGen5FactorsOnly = strictGen5FactorsOnly;
                NeutralDiagonalFactor = neutralDiagonalFactor;
            }

            /// <summary>Attack type deals super-effective damage into defense type for triangle purposes.</summary>
            public bool IsSuperEffectiveAgainst(byte[] chart, int maxTypeInclusive, int attackType, int defenseType)
            {
                int eff = Effectiveness(chart, maxTypeInclusive, attackType, defenseType);
                if (StrictGen5FactorsOnly)
                    return eff == FactorSuperEffective;
                return eff != FactorImmune && eff > NeutralDiagonalFactor;
            }

            public bool FormsDirectedTriangle(byte[] chart, int maxTypeInclusive, int t0, int t1, int t2)
                => IsSuperEffectiveAgainst(chart, maxTypeInclusive, t0, t1)
                   && IsSuperEffectiveAgainst(chart, maxTypeInclusive, t1, t2)
                   && IsSuperEffectiveAgainst(chart, maxTypeInclusive, t2, t0);
        }

        static bool ChartUsesOnlyStandardFactors(byte[] chart)
        {
            if (chart == null) return true;
            foreach (byte b in chart)
            {
                if (b != FactorImmune && b != FactorResisted && b != FactorNeutral && b != FactorSuperEffective)
                    return false;
            }
            return true;
        }

        /// <summary>Neutral mono-vs-mono factor: mode of diagonal entries, else Gen 5 default (4).</summary>
        static byte InferNeutralFactorFromDiagonal(byte[] chart, int maxTypeInclusive)
        {
            int n = maxTypeInclusive + 1;
            var counts = new Dictionary<byte, int>();
            for (int i = 0; i < n; i++)
            {
                byte d = chart[i * n + i];
                counts[d] = counts.TryGetValue(d, out int c) ? c + 1 : 1;
            }

            byte best = FactorNeutral;
            int bestCount = 0;
            foreach (var kv in counts)
            {
                if (kv.Value > bestCount)
                {
                    bestCount = kv.Value;
                    best = kv.Key;
                }
            }

            if (best == FactorImmune || bestCount == 0)
                return FactorNeutral;
            return best;
        }

        internal static int CountSuperEffectiveMatchups(byte[] chart, int maxTypeInclusive, TypeTriangleEvalContext ctx)
        {
            if (chart == null) return 0;
            int n = maxTypeInclusive + 1;
            int c = 0;
            for (int def = 0; def < n; def++)
            {
                for (int atk = 0; atk < n; atk++)
                {
                    if (def == atk) continue;
                    if (ctx.IsSuperEffectiveAgainst(chart, maxTypeInclusive, atk, def))
                        c++;
                }
            }
            return c;
        }

        static int CountNonStandardChartBytes(byte[] chart, int maxTypeInclusive)
        {
            if (chart == null) return 0;
            int bad = 0;
            for (int i = 0; i < chart.Length; i++)
            {
                byte b = chart[i];
                if (b != FactorSuperEffective && b != FactorNeutral && b != FactorResisted && b != FactorImmune)
                    bad++;
            }
            return bad;
        }

        /// <summary>User-facing detail when <see cref="EnumerateTypeTriangles"/> is empty.</summary>
        internal static string ExplainNoTypeTrianglesMessage(byte[] chart, int maxTypeInclusive)
        {
            var ctx = TypeTriangleEvalContext.Create(chart, maxTypeInclusive);
            int se = CountSuperEffectiveMatchups(chart, maxTypeInclusive, ctx);
            int nonStd = CountNonStandardChartBytes(chart, maxTypeInclusive);
            if (se == 0)
            {
                if (nonStd > 0)
                    return
                        "No type triangle exists: no matchup reads as super-effective on this chart (strict ×2 factor 8 for standard charts, or factor above neutral from diagonal for non-standard bytes). Some cells are not 0/2/4/8 — check overlay offset, 17 vs 18 row stride (decompress overlay 93/167), and Include Fairy-type.";
                return
                    "No type triangle exists: no super-effective matchups were found. The battle overlay chart may be misread (wrong offset or stride — decompress the overlay), or the chart has no SE entries.";
            }
            if (nonStd > chart.Length / 4 && ctx.StrictGen5FactorsOnly)
                return
                    "No type triangle exists: there are SE (factor 8) edges, but no directed three-way cycle. Many cells are not 0/2/4/8 — the chart may be corrupted or misread; fix alignment or relax starter mode.";
            return
                "No type triangle exists: this ROM’s chart has super-effective matchups, but no three distinct types A,B,C with A→B, B→C, and C→A all super-effective. Edit the type chart or choose another starter type restriction.";
        }

        /// <summary>
        /// Multiplier factor for a move of <paramref name="attackType"/> used against a pure <paramref name="defenseType"/> Pokémon.
        /// Matches overlay layout used by the in-editor chart: row = defending type, column = attacking type (<c>defense * n + attack</c>).
        /// </summary>
        public static int Effectiveness(byte[] chart, int maxTypeInclusive, int attackType, int defenseType)
        {
            int n = maxTypeInclusive + 1;
            return chart[defenseType * n + attackType];
        }

        /// <summary>
        /// Super-effective for Gen 5 overlay semantics: factor 8 when the chart uses only 0/2/4/8; otherwise any factor above neutral-inferred-from-diagonal (supports scaled or hack charts).
        /// </summary>
        public static bool IsSuperEffective(byte[] chart, int maxTypeInclusive, int attackType, int defenseType)
        {
            var ctx = TypeTriangleEvalContext.Create(chart, maxTypeInclusive);
            return ctx.IsSuperEffectiveAgainst(chart, maxTypeInclusive, attackType, defenseType);
        }

        /// <summary>
        /// True if the attacking type deals strictly more damage (higher chart factor) than the defending type does back in a mono-vs-mono matchup.
        /// Works for edited charts as long as factors stay ordered consistently (not only raw 8/4/2/0).
        /// </summary>
        public static bool AttackStrictlyBeats(byte[] chart, int maxTypeInclusive, int attackType, int defenseType)
        {
            int fwd = Effectiveness(chart, maxTypeInclusive, attackType, defenseType);
            int rev = Effectiveness(chart, maxTypeInclusive, defenseType, attackType);
            return fwd > 0 && fwd > rev;
        }

        /// <summary>
        /// Directed RPS: Type₀ moves SE vs Type₁, Type₁ vs Type₂, Type₂ vs Type₀ — same rule as UPR-FVX, reading factors from this ROM’s battle-overlay chart.
        /// </summary>
        public static bool FormsDirectedTriangle(byte[] chart, int maxTypeInclusive, int t0, int t1, int t2)
            => FormsDirectedTriangle(chart, maxTypeInclusive, t0, t1, t2, TypeTriangleEvalContext.Create(chart, maxTypeInclusive));

        internal static bool FormsDirectedTriangle(byte[] chart, int maxTypeInclusive, int t0, int t1, int t2, TypeTriangleEvalContext ctx)
            => ctx.FormsDirectedTriangle(chart, maxTypeInclusive, t0, t1, t2);

        /// <summary>Enumerate type triplets (t0,t1,t2) that form a rock-paper-scissors cycle.</summary>
        public static List<(byte a, byte b, byte c)> EnumerateTypeTriangles(byte[] chart, int maxTypeInclusive)
        {
            var ctx = TypeTriangleEvalContext.Create(chart, maxTypeInclusive);
            var list = new List<(byte, byte, byte)>();
            int n = maxTypeInclusive + 1;
            for (int a = 0; a < n; a++)
            {
                for (int b = 0; b < n; b++)
                {
                    if (a == b) continue;
                    for (int c = 0; c < n; c++)
                    {
                        if (c == a || c == b) continue;
                        if (ctx.FormsDirectedTriangle(chart, maxTypeInclusive, a, b, c))
                            list.Add(((byte)a, (byte)b, (byte)c));
                    }
                }
            }
            return list;
        }
    }
}
