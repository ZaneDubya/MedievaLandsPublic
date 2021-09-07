using XPT.Legacy.Games.Yserbius.Constants;

namespace XPT.Legacy.Games.Yserbius.Battle {
    class YsBattleConstants {
        public static readonly int[] DefaultTileOrder = { 1, 2, 0, 3, 4, 5 };

        public static bool IsFrontRow(int tile) {
            if (tile < 4) {
                return true;
            }
            return false;
        }

        // === Combat tables =========================================================================================
        // ===========================================================================================================

        public struct CriticalRecord {
            public readonly EBattleCritType CritType;
            public readonly float DamageMultiplier;
            public readonly short DefenseMod;
            public readonly short InitiativeMod;

            public CriticalRecord(EBattleCritType critType, float dmgMul, short defMod, short iniMod) {
                CritType = critType;
                DamageMultiplier = dmgMul;
                DefenseMod = defMod;
                InitiativeMod = iniMod;
            }
        }

        public const int HitTypeMiss = 0;
        public const int HitTypeStandard = 1;
        public const int HitTypeCritical = 2;
        public const int HitTypeGlancing = 3;

        /// <summary>
        /// This is an array of 16 x 15 bytes. Each byte corresponds to an entry in the HitType enum
        /// </summary>
        public static readonly byte[] HitTable = new byte[240] {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 1,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 1, 1,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 1, 1, 1,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 1, 1, 1, 1,
            0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 1, 1, 1, 1, 1,
            0, 0, 0, 0, 0, 0, 0, 3, 3, 1, 1, 1, 1, 1, 1,
            0, 0, 0, 0, 0, 0, 3, 3, 1, 1, 1, 1, 1, 1, 1,
            0, 0, 0, 0, 0, 3, 3, 1, 1, 1, 1, 1, 1, 1, 1,
            0, 0, 0, 0, 3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            0, 0, 0, 3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            0, 0, 3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2,
            0, 3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2,
            3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2,
            3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2
        };

        public static readonly CriticalRecord[] CriticalTable = new CriticalRecord[15] {
            new CriticalRecord(EBattleCritType.DeepWound, 1.25f, -1, 0 ), // used to be 125%
            new CriticalRecord(EBattleCritType.DeepWound, 1.30f, -1, 0 ), // used to be 150%
            new CriticalRecord(EBattleCritType.DeepWound, 1.35f, -2, 0 ), // used to be 175%
            new CriticalRecord(EBattleCritType.SuckingWound, 1.40f, -2, 0 ), // used to be 200%
            new CriticalRecord(EBattleCritType.SuckingWound, 1.45f, -2, 0 ), // used to be 225%
            new CriticalRecord(EBattleCritType.SuckingWound, 1.50f, -3, 0 ), // used to be 250%
            new CriticalRecord(EBattleCritType.DevastatingWound, 1.60f, -3, 0 ), // used to be 275%
            new CriticalRecord(EBattleCritType.DevastatingWound, 1.65f, -4, 0 ), // used to be 300%
            new CriticalRecord(EBattleCritType.DevastatingWound, 1.70f, -4, 0 ), // used to be 325%
            new CriticalRecord(EBattleCritType.DevastatingWound, 1.75f, -5, 0 ), // used to be 350%
            new CriticalRecord(EBattleCritType.Maim, 1.80f, -3, -1 ), // used to be 375%
            new CriticalRecord(EBattleCritType.Maim, 1.85f, -3, -2 ), // used to be 400%
            new CriticalRecord(EBattleCritType.Maim, 1.90f, -4, -3 ), // used to be 425%
            new CriticalRecord(EBattleCritType.Maim, 1.95f, -4, -3 ), // used to be 450%
            new CriticalRecord(EBattleCritType.Maim, 2.00f, -5, -4 ), // used to be 500%
            // removed kill because of potential unbalanced criticals vs single monsters
            // new CriticalRecord(EWoundType.Kill, 1, 1, 0, 0 ),
        };

        public static readonly byte[] MagicalHitTable = new byte[15] {
            3, 3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2
        };
    }
}
