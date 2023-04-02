using System;
using System.Linq;
using Verse;

namespace GTM
{
    internal class ModCompatibilityCheck
    {
        public static bool CombatExtendedIsActive
        {
            get
            {
                return ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.Name == "Combat Extended");
            }
        }
        public static bool TurretExtensionsIsActive
        {
            get
            {
                return ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.Name == "[XND] Turret Extensions (Continued)");
            }
        }
    }
}
