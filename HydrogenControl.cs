using System;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Gui;
using SpaceEngineers.Game.Entities.Blocks;

namespace Scripting.HydrogenControl
{
    public class Program : MyGridProgram
    {
        #region Fields

        private IMyTextSurface _PanelTextSurface = null;
        List<IMyBatteryBlock> _Batteries = null;
        List<IMyPowerProducer> _Engines = null;

        #endregion

        #region Constructor

        public Program()
        {
            _PanelTextSurface = Me.GetSurface(0);
            _PanelTextSurface.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            _PanelTextSurface.FontSize = 2;
            _PanelTextSurface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            _Batteries = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType(_Batteries);

            _Engines = new List<IMyPowerProducer>();
            GridTerminalSystem.GetBlockGroupWithName("Base Hydrogen Engines").GetBlocksOfType(_Engines);

            _Batteries = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlockGroupWithName("Base Batteries").GetBlocksOfType(_Batteries);
        }

        #endregion

        #region Methods
        public void Main(string argument, UpdateType updateSource)
        {
            float power_gen = 0.0f;
            float max_stored = 0.0f;
            foreach (var battery in _Batteries)
            {
                power_gen += battery.CurrentStoredPower;
                max_stored += battery.MaxStoredPower;
            }

            int percentage = Convert.ToInt32((power_gen / max_stored) * 100);

            if (percentage > 95)
            {
                _Engines.ForEach(engine => engine.Enabled = false);
            }
            else if (percentage < 80)
            {
                _Engines.ForEach(engine => engine.Enabled = true);
            }

            Me.CustomData = _Engines.First().Enabled ? "ON" : "OFF";

            _PanelTextSurface.WriteText(String.Format(
                "Stored: {0:0.00} MWh\nMax: {1:0.00} MWh\n{2} %\n{3}",
                power_gen,
                max_stored,
                percentage,
                "Engines " + Me.CustomData
                ));
        }

        #endregion
    }
}
