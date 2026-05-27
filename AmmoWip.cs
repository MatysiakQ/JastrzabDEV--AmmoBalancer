using Exiled.API.Features;
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
        public override Version Version => new Version(3, 0, 0);
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

            RoleTypeId initialRole = ev.Player.Role.Type;

            Timing.CallDelayed(1.5f, () =>
            {
                if (ev.Player == null || !ev.Player.IsAlive || ev.Player.Role.Type != initialRole) return;

                if (this.Config.RoleAmmoLimits.TryGetValue(initialRole, out var ammoConfig))
                {
                    foreach (var ammoEntry in ammoConfig)
                    {
                        ev.Player.SetAmmo(ammoEntry.Key, ammoEntry.Value);
                    }
                }
            });
        }
    }
}