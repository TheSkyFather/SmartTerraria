using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.Collections.Generic;
using SmartTerraria.Content.Items.Armor;
using Terraria.ID;
using System;

namespace SmartTerraria
{
    public class SmartTerrariaPlayer : ModPlayer
    {
        // Существующие поля для боссов и бонусов
        public int bossesKilled;
        public HashSet<string> killedBosses = new HashSet<string>();
        public bool hasBaseBonus = false;
        public bool hasAdditionalBonus = false;
        public DamageTrackerPlayer damageTracker; 
        public float meleeDamageDealt => damageTracker.meleeDamageDealt;
        public float rangedDamageDealt => damageTracker.rangedDamageDealt;
        public float magicDamageDealt => damageTracker.magicDamageDealt;
        public float summonDamageDealt => damageTracker.summonDamageDealt;

        // Новые поля для постоянного иммунитета – они обновляются только при надетом шлеме
        public bool hasOnfireImmunity = false;     // Разблокируется, если в инвентаре 200+ Hellstone
        public bool hasSilencedImmunity = false;     // Разблокируется, если в инвентаре 100+ Falling Stars

        // Остальные поля для условий дебаффов
        public float jungleDamageAccumulated = 0f;   // Для дебаффа Poisoned (15к урона в джунглях)
        public bool poisonedDebuffTriggered = false;
        public bool confusedDebuffTriggered = false;
        public bool cursedDebuffTriggered = false;
        public int weakKillCount = 0;
        public bool weakDebuffTriggered = false;
        public bool deerclopsDefeated = false;

        public override void Initialize()
        {
            bossesKilled = 0;
            killedBosses.Clear();
            hasBaseBonus = false;
            hasAdditionalBonus = false;
            damageTracker = Player.GetModPlayer<DamageTrackerPlayer>();

            hasOnfireImmunity = false;
            hasSilencedImmunity = false;
            jungleDamageAccumulated = 0f;
            poisonedDebuffTriggered = false;
            confusedDebuffTriggered = false;
            cursedDebuffTriggered = false;
            weakKillCount = 0;
            weakDebuffTriggered = false;
            deerclopsDefeated = false;
        }

        /// <summary>
        /// Проверяет, надет ли игрок шлем UniversalAdaptationHelmet.
        /// Обычно шлем находится в слоте головы (индекс 0).
        /// </summary>
        public bool IsWearingHelmet()
        {
            return Player.armor[0] != null && Player.armor[0].type == ModContent.ItemType<UniversalAdaptationHelmet>();
        }

        public override void PostUpdate()
        {
            if (!IsWearingHelmet())
            {
                Player.buffImmune[BuffID.OnFire] = false;
                Player.buffImmune[BuffID.Silenced] = false;
                Main.NewText("DEBUG: Шлем не надет!", 255, 255, 0);
                return;
            }

            // Проверка OnFire
            if (!hasOnfireImmunity)
            {
                int hellstoneCount = 0;
                foreach (Item item in Player.inventory)
                {
                    if (item.type == ItemID.Hellstone)
                        hellstoneCount += item.stack;
                }
                Main.NewText("DEBUG: Hellstone count = " + hellstoneCount, 200, 200, 200);
                if (hellstoneCount >= 200)
                {
                    hasOnfireImmunity = true;
                    Main.NewText("DEBUG: OnFire иммунитет разблокирован!", 255, 0, 0);
                }
            }
            if (hasOnfireImmunity)
            {
                Player.buffImmune[BuffID.OnFire] = true;
            }

            // Аналогично для Falling Stars
            if (!hasSilencedImmunity)
            {
                int fallingStarCount = 0;
                foreach (Item item in Player.inventory)
                {
                    if (item.type == ItemID.FallenStar)
                        fallingStarCount += item.stack;
                }
                Main.NewText("DEBUG: Falling Stars count = " + fallingStarCount, 200, 200, 200);
                if (fallingStarCount >= 100)
                {
                    hasSilencedImmunity = true;
                    Main.NewText("DEBUG: Silenced иммунитет разблокирован!", 255, 0, 0);
                }
            }
            if (hasSilencedImmunity)
            {
                Player.buffImmune[BuffID.Silenced] = true;
            }
        }

        // Метод для накопления урона в джунглях (для дебаффа Poisoned).
        // Выполняется только если шлем надет.
        public void AddJungleDamage(int damage)
        {
            if (!IsWearingHelmet())
                return;

            if (Player.ZoneJungle && !poisonedDebuffTriggered)
            {
                jungleDamageAccumulated += damage;
                if (jungleDamageAccumulated >= 15000f)
                {
                    poisonedDebuffTriggered = true;
                    Player.AddBuff(BuffID.Poisoned, 60 * 60);
                }
            }
        }

