using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.ModAPI.Ingame;

namespace Scripting
{
    public class Program : MyGridProgram
    {
        private class AirLock
        {
            #region Properties

            private int _InsideDoorTicks = DOOR_DELAY;
            private int _OutsideDoorTicks = DOOR_DELAY;
            private bool _DoorNeedsClosing = false;

            IMyDoor _InsideDoor = null;
            IMyDoor _OutsideDoor = null;
            IMyAirVent _AirVent = null;

            #endregion

            #region Constructor

            public AirLock(IMyBlockGroup airLockGroup)
            {
                if (airLockGroup == null)
                {
                    throw new ArgumentNullException("airLockGroup cannot be null");
                }

                _InsideDoor = ParseFromGroup<IMyDoor>(airLockGroup, "In");
                _OutsideDoor = ParseFromGroup<IMyDoor>(airLockGroup, "Out");
                _AirVent = ParseFromGroup<IMyAirVent>(airLockGroup, "Vent");
            }

            #endregion

            #region Methods

            private static T ParseFromGroup<T>(IMyBlockGroup airLockGroup, string postfix) where T : class, IMyCubeBlock
            {
                List<T> blocks = new List<T>();

                airLockGroup.GetBlocksOfType(blocks, (T predicateParam) => predicateParam.DisplayNameText.ToLower().EndsWith(postfix.ToLower()));

                if (blocks.Count != 1)
                {
                    throw new Exception(String.Format(
                        "Ambiguity when parsing a block of type {0} with postfix \"{1}\"",
                            typeof(T).Name,
                            postfix));
                }

                return blocks[0];
            }

            public void ControlDoors(IMyProgrammableBlock owner)
            {
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
                    _InsideDoor.CloseDoor();
                    _InsideDoorTicks = -1;
                    _DoorNeedsClosing = false;
                }

                if (_OutsideDoorTicks == 0)
                {
                    _OutsideDoor.CloseDoor();
                    _OutsideDoorTicks = -1;
                    _DoorNeedsClosing = false;
                }

                int storedOxygenPercentage = Int32.Parse(owner.CustomData);

                if (_InsideDoor.Status == DoorStatus.Closed &&
                    _OutsideDoor.Status == DoorStatus.Closed &&
                    (storedOxygenPercentage > 95 || _AirVent.GetOxygenLevel() == 0.0f))
                {
                    _InsideDoor.Enabled = true;
                    _OutsideDoor.Enabled = true;
                    _InsideDoorTicks = -1;
                    _OutsideDoorTicks = -1;
                    _DoorNeedsClosing = false;
                }

                if (_OutsideDoor.Status == DoorStatus.Open || _OutsideDoor.Status == DoorStatus.Opening)
                {
                    _InsideDoor.CloseDoor();
                    _InsideDoor.Enabled = false;

                    if (!_DoorNeedsClosing)
                    {
                        _OutsideDoorTicks = DOOR_DELAY;
                        _DoorNeedsClosing = true;
                    }
                }

                if (_InsideDoor.Status == DoorStatus.Open || _InsideDoor.Status == DoorStatus.Opening)
                {
                    _OutsideDoor.CloseDoor();
                    _OutsideDoor.Enabled = false;

                    if (!_DoorNeedsClosing)
                    {
                        _InsideDoorTicks = DOOR_DELAY;
                        _DoorNeedsClosing = true;
                    }
                }
            }

            #endregion
        }

        #region Properties and Fields

        private const int DOOR_DELAY = 120;
        private IMyTextSurface _PanelTextSurface = null;
        private HashSet<AirLock> _AirLocks = new HashSet<AirLock>();

        #endregion

        #region Constructor

        public Program()
        {
            _PanelTextSurface = Me.GetSurface(0);
            _PanelTextSurface.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            _PanelTextSurface.FontSize = 2;
            _PanelTextSurface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            List<IMyBlockGroup> airlockGroups = new List<IMyBlockGroup>();
            GridTerminalSystem.GetBlockGroups(airlockGroups, (IMyBlockGroup blockGroup) => blockGroup.Name.ToLower().StartsWith("airlock"));

            foreach (IMyBlockGroup blockGroup in airlockGroups)
            {
                _AirLocks.Add(new AirLock(blockGroup));
            }
        }

        #endregion

        #region Methods

        public void Main(string argument, UpdateType updateSource)
        {
            List<IMyGasTank> oxygenTanks = new List<IMyGasTank>();
            GridTerminalSystem.GetBlockGroupWithName("Oxygen Tanks").GetBlocksOfType(oxygenTanks);

            float maxOxygen = 0f;
            float storedOxygen = 0f;

            foreach (IMyGasTank oxygenTank in oxygenTanks)
            {
                maxOxygen += oxygenTanks.Capacity;
                storedOxygen += (float)oxygenTank.FilledRatio * oxygenTank.Capacity;
            }

            int oxygenPercentage = (int)(100 * storedOxygen / maxOxygen);

            Me.CustomData = oxygenPercentage.ToString();

            foreach (AirLock airLock in _AirLocks)
            {
                airLock.ControlDoors(Me);
            }
        }

        #endregion
    }
}
