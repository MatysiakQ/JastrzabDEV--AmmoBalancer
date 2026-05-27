using Exiled.API.Enums;
using Exiled.API.Interfaces;
using PlayerRoles;
using System.Collections.Generic;

namespace MatysiakAmmoWipe
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        public Dictionary<RoleTypeId, Dictionary<AmmoType, ushort>> RoleAmmoLimits { get; set; } = new Dictionary<RoleTypeId, Dictionary<AmmoType, ushort>>
        {
            {
                RoleTypeId.FacilityGuard, new Dictionary<AmmoType, ushort>
                {
                    { AmmoType.Nato9, 60 }
                }
            },
            {
                RoleTypeId.NtfPrivate, new Dictionary<AmmoType, ushort>
                {
                    { AmmoType.Nato556, 120 },
                    { AmmoType.Nato9, 30 }
                }
            },
            {
                RoleTypeId.NtfSergeant, new Dictionary<AmmoType, ushort>
                {
                    { AmmoType.Nato556, 160 },
                    { AmmoType.Nato9, 30 }
                }
            },
            {
                RoleTypeId.NtfCaptain, new Dictionary<AmmoType, ushort>
                {
                    { AmmoType.Nato556, 200 },
                    { AmmoType.Nato9, 60 }
                }
            },
            {
                RoleTypeId.NtfSpecialist, new Dictionary<AmmoType, ushort>
                {
                    { AmmoType.Nato556, 200 },
                    { AmmoType.Nato9, 60 }
                }
            },
            {
                RoleTypeId.ChaosConscript, new Dictionary<AmmoType, ushort>
                {
                    { AmmoType.Nato762, 120 },
                    { AmmoType.Ammo12Gauge, 14 }
                }
            },
            {
                RoleTypeId.ChaosRifleman, new Dictionary<AmmoType, ushort>
                {
                    { AmmoType.Nato762, 160 }
                }
            },
            {
                RoleTypeId.ChaosRepressor, new Dictionary<AmmoType, ushort>
                {
                    { AmmoType.Nato762, 200 }
                }
            },
            {
                RoleTypeId.ChaosMarauder, new Dictionary<AmmoType, ushort>
                {
                    { AmmoType.Ammo12Gauge, 54 },
                    { AmmoType.Ammo44Cal, 48 }
                }
            }
        };
    }
}