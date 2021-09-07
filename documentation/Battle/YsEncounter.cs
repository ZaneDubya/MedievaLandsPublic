using System.Collections.Generic;
using System.Linq;
using XPT.Core.Utilities;
using XPT.Legacy.Games.Generic;
using XPT.Legacy.Games.Generic.Battle;
using XPT.Legacy.Games.Generic.Constants;
using XPT.Legacy.Games.Generic.Entities;
using XPT.Legacy.Games.Generic.Maps;
using XPT.Legacy.Games.Generic.Systems;
using XPT.Legacy.Games.Medieva.Systems;
using XPT.Legacy.Games.Yserbius.Assets.Skills;
using XPT.Legacy.Games.Yserbius.Constants;
using XPT.Legacy.Games.Yserbius.Entities;
using XPT.Legacy.Games.Yserbius.Entities.Buffs;
using XPT.Legacy.Games.Yserbius.Entities.Items;
using XPT.Legacy.Games.Yserbius.Localization;
using XPT.Legacy.Games.Yserbius.Systems;
using XPT.Legacy.Packets;
using XPT.Server;

namespace XPT.Legacy.Games.Yserbius.Battle {
    class YsEncounter : GenericEncounter {

        /// <summary>
        /// If true, then this encounter was not created by a map script.
        /// </summary>
        public bool IsRandomEnouncter { get; set; } = false;
        protected int _ExpFromMonsters = 0;
        protected int _GoldFromMonsters = 0;

        // === Ctor, timer, and management routines ==================================================================
        // ===========================================================================================================

        /// <summary>
        /// Creates a new Encounter object. Should only be called from BattleSystem, as it manages the collection of Encounters.
        /// </summary>
        public YsEncounter(GenericPlayer partyLeader)
            : base(partyLeader) {

        }

        /// <summary>
        /// Creates a new Challenge Encounter object. Should only be called from BattleSystem, as it manages the collection of Encounters
        /// </summary>
        public YsEncounter(GenericPlayer party0Leader, GenericPlayer party1Leader)
            : base(party0Leader, party1Leader) {

        }

        protected override void OnBegin() {
            InstanceNewbieBuffs();
            AddMonsterRewards();
        }

        private void InstanceNewbieBuffs() {
            if (_IsPvP) {
                return;
            }
            foreach (GenericPlayer p in AllPlayers()) {
                YsBuffNewbie.InstanceNewbieBuff(p);
            }
        }

        protected void AddMonsterRewards() {
            int monsterCount = 0;
            foreach (GenericMonster m in AllMonsters()) {
                if (m is YsMonsterStack ym) {
                    ym.GetReward(out int gold, out int xp);
                    _GoldFromMonsters += gold;
                    _ExpFromMonsters += xp;
                    // BEGIN OLD_TREASURE
                    monsterCount += 1;
                    foreach (GenericPlayer player in AllPlayers().Where(d => !d.ToggleNewTreasureSystem)) {
                        if (player.LootCount < monsterCount) {
                            player.EncounterLootAdd(YsTreasureSystem.GetPick(player, m.GetTreasurePicks()));
                        }
                    }
                    // END OLD_TREASURE
                }
            }
            // BEGIN NEW_TREASURE
            int stackCount = AllMonsters().Count();
            AMap map = AllPlayers().First().Map;
            foreach (GenericPlayer player in AllPlayers().Where(d => d.ToggleNewTreasureSystem)) {
                foreach (TreasurePick pick in MedievaTreasureSystem.GetPicks(player, map, false, stackCount - player.LootCount)) {
                    player.EncounterLootAdd(pick);
                }
            }
            // END NEW_TREASURE
        }

        public void AddMonsterGold(int amount) {
            _GoldFromMonsters += amount;
        }

        private void AddMonsterExperience(int amount) {
            _ExpFromMonsters += amount;
        }

