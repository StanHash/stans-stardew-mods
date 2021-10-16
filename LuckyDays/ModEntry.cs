using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace LuckyDays
{
    public class ModEntry : Mod
    {
        private ModConfig m_config;

        public override void Entry(IModHelper helper)
        {
            m_config = LoadConfig(helper);

            helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            // The value that dictates daily luck is Game1.player.team.sharedDailyLuck
            // It is generated at the start of every day, before being saved.
            // Since GameLoop.DayStarted runs after the save, daily has already been generated
            // We modify it here and it should affect the entire rest of the day.

            if (m_config.Debug)
            {
                Monitor.Log($"Daily luck before transformation: {Game1.player.team.sharedDailyLuck}", LogLevel.Debug);
            }

            if (m_config.FixedDailyLuck.HasValue)
            {
                Game1.player.team.sharedDailyLuck.Value = m_config.FixedDailyLuck.Value;
            }
            else
            {
                double scaled_luck = (Game1.player.team.sharedDailyLuck.Value + 0.1) / 0.2;
                double scaled_transformed_luck = ReverseSquareOfScaledLuck(scaled_luck);
                Game1.player.team.sharedDailyLuck.Value = scaled_transformed_luck * 0.2 - 0.1;
            }

            if (m_config.Debug)
            {
                Monitor.Log($"Daily luck after transformation: {Game1.player.team.sharedDailyLuck}", LogLevel.Debug);
            }
        }

        private static ModConfig LoadConfig(IModHelper helper)
        {
            ModConfig config = helper.ReadConfig<ModConfig>();

            if (config.FixedDailyLuck.HasValue)
            {
                double fixed_luck = config.FixedDailyLuck.Value;
                double fixed_luck_clamped = Math.Min(0.1, Math.Max(-0.1, fixed_luck));

                if (fixed_luck != fixed_luck_clamped)
                {
                    config.FixedDailyLuck = fixed_luck_clamped;
                    helper.WriteConfig(config);
                }
            }

            return config;
        }

        private static double ReverseSquareOfScaledLuck(double luck)
        {
            double unluck = 1.0 - Math.Min(1.0, Math.Max(0.0, luck));
            return 1.0 - unluck * unluck;
        }
    }
}
