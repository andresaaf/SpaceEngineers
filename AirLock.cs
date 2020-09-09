using System;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Ingame;

namespace Scripting.AirLock
{
    public class Program : MyGridProgram
    {
        #region Fields

        private const int DOOR_DELAY = 120;

        private int _InsideDoorTicks = DOOR_DELAY;
        private int _OutsideDoorTicks = DOOR_DELAY;
        private IMyTextSurface _PanelTextSurface = null;
        private bool _DoorNeedsClosing = false;

        #endregion

        #region Constructor

        public Program()
        {
            _PanelTextSurface = Me.GetSurface(0);
            _PanelTextSurface.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            _PanelTextSurface.FontSize = 2;
            _PanelTextSurface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        #endregion

        #region Methods

        public void Main(string argument, UpdateType updateSource)
        {
            ControlDoors();
        }

        private void ControlDoors()
        {
            IMyAirtightSlideDoor insideDoor = GridTerminalSystem.GetBlockWithName("AirLock SlidingDoor In") as IMyAirtightSlideDoor;
            IMyAirtightSlideDoor outsideDoor = GridTerminalSystem.GetBlockWithName("AirLock SlidingDoor Out") as IMyAirtightSlideDoor;
            IMyAirVent airVent = GridTerminalSystem.GetBlockWithName("AirLock Vent") as IMyAirVent;

            /*if (insideDoor == null || outsideDoor == null || airVent == null)
            {
                return;
            }*/

            _PanelTextSurface.WriteText(String.Format(
                "Inside door ticks: {0}\nOutside door ticks: {1}\nPressure: {2}",
                _InsideDoorTicks,
                _OutsideDoorTicks,
                airVent.GetOxygenLevel()
                ));



            if (_InsideDoorTicks != -1)
            {
                --_InsideDoorTicks;
            }

            if (_OutsideDoorTicks != -1)
            {
                --_OutsideDoorTicks;
            }

            if (_InsideDoorTicks == 0)
            {
                insideDoor.CloseDoor();
                _InsideDoorTicks = -1;
                _DoorNeedsClosing = false;
            }

            if (_OutsideDoorTicks == 0)
            {
                outsideDoor.CloseDoor();
                _OutsideDoorTicks = -1;
                _DoorNeedsClosing = false;
            }

            if (insideDoor.Status == DoorStatus.Closed &&
                outsideDoor.Status == DoorStatus.Closed &&
                airVent.GetOxygenLevel() == 0.0f)
            {
                insideDoor.Enabled = true;
                outsideDoor.Enabled = true;
                _InsideDoorTicks = -1;
                _OutsideDoorTicks = -1;
                _DoorNeedsClosing = false;
            }

            if (outsideDoor.Status == DoorStatus.Open || outsideDoor.Status == DoorStatus.Opening)
            {
                insideDoor.CloseDoor();
                insideDoor.Enabled = false;

                if (!_DoorNeedsClosing)
                {
                    _OutsideDoorTicks = DOOR_DELAY;
                    _DoorNeedsClosing = true;
                }
            }

            if (insideDoor.Status == DoorStatus.Open || insideDoor.Status == DoorStatus.Opening)
            {
                outsideDoor.CloseDoor();
                outsideDoor.Enabled = false;
                if (!_DoorNeedsClosing)
                {
                    _InsideDoorTicks = DOOR_DELAY;
                    _DoorNeedsClosing = true;
                }
            }
        }

        #endregion
    }
}
