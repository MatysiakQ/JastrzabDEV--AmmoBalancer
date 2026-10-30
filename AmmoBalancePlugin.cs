using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
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
        private bool active;

        public override string Name => "AmmoBalance";
        public override string Author => "JastrzabDev";
        public override Version Version => new Version(3, 2, 0);
        public override string Prefix => "ammo_balance";

        public override void OnEnabled()
        {
            Config.Normalize(Log.Warn);
            active = true;
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.Left += OnLeft;
            Exiled.Events.Handlers.Server.RestartingRound += CancelAll;
            Exiled.Events.Handlers.Server.WaitingForPlayers += CancelAll;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            active = false;
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

            // Cancel only after successful changes; ChangingRole may still be denied.
            Cancel(player);
            Config.Normalize(Log.Warn);
            object spawnedRole = player.Role;
            if (!CanApply(player, spawnedRole))
                return;

            if (!Config.ApplyToPreservedInventory && !ev.SpawnFlags.HasFlag(RoleSpawnFlags.AssignInventory))
            {
                Debug($"{player.Nickname}: preserved inventory, skipping starting ammo.");
                return;
            }

            try
            {
                var grants = new List<AmmoGrant>();
                // The standard inventory is already assigned before EXILED raises Spawned.
                foreach (AmmoType type in GetWeaponAmmoTypes(player))
                {
                    if (!Config.MaxAmmo.TryGetValue(type, out ushort configured))
                        continue;

                    ushort target = GetTarget(player, type, configured);
                    ushort initial = player.GetAmmo(type);
                    if (Config.Delay == 0f || initial > target)
                    {
                        // Trim excess before it can be dropped, even when top-ups are delayed.
                        player.SetAmmo(type, target);
                        Debug($"{player.Nickname} ({player.Role.Type}): {type} {initial} -> {target}.");
                    }
                    else if (initial < target)
                    {
                        grants.Add(new AmmoGrant(type, target, (ushort)(target - initial)));
                    }
                }

                if (grants.Count == 0)
                    return;

                var request = new PendingGrant(spawnedRole, grants);
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
            // Identity also rejects callbacks belonging to an earlier enable/round.
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
                    // Add only the spawn-time deficit. Dropping/reloading cannot create
                    // another full reserve. Use int to prevent ushort overflow.
                    ushort after = (ushort)Math.Min(target, (int)before + grant.Additional);
                    player.SetAmmo(grant.Type, after);
                    Debug($"{player.Nickname} ({player.Role.Type}): {grant.Type} {before} -> {after} (delayed).");
                }
            }
            catch (Exception exception)
            {
                Log.Error($"{Name}: could not complete starting ammo: {exception}");
            }
        }

        private bool CanApply(Player player, object spawnedRole)
        {
            if (!active || !Config.IsEnabled || player == null || !player.IsConnected || !player.IsAlive)
                return false;

            // EXILED replaces the cached wrapper even on same-class spawns.
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
            return new HashSet<AmmoType>(player.Items.OfType<Firearm>()
                .Select(firearm => firearm.AmmoType).Where(type => type != AmmoType.None));
        }

        private void OnLeft(LeftEventArgs ev) => Cancel(ev.Player);

        private void Cancel(Player player)
        {
            if (player != null && pending.TryGetValue(player, out PendingGrant request))
            {
                pending.Remove(player);
                Timing.KillCoroutines(request.Handle);
            }
        }

        private void CancelAll()
        {
            PendingGrant[] requests = pending.Values.ToArray();
            pending.Clear();
            foreach (PendingGrant request in requests)
                Timing.KillCoroutines(request.Handle);
        }

        private void Debug(string message)
        {
            if (Config.Debug)
                Log.Debug(message);
        }

        private sealed class PendingGrant
        {
            public PendingGrant(object role, List<AmmoGrant> grants)
            {
                Role = role;
                Grants = grants;
            }

            public object Role { get; }
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