        public override void SendRewards() {
            if (_IsPvP) {
                return;
            }
            // XP Bonus for partying:
            int count = AllPlayers().Count();
            if (count > 1) {
                float xpBoost = 1f + count * 0.25f;
                _ExpFromMonsters = (int)(_ExpFromMonsters * xpBoost);
            }
            foreach (GenericPlayer p in AllPlayers()) {
                if (p.IsDead) {
                    // REMOVE: p.EncounterLootClear(); // died
                    p.SendMessage(EMessageType.Flavor, LocalStrings.BattleEnds); // you don't get any loot because you are dead.
                    continue;
                }
                // XP Bonus for low level characters:
                int xp = _ExpFromMonsters;
                (p as YsPlayerServer).SendRewards(xp, _GoldFromMonsters);
            }
        }

        /// <summary>
        /// We are in Phase1. A player has submitted an action to the server.
        /// Return true if the action will be set for this player and the game should procede to run that action.
        /// </summary>
        protected override bool Phase1_VerifyActionAcceptable(GenericPlayer player, EBattleActionType type, int offset, ref Serial targetSerial, int tile) {
            if (player.IsUnableToActInCombat) {
                return false;
            }
            if (type == EBattleActionType.Flee) {
                // only leader should be allowed to flee.
                if (Support_GetTeamIndex(player) != 0) {
                    player.SendMessage(EMessageType.Flavor, LocalStrings.BattleFleeNotPartyLeader);
                    return false;
                }
                bool ableToFlee = Phase1_AttemptFlee(player);
                if (ableToFlee) {
                    _EndType = EBattleEndType.Flee;
                    BattleSystem.End(this, _EndType);
                }
                else {
                    _FleeAllowed = false;
                }
                return false; // return false to enter a state where we can stop sending messages.
            }
            if (type == EBattleActionType.PhysicalAttack) {
                if (!TryGetCombatant(targetSerial, out ICombatant target) || target.IsDead) {
                    player.SendMessage(EMessageType.Flavor, "You cannot find that enemy.");
                    return false;
                }
                int battleTile = (target as ICombatant)?.BattleTile ?? 0;
                if ((battleTile == 4 || battleTile == 5) && !(player as YsPlayer).CanAttackBackRow()) {
                    player.SendMessage(EMessageType.Flavor, "You cannot reach that enemy.");
                    return false;
                }
                return true;
            }
            if (type == EBattleActionType.UseSpell || type == EBattleActionType.UseSkill) {
                bool isSpell = (type == EBattleActionType.UseSpell);
                if (isSpell && (player as YsPlayer).MapData.IsNoSpells) {
                    player.SendMessage(EMessageType.Flavor, "Something about this place causes your spell to fizzle.");
                    return false;
                }
                if (!isSpell && (player as YsPlayer).MapData.IsNoSkills) {
                    player.SendMessage(EMessageType.Flavor, "Something about this place keeps you from using your skills.");
                    return false;
                }
                if (!World.GetWorld(player).GetSpellSkillInformation(player, offset, isSpell, true, true, out ISkillData data, out int levelZeroIndexed, out ETypeOfTarget targetType, out int manaCost)) {
                    return false;
                }
                if (!Phase1_ApprovePlayerTarget(player, targetSerial, data.TargetType(levelZeroIndexed))) {
                    // Phase1_MatchTargetToTargetType will send an appropriate error message.
                    return false;
                }
                if (!isSpell && offset == YsIndexes.SKILL_LEADERSHIP && Support_GetTeamIndex(player) != 0) {
                    player.SendMessage(EMessageType.Flavor, "Only the party leader may use that skill.");
                    return false;
                }
                // everything works! The player will use this skill / cast this spell (if they are still alive) when their initiative order comes up.
                return true;
            }
            if (type == EBattleActionType.UseItem) {
                // offset is item serial, targetSerial is target (where necessary)
                GenericItem item = ServerEntities.FindItem(offset);
                if (!World.GetWorld(player).TryUseItem(player, item, true)) {
                    // TryUseItemSpell will send an appropriate error message.
                    return false;
                }
                if (!item.TryGetSkillUsable(player.GameID, out bool isSpell, out ISkillData data, out int levelZeroIndexed)) {
                    // TryUseItemSpell will have already sent an appropriate message.
                    return false;
                }
                if (data.TargetType(levelZeroIndexed) == ETypeOfTarget.AllyOne || data.TargetType(levelZeroIndexed) == ETypeOfTarget.Self) {
                    targetSerial = player.Serial;
                }
                if (!Phase1_ApprovePlayerTarget(player, targetSerial, data.TargetType(levelZeroIndexed))) {
                    // Phase1_MatchTargetToTargetType will send an appropriate error message.
                    return false;
                }
                // everything works! The player will use the item's skill/spell (if they are still alive) when their initiative order comes up.
                return true;
            }
            // what is this action?
            player.SendMessage(EMessageType.Flavor, "You cannot do that now.");
            return false;
        }

