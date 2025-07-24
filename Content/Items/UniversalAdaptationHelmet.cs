using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using System.Collections.Generic;

namespace SmartTerraria.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class UniversalAdaptationHelmet : ModItem
    {
        public static int CurrentDefense { get; private set; } = 0;
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.defense = 1000;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Green;
            CurrentDefense = Item.defense; // Устанавливаем начальную защиту
        }
        public static float CurrentBaseBonus { get; private set; } = 0f;
        public static float CurrentAdditionalBonus { get; private set; } = 0f;
        
        public override void UpdateEquip(Player player)
        { 
            CurrentDefense = Item.defense; // Обновляем текущую защиту
            SmartTerrariaPlayer smartPlayer = player.GetModPlayer<SmartTerrariaPlayer>();
            DamageTrackerPlayer damageTracker = player.GetModPlayer<DamageTrackerPlayer>();
            // Базовый бонус
            float baseBonus = smartPlayer.hasBaseBonus ? 0.05f : 0f; // 5% после первого босса
            CurrentBaseBonus = baseBonus;
            // Дополнительный бонус
            float additionalBonus = smartPlayer.hasAdditionalBonus ? (smartPlayer.bossesKilled - 1) * 0.01666666666666667f : 0f; // Начинается после второго босса
            CurrentAdditionalBonus = additionalBonus;
            // Получаем доминирующий класс урона
            DamageClass highestClass = smartPlayer.GetHighestDamageClass();

            // Применяем бонус только к доминирующему классу
            if (highestClass != DamageClass.Generic)
            {
                player.GetDamage(highestClass) += baseBonus;
            }
            if (highestClass != DamageClass.Generic)
            {
                player.GetDamage(highestClass) += additionalBonus;
            }
            
            // Удалён блок, связанный с условием получения иммунитета к огню

            // Иммунитеты
            

            // Эффекты под землёй
            if (player.position.Y > Main.worldSurface * 16 && smartPlayer.bossesKilled >= 4)
            {
                player.ClearBuff(BuffID.Shine);
                player.AddBuff(BuffID.Shine, 1);
            }
            if (player.position.Y > Main.worldSurface * 16 && smartPlayer.bossesKilled >= 6)
            {
                player.ClearBuff(BuffID.Mining);
                player.AddBuff(BuffID.Mining, 1);
            }
            if (player.position.Y > Main.worldSurface * 16 && smartPlayer.bossesKilled >= 8)
            {
                player.ClearBuff(BuffID.Spelunker);
                player.AddBuff(BuffID.Spelunker, 1);
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.RemoveAll(line => line.Name != "ItemName");
        }
    }
}
