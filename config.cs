using Exiled.API.Interfaces;

namespace MatysiakAmmoWipe
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public ushort GuardExtraAmmo { get; set; } = 30;
        public ushort MaxAmmo556 { get; set; } = 200;
        public ushort MaxAmmo762 { get; set; } = 200;
        public ushort MaxAmmo9mm { get; set; } = 200;
        public ushort MaxAmmo12Gauge { get; set; } = 54;
        public ushort MaxAmmo44Cal { get; set; } = 48;
    }
}