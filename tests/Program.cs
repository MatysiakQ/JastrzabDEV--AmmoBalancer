using AmmoBalance;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using Events = Exiled.Events.Handlers.Player;
using Server = Exiled.Events.Handlers.Server;

internal static class Program
{
    private static int passed;

    private static void Main()
    {
        Test("Immediate grant uses actual ammo limit and deduplicates calibres", plugin =>
        {
            Player p = Armed(100);
            p.Items.Add(new Firearm(AmmoType.Nato556));
            p.Items.Add(new Firearm(AmmoType.Nato9));
            p.Limits[AmmoType.Nato556] = 120;
            Events.Spawn(p);
            Equal(120, p.GetAmmo(AmmoType.Nato556));
            Equal(200, p.GetAmmo(AmmoType.Nato9));
            Equal(2, p.Writes.Count);
            Equal(0, Timing.Count);
        });

        Test("Explicit limit override works", plugin =>
        {
            plugin.Config.RespectAmmoLimits = false;
            Player p = Armed(100); p.Limits[AmmoType.Nato556] = 120;
            Events.Spawn(p); Equal(200, p.GetAmmo(AmmoType.Nato556));
        });

        Test("Excluded and non-military roles are skipped", plugin =>
        {
            Player p = Armed(20);
            p.Role = new TestRole(RoleTypeId.FacilityGuard, Team.FoundationForces);
            Events.Spawn(p); Equal(0, p.Writes.Count);
            plugin.Config.ExcludedRoles.Clear();
            Events.Spawn(p); Equal(200, p.GetAmmo(AmmoType.Nato556));
            p.Writes.Clear(); p.Role = new TestRole(RoleTypeId.Scientist, Team.Scientists);
            Events.Spawn(p); Equal(0, p.Writes.Count);
        });

        Test("Unarmed, None and unconfigured calibres are skipped", plugin =>
        {
            Player p = Armed(20); p.Items.Clear(); Events.Spawn(p);
            p.Items.Add(new Firearm(AmmoType.None)); Events.Spawn(p);
            p.Items.Add(new Firearm(AmmoType.Nato556)); plugin.Config.MaxAmmo.Clear(); Events.Spawn(p);
            Equal(0, p.Writes.Count);
        });

        Test("Preserved inventories are opt-in", plugin =>
        {
            Player p = Armed(300);
            Events.Spawn(p, RoleSpawnFlags.None); Equal(300, p.GetAmmo(AmmoType.Nato556));
            plugin.Config.ApplyToPreservedInventory = true;
            Events.Spawn(p, RoleSpawnFlags.None); Equal(200, p.GetAmmo(AmmoType.Nato556));
        });

        Test("Delayed grant preserves the spawn-time budget after dropping ammo", plugin =>
        {
            plugin.Config.Delay = 5; Player p = Armed(100);
            Events.Spawn(p); p.Ammo[AmmoType.Nato556] = 40;
            Timing.Advance(5); Equal(140, p.GetAmmo(AmmoType.Nato556));
        });

        Test("Delayed grant preserves reserve spent on reloading", plugin =>
        {
            plugin.Config.Delay = 5; Player p = Armed(100);
            Events.Spawn(p); p.Ammo[AmmoType.Nato556] = 80;
            Timing.Advance(5); Equal(180, p.GetAmmo(AmmoType.Nato556));
        });

        Test("Excess ammo is trimmed before a delayed grant", plugin =>
        {
            plugin.Config.Delay = 5; Player p = Armed(300);
            Events.Spawn(p); Equal(200, p.GetAmmo(AmmoType.Nato556)); Equal(0, Timing.Count);
        });

        Test("Repeated spawn in the same class cancels the old life", plugin =>
        {
            plugin.Config.Delay = 5; Player p = Armed(100);
            Events.Spawn(p); Action old = Timing.LastScheduled;
            Timing.Advance(2);
            p.Role = new TestRole(RoleTypeId.NtfSergeant, Team.FoundationForces);
            p.Ammo[AmmoType.Nato556] = 100; Events.Spawn(p);
            old(); Timing.Advance(3); Equal(100, p.GetAmmo(AmmoType.Nato556));
            Timing.Advance(2); Equal(200, p.GetAmmo(AmmoType.Nato556)); Equal(1, p.Writes.Count);
        });

        Test("Role identity blocks a stale callback even without another event", plugin =>
        {
            plugin.Config.Delay = 5; Player p = Armed(100); Events.Spawn(p);
            p.Role = new TestRole(RoleTypeId.NtfSergeant, Team.FoundationForces);
            Timing.Advance(5); Equal(100, p.GetAmmo(AmmoType.Nato556));
        });

        Test("Successful transition to spectator cancels the pending grant", plugin =>
        {
            plugin.Config.Delay = 5; Player p = Armed(100); Events.Spawn(p);
            p.Role = new TestRole(RoleTypeId.Spectator, Team.Dead); p.IsAlive = false;
            Events.Spawn(p); Equal(0, Timing.Count);
        });

        Test("Disconnect cancels pending work", plugin =>
        {
            plugin.Config.Delay = 5; Player p = Armed(100); Events.Spawn(p);
            Action stale = Timing.LastScheduled; Events.Disconnect(p); stale();
            Equal(0, Timing.Count); Equal(0, p.Writes.Count);
        });

        Test("Disconnected or dead players are checked again at execution", plugin =>
        {
            plugin.Config.Delay = 5; Player p = Armed(100); Events.Spawn(p);
            p.IsConnected = false; Timing.Advance(5); Equal(0, p.Writes.Count);
            p.IsConnected = true; Events.Spawn(p); p.IsAlive = false;
            Timing.Advance(5); Equal(0, p.Writes.Count);
        });

        Test("Disable and re-enable reject callbacks from the old instance state", plugin =>
        {
            plugin.Config.Delay = 5; Player p = Armed(100); Events.Spawn(p);
            Action old = Timing.LastScheduled; plugin.OnDisabled(); Equal(0, Timing.Count);
            plugin.OnEnabled(); Events.Spawn(p); old(); Equal(100, p.GetAmmo(AmmoType.Nato556));
            Timing.Advance(5); Equal(200, p.GetAmmo(AmmoType.Nato556)); Equal(1, p.Writes.Count);
        });

        Test("Round restart and waiting state cancel all players", plugin =>
        {
            plugin.Config.Delay = 5; Player a = Armed(100), b = Armed(100);
            Events.Spawn(a); Events.Spawn(b); Equal(2, Timing.Count);
            Action stale = Timing.LastScheduled; Server.Restart(); stale(); Equal(0, Timing.Count);
            Events.Spawn(a); Server.Wait(); Timing.Advance(5);
            Equal(0, a.Writes.Count); Equal(0, b.Writes.Count);
        });

        Test("Live is_enabled and exclusions are honored before completion", plugin =>
        {
            plugin.Config.Delay = 5; Player p = Armed(100); Events.Spawn(p);
            plugin.Config.IsEnabled = false; Timing.Advance(5); Equal(0, p.Writes.Count);
            plugin.Config.IsEnabled = true; Events.Spawn(p);
            plugin.Config.ExcludedRoles.Add(RoleTypeId.NtfSergeant);
            Timing.Advance(5); Equal(0, p.Writes.Count);
        });

        Test("A removed gun does not receive a delayed calibre grant", plugin =>
        {
            plugin.Config.Delay = 5; Player p = Armed(100); Events.Spawn(p);
            p.Items.Clear(); p.Items.Add(new Firearm(AmmoType.Nato9));
            Timing.Advance(5); Equal(0, p.Writes.Count);
        });

        Test("Armor and config reductions are respected at completion", plugin =>
        {
            plugin.Config.Delay = 5; Player p = Armed(100); Events.Spawn(p);
            p.Limits[AmmoType.Nato556] = 120; Timing.Advance(5); Equal(120, p.GetAmmo(AmmoType.Nato556));
            p.Ammo[AmmoType.Nato556] = 50; Events.Spawn(p);
            plugin.Config.MaxAmmo[AmmoType.Nato556] = 80;
            Timing.Advance(5); Equal(80, p.GetAmmo(AmmoType.Nato556));
        });

        Test("Pickups and large numbers do not exceed the target or overflow", plugin =>
        {
            plugin.Config.Delay = 5; Player p = Armed(100); Events.Spawn(p);
            p.Ammo[AmmoType.Nato556] = 180; Timing.Advance(5); Equal(200, p.GetAmmo(AmmoType.Nato556));
            plugin.Config.MaxAmmo[AmmoType.Nato556] = ushort.MaxValue;
            p.Ammo[AmmoType.Nato556] = 0; Events.Spawn(p);
            p.Ammo[AmmoType.Nato556] = 65000; Timing.Advance(5); Equal(65535, p.GetAmmo(AmmoType.Nato556));
        });

        Test("Null collections and invalid enum keys are normalized", plugin =>
        {
            plugin.Config.ExcludedRoles = null; plugin.Config.MaxAmmo = null;
            Player p = Armed(100); Events.Spawn(p); Equal(0, p.Writes.Count);
            Equal(1, plugin.Config.ExcludedRoles.Count);
            plugin.Config.MaxAmmo[AmmoType.None] = 100;
            plugin.Config.MaxAmmo[(AmmoType)999] = 100;
            Events.Spawn(p); Equal(0, plugin.Config.MaxAmmo.Count);
        });

        Test("Invalid delays reset to immediate assignment", plugin =>
        {
            foreach (float delay in new[] { -1f, 11f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            {
                plugin.Config.Delay = delay; Player p = Armed(100); Events.Spawn(p);
                Equal(200, p.GetAmmo(AmmoType.Nato556)); Equal(0, Timing.Count);
            }
        });

        Console.WriteLine($"PASS: {passed} regression scenarios (production sources with API/scheduler doubles).");
    }

    private static Player Armed(ushort initial)
    {
        var p = new Player(); p.Items.Add(new Firearm(AmmoType.Nato556));
        p.Ammo[AmmoType.Nato556] = initial; return p;
    }

    private static void Test(string name, Action<AmmoBalancePlugin> run)
    {
        Timing.Reset(); Log.Errors.Clear(); Log.Warnings.Clear();
        var plugin = new AmmoBalancePlugin(); plugin.OnEnabled();
        try
        {
            run(plugin); Equal(0, Log.Errors.Count); passed++; Console.WriteLine("PASS " + name);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("FAIL " + name, exception);
        }
        finally { plugin.OnDisabled(); }
        Equal(0, Timing.Count);
    }

    private static void Equal(int expected, int actual)
    {
        if (expected != actual) throw new Exception($"Expected {expected}, got {actual}");
    }
}
