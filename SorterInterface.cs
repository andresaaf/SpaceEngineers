using System;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Ingame;

namespace Scripting.SorterInterface
{
    public class Program : MyGridProgram
    {
        #region Fields

        private IMyTextSurface _PanelTextSurface = null;
        private IMyCargoContainer _CargoInterface = null;
        private IMyConveyorSorter _SorterOut = null;
        private IMyConveyorSorter _SorterIn = null;
        private bool _Flush = false;

        #endregion

        #region Constructor

        public Program()
        {
            _PanelTextSurface = Me.GetSurface(0);
            _PanelTextSurface.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            _PanelTextSurface.FontSize = 2;
            _PanelTextSurface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            _CargoInterface = GridTerminalSystem.GetBlockWithName("Cargo Interface") as IMyCargoContainer;
            _SorterOut = GridTerminalSystem.GetBlockWithName("Sorter Interface Out") as IMyConveyorSorter;
            _SorterIn = GridTerminalSystem.GetBlockWithName("Sorter Interface In") as IMyConveyorSorter;
            
            _SorterIn.Enabled = true;
            _SorterOut.Enabled = false;
            _Flush = false;
        }

        #endregion

        #region Methods
        public void Main(string argument, UpdateType updateSource)
        {
            var items = _CargoInterface.GetInventory().ItemCount;
            if (items > 0)
            {
                if (!_Flush)
                {
                    _SorterIn.Enabled = false;
                    _SorterOut.Enabled = true;
                    _Flush = true;
                }
            }
            else if (_Flush)
            {
                _SorterOut.Enabled = false;
                _SorterIn.Enabled = true;
                _Flush = false;
            }

            _PanelTextSurface.WriteText(String.Format(
                "Items: {0}\nIn: {1}\nOut: {2}\nFlush: {3}",
                items,
                _SorterIn.Enabled,
                _SorterOut.Enabled,
                _Flush
                ));
        }

        #endregion
    }
}
