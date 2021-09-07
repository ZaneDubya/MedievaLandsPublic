using System;
using XPT.Core.Utilities;
using XPT.Legacy.Games.Generic.Entities;
using XPT.Legacy.Games.Yserbius.Constants;
using XPT.Legacy.Games.Yserbius.Entities;
using XPT.Legacy.Games.Yserbius.Entities.Buffs;
using XPT.Legacy.Games.Yserbius.Systems;
using XPT.Legacy.Packets;

namespace XPT.Legacy.Games.Yserbius.Battle {
    /// <summary>
    /// Battle mechanics used for Yserbius and Twinion.
    /// </summary>
    static class YsBattleMechanics {

        internal static int RollInitiative(int initiative) {
            int iniRnd = initiative / 2;
            int ini = (initiative - iniRnd) + Randoms.Next(iniRnd * 2);
            return ini;
        }

        public static EBattleHitType CalcPhysicalAttack(IMobile source, IMobile target, int count, bool allowCritHits, out int dmg, out EBattleCritType critType, out int inflictedStates, out int resistedStates) {
            IYsMobileServer atk = (IYsMobileServer)source;
            IYsMobileServer def = (IYsMobileServer)target;
            inflictedStates = 0;
            resistedStates = 0;
            critType = EBattleCritType.NoCrit;
            int hitDiff = (Math.Clamp(atk.StrengthTotal - def.DefenseTotal, -14, 14) + 14) / 2; // 0-14
            int hitRand = Randoms.Next(15); // 0-15
            int hitResult = YsBattleConstants.HitTable[hitDiff * 16 + hitRand];
            if (hitResult == YsBattleConstants.HitTypeMiss) {
                dmg = 0;
                return EBattleHitType.Miss;
            }
            // calculate damage base and randomization factor, applying mods:
            dmg = 0;
            int dmgValue = atk.DexterityTotal + ((source as YsPlayer)?.PhysicalDmgMod ?? 0);
            if (source is YsPlayer p && target is YsMonsterStack) {
                YsMonsterStack m = target as YsMonsterStack;
                if (m.IsBeast) {
                    dmgValue += p.PhysicalDmgVsBeastMod;
                }
                if (m.IsUndead) {
                    dmgValue += p.PhysicalDmgVsUndeadMod;
                }
            }
            int dmgBase = (dmgValue * 3) / 4 + ((source as YsPlayer)?.PhysicalDmgMinMod ?? 0);
            int dmgRnd = dmgValue / 2 + ((source as YsPlayer)?.PhysicalDmgMaxMod ?? 0);
            // calculate damage, multiple times if this is a monster stack:
            int attackCount = 1;
            if (((atk as YsMonsterStack)?.Count ?? 1) > 1) {
                YsMonsterStack m = atk as YsMonsterStack;
                attackCount = m.Count < m.AttackCount ? m.Count : m.AttackCount;
            }
            for (int i = 0; i < attackCount; i++) {
                dmg += dmgBase + Randoms.Next(dmgRnd);
            }
            // reduce damage by armor and resist mods:
            dmg -= def.ArmorPhysical;
            dmg = MathUtility.RoundFloat(dmg * ((100 - def.DamageResistPhysical) / 100f));
            if (dmg <= 0) {
                dmg = 0;
                return EBattleHitType.Parry;
            }
            // protection against undead:
            if ((atk as YsMonsterStack)?.IsUndead ?? false) {
                int undeadProtection = YsBuffSystem.AllUndeadProtection(def);
                if (undeadProtection > 0) {
                    dmg >>= undeadProtection;
                }
            }
            // inflict states, accounting for resist state buffs:
            if (atk.AttackSpecial != 0 || LegacyConfig.DEBUG_ALWAYS_POISON) {
                // poison:
                if ((atk.AttackSpecial & YsBuffSystem.POISON) != 0 || LegacyConfig.DEBUG_ALWAYS_POISON) {
                    if (def.HasAnyOfProtectionFlags(YsBuffSystem.BLOCK_POISON)) {
                        resistedStates |= YsBuffSystem.POISON;
                    }
                    else {
                        YsBuffSystem.AddBuff(def, new YsDebuffPoison(def.BattlePoisonAmount, def.Count));
                        inflictedStates |= YsBuffSystem.POISON;
                    }
                }
                // incapacitate:
                if ((atk.AttackSpecial & YsBuffSystem.INCAPACITATE) != 0) {
                    if (def.HasAnyOfProtectionFlags(YsBuffSystem.BLOCK_INCAPACITATE)) {
                        resistedStates |= YsBuffSystem.INCAPACITATE;
                    }
                    else {
                        YsBuffSystem.AddBuff(def, new YsDebuffIncap(2, 1));
                        inflictedStates |= YsBuffSystem.INCAPACITATE;
                    }
                }
                // petrify:
                if ((atk.AttackSpecial & YsBuffSystem.PETRIFY) != 0) {
                    if (def.HasAnyOfProtectionFlags(YsBuffSystem.BLOCK_PETRIFY)) {
                        resistedStates |= YsBuffSystem.PETRIFY;
                    }
                    else {
                        YsBuffSystem.AddBuff(def, new YsDebuffPetrified(2, 1));
                        inflictedStates |= YsBuffSystem.PETRIFY;
                    }
                }
                // backfire:
                if ((atk.AttackSpecial & YsBuffSystem.BACKFIRE) != 0) {
                    if (def.HasAnyOfProtectionFlags(YsBuffSystem.BLOCK_BACKFIRE)) {
                        resistedStates |= YsBuffSystem.BACKFIRE;
                    }
                    else {
                        YsBuffSystem.AddBuff(def, new YsDebuffBackfire(YsDebuffBackfire.FAIL_50_PERCENT, 2));
                        inflictedStates |= YsBuffSystem.BACKFIRE;
                    }
                }
                // control:
                if ((atk.AttackSpecial & YsBuffSystem.CONTROL) != 0) {
                    if (def.HasAnyOfProtectionFlags(YsBuffSystem.BLOCK_CONTROL)) {
                        resistedStates |= YsBuffSystem.CONTROL;
                    }
                    else {
                        YsBuffSystem.AddBuff(def, new YsDebuffControlled(2, 1));
                        inflictedStates |= YsBuffSystem.CONTROL;
                    }
                }
            }
            // critical damage:
            if (allowCritHits && hitResult == YsBattleConstants.HitTypeCritical) {
                int critRand = (hitDiff / 2) + Randoms.Next(7);
                YsBattleConstants.CriticalRecord critResult = YsBattleConstants.CriticalTable[critRand];
                dmg = MathUtility.RoundFloatUp(dmg * critResult.DamageMultiplier);
                YsBuffSystem.AddBuff(def, new YsDebuffCritHit(critResult.DefenseMod, critResult.InitiativeMod));
                critType = critResult.CritType;
                return EBattleHitType.Critical;
            }
            // glancing damage:
            else if (hitResult == YsBattleConstants.HitTypeGlancing) {
                dmg /= 2;
                return EBattleHitType.Glancing;
            }
            else {
                return EBattleHitType.Hit;
            }
        }

