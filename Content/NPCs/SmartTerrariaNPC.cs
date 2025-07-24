using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SmartTerraria
{
    public class SmartTerrariaNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            string bossName = string.Empty;

            switch (npc.type)
            {
                // Боссы событий
                case NPCID.MourningWood:
                    bossName = "Mourning Wood";
                    Main.NewText("Плакучий древень побежден!", 151, 65, 221);
                    break;
                case NPCID.Pumpking:
                    bossName = "Pumpking";
                    Main.NewText("Тыквенный король побежден!", 151, 65, 221);
                    break;
                case NPCID.Everscream:
                    bossName = "Everscream";
                    Main.NewText("Плакучий древень побежден!", 151, 65, 221);
                    break;
                case NPCID.IceQueen:
                    bossName = "Ice Queen";
                    Main.NewText("Ледяная королева побеждена!", 151, 65, 221);
                    break;
                case NPCID.SantaNK1:
                    bossName = "SantaNK1";
                    Main.NewText("Санта-NK1 побежден!", 151, 65, 221);
                    break;
                case NPCID.MartianSaucerCore:
                    bossName = "Martian Saucer";
                    break;
                case NPCID.TorchGod:
                    bossName = "Torch God";
                    Main.NewText("Бог факелов побежден!", 151, 65, 221);
                    break;
                    
                // Стандартные боссы
                case NPCID.Deerclops:
                    bossName = "Deerclops";
                    break;
                case NPCID.KingSlime:
                    bossName = "Король слизней";
                    break;
                case NPCID.EyeofCthulhu:
                    bossName = "Глаз Ктулуху";
                    break;
                case NPCID.EaterofWorldsHead:
                    bossName = "Пожиратель миров";
                    break;
                case NPCID.BrainofCthulhu:
                    bossName = "Мозг Ктулху";
                    break;
                case NPCID.QueenBee:
                    bossName = "Королева пчёл";
                    break;
                case NPCID.SkeletronHead:
                    bossName = "Скелетрон";
                    break;
                case NPCID.WallofFlesh:
                    bossName = "Стена плоти";
                    break;
                case NPCID.QueenSlimeBoss:
                    bossName = "Королева слизней";
                    break;
                case NPCID.TheDestroyer:
                    bossName = "Уничтожитель";
                    break;
                case NPCID.Spazmatism:
                case NPCID.Retinazer:
                    bossName = "Близнецы";
                    break;
                case NPCID.SkeletronPrime:
                    bossName = "Скелетрон-Прайм";
                    break;
                case NPCID.Plantera:
                    bossName = "Плантера";
                    break;
                case NPCID.HallowBoss:
                    bossName = "Императрица света";
                    break;
                case NPCID.Golem:
                    bossName = "Голем";
                    break;
                case NPCID.DukeFishron:
                    bossName = "Герцог Рыброн";
                    break;
                case NPCID.CultistBoss:
                    bossName = "Культист Лунатик";
                    break;
                case NPCID.MoonLordCore:
                    bossName = "Лунный Лорд";
                    break;
            }

            if (!string.IsNullOrEmpty(bossName))
            {
                Player player = Main.player[Main.myPlayer];
                SmartTerrariaPlayer stp = player.GetModPlayer<SmartTerrariaPlayer>();
                stp.IncrementBossKill(bossName);
            }
            
            // Дебафф Confused: единожды победить Brain of Cthulhu или Eater of Worlds
            if (npc.type == NPCID.BrainofCthulhu || npc.type == NPCID.EaterofWorldsHead)
            {
                Player player = Main.player[Main.myPlayer];
                SmartTerrariaPlayer stp = player.GetModPlayer<SmartTerrariaPlayer>();
                if (!stp.confusedDebuffTriggered)
                {
                    stp.confusedDebuffTriggered = true;
                    player.AddBuff(BuffID.Confused, 60 * 60);
                }
                // Дебафф Weak: 3 раза убить Brain of Cthulhu или Eater of Worlds
                stp.weakKillCount++;
                if (stp.weakKillCount >= 3 && !stp.weakDebuffTriggered)
                {
                    stp.weakDebuffTriggered = true;
                    player.AddBuff(BuffID.Weak, 60 * 60);
                }
            }
            
            // Дебафф Frostburn: убить Deerclops единожды
            if (npc.type == NPCID.Deerclops)
            {
                Player player = Main.player[Main.myPlayer];
                SmartTerrariaPlayer stp = player.GetModPlayer<SmartTerrariaPlayer>();
                if (!stp.deerclopsDefeated)
                {
                    stp.deerclopsDefeated = true;
                    player.AddBuff(BuffID.Frostburn, 60 * 60);
                }
            }
        }
    }
}
