using System;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;
using VRage.Game;

namespace Scripting.ejb
{
    public class Program : MyGridProgram
    {
        #region Fields

        private IMyTextSurface _PanelTextSurface = null;

        private IMyConveyorSorter _Sorter = null;
        private IMyShipConnector _Connector = null;

        private List<MyInventoryItemFilter> _ItemList = null;
        private List<MyInventoryItemFilter> _ItemListNone = null;
        #endregion

        #region Constructor

        public Program()
        {
            IMyCockpit cockpit = GridTerminalSystem.GetBlockWithName("Janne cock") as IMyCockpit;

            _PanelTextSurface = cockpit.GetSurface(0);
            _PanelTextSurface.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            _PanelTextSurface.FontSize = 2;
            _PanelTextSurface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;

            Runtime.UpdateFrequency = UpdateFrequency.None;

            _Sorter = GridTerminalSystem.GetBlockWithName("Janne Sort") as IMyConveyorSorter;
            _Connector = GridTerminalSystem.GetBlockWithName("Janne Connector") as IMyShipConnector;

            _ItemList = new List<MyInventoryItemFilter>() { new MyInventoryItemFilter("MyObjectBuilder_Ore/Stone", false) };
            _ItemListNone = new List<MyInventoryItemFilter>();

            _Connector.ThrowOut = false;
            _Sorter.SetFilter(MyConveyorSorterMode.Blacklist, _ItemListNone);
            _PanelTextSurface.WriteText("Not throwing out\nAllow all");
        }

        #endregion

        #region Methods
        public void Main(string argument, UpdateType updateSource)
        {
            if (_Sorter.Mode == MyConveyorSorterMode.Blacklist)
            {
                _Sorter.SetFilter(MyConveyorSorterMode.Whitelist, _ItemList);
                _PanelTextSurface.WriteText("Throwing out stone");
                _Connector.ThrowOut = true;
            }
            else
            {
                _Connector.ThrowOut = false;
                _Sorter.SetFilter(MyConveyorSorterMode.Blacklist, _ItemListNone);
                _PanelTextSurface.WriteText("Not throwing out\nAllow all");
            }
        }

        #endregion
    }
}