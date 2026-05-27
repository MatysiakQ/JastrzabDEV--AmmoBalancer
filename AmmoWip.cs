using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;

namespace MatysiakAmmoWipe
{
    public class FreshAmmoPlugin : Plugin<Config>
    {
        public override string Name => "FreshAmmoRules";
        public override string Author => "JastrzabDev";
        public override Version Version => new Version(2, 0, 0);
        public override string Prefix => "fresh_ammo";

        public override void OnEnabled()
        {
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            base.OnDisabled();
        }

        private void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev.Player == null || ev.Player.Role == null) return;

            Team team = ev.Player.Role.Team;
            RoleTypeId initialRole = ev.Player.Role.Type;

            if (team != Team.FoundationForces && team != Team.ChaosInsurgency)
                return;

            Timing.CallDelayed(1.5f, () =>
            {
                if (ev.Player == null || !ev.Player.IsAlive || ev.Player.Role.Type != initialRole) return;

                List<AmmoType> processedAmmo = new List<AmmoType>();

                foreach (var item in ev.Player.Items)
                {
                    if (item is Firearm firearm)
                    {
                        AmmoType ammoType = (AmmoType)firearm.AmmoType;

                        if (processedAmmo.Contains(ammoType)) continue;
                        processedAmmo.Add(ammoType);

                        if (initialRole == RoleTypeId.FacilityGuard)
                        {
                            ev.Player.AddAmmo(ammoType, this.Config.GuardExtraAmmo);
                        }
                        else
                        {
                            ushort maxAmmo = GetMaxAmmoLimit(ammoType);
                            ev.Player.SetAmmo(ammoType, maxAmmo);
                        }
                    }
                }
            });
        }

        private ushort GetMaxAmmoLimit(AmmoType ammoType)
        {
            switch (ammoType)
            {
                case AmmoType.Nato556: return this.Config.MaxAmmo556;
                case AmmoType.Nato762: return this.Config.MaxAmmo762;
                case AmmoType.Nato9: return this.Config.MaxAmmo9mm;
                case AmmoType.Ammo12Gauge: return this.Config.MaxAmmo12Gauge;
                case AmmoType.Ammo44Cal: return this.Config.MaxAmmo44Cal;
                default: return 0;
            }
        }
    }
}