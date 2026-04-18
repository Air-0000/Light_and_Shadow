using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Light_and_Shadow.Content.Items.Stuffs.Weapons
{
	public class CalamitySword : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 10;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.DamageType = DamageClass.Melee;
			Item.autoReuse = true;
			Item.damage = 1000000000;

			Item.knockBack = 15f;
			Item.value = Item.buyPrice(platinum: 10);
			Item.rare = ItemRarityID.Red;
			Item.UseSound = SoundID.Item71;

			Item.shoot = ProjectileID.TerraBeam;
			Item.shootSpeed = 30f;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			position = player.Center + Vector2.Normalize(velocity) * 35f;

			// ✅ 修复来源：Item.GetSource_ItemUse → player.GetSource_ItemUse
			Projectile p = Projectile.NewProjectileDirect(
				player.GetSource_ItemUse(Item),
				position, velocity, type, damage, knockback, player.whoAmI);

			p.tileCollide = false; // 穿墙
			p.penetrate = 10;      // 穿透10敌

			type = 0; // 阻止原版重复生成
		}
	}
}