        protected override bool Phase1_ApprovePlayerTarget(GenericPlayer m, Serial targetSerial, ETypeOfTarget targetType) {
            Support_GetTeams(m, out List<ICombatant> allies, out List<ICombatant> enemies);
            switch (targetType) {
                case ETypeOfTarget.EnemyAll:
                    // there will always be at least one enemy alive during Phase 1
                    return true;
                case ETypeOfTarget.AllyAll:
                    // the caster, at least, will always be alive during Phase 1
                    return true;
                case ETypeOfTarget.Self:
                    return true;
                case ETypeOfTarget.AllyOne:
                    ICombatant ally = allies.Where(d => d.Serial == targetSerial).FirstOrDefault();
                    if (ally == null) {
                        m.SendMessage(EMessageType.Flavor, "You must cast that on an ally.");
                        return false;
                    }
                    return true;
                case ETypeOfTarget.EnemyOne:
                case ETypeOfTarget.EnemyOneMage:
                case ETypeOfTarget.EnemyGroup:
                case ETypeOfTarget.EnemyGroupUndead:
                    // must target an enemy, although some have more requirements:
                    ICombatant enemy = enemies.Where(d => d.Serial == targetSerial).FirstOrDefault();
                    if (enemy == null) {
                        m.SendMessage(EMessageType.Flavor, "You must cast that on an enemy.");
                        return false;
                    }
                    if (targetType == ETypeOfTarget.EnemyGroupUndead && ((enemy as YsMonsterStack)?.IsMonster ?? false) == false) {
                        m.SendMessage(EMessageType.Flavor, "That spell only targets the undead.");
                        return false;
                    }
                    return true;
                default:
                    return false;
            }
        }

        protected override int GetFleeChance() {
            if (IsRandomEnouncter) {
                return 60;
            }
            return 35;
            /*int fleeChance = 0;
            if (!_IsPvP) { // fighting monsters
                fleeChance = 15;
                //find the smallest flee chance of all the opponents
                foreach (YsMonster m in AllMonsters()) {
                    if (!m.IsDead && m.FleeThreshold < fleeChance) {
                        fleeChance = m.FleeThreshold;
                    }
                }
            }
            else { // fighting players, can't flee.
                fleeChance = 0;
            }
            return fleeChance;*/
        }

        // ==== DoRound Monster actions ===============================================================================
        // ============================================================================================================