        internal static void CalcMagicalAttack(SmsgBattleRound roundPacket, YsSpellSkillArgs args, bool singleTarget, ELegacySound sfx, EBattleParticle particle) {
            args.SoundEffect = sfx;
            args.Damage = MathUtility.RoundFloat(args.Damage * (args.GetSourceAs<IYsMobileServer>().SpellPower / 100f)); // apply spell power
            if (singleTarget) {
                if (args.SavingThrow > 0) {
                    args.Damage = MathUtility.RoundFloat(args.Damage * 0.5f); // apply spell saving throw
                }
            }
            else {
                args.Damage = MathUtility.RoundFloat(args.Damage * ((float)(args.Target.Count * 2 - args.SavingThrow) / 2)); // apply spell saving throw
            }
            args.Damage -= args.GetTargetAs<IYsMobileServer>().ArmorMagic; // apply spell armor
            args.Damage = MathUtility.RoundFloat(args.Damage * ((100 - args.GetTargetAs<IYsMobileServer>().DamageResistMagical) / 100f)); // apply spell resist
            if (args.Damage < 0) {
                args.Damage = 0;
            }
            int killCount = 0;
            if (singleTarget) {
                args.Target.AdjustHealthSingle(-args.Damage, out killCount);
            }
            else {
                args.Target.AdjustHealth(-args.Damage, out killCount);
            }
            args.KillCount += killCount;
            roundPacket?.WriteParticle(particle, args.Target.Serial);
        }

        /// <summary>
        /// Returns true if the spell failed.
        /// </summary>
        internal static bool CheckBackfire(IMobile attacker) {
            if (Randoms.Next(254) + 1 <= YsBuffSystem.ChanceCastFail(attacker)) {
                return true;
            }
            return false;
        }
    }
}
