// Minimal test doubles. Production compilation uses the real REFERENCES/*.dll.
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayerRoles
{
    public enum Team { SCPs, FoundationForces, ChaosInsurgency, Scientists, Dead }
    public enum RoleTypeId { Spectator, FacilityGuard, NtfSergeant, NtfPrivate, ChaosRifleman, Scientist, Scp049 }
    [Flags]
    public enum RoleSpawnFlags { None = 0, AssignInventory = 1, All = 3 }
}

namespace Exiled.API.Enums
{
    public enum AmmoType { None, Nato9, Nato556, Nato762, Ammo12Gauge, Ammo44Cal }
}

namespace Exiled.API.Interfaces
{
    public interface IConfig { bool IsEnabled { get; set; } bool Debug { get; set; } }
}

namespace Exiled.API.Features.Items
{
    public class Item { }
    public class Firearm : Item
    {
        public Firearm(Exiled.API.Enums.AmmoType type) { AmmoType = type; }
        public Exiled.API.Enums.AmmoType AmmoType { get; }
    }
}

namespace Exiled.API.Features
{
    using Exiled.API.Enums;
    using Exiled.API.Features.Items;
    using PlayerRoles;

    public class TestRole
    {
        public TestRole(RoleTypeId type, Team team) { Type = type; Team = team; }
        public RoleTypeId Type { get; }
        public Team Team { get; }
    }

    public class Player
    {
        public string Nickname { get; set; } = "Test player";
        public bool IsAlive { get; set; } = true;
        public bool IsConnected { get; set; } = true;
        public TestRole Role { get; set; } = new TestRole(RoleTypeId.NtfSergeant, Team.FoundationForces);
        public List<Item> Items { get; } = new List<Item>();
        public Dictionary<AmmoType, ushort> Ammo { get; } = new Dictionary<AmmoType, ushort>();
        public Dictionary<AmmoType, ushort> Limits { get; } = new Dictionary<AmmoType, ushort>();
        public List<KeyValuePair<AmmoType, ushort>> Writes { get; } = new List<KeyValuePair<AmmoType, ushort>>();
        public ushort GetAmmo(AmmoType type) => Ammo.TryGetValue(type, out ushort value) ? value : (ushort)0;
        public ushort GetAmmoLimit(AmmoType type) => Limits.TryGetValue(type, out ushort value) ? value : ushort.MaxValue;
        public void SetAmmo(AmmoType type, ushort value)
        {
            if (!IsConnected) throw new InvalidOperationException("Disconnected inventory accessed");
            Ammo[type] = value;
            Writes.Add(new KeyValuePair<AmmoType, ushort>(type, value));
        }
    }

    public abstract class Plugin<T> where T : new()
    {
        public T Config { get; } = new T();
        public virtual string Name => "";
        public virtual string Author => "";
        public virtual string Prefix => "";
        public virtual Version Version => new Version();
        public virtual void OnEnabled() { }
        public virtual void OnDisabled() { }
    }

    public static class Log
    {
        public static List<string> Warnings { get; } = new List<string>();
        public static List<string> Errors { get; } = new List<string>();
        public static void Warn(string message) => Warnings.Add(message);
        public static void Error(string message) => Errors.Add(message);
        public static void Debug(string message) { }
    }
}

namespace Exiled.Events.EventArgs.Player
{
    public class SpawnedEventArgs
    {
        public SpawnedEventArgs(Exiled.API.Features.Player player, PlayerRoles.RoleSpawnFlags flags = PlayerRoles.RoleSpawnFlags.All)
        { Player = player; SpawnFlags = flags; }
        public Exiled.API.Features.Player Player { get; }
        public PlayerRoles.RoleSpawnFlags SpawnFlags { get; }
    }
    public class LeftEventArgs
    {
        public LeftEventArgs(Exiled.API.Features.Player player) { Player = player; }
        public Exiled.API.Features.Player Player { get; }
    }
}

namespace Exiled.Events.Handlers
{
    public static class Player
    {
        public static event Action<Exiled.Events.EventArgs.Player.SpawnedEventArgs> Spawned;
        public static event Action<Exiled.Events.EventArgs.Player.LeftEventArgs> Left;
        public static void Spawn(Exiled.API.Features.Player player, PlayerRoles.RoleSpawnFlags flags = PlayerRoles.RoleSpawnFlags.All)
            => Spawned?.Invoke(new Exiled.Events.EventArgs.Player.SpawnedEventArgs(player, flags));
        public static void Disconnect(Exiled.API.Features.Player player)
        { player.IsConnected = false; Left?.Invoke(new Exiled.Events.EventArgs.Player.LeftEventArgs(player)); }
    }
    public static class Server
    {
        public static event Action RestartingRound;
        public static event Action WaitingForPlayers;
        public static void Restart() => RestartingRound?.Invoke();
        public static void Wait() => WaitingForPlayers?.Invoke();
    }
}

namespace MEC
{
    public struct CoroutineHandle { public int Id; }
    public static class Timing
    {
        private sealed class Work { public int Id; public float Due; public Action Action; }
        private static readonly List<Work> pending = new List<Work>();
        private static int nextId;
        private static float now;
        public static int Count => pending.Count;
        public static Action LastScheduled { get; private set; }
        public static CoroutineHandle CallDelayed(float delay, Action action)
        {
            int id = ++nextId;
            pending.Add(new Work { Id = id, Due = now + delay, Action = action });
            LastScheduled = action;
            return new CoroutineHandle { Id = id };
        }
        public static int KillCoroutines(CoroutineHandle handle) => pending.RemoveAll(work => work.Id == handle.Id);
        public static void Advance(float seconds)
        {
            now += seconds;
            foreach (Work work in pending.Where(work => work.Due <= now).OrderBy(work => work.Due).ToArray())
            { pending.Remove(work); work.Action(); }
        }
        public static void Reset() { pending.Clear(); now = 0; LastScheduled = null; }
    }
}
