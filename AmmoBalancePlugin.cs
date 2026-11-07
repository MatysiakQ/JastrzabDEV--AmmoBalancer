using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmmoBalance
{
    public class AmmoBalancePlugin : Plugin<Config>
    {
        private readonly Dictionary<Player, PendingGrant> pending = new Dictionary<Player, PendingGrant>();

        public override string Name => "AmmoBalance";

        public override string Author => "JastrzabDev";

        public override Version Version => new Version(3, 2, 0);

        public override string Prefix => "ammo_balance";

        public override void OnEnabled()
        {
            Config.Normalize(Log.Warn);

            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.Left += OnLeft;
            Exiled.Events.Handlers.Server.RestartingRound += CancelAll;
            Exiled.Events.Handlers.Server.WaitingForPlayers += CancelAll;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.Left -= OnLeft;
            Exiled.Events.Handlers.Server.RestartingRound -= CancelAll;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= CancelAll;

            CancelAll();

            base.OnDisabled();
        }

        private void OnSpawned(SpawnedEventArgs ev)
        {
            Player player = ev.Player;
            if (player == null)
                return;

            Cancel(player);
            Config.Normalize(Log.Warn);

            Role role = player.Role;
            if (!CanApply(player, role))
                return;

            if (!Config.ApplyToPreservedInventory && !ev.SpawnFlags.HasFlag(RoleSpawnFlags.AssignInventory))
            {
                LogDebug($"{player.Nickname}: preserved inventory, skipping starting ammo.");
                return;
            }

            try
            {
                var grants = new List<AmmoGrant>();

                foreach (AmmoType type in GetWeaponAmmoTypes(player))
                {
                    if (!Config.MaxAmmo.TryGetValue(type, out ushort configured))
                        continue;

                    ushort target = GetTarget(player, type, configured);
                    ushort current = player.GetAmmo(type);

                    if (current == target)
                        continue;

                    if (current > target || Config.Delay == 0f)
                    {
                        player.SetAmmo(type, target);
                        LogDebug($"{player.Nickname} ({role.Type}): {type} {current} -> {target}.");
                    }
                    else
                    {
                        grants.Add(new AmmoGrant(type, target, (ushort)(target - current)));
                    }
                }

                if (grants.Count == 0)
                    return;

                var request = new PendingGrant(role, grants);
                pending[player] = request;
                request.Handle = Timing.CallDelayed(Config.Delay, () => Complete(player, request));
            }
            catch (Exception exception)
            {
                Cancel(player);
                Log.Error($"{Name}: could not prepare starting ammo: {exception}");
            }
        }

        private void Complete(Player player, PendingGrant request)
        {
            if (!pending.TryGetValue(player, out PendingGrant current) || !ReferenceEquals(current, request))
                return;

            pending.Remove(player);
            Config.Normalize(Log.Warn);

            if (!CanApply(player, request.Role))
                return;

            try
            {
                HashSet<AmmoType> weaponAmmo = GetWeaponAmmoTypes(player);

                foreach (AmmoGrant grant in request.Grants)
                {
                    if (!weaponAmmo.Contains(grant.Type) || !Config.MaxAmmo.TryGetValue(grant.Type, out ushort configured))
                        continue;

                    ushort target = GetTarget(player, grant.Type, Math.Min(grant.Target, configured));
                    ushort before = player.GetAmmo(grant.Type);
                    ushort after = (ushort)Math.Min(target, before + grant.Additional);

                    player.SetAmmo(grant.Type, after);
                    LogDebug($"{player.Nickname} ({player.Role.Type}): {grant.Type} {before} -> {after} (delayed).");
                }
            }
            catch (Exception exception)
            {
                Log.Error($"{Name}: could not complete starting ammo: {exception}");
            }
        }

        private bool CanApply(Player player, Role spawnedRole)
        {
            if (!Config.IsEnabled || player == null || !player.IsConnected || !player.IsAlive)
                return false;

            if (spawnedRole == null || !ReferenceEquals(player.Role, spawnedRole))
                return false;

            Team team = player.Role.Team;
            return (team == Team.FoundationForces || team == Team.ChaosInsurgency)
                && !Config.ExcludedRoles.Contains(player.Role.Type);
        }

        private ushort GetTarget(Player player, AmmoType type, ushort configured)
        {
            return Config.RespectAmmoLimits ? Math.Min(configured, player.GetAmmoLimit(type)) : configured;
        }

        private static HashSet<AmmoType> GetWeaponAmmoTypes(Player player)
        {
            return new HashSet<AmmoType>(player.Items
                .OfType<Firearm>()
                .Select(firearm => firearm.AmmoType)
                .Where(type => type != AmmoType.None));
        }

        private void OnLeft(LeftEventArgs ev) => Cancel(ev.Player);

        private void Cancel(Player player)
        {
            if (player == null || !pending.TryGetValue(player, out PendingGrant request))
                return;

            pending.Remove(player);
            Timing.KillCoroutines(request.Handle);
        }

        private void CancelAll()
        {
            foreach (PendingGrant request in pending.Values)
                Timing.KillCoroutines(request.Handle);

            pending.Clear();
        }

        private void LogDebug(string message)
        {
            if (Config.Debug)
                Log.Debug(message);
        }

        private sealed class PendingGrant
        {
            public PendingGrant(Role role, List<AmmoGrant> grants)
            {
                Role = role;
                Grants = grants;
            }

            public Role Role { get; }

            public List<AmmoGrant> Grants { get; }

            public CoroutineHandle Handle { get; set; }
        }

        private sealed class AmmoGrant
        {
            public AmmoGrant(AmmoType type, ushort target, ushort additional)
            {
                Type = type;
                Target = target;
                Additional = additional;
            }

            public AmmoType Type { get; }

            public ushort Target { get; }

            public ushort Additional { get; }
        }
    }
}