        protected override void Phase2_MonsterAction(SmsgBattleRound roundPacket, GenericMonster monster) {
            if (!(monster is YsMonsterStack m)) {
                return;
            }
            // 1. get count of attackers that are not incapacitated and/or paralyzed:
            int attackCount = m.Count - MathUtility.HighestOf(
                YsBuffSystem.GetBuffByType<YsDebuffIncap>(m)?.Count ?? 0,
                YsBuffSystem.GetBuffByType<YsDebuffPetrified>(m)?.Count ?? 0
                );
            if (attackCount <= 0) {
                return;
            }
            // 2. controlled monsters have their go:
            int controlledCount = YsBuffSystem.GetBuffByType<YsDebuffControlled>(m)?.Count ?? 0;
            if (controlledCount > 0) {
                int controlledAttackCount = MathUtility.LowestOf(attackCount, controlledCount, m.AttackCount);
                Phase2_MonsterAction_PhysicalAttack(roundPacket, m, controlledAttackCount, isControlled: true);
                attackCount -= controlledCount;
            }
            if (attackCount <= 0) {
                return;
            }
            // 3. If the monster is alone and hurt, it may attempt to flee:
            if (monster.WillAttemptToFlee() && Randoms.Next(15) <= m.ChanceFlee) {
                roundPacket.WriteFlee(monster.Name);
                monster.KillAll();
                roundPacket.WriteUpdateStatus(monster);
                return;
            }
            // 4. attempt to cast a spell,and then until attack once with physical attack if any attack count is left.
            // 4a. check to cast a spell:
            int chanceSpell = Randoms.Next(99) + 1;
            if (LegacyConfig.DEBUG_MONSETERS_ALWAYS_HEAL) {
                Phase2_MonsterAction_Spell(roundPacket, m, YsIndexes.SPELL_HEAL, 0, isControlled: false);
                attackCount -= 1;
            }
            else if (chanceSpell < m.ChanceCast) { // cast a spell
                int spellDataIndex = Randoms.Next(9);
                int castIndex = m.SpellData[spellDataIndex * 2];
                int castLevelZeroIndex = m.SpellData[spellDataIndex * 2 + 1] - 1;
                Phase2_MonsterAction_Spell(roundPacket, m, castIndex, castLevelZeroIndex, isControlled: false);
                attackCount -= 1; // decrement attackCount by caster
            }
            // 4b. physical attack
            Phase2_MonsterAction_PhysicalAttack(roundPacket, m, MathUtility.LowestOf(m.AttackCount, attackCount), isControlled: false);
        }

        private void Phase2_MonsterAction_PhysicalAttack(SmsgBattleRound roundPacket, YsMonsterStack monster, int count, bool isControlled) {
            if (count <= 0) {
                return;
            }
            if (!Support_FindRandomLivingTarget(monster, isControlled, out ICombatant target)) {
                // could not find a target, do nothing
                return;
            }
            EBattleHitType hitType = YsBattleMechanics.CalcPhysicalAttack(monster, target, count, true, out int dmg, out EBattleCritType critType, out int inflictedStates, out int resistedStates);
            target.AdjustHealth(-dmg, out int killCount);
            if (dmg > 0) {
                roundPacket.WriteSound(ELegacySound.Combat_Hit2);
                roundPacket.WriteParticle(EBattleParticle.LegacyBloodSpatter, target.Serial);
            }
            roundPacket.WriteFloatingPhysical(target.Serial, hitType, dmg);
            roundPacket.WriteUpdateStatus(target);
            roundPacket.WriteMessageEx_PhysicalAttack(monster, target, hitType, critType, dmg, count, killCount, isControlled);
            if (target.IsAlive) {
                roundPacket.WriteMessageEx_StatesInflictedAndResisted(target, inflictedStates, resistedStates);
            }
            monster.ChangeWeapon();
        }

