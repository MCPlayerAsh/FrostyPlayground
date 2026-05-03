using NewEditor.Data.NARCTypes;

namespace NewEditor.Data.Randomization.FvxGen5
{
    internal static class FvxGen5MoveScoring
    {
        /// <summary>FVX Move.isGoodDamaging (perfectAccuracy from ROM; Gen5 handler often uses 100).</summary>
        public static bool IsGoodDamaging(MoveDataEntry mv, int perfectAccuracy)
        {
            double hitCount = GetHitCount(mv);
            int power = mv.basePower & 0xFF;
            double effective = power * hitCount;
            double hit = mv.accuracy == 0 ? 100 : System.Math.Min(100, mv.accuracy & 0xFF);
            return effective >= 2 * FvxGen5Constants.MinDamagingMovePower
                || (effective >= FvxGen5Constants.MinDamagingMovePower && (hit >= 90 || hit == perfectAccuracy));
        }

        public static double GetHitCount(MoveDataEntry mv)
        {
            if (mv.maxMultiHit == 0 && mv.minMultiHit == 0) return 1;
            return (mv.minMultiHit + mv.maxMultiHit) / 2.0;
        }

        /// <summary>Physical vs special for FVX Species attack ratio filter (Gen5 category: 0 phys, 1 spec, 2 status).</summary>
        public static bool IsPhysicalCategory(MoveDataEntry mv) => mv.category == 0;

        public static bool IsDamaging(MoveDataEntry mv) => mv.damageType != 0 && mv.basePower > 0;
    }
}
