using Exiled.API.Enums;
using Exiled.API.Interfaces;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AmmoBalance
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        [Description("Opcjonalne opóźnienie uzupełnienia (0–10 s). Domyślnie 0: ekwipunek jest już nadany w Spawned. Nie odnawia wyrzuconej/zużytej amunicji.")]
        public float Delay { get; set; } = 0f;

        [Description("Respektuj limit amunicji gracza, w tym pancerz i limity gry. False celowo pozwala je przekroczyć.")]
        public bool RespectAmmoLimits { get; set; } = true;

        [Description("Przydzielaj amunicję również przy zmianach roli z zachowaniem ekwipunku. Domyślnie takie zmiany są pomijane.")]
        public bool ApplyToPreservedInventory { get; set; } = false;

        [Description("Role pomijane przez plugin. [] nie wyklucza żadnej roli; null przywraca FacilityGuard.")]
        public List<RoleTypeId> ExcludedRoles { get; set; } = new List<RoleTypeId> { RoleTypeId.FacilityGuard };

        [Description("Docelowa rezerwa amunicji na spawn, nie stały limit zbierania. Typy nieobecne i pusty słownik są pomijane. Null wyłącza przydziały.")]
        public Dictionary<AmmoType, ushort> MaxAmmo { get; set; } = new Dictionary<AmmoType, ushort>
        {
            { AmmoType.Nato9, 200 },
            { AmmoType.Nato556, 200 },
            { AmmoType.Nato762, 200 },
            { AmmoType.Ammo12Gauge, 54 },
            { AmmoType.Ammo44Cal, 48 }
        };

        internal void Normalize(Action<string> warn)
        {
            if (float.IsNaN(Delay) || float.IsInfinity(Delay) || Delay < 0f || Delay > 10f)
            {
                warn("AmmoBalance: delay must be finite and between 0 and 10 seconds; using 0.");
                Delay = 0f;
            }

            if (ExcludedRoles == null)
            {
                warn("AmmoBalance: excluded_roles is null; restoring FacilityGuard. Use [] to exclude no roles.");
                ExcludedRoles = new List<RoleTypeId> { RoleTypeId.FacilityGuard };
            }

            if (MaxAmmo == null)
            {
                warn("AmmoBalance: max_ammo is null; no ammo will be assigned. Use {} for an empty mapping.");
                MaxAmmo = new Dictionary<AmmoType, ushort>();
            }

            foreach (AmmoType type in MaxAmmo.Keys.Where(type => type == AmmoType.None || !Enum.IsDefined(typeof(AmmoType), type)).ToArray())
            {
                warn($"AmmoBalance: ignoring unsupported ammo type {type}.");
                MaxAmmo.Remove(type);
            }
        }
    }
}