        private void Phase2_MonsterAction_Spell(SmsgBattleRound roundPacket, YsMonsterStack monster, int spellIndex, int levelZeroIndexed, bool isControlled) {
            ISkillData data = World.GetWorld(monster.GameID).GetSpellData(spellIndex);
            // check for spell failure (backfire):
            if (YsBattleMechanics.CheckBackfire(monster)) {
                roundPacket.WriteSpellFailure(new YsSpellSkillArgs(monster, data)); // monster spell - backfire failure
                return; // no action
            }
            // set up variables:
            Support_GetTeams(monster, out List<ICombatant> allies, out List<ICombatant> enemies);
            ETypeOfTarget targetType = data.TargetType(levelZeroIndexed);
            bool targetPlayerParty = false;
            YsSpellSkillArgs args = new YsSpellSkillArgs(monster, null, data, levelZeroIndexed, true);
            // cast spell, changing world state:
            switch (targetType) {
                // casting/using on self:
                case ETypeOfTarget.Self:
                    args.Target = monster;
                    World.GetWorld(monster.GameID).InvokeCombat(roundPacket, args);
                    roundPacket.WriteFloatingMagical(monster.Serial, args.Damage, false);
                    roundPacket.WriteUpdateStatus(monster);
                    break;
                // casting on one ally:
                case ETypeOfTarget.AllyOne:
                    if (Support_FindRandomLivingTarget(monster, !isControlled, out ICombatant allyTarget)) {
                        args.Target = allyTarget;
                        World.GetWorld(monster.GameID).InvokeCombat(roundPacket, args);
                        roundPacket.WriteFloatingMagical(args.Target.Serial, args.Damage, false);
                        roundPacket.WriteUpdateStatus(args.Target);
                    }
                    else {
                        return; // no action
                    }
                    break;
                // casting on one enemy stack:
                case ETypeOfTarget.EnemyGroup:
                case ETypeOfTarget.EnemyGroupUndead:
                case ETypeOfTarget.EnemyOne:
                case ETypeOfTarget.EnemyOneMage: // casting on one enemy:
                    if (Support_FindRandomLivingTarget(monster, isControlled, out ICombatant enemyTarget)) {
                        args.Target = enemyTarget;
                        World.GetWorld(monster.GameID).InvokeCombat(roundPacket, args);
                        roundPacket.WriteFloatingMagical(args.Target.Serial, args.Damage, false);
                        roundPacket.WriteUpdateStatus(args.Target);
                    }
                    else {
                        return; // no action
                    }
                    break;
                // casting/using on all allies or enemies:
                case ETypeOfTarget.AllyAll:
                case ETypeOfTarget.EnemyAll:
                    YsSpellSkillArgs argsEachTarget = new YsSpellSkillArgs(monster, null, data, levelZeroIndexed, true);
                    targetPlayerParty = (targetType == ETypeOfTarget.AllyAll && isControlled) || (targetType == ETypeOfTarget.EnemyAll && !isControlled);
                    IEnumerable<ICombatant> targets = targetPlayerParty ? enemies : allies;
                    foreach (ICombatant eachTarget in targets) {
                        if (eachTarget.IsDead) { // do not allow attacks on dead targets.
                            continue;
                        }
                        argsEachTarget.Target = eachTarget;
                        World.GetWorld(monster.GameID).InvokeCombat(roundPacket, argsEachTarget);
                        roundPacket.WriteFloatingMagical(eachTarget.Serial, argsEachTarget.Damage, false);
                        roundPacket.WriteUpdateStatus(eachTarget);
                        args.CopyFrom(argsEachTarget);
                    }
                    break;
            }
            // output to clients:
            roundPacket.WriteSound(args.SoundEffect); // monster spell
            roundPacket.WriteMessageEx_SpellSkill(args, targetPlayerParty, true, isControlled); // monster spell
            roundPacket.WriteSpellSuccess(args); // monster spell
        }

        // ==== DoRound GenericPlayer actions ================================================================================
        // ============================================================================================================

        protected override void Phase2_PlayerAction(SmsgBattleRound roundPacket, GenericPlayer player) {
            // don't act if we are incapacitated!
            if (YsBuffSystem.HasAnyOfStateFlags(player, YsBuffSystem.CANNOT_ACT_AT_ALL_STATES)) {
                Phase2_WriteCannotActMessage(roundPacket, player);
                return;
            }
            // are we controlled? If so, physically attack self or party member:
            if (YsBuffSystem.HasAnyOfStateFlags(player, YsBuffSystem.CONTROL)) {
                Phase2_PlayerAction_Controlled(roundPacket, player);
            }
            // regular actions:
            switch (player.BattleAction) {
                case EBattleActionType.PhysicalAttack:
                    Phase2_PlayerAction_PhysicalAttack(roundPacket, player, isControlled: false);
                    break;
                case EBattleActionType.UseSpell:
                    Phase2_PlayerAction_SpellOrSkill(roundPacket, player, player.BattleActionOffset, isSpell: true);
                    break;
                case EBattleActionType.UseSkill:
                    Phase2_PlayerAction_SpellOrSkill(roundPacket, player, player.BattleActionOffset, isSpell: false);
                    break;
                case EBattleActionType.UseItem:
                    Phase2_PlayerAction_UseItem(roundPacket, player);
                    break;
                default:
                    // do nothing if 'none selected' or 'timed out'
                    break;
            }
        }

