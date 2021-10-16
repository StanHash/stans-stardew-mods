using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace FastGrowth
{
    public class ModEntry : Mod
    {
        private ModConfig m_config;

        public override void Entry(IModHelper helper)
        {
            m_config = LoadConfig(helper);

            helper.Events.GameLoop.DayEnding += OnDayEnding;
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            foreach (GameLocation location in GetAllLocations())
            {
                // Grow crops on tilled dirt

                foreach (KeyValuePair<Vector2, TerrainFeature> pair in location.terrainFeatures.Pairs.Where(pair => pair.Value is HoeDirt))
                {
                    GrowCropOn(location, pair.Key, pair.Value as HoeDirt);
                }

                // Grow crops in pots

                foreach (StardewValley.Object obj in location.objects.Values.Where(obj => obj is IndoorPot))
                {
                    GrowCropOn(location, obj.TileLocation, (obj as IndoorPot).hoeDirt.Value);
                }
            }
        }

        private void GrowCropOn(GameLocation location, Vector2 tile, HoeDirt hoe_dirt)
        {
            if (hoe_dirt.crop != null && (!m_config.RequiresWatered || hoe_dirt.state.Value == HoeDirt.watered))
            {
                if (m_config.Debug)
                {
                    Monitor.Log($"Growing crop at {tile} in {location.Name}.", LogLevel.Debug);
                }

                hoe_dirt.crop.newDay(
                    HoeDirt.watered,
                    hoe_dirt.fertilizer.Value,
                    (int)tile.X, (int)tile.Y,
                    location);
            }
        }

        private static ModConfig LoadConfig(IModHelper helper)
        {
            return helper.ReadConfig<ModConfig>();
        }

        private static IEnumerable<GameLocation> GetAllLocations()
        {
            return Game1.locations.Concat(
                from location in Game1.locations.OfType<BuildableGameLocation>()
                from building in location.buildings
                where building.indoors.Value != null
                select building.indoors.Value);
        }
    }
}
