using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.Sound;

namespace BananaTraps
{
    public class Projectile_BananaSplosive : Projectile_Explosive
	{
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.ticksToDetonation, "ticksToDetonation", 0, false);
		}
		public override void Tick()
		{
			base.Tick();
			if (this.ticksToDetonation > 0)
			{
				this.ticksToDetonation--;
				if (this.ticksToDetonation <= 0)
				{
					this.Explode();
				}
			}
		}
		protected override void Impact(Thing hitThing)
		{
			if (this.def.projectile.explosionDelay == 0)
			{
				this.Explode();
				return;
			}
			this.landed = true;
			this.ticksToDetonation = this.def.projectile.explosionDelay;
			GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(this, this.def.projectile.damageDef, this.launcher.Faction, this.launcher);
		}
		protected override void Explode()
		{
			Map map = base.Map;
			
			if (Rand.Value > this.def.projectile.preExplosionSpawnChance && base.Position.InBounds(map) && base.Position.Walkable(map))
			{
				Thing spawnedThing = ThingMaker.MakeThing(this.def.projectile.preExplosionSpawnThingDef, null);
				spawnedThing.SetFaction(this.launcher.Faction);
				spawnedThing.stackCount = this.def.projectile.preExplosionSpawnThingCount;
				GenSpawn.Spawn(spawnedThing, base.Position, base.Map, WipeMode.Vanish);
			}
			this.Destroy(DestroyMode.Vanish);
			if (this.def.projectile.explosionEffect != null)
			{
				Effecter effecter = this.def.projectile.explosionEffect.Spawn();
				effecter.Trigger(new TargetInfo(base.Position, map, false), new TargetInfo(base.Position, map, false));
				effecter.Cleanup();
			}
			IntVec3 position = base.Position;
			Map map2 = map;
			float explosionRadius = this.def.projectile.explosionRadius;
			DamageDef damageDef = this.def.projectile.damageDef;
			Thing launcher = this.launcher;
			int damageAmount = base.DamageAmount;
			float armorPenetration = base.ArmorPenetration;
			SoundDef soundExplode = this.def.projectile.soundExplode;
			ThingDef equipmentDef = this.equipmentDef;
			ThingDef def = this.def;
			Thing thing = this.intendedTarget.Thing;
			ThingDef postExplosionSpawnThingDef = this.def.projectile.postExplosionSpawnThingDef;
			float postExplosionSpawnChance = this.def.projectile.postExplosionSpawnChance;
			int postExplosionSpawnThingCount = this.def.projectile.postExplosionSpawnThingCount;
			GenExplosion.DoExplosion(position, map2, explosionRadius, damageDef, launcher, damageAmount, armorPenetration, soundExplode, equipmentDef, def, thing, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, this.def.projectile.applyDamageToExplosionCellsNeighbors, null, 0, 0, this.def.projectile.explosionChanceToStartFire, this.def.projectile.explosionDamageFalloff, new float?(this.origin.AngleToFlat(this.destination)), null);
		}
		private int ticksToDetonation;
	}
	public class Building_TrapBanana : Building_TrapDamager
    {
		[DefOf]
		public static class BananaDefOf
        {
			public static HediffDef Slipped;
			public static SoundDef SlippedSound;
		}
		protected override void SpringSub(Pawn p)
        {
			BananaDefOf.SlippedSound.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
			if (p == null)
			{
				return;
			}
			float num = this.GetStatValue(StatDefOf.TrapMeleeDamage, true) * Building_TrapBanana.DamageRandomFactorRange.RandomInRange / Building_TrapBanana.DamageCount;
			float armorPenetration = num * 0.015f;
			int num2 = 0;
			while ((float)num2 < Building_TrapBanana.DamageCount)
			{
				DamageInfo dinfo = new DamageInfo(DamageDefOf.Stab, num, armorPenetration, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true);
				DamageWorker.DamageResult damageResult = p.TakeDamage(dinfo);
				if (num2 == 0)
				{
					BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(p, RulePackDefOf.DamageEvent_TrapSpike, null);
					Find.BattleLog.Add(battleLogEntry_DamageTaken);
					damageResult.AssociateWithLog(battleLogEntry_DamageTaken);
				}
				num2++;
			}
			p.health.AddHediff(BananaDefOf.Slipped);
        }
		private static readonly FloatRange DamageRandomFactorRange = new FloatRange(0.8f, 1.2f);
		private static readonly float DamageCount = 5f;
	}
}