        // Метод для обработки уникальных убийств боссов (для дебаффа Cursed).
        // Выполняется только если шлем надет.
        public void IncrementBossKill(string bossName)
        {
            if (!IsWearingHelmet())
                return;

            if (!killedBosses.Contains(bossName))
            {
                bossesKilled++;
                killedBosses.Add(bossName);
                Main.NewText($"Убито боссов: {bossesKilled}", 255, 165, 0);

                if (bossesKilled == 1)
                {
                    hasBaseBonus = true;
                    Main.NewText("Получен базовый бонус 5% для текущего класса!", 255, 165, 0);
                }
                if (bossesKilled >= 2)
                {
                    hasAdditionalBonus = true;
                    Main.NewText("Получен дополнительный бонус к урону 1.66% для текущего класса!", 255, 165, 0);
                }
                // Дебафф Cursed: 8 уникальных боссов
                if (bossesKilled >= 8 && !cursedDebuffTriggered)
                {
                    cursedDebuffTriggered = true;
                    Player.AddBuff(BuffID.Cursed, 60 * 60);
                }
            }
            else
            {
                Main.NewText($"Босс {bossName} был уже убит ранее.", 255, 165, 0);
            }
        }

        public DamageClass GetHighestDamageClass()
        {
            float meleeDamage = meleeDamageDealt;
            float rangedDamage = rangedDamageDealt;
            float magicDamage = magicDamageDealt;
            float summonDamage = summonDamageDealt;
            float maxDamage = Math.Max(Math.Max(meleeDamage, rangedDamage), Math.Max(magicDamage, summonDamage));
            if (meleeDamage == maxDamage && meleeDamage > 0)
                return DamageClass.Melee;
            if (rangedDamage == maxDamage && rangedDamage > 0)
                return DamageClass.Ranged;
            if (magicDamage == maxDamage && magicDamage > 0)
                return DamageClass.Magic;
            if (summonDamage == maxDamage && summonDamage > 0)
                return DamageClass.Summon;
            return DamageClass.Generic;
        }

        public override void LoadData(TagCompound tag)
        {
            bossesKilled = tag.GetInt("bossesKilled");
            hasBaseBonus = tag.GetBool("hasBaseBonus");
            hasAdditionalBonus = tag.GetBool("hasAdditionalBonus");
            killedBosses = new HashSet<string>(tag.GetList<string>("killedBosses"));

            damageTracker.meleeDamageDealt = tag.GetFloat("meleeDamageDealt");
            damageTracker.rangedDamageDealt = tag.GetFloat("rangedDamageDealt");
            damageTracker.magicDamageDealt = tag.GetFloat("magicDamageDealt");
            damageTracker.summonDamageDealt = tag.GetFloat("summonDamageDealt");

            hasOnfireImmunity = tag.GetBool("hasOnfireImmunity");
            hasSilencedImmunity = tag.GetBool("hasSilencedImmunity");
            jungleDamageAccumulated = tag.GetFloat("jungleDamageAccumulated");
            poisonedDebuffTriggered = tag.GetBool("poisonedDebuffTriggered");
            confusedDebuffTriggered = tag.GetBool("confusedDebuffTriggered");
            cursedDebuffTriggered = tag.GetBool("cursedDebuffTriggered");
            weakKillCount = tag.GetInt("weakKillCount");
            weakDebuffTriggered = tag.GetBool("weakDebuffTriggered");
            deerclopsDefeated = tag.GetBool("deerclopsDefeated");
        }

        public override void SaveData(TagCompound tag)
        {
            tag["bossesKilled"] = bossesKilled;
            tag["hasBaseBonus"] = hasBaseBonus;
            tag["hasAdditionalBonus"] = hasAdditionalBonus;
            tag["killedBosses"] = new List<string>(killedBosses);

            tag["meleeDamageDealt"] = damageTracker.meleeDamageDealt;
            tag["rangedDamageDealt"] = damageTracker.rangedDamageDealt;
            tag["magicDamageDealt"] = damageTracker.magicDamageDealt;
            tag["summonDamageDealt"] = damageTracker.summonDamageDealt;

            tag["hasOnfireImmunity"] = hasOnfireImmunity;
            tag["hasSilencedImmunity"] = hasSilencedImmunity;
            tag["jungleDamageAccumulated"] = jungleDamageAccumulated;
            tag["poisonedDebuffTriggered"] = poisonedDebuffTriggered;
            tag["confusedDebuffTriggered"] = confusedDebuffTriggered;
            tag["cursedDebuffTriggered"] = cursedDebuffTriggered;
            tag["weakKillCount"] = weakKillCount;
            tag["weakDebuffTriggered"] = weakDebuffTriggered;
            tag["deerclopsDefeated"] = deerclopsDefeated;
        }
    }
}
