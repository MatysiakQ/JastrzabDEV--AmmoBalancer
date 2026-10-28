using Exiled.API.Enums;
using Exiled.API.Interfaces;
using PlayerRoles;
using System.Collections.Generic;
using System.ComponentModel;

namespace AmmoBalance
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        [Description("Opóźnienie (s) po spawnie, żeby gra zdążyła nadać domyślny ekwipunek.")]
        public float Delay { get; set; } = 1f;

        [Description("Role, które mają być pominięte (np. Facility Guard).")]
        public List<RoleTypeId> ExcludedRoles { get; set; } = new List<RoleTypeId>
        {
            RoleTypeId.FacilityGuard
        };

        [Description("Maksymalna ilość amunicji dla danego typu. Ustaw zgodnie z limitami na serwerze.")]
        public Dictionary<AmmoType, ushort> MaxAmmo { get; set; } = new Dictionary<AmmoType, ushort>
        {
            { AmmoType.Nato9, 200 },
            { AmmoType.Nato556, 200 },
            { AmmoType.Nato762, 200 },
            { AmmoType.Ammo12Gauge, 54 },
            { AmmoType.Ammo44Cal, 48 }
        };
    }
}