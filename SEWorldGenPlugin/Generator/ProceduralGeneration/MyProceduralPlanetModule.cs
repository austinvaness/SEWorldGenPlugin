﻿using Sandbox.Definitions;
using Sandbox.Engine.Voxels;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using Sandbox.Game.World.Generator;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using VRage.Game.Voxels;
using VRage.Library.Utils;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator.ProceduralGeneration
{
    /// <summary>
    /// Class that is a generator module for the MyProceduralGeneratorClass, that generates all
    /// planets and moons of the star system.
    /// </summary>
    public class MyProceduralPlanetModule : MyAbstractProceduralObjectModul
    {
        /// <summary>
        /// Creates a new Instance of this module
        /// </summary>
        /// <param name="seed"></param>
        public MyProceduralPlanetModule(int seed) : base(seed)
        {

        }

        public override void GenerateObjects()
        {
            var system = MyStarSystemGenerator.Static.StarSystem;

            if (system == null || system.CenterObject == null) return;

            var objs = system.GetAllObjects();

            foreach(var obj in objs)
            {
                if (obj == null) continue;
                if (obj.Type != MySystemObjectType.PLANET || obj.Type != MySystemObjectType.MOON) continue;

                MySystemPlanet planet = obj as MySystemPlanet;

                if (planet.Generated) continue;

                var definition = MyDefinitionManager.Static.GetDefinition<MyPlanetGeneratorDefinition>(MyStringHash.GetOrCompute(planet.SubtypeId));

                if (definition == null) continue;

                long id = MyRandom.Instance.NextLong();
                string name = MyStarSystemGenerator.GetPlanetStorageName(planet);

                MyPlanet generatedPlanet = MyWorldGenerator.AddPlanet(name, planet.DisplayName, planet.SubtypeId, planet.CenterPosition - GetPlanetOffset(definition, planet.Diameter), m_seed, (float)planet.Diameter, true, id, false, true);

                if(generatedPlanet != null)
                {
                    generatedPlanet.DisplayNameText = planet.DisplayName;
                    generatedPlanet.AsteroidName = planet.DisplayName;

                    planet.Generated = true;
                }
            }
        }

        public override void UpdateGpsForPlayer(MyEntityTracker tracker)
        {
            if (!(tracker.Entity is MyCharacter)) return;

            var entity = tracker.Entity as MyCharacter;
            var systemObjects = MyStarSystemGenerator.Static.StarSystem.GetAllObjects();
            var settings = MySettingsSession.Static.Settings.GeneratorSettings.GPSSettings;
            if (settings.PlanetGPSMode != MyGPSGenerationMode.DISCOVERY && settings.MoonGPSMode != MyGPSGenerationMode.DISCOVERY) return;
            if(systemObjects.Count > 0)
            {
                foreach(var obj in systemObjects)
                {
                    if(obj is MySystemPlanet)
                    {
                        if((obj.Type == MySystemObjectType.MOON && settings.MoonGPSMode == MyGPSGenerationMode.DISCOVERY) ||
                            (obj.Type == MySystemObjectType.PLANET && settings.PlanetGPSMode == MyGPSGenerationMode.DISCOVERY))
                        {
                            var distance = Vector3D.Distance(tracker.CurrentPosition, obj.CenterPosition);

                            if(distance <= 50000000)
                            {
                                if (MyGPSManager.Static.DynamicGpsExists(obj.DisplayName, Color.White, entity.GetPlayerIdentityId()))
                                {
                                    MyGPSManager.Static.ModifyDynamicGps(obj.DisplayName, Color.White, obj.CenterPosition, entity.GetPlayerIdentityId());
                                }
                                else
                                {
                                    MyGPSManager.Static.AddDynamicGps(obj.DisplayName, Color.White, obj.CenterPosition, entity.GetPlayerIdentityId());
                                }

                                return;
                            }

                            MyGPSManager.Static.RemoveDynamicGps(obj.DisplayName, Color.White, entity.GetPlayerIdentityId());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the offset position of the planet based on its center position,
        /// since SE planets are generated based of the offset position.
        /// </summary>
        /// <param name="definition">The generator definition of the planet</param>
        /// <param name="size">The diameter of the planet in meters</param>
        /// <returns>The offset position for the planet</returns>
        private Vector3D GetPlanetOffset(MyPlanetGeneratorDefinition definition, double size)
        {
            MyPlanetStorageProvider myPlanetStorageProvider = new MyPlanetStorageProvider();
            myPlanetStorageProvider.Init(0, definition, size / 2f);
            IMyStorage myStorage = new MyOctreeStorage(myPlanetStorageProvider, myPlanetStorageProvider.StorageSize);

            return myStorage.Size / 2.0f;
        }
    }
}
