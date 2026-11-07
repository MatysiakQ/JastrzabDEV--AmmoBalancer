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
        private static readonly HashSet<AmmoType> SupportedTypes = new HashSet<AmmoType>
        {
            AmmoType.Nato9,
            AmmoType.Nato556,
            AmmoType.Nato762,
            AmmoType.Ammo12Gauge,
            AmmoType.Ammo44Cal,
        };

        public bool IsEnabled { get; set; } = true;

        public bool Debug { get; set; }

        [Description("Opóźnienie uzupełnienia w sekundach (0-10). 0 = natychmiast przy spawnie.")]
        public float Delay { get; set; }

        [Description("Ogranicza przydział do realnego limitu gracza (pancerz, limity gry).")]
        public bool RespectAmmoLimits { get; set; } = true;

        [Description("Przydziela amunicję także przy zmianie roli z zachowaniem ekwipunku.")]
        public bool ApplyToPreservedInventory { get; set; }

        [Description("Role pomijane przez plugin. Pusta lista nie wyklucza żadnej roli.")]
        public List<RoleTypeId> ExcludedRoles { get; set; } = CreateDefaultExcludedRoles();

        [Description("Docelowa rezerwa amunicji przy spawnie. Kalibry spoza listy nie są zmieniane.")]
        public Dictionary<AmmoType, ushort> MaxAmmo { get; set; } = new Dictionary<AmmoType, ushort>
        {
            { AmmoType.Nato9, 200 },
            { AmmoType.Nato556, 200 },
            { AmmoType.Nato762, 200 },
            { AmmoType.Ammo12Gauge, 54 },
            { AmmoType.Ammo44Cal, 48 },
        };

        internal void Normalize(Action<string> warn)
        {
            if (float.IsNaN(Delay) || Delay < 0f || Delay > 10f)
            {
                warn("AmmoBalance: delay must be between 0 and 10 seconds; using 0.");
                Delay = 0f;
            }

            if (ExcludedRoles == null)
            {
                warn("AmmoBalance: excluded_roles is null; restoring FacilityGuard. Use [] to exclude no roles.");
                ExcludedRoles = CreateDefaultExcludedRoles();
            }

            if (MaxAmmo == null)
            {
                warn("AmmoBalance: max_ammo is null; no ammo will be assigned. Use {} for an empty mapping.");
                MaxAmmo = new Dictionary<AmmoType, ushort>();
            }

            foreach (AmmoType type in MaxAmmo.Keys.Where(IsUnsupported).ToArray())
            {
                warn($"AmmoBalance: ignoring unsupported ammo type {type}.");
                MaxAmmo.Remove(type);
            }
        }

        private static bool IsUnsupported(AmmoType type) => !SupportedTypes.Contains(type);

        private static List<RoleTypeId> CreateDefaultExcludedRoles()
        {
            return new List<RoleTypeId> { RoleTypeId.FacilityGuard };
        }
    }
}