        private void Phase2_WriteCannotActMessage(SmsgBattleRound roundPacket, ICombatant m) {
            if (YsBuffSystem.HasAnyOfStateFlags(m, YsBuffSystem.INCAPACITATE)) {
                roundPacket.WriteMessage_Selective(EMessageType.Flavor, m.Serial,
                    "You are incapacitated and cannot act.",
                    $"{m.Name} {(m.IsPlural ? "are" : "is")} incapacitated and cannot act.");
            }
            else if (YsBuffSystem.HasAnyOfStateFlags(m, YsBuffSystem.PETRIFY)) {
                roundPacket.WriteMessage_Selective(EMessageType.Flavor, m.Serial,
                    "You are petrified and cannot act.",
                    $"{m.Name} {(m.IsPlural ? "are" : "is")} petrified and cannot act.");
            }
            else {
                roundPacket.WriteMessage_Selective(EMessageType.Flavor, m.Serial,
                    "You cannot act.",
                    $"{m.Name} cannot act.");
            }
        }

        private void Phase2_PlayerAction_Controlled(SmsgBattleRound roundPacket, GenericPlayer player) {
            if (Support_FindRandomLivingTarget(player, true, out ICombatant controlTarget)) {
                player.BattleActionTarget = controlTarget.Serial;
                Phase2_PlayerAction_PhysicalAttack(roundPacket, player, isControlled: true);
            }
        }

        private void Phase2_PlayerAction_PhysicalAttack(SmsgBattleRound roundPacket, GenericPlayer attacker, bool isControlled) {
            if (!TryGetCombatant(attacker.BattleActionTarget, out ICombatant target) || target.IsDead) {
                roundPacket.WriteMessage_Selective(EMessageType.Flavor, attacker.Serial,
                    $"Your attack is futile!",
                    $"{attacker.NamePossessive} attack is futile!");
                return;
            }
            EBattleHitType hitType = YsBattleMechanics.CalcPhysicalAttack(attacker, target, 1, !_IsPvP, out int dmg, out EBattleCritType critType, out int inflictedStates, out int resistedStates);
            target.AdjustHealth(-dmg, out int killCount);
            if (dmg > 0) {
                roundPacket.WriteSound(ELegacySound.Combat_Hit1);
                roundPacket.WriteParticle(EBattleParticle.LegacyBloodSpatter, target.Serial);
            }
            roundPacket.WriteFloatingPhysical(target.Serial, hitType, dmg);
            roundPacket.WriteUpdateStatus(target);
            roundPacket.WriteMessageEx_PhysicalAttack(attacker, target, hitType, critType, dmg, 1, killCount, isControlled);
            roundPacket.WriteMessageEx_StatesInflictedAndResisted(target, inflictedStates, resistedStates);
            if (killCount > 0) {
                attacker.OnKilledEnemy(killCount);
            }
        }

        private void Phase2_PlayerAction_SpellOrSkill(SmsgBattleRound roundPacket, GenericPlayer attacker, int skillIndex, bool isSpell) {
            World.GetWorld(attacker).GetSpellSkillInformation(attacker, skillIndex, isSpell, true, false, out ISkillData data, out int level, out ETypeOfTarget targetType, out int manaCost);
            // check for spell failure (backfire):
            if (isSpell && YsBattleMechanics.CheckBackfire(attacker)) {
                roundPacket.WriteSpellFailure(new YsSpellSkillArgs(attacker, data)); // player spell - backfire failure
                if (isSpell) {
                    attacker.AdjustMana(-manaCost);
                }
                return;
            }
            // spell succeeded:
            Phase2_PlayerAction_SpellOrSkill_Underlying(roundPacket, attacker, data, level, isSpell, targetType, DO_SOUNDS, out bool useMana);
            if (isSpell && useMana) {
                attacker.AdjustMana(-manaCost);
            }
        }

        private void Phase2_PlayerAction_UseItem(SmsgBattleRound roundPacket, GenericPlayer attacker) {
            // the game has already checked if the player can use this item and the item has charges left.
            YsItem item = ServerEntities.FindItem(attacker.BattleActionOffset) as YsItem;
            if (!item.TryGetSkillUsable(attacker.GameID, out bool isSpell, out ISkillData data, out int levelZeroIndexed)) {
                roundPacket.WriteMessage_Selective(EMessageType.Flavor, attacker.Serial,
                    $"Your attempt to use an item fails!",
                    $"{attacker.NamePossessive} attack is futile!");
                return;
            }
            ETypeOfTarget targetType = data.TargetType(levelZeroIndexed);
            Phase2_PlayerAction_SpellOrSkill_Underlying(roundPacket, attacker, data, levelZeroIndexed, isSpell, targetType, NO_SOUNDS, out bool unusedUseMana);
            item.UseItemCharge();
        }

