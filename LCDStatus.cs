using System;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace Scripting.LCDStatus
{
    public class Program : MyGridProgram
    {
        #region Fields

        private List<IMyTextSurface> _LCDs = null;

        private const int UPDATE_FREQUENCY = 10;
        private int _FrequencyTimer = 0;

        #endregion

        #region Constructor

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            _LCDs = new List<IMyTextSurface>();
            GridTerminalSystem.GetBlockGroupWithName("LCD Status").GetBlocksOfType(_LCDs);

            foreach (var LCD in _LCDs)
            {
                LCD.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                LCD.FontSize = 2;
                LCD.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.LEFT;
            }

            _LCDs[2].FontSize = 1.8f;
            _LCDs[3].FontSize = 1.8f;
        }

        #endregion

        #region Methods
        public void Main(string argument, UpdateType updateSource)
        {
            if (++_FrequencyTimer >= UPDATE_FREQUENCY)
            {
                _FrequencyTimer = 0;

                UpdateBatteryStatus(_LCDs[0]);
                UpdateGasStatus(_LCDs[1]);
                UpdateOreStatus(_LCDs[2]);
                UpdateControlStatus(_LCDs[3]);
            }
        }

        private void UpdateBatteryStatus(IMyTextSurface surface)
        {
            List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlockGroupWithName("Base Batteries").GetBlocksOfType<IMyBatteryBlock>(batteries);

            float max_stored = 0.0f;
            float current_stored = 0.0f;
            float current_input = 0.0f;
            float current_output = 0.0f;
            foreach (var battery in batteries)
            {
                max_stored += battery.MaxStoredPower;
                current_stored += battery.CurrentStoredPower;
                current_input += battery.CurrentInput;
                current_output += battery.CurrentOutput;
            }

            float change = current_input - current_output;
            float hours = 0.0f;
            string battery_life = "";
            if (change > 0)
            {
                // Towards full charge
                float remaining_MWh = max_stored - current_stored;
                hours = remaining_MWh / change;
                battery_life = "Recharged in: ";
            }
            else
            {
                // Towards empty
                hours = current_stored / (-change);
                battery_life = "Depleted in: ";
            }

            if (hours >= 1)
            {
                battery_life += Convert.ToInt32(hours).ToString() + " h";
            }
            else
            {
                float minutes = hours * 60;
                if (minutes >= 1)
                {
                    battery_life += Convert.ToInt32(minutes).ToString() + " m";
                }
                else
                {
                    battery_life += Convert.ToInt32(minutes * 60).ToString() + " s";
                }
            }

            string input_unit = "MW";
            if (current_input < 1.0f)
            {
                current_input *= 1000.0f;
                input_unit = "kW";
            }

            string output_unit = "MW";
            if (current_output < 1.0f)
            {
                current_output *= 1000.0f;
                output_unit = "kW";
            }

            int percentage = Convert.ToInt32(100 * current_stored / max_stored);
            int num_bars = (percentage * 20) / 100;
            string bars = new string('|', num_bars);
            if (num_bars < 20)
            {
                bars += new string(' ', 20 - num_bars);
            }

            surface.WriteText(String.Format(
                "- Power\nIn: {0:0.##} {1}\nOut: {2:0.##} {3}\nStored: {4:0.##} MWh\nMax: {5:0.##} MWh\n\n{6}\n[{7}] {8} %",
                current_input,
                input_unit,
                current_output,
                output_unit,
                current_stored,
                max_stored,
                battery_life,
                bars,
                percentage
                ));
        }

        private void UpdateGasStatus(IMyTextSurface surface)
        {
            // Hydrogen
            List<IMyGasTank> gasTanks = new List<IMyGasTank>();
            GridTerminalSystem.GetBlockGroupWithName("Hydrogen Tanks").GetBlocksOfType(gasTanks);

            float hydrogen_max = 0.0f;
            float hydrogen_stored = 0.0f;
            foreach (var tank in gasTanks)
            {
                hydrogen_max += tank.Capacity;
                hydrogen_stored += (float)tank.FilledRatio * tank.Capacity;
            }

            hydrogen_max /= 1000;
            hydrogen_stored /= 1000;

            int hydrogen_percentage = Convert.ToInt32(100 * hydrogen_stored / hydrogen_max);
            int num_bars = (hydrogen_percentage * 20) / 100;
            string hydrogen_bars = new string('|', num_bars);
            if (num_bars < 20)
            {
                hydrogen_bars += new string(' ', 20 - num_bars);
            }

            // Oxygen
            List<IMyGasTank> oxygenTanks = new List<IMyGasTank>();
            GridTerminalSystem.GetBlockGroupWithName("Oxygen Tanks").GetBlocksOfType(oxygenTanks);

            float oxygen_max = 0.0f;
            float oxygen_stored = 0.0f;
            foreach (var tank in oxygenTanks)
            {
                oxygen_max += tank.Capacity;
                oxygen_stored += (float)tank.FilledRatio * tank.Capacity;
            }

            oxygen_max /= 1000;
            oxygen_stored /= 1000;

            int oxygen_percentage = Convert.ToInt32(100 * oxygen_stored / oxygen_max);
            num_bars = (oxygen_percentage * 20) / 100;
            string oxygen_bars = new string('|', num_bars);
            if (num_bars < 20)
            {
                oxygen_bars += new string(' ', 20 - num_bars);
            }

            surface.WriteText(String.Format(
                "- Hydrogen\nStored: {0:0.##} m³\nMax: {1:0.##} m³\n[{2}] {3} %\n- Oxygen\nStored: {4:0.##} m³\nMax: {5:0.##} m³\n[{6}] {7} %",
                hydrogen_stored,
                hydrogen_max,
                hydrogen_bars,
                hydrogen_percentage,
                oxygen_stored,
                oxygen_max,
                oxygen_bars,
                oxygen_percentage
                ));
        }

        struct OreStats
        {
            public long iron;
            public long nickel;
            public long cobalt;
            public long magnesium;
            public long silicon;
            public long silver;
            public long gold;
            public long platinum;

            public static string Repr(long v)
            {
                float val = v / 1000000.0f;
                return val.ToString("N2") + " kg";
            }
        }
        private void UpdateOreStatus(IMyTextSurface surface)
        {
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocks(blocks);

            OreStats stats = new OreStats();
            stats.iron = 0;

            foreach(var block in blocks)
            {
                if (block.HasInventory)
                {
                    for (int inv = 0; inv < block.InventoryCount; ++inv)
                    {
                        List<MyInventoryItem> items = new List<MyInventoryItem>();
                        block.GetInventory(inv).GetItems(items);
                        foreach (var item in items)
                        {
                            if (item.Type.TypeId == "MyObjectBuilder_Ingot")
                            {
                                switch (item.Type.SubtypeId)
                                {
                                    case "Iron":
                                        stats.iron += item.Amount.RawValue;
                                        break;
                                    case "Nickel":
                                        stats.nickel += item.Amount.RawValue;
                                        break;
                                    case "Cobalt":
                                        stats.cobalt += item.Amount.RawValue;
                                        break;
                                    case "Magnesium":
                                        stats.magnesium += item.Amount.RawValue;
                                        break;
                                    case "Silicon":
                                        stats.silicon += item.Amount.RawValue;
                                        break;
                                    case "Silver":
                                        stats.silver += item.Amount.RawValue;
                                        break;
                                    case "Gold":
                                        stats.gold += item.Amount.RawValue;
                                        break;
                                    case "Platinum":
                                        stats.platinum += item.Amount.RawValue;
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            surface.WriteText(String.Format(
                "- Ingots\nIron {0}\nNickel {1}\nCrobale {2}\nMagnesium {3}\nSilicon {4}\nSilver {5}\nGold {6}\nPlatinum {7}",
                OreStats.Repr(stats.iron),
                OreStats.Repr(stats.nickel),
                OreStats.Repr(stats.cobalt),
                OreStats.Repr(stats.magnesium),
                OreStats.Repr(stats.silicon),
                OreStats.Repr(stats.silver),
                OreStats.Repr(stats.gold),
                OreStats.Repr(stats.platinum)
                ));
        }

        private void UpdateControlStatus(IMyTextSurface surface)
        {
            var hydrogen_control = GridTerminalSystem.GetBlockWithName("Hydrogen Control Program") as IMyProgrammableBlock;
            surface.WriteText(String.Format(
                "- Control Status\nHydrogen {0}",
                hydrogen_control.CustomData
                ));
        }

        #endregion
    }
}
