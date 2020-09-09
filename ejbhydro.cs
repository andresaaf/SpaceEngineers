using System;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Gui;
using SpaceEngineers.Game.Entities.Blocks;

namespace Scripting.ejbhydro
{
    public class Program : MyGridProgram
    {
        #region Fields

        private IMyTextSurface _PanelTextSurface = null;
        List<IMyBatteryBlock> _Batteries = null;
        IMyPowerProducer _Engine = null;

        #endregion

        #region Constructor

        public Program()
        {
            //_PanelTextSurface = Me.GetSurface(0);
            _PanelTextSurface = GridTerminalSystem.GetBlockWithName("Transparent LCD") as IMyTextSurface;
            _PanelTextSurface.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            _PanelTextSurface.FontSize = 2;
            _PanelTextSurface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            _Batteries = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType(_Batteries);

            _Engine = GridTerminalSystem.GetBlockWithName("Hydrogen Engine") as IMyPowerProducer;
            _Engine.Enabled = false;
        }

        #endregion

        #region Methods
        public void Main(string argument, UpdateType updateSource)
        {
            float power_gen = 0.0f;
            float max_stored = 0.0f;
            foreach(var battery in _Batteries)
            {
                power_gen += battery.CurrentStoredPower;
                max_stored += battery.MaxStoredPower;
            }

            int percentage = Convert.ToInt32((power_gen / max_stored) * 100);

            if (percentage > 95)
            {
                _Engine.Enabled = false;
            }
            else if (percentage < 65)
            {
                _Engine.Enabled = true;
            }

            _PanelTextSurface.WriteText(String.Format(
                "Stored: {0:0.00} MWh\nMax: {1:0.00} MWh\n{2} %\n{3}",
                power_gen,
                max_stored,
                percentage,
                _Engine.Enabled ? "Engine ON" : "Engine OFF"
                ));
        }

        #endregion
    }
}