        /// <summary>
        /// A player has chosen to cast a spell. Note: controlled players never cast spells.
        /// </summary>
        private void Phase2_PlayerAction_SpellOrSkill_Underlying(SmsgBattleRound roundPacket, GenericPlayer source, ISkillData data, int levelZeroIndexed, bool isSpell, ETypeOfTarget targetType, bool doSound, out bool useMana) {
            // Set up variables:
            Support_GetTeams(source, out List<ICombatant> allies, out List<ICombatant> enemies);
            YsSpellSkillArgs args = new YsSpellSkillArgs(source, null, data, levelZeroIndexed, true);
            bool targetPlayerParty = false;
            // cast spell, changing world state:
            switch (targetType) {
                // casting/using on self:
                case ETypeOfTarget.Self:
                    args.Target = source;
                    World.GetWorld(source).InvokeCombat(roundPacket, args);
                    roundPacket.WriteFloatingMagical(source.Serial, args.Damage, false);
                    roundPacket.WriteUpdateStatus(source);
                    break;
                // casting/using on one ally or enemy:
                case ETypeOfTarget.AllyOne:
                case ETypeOfTarget.EnemyGroup:
                case ETypeOfTarget.EnemyGroupUndead:
                case ETypeOfTarget.EnemyOne:
                case ETypeOfTarget.EnemyOneMage:
                    // make sure target is living:
                    if (!TryGetCombatant(source.BattleActionTarget, out ICombatant target) || target.IsDead) {
                        if (target.IsDead && !target.IsMonster && data.IsSpell && data.Index == YsIndexes.SPELL_RESUSCITATE) {
                            // special case: you may cast res on a dead ally.
                        }
                        else { // do not allow attacks on dead enemies.
                            roundPacket.WriteSpellFutile(source, data);
                            useMana = false;
                            return;
                        }
                    }
                    args.Target = target;
                    args.AllowSavingThrow = !_IsPvP;
                    World.GetWorld(source).InvokeCombat(roundPacket, args);
                    roundPacket.WriteFloatingMagical(target.Serial, args.Damage, false);
                    roundPacket.WriteUpdateStatus(target);
                    break;
                // casting/using on all allies or enemeies:
                case ETypeOfTarget.AllyAll:
                case ETypeOfTarget.EnemyAll:
                    targetPlayerParty = (targetType == ETypeOfTarget.AllyAll);
                    YsSpellSkillArgs argsEachTarget = new YsSpellSkillArgs(source, null, data, levelZeroIndexed, true);
                    IEnumerable<ICombatant> targets = (targetType == ETypeOfTarget.AllyAll) ? allies : enemies;
                    foreach (ICombatant eachTarget in targets) {
                        if (eachTarget.IsDead) { // do not allow attacks on dead targets.
                            continue;
                        }
                        argsEachTarget.Target = eachTarget;
                        World.GetWorld(source).InvokeCombat(roundPacket, argsEachTarget);
                        roundPacket.WriteFloatingMagical(eachTarget.Serial, argsEachTarget.Damage, false);
                        roundPacket.WriteUpdateStatus(eachTarget);
                        args.CopyFrom(argsEachTarget);
                    }
                    break;
            }
            // output to clients:
            useMana = args.UseMana;
            roundPacket.WriteSound(args.SoundEffect); // player spell
            roundPacket.WriteMessageEx_SpellSkill(args, targetPlayerParty, true, false); // player spell
            roundPacket.WriteSpellSuccess(args); // player spell
            if (args.KillCount > 0) {
                source.OnKilledEnemy(args.KillCount);
            }
        }

        // === Support methods ========================================================================================
        // ============================================================================================================

