using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using System;
using System.Linq;

namespace AmmoBalance
{
    public class AmmoBalancePlugin : Plugin<Config>
    {
        public override string Name => "AmmoBalance";
        public override string Author => "JastrzabDev";
        public override Version Version => new Version(3, 1, 0);
        public override string Prefix => "ammo_balance";

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
            Player player = ev.Player;
            if (player == null || player.Role == null) return;

            // Tylko MTF i Chaos
            Team team = player.Role.Team;
            if (team != Team.FoundationForces && team != Team.ChaosInsurgency) return;

            RoleTypeId role = player.Role.Type;
            if (Config.ExcludedRoles.Contains(role)) return;

            Timing.CallDelayed(Config.Delay, () =>
            {
                // Gracz mógł wyjść, zginąć albo zmienić rolę
                if (player == null || !player.IsAlive || player.Role.Type != role) return;

                foreach (Firearm firearm in player.Items.OfType<Firearm>().ToList())
                {
                    AmmoType ammoType = firearm.AmmoType;
                    if (ammoType == AmmoType.None) continue;

                    if (!Config.MaxAmmo.TryGetValue(ammoType, out ushort max)) continue;

                    player.SetAmmo(ammoType, max);

                    if (Config.Debug)
                        Log.Debug($"{player.Nickname} ({role}): {firearm.Type} -> {ammoType} = {max}");
                }
            });
        }
    }
}