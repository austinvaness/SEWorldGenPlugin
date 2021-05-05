﻿namespace SEWorldGenPlugin.Utilities
{
    /// <summary>
    /// Simple class to have a list of all static texts for the plugin.
    /// </summary>
    public class MyPluginTexts
    {
        public static ToolTips TOOLTIPS = new ToolTips();
        public static Messages MESSAGES = new Messages();
    }
    /// <summary>
    /// Class, that contains all tooltips
    /// </summary>
    public class ToolTips
    {
        public readonly string SYS_GEN_MODE_COMBO = "The generation method for planets and moons to use with this world.";
        public readonly string ASTEROID_GEN_MODE_COMBO = "The type of asteroid generator to use for this world.";
        public readonly string VANILLA_PLANETS_CHECK = "Whether vanilla planets are allowed to be generated by the plugin.";
        public readonly string PLANET_COUNT_SLIDER = "The minimum and maximum amount of planets in the system. The generator will choose a random number in this range. To specify an exact amount, set min and max equal.";
        public readonly string ASTEROID_COUNT_SLIDER = "The minimum and maximum amount of asteroid objects, such as asteroid belts, in the system. This does not include asteroid rings around planets currently. " +
                                                       "The generator will choose a random number in this range. To specify an exact amount, set min and max equal.";
        public readonly string ORBIT_DISTANCE_SLIDER = "The range for orbit distances between the system objects. In KM";
        public readonly string SYSTEM_PLANE_DEV_SLIDER = "The angle the orbits of planets can deviate from the system plane.";
        public readonly string ASTEROID_DENS_SLIDER = "The density for asteroid generation. 1 is the densest.";
        public readonly string WORLD_SIZE_SLIDER = "The max size of the system. After that nothing will be generated. In KM";
        public readonly string PLANET_SIZE_CAP_SLIDER = "The max size a planet can have in this system. In metres";
        public readonly string PLANET_SIZE_MULT = "The multiplier for the planets size. 1G planet equals to 120km * this value. Gravity is fixed for each planet, only Diameter will be affected. A gas giant will be double the size.";
        public readonly string PLANET_SIZE_DEV = "The percentage the planets diameter can deviate from the calculated planet size (surfaceGravity * 120km * SizeMultiplier). The deviation is random but wont exceed this value.";
        public readonly string PLANET_MOON_PROP = "The base probability for a planet to generate moons. Scales with gravity, so higher gravity means higher probability.";
        public readonly string PLANET_RING_PROP = "The base probability for a planet to generate a ring. Scales with gravity, so higher gravity means higher probability.";
        public readonly string PLANET_MOON_COUNT = "The minimum and maximum limits for the amount of moons around a planet.";
        public readonly string PLANET_GPS_COMBO = "The generation mode for planet gpss. Discovery means a player needs to be within 50k km to see the dynamic gps.";
        public readonly string MOON_GPS_COMBO = "The generation mode for moons gpss. Discovery means a player needs to be within 50k km to see the dynamic gps.";
        public readonly string ASTEROID_GPS_COMBO = "The generation mode for asteroid gpss. Discovery means a player needs to be within 50k km to see the dynamic gps.";

        public readonly string ADMIN_SPAWN_TYPE = "The type of system object you want to spawn";
        public readonly string ADMIN_PLANET_DIAM = "The diameter of the planet";
        public readonly string ADMIN_PLANET_SPAWN = "Activates the mode to spawn the planet where you place it";
        public readonly string ADMIN_PLANET_SPAWN_COORD = "Opens a message to enter the coordinate, where the planet will be spawned";
        public readonly string ADMIN_ROID_TYPE = "The type of asteroid object to spawn";
        public readonly string ADMIN_NAME = "The name of this object";
    }

    /// <summary>
    /// Class that contains all messages
    /// </summary>
    public class Messages
    {
        public readonly string UPDATE_AVAILABLE_TITLE = "SEWorldGenPlugin Update available";
        public readonly string UPDATE_AVAILABLE_BOX = "A new version of the SEWorldGenPlugin is a available. Do you want to visit the download page now?";
    }

}