        /// <summary>
        /// We are in Phase2 running the combat round. Finds a random living target for this attacker.
        /// </summary>
        protected override bool Support_FindRandomLivingTarget(ICombatant attacker, bool chooseAlly, out ICombatant target) {
            target = null;
            Support_GetTeams(attacker, out List<ICombatant> allies, out List<ICombatant> enemies);
            if (chooseAlly) {
                return Support_FindRandom(allies, out target);
            }
            else {
                return Support_FindRandom(enemies, out target);
            }
        }

        /// <summary>
        /// Chooses a random target from the first row of combatants.
        /// </summary>
        /// <returns></returns>
        private bool Support_FindRandom(IEnumerable<ICombatant> cs, out ICombatant target) {
            target = null;
            List<ICombatant> validTargets = new List<ICombatant>();
            foreach (ICombatant c in cs) {
                if (YsBattleConstants.IsFrontRow(c.BattleTile) && c.IsAlive) {
                    validTargets.Add(c);
                }
            }
            if (validTargets.Count == 0) {
                return false;
            }
            int selection = Randoms.Next(validTargets.Sum(d => d.BattleGetThreat()) - 1);
            foreach (ICombatant c in validTargets) {
                int threat = c.BattleGetThreat();
                if (selection < c.BattleGetThreat()) {
                    target = c;
                    return true;
                }
                selection -= threat;
            }
            return false;
        }

        /// <summary>
        /// Gets a tile for a new monster.
        /// </summary>
        protected override int Support_GetFirstUnusedMonsterTile() {
            if (AllMonstersTeam1().Count() >= 6) { // reference ok
                return -1;
            }
            foreach (int tile in YsBattleConstants.DefaultTileOrder) {
                bool tileFilled = false;
                foreach (ICombatant m in AllMonstersTeam1()) { // reference ok
                    if (m.BattleTile == tile) {
                        tileFilled = true;
                    }
                }
                if (!tileFilled) {
                    return tile;
                }
            }
            return -1;
        }

        /// <summary>
        /// Places combantants in their requested tiles, and then bumps them to valid tiles if they are dead.
        /// If defaultCombatPlacement, then place players in default tiles.
        /// </summary>
        protected override void Support_RearrangePositions(IEnumerable<ICombatant> cs, bool defaultCombatPlacement = false) {
            if (defaultCombatPlacement) {
                int tile = 0;
                foreach (ICombatant c in cs) {
                    c.BattleTile = YsBattleConstants.DefaultTileOrder[tile++];
                }
            }
            Support_RearrangeDeadCombatants(cs, 1, 0, 4);
            Support_RearrangeDeadCombatants(cs, 2, 3, 5);
        }

        /// <summary>
        /// Called at the end of every round. Puts dead player combatants to the side or in back. Proceeds in two steps:
        /// 1. If front left is dead, then replace with (1A) the front far left, or (1B) the rear left.
        /// 2. If front right is dead, then replace with (2A) the front far right, or (2B) the rear right.
        /// </summary>
        private void Support_RearrangeDeadCombatants(IEnumerable<ICombatant> cs, int tileFront, int tileFlank, int tileRear) {
            ICombatant front = cs.FirstOrDefault(c => c.BattleTile == tileFront);
            ICombatant flank = cs.FirstOrDefault(c => c.BattleTile == tileFlank);
            ICombatant rear = cs.FirstOrDefault(c => c.BattleTile == tileRear);
            if (front == null) {
                if (rear != null) {
                    rear.BattleTile = tileFront;
                    front = rear;
                    rear = null;
                }
                else if (flank != null) {
                    flank.BattleTile = tileFront;
                    front = flank;
                    flank = null;
                }
            }
            if (front?.IsDead == true) {
                if (flank != null && !(flank?.IsDead == true)) {
                    flank.BattleTile = tileFront;
                    if (front != null) {
                        front.BattleTile = tileFlank;
                    }
                }
                else if (rear != null && !(rear?.IsDead == true)) {
                    rear.BattleTile = tileFront;
                    if (front != null) {
                        front.BattleTile = (flank == null) ? tileFlank : tileRear;
                    }
                }
            }
        }
    }
}
