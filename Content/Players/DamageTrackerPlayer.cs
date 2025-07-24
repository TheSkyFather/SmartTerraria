using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader.IO;
using System.Collections.Generic;
using SmartTerraria.Content.Items.Armor;

namespace SmartTerraria
{
    public class DamageTrackerPlayer : ModPlayer
    {
        public float meleeDamageDealt;
        public float rangedDamageDealt;
        public float magicDamageDealt;
        public float summonDamageDealt;

        public override void Initialize()
        {
            meleeDamageDealt = 0;
            rangedDamageDealt = 0;
            magicDamageDealt = 0;
            summonDamageDealt = 0;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (item.DamageType.CountsAsClass<MeleeDamageClass>())
            {
                meleeDamageDealt += damageDone;
                if (Player.ZoneJungle)
                    Player.GetModPlayer<SmartTerrariaPlayer>().AddJungleDamage(damageDone);
                UpdateHelmTooltips();
            }
            else if (item.DamageType.CountsAsClass<RangedDamageClass>())
            {
                rangedDamageDealt += damageDone;
                if (Player.ZoneJungle)
                    Player.GetModPlayer<SmartTerrariaPlayer>().AddJungleDamage(damageDone);
                UpdateHelmTooltips();
            }
            else if (item.DamageType.CountsAsClass<MagicDamageClass>())
            {
                magicDamageDealt += damageDone;
                if (Player.ZoneJungle)
                    Player.GetModPlayer<SmartTerrariaPlayer>().AddJungleDamage(damageDone);
                UpdateHelmTooltips();
            }
            else if (item.DamageType.CountsAsClass<SummonDamageClass>())
            {
                summonDamageDealt += damageDone;
                if (Player.ZoneJungle)
                    Player.GetModPlayer<SmartTerrariaPlayer>().AddJungleDamage(damageDone);
                UpdateHelmTooltips();
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.DamageType.CountsAsClass<MeleeDamageClass>())
            {
                meleeDamageDealt += damageDone;
                if (Player.ZoneJungle)
                    Player.GetModPlayer<SmartTerrariaPlayer>().AddJungleDamage(damageDone);
                UpdateHelmTooltips();
            }
            else if (proj.DamageType.CountsAsClass<RangedDamageClass>())
            {
                rangedDamageDealt += damageDone;
                if (Player.ZoneJungle)
                    Player.GetModPlayer<SmartTerrariaPlayer>().AddJungleDamage(damageDone);
                UpdateHelmTooltips();
            }
            else if (proj.DamageType.CountsAsClass<MagicDamageClass>())
            {
                magicDamageDealt += damageDone;
                if (Player.ZoneJungle)
                    Player.GetModPlayer<SmartTerrariaPlayer>().AddJungleDamage(damageDone);
                UpdateHelmTooltips();
            }
            else if (proj.DamageType.CountsAsClass<SummonDamageClass>())
            {
                summonDamageDealt += damageDone;
                if (Player.ZoneJungle)
                    Player.GetModPlayer<SmartTerrariaPlayer>().AddJungleDamage(damageDone);
                UpdateHelmTooltips();
            }
        }
		
		private void UpdateHelmTooltips()
		{
			if (Player.whoAmI == Main.myPlayer)
			{
				foreach (var item in Player.armor)
				{
					if (item.type == ModContent.ItemType<UniversalAdaptationHelmet>())
					{
						item.ModItem.ModifyTooltips(new List<TooltipLine>());
					}
				}
			}
		}

		public override void LoadData(TagCompound tag)
		{     
			meleeDamageDealt = tag.GetFloat("meleeDamageDealt");
			rangedDamageDealt = tag.GetFloat("rangedDamageDealt");
			magicDamageDealt = tag.GetFloat("magicDamageDealt");
			summonDamageDealt = tag.GetFloat("summonDamageDealt");
		}

		public override void SaveData(TagCompound tag)
		{
			tag["meleeDamageDealt"] = meleeDamageDealt;
			tag["rangedDamageDealt"] = rangedDamageDealt;
			tag["magicDamageDealt"] = magicDamageDealt;
			tag["summonDamageDealt"] = summonDamageDealt;
		}
    }
}
