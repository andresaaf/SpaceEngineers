using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceEngineers.SolarPanelRegulator
{
    public class Program : MyGridProgram
    {
        #region Properties

        /// <summary>
        /// Reference to the debug screen
        /// </summary>
        private IMyTextSurface _DebugLCD = null;

        /// <summary>
        /// The rotor rotating araound the Y axis
        /// </summary>
        private IMyMotorStator _RotorY = null;

        /// <summary>
        /// The rotor rotating around the X axes
        /// </summary>
        private IMyMotorStator _RotorX = null;

        /// <summary>
        /// The rotor rotating around the Z axis
        /// </summary>
        private IMyMotorStator _RotorZ = null;

        /// <summary>
        /// All solar panels connected to the rotors
        /// </summary>
        private List<IMySolarPanel> _SolarPanels = null;

        /// <summary>
        /// Last average value of the solar panels
        /// </summary>
        private float _LastAverage = 0f;

        private float _LastXAngle = 0f;

        /// <summary>
        /// Maximum degrees in radians (+45 degrees)
        /// </summary>
        private const float MAX_DEGREES = (float)(Math.PI / 4);

        /// <summary>
        /// Minimum degrees in radians (-45 degrees)
        /// </summary>
        private const float MIN_DEGREES = -MAX_DEGREES;

        /// <summary>
        /// The maximum value for a solar panel to output
        /// </summary>
        private const float TARGET_VALUE = 1.6f;

        /// <summary>
        /// The error between the set point and the target value
        /// </summary>
        private float _ERROR = 0f;

        /// <summary>
        /// The error from the previous iteration
        /// </summary>
        private float _LAST_ERROR = 0f;

        private float _PROPORTIONAL = 2f;

        private float _INTEGRAL = 2f;

        private float _DERIVATIVE = 2f;

        #endregion

        #region Constructor

        public Program()
        {
            _DebugLCD = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("Debug LCD");
            _DebugLCD.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            _DebugLCD.FontSize = 2;
            _DebugLCD.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;

            _RotorY = (IMyMotorStator)GridTerminalSystem.GetBlockWithName("RotorY");
            _RotorX = (IMyMotorStator)GridTerminalSystem.GetBlockWithName("RotorX");
            _RotorZ = (IMyMotorStator)GridTerminalSystem.GetBlockWithName("RotorZ");

            _RotorY.TargetVelocityRPM = .2f;
            _RotorX.TargetVelocityRPM = .2f;
            _RotorZ.TargetVelocityRPM = .2f;
            _RotorY.Torque = 1500000f;
            _RotorX.Torque = 1500000f;
            _RotorZ.Torque = 1500000f;

            List<IMyTerminalBlock> tempList = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlockGroupWithName("Solar Panels").GetBlocksOfType<IMySolarPanel>(tempList);

            _SolarPanels = tempList.Cast<IMySolarPanel>().ToList();

            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        #endregion

        #region Main

        public void Main(string argument, UpdateType updateSource)
        {
            ControlSolarPanels();
        }

        #endregion

        #region Methods

        private void ControlSolarPanels()
        {
            float averagePower = 0f;

            foreach (IMySolarPanel solarPanel in _SolarPanels)
            {
                averagePower += solarPanel.MaxOutput;
            }

            //Calculate average and convert to kilowatts
            averagePower = 1000 * (averagePower /= _SolarPanels.Count);

            LogValues(
                "Last average power: ", _LastAverage.ToString(), "kW\n",
                "Current average power:", averagePower.ToString(), "kW\n",
                "Last X angle:", (_LastXAngle * (180 / Math.PI)).ToString(), "\n",
                "Current X angle:", (_RotorX.Angle * (180 / Math.PI)).ToString());

            ShittyRegulation(_RotorX, averagePower);
            ShittyRegulation(_RotorY, averagePower);
            ShittyRegulation(_RotorZ, averagePower);

            _LastAverage = averagePower;
        }

        private void ShittyRegulation(IMyMotorStator regulator, float averagePower)
        {
            if (regulator == null)
            {
                throw new ArgumentException("regulator is null");
            }

            float integral = 3f;
            float deltaError = _ERROR - _LAST_ERROR;

            _ERROR = TARGET_VALUE - averagePower;
            integral = integral * _ERROR;

            float controlVar = _PROPORTIONAL * _ERROR * _INTEGRAL * integral * _DERIVATIVE * deltaError;

            if (controlVar > 1)
            {
                regulator.TargetVelocityRPM = 3;
            }
            else if (controlVar < 1)
            {
                regulator.TargetVelocityRPM = -3;
            }
            else
            {
                regulator.TargetVelocityRPM = 0;
            }

            _LAST_ERROR = _ERROR;
        }

        private void LogValues(params string[] messages)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string message in messages)
            {
                sb.Append(message + " ");
            }

            _DebugLCD.WriteText(sb.ToString());
        }

        #endregion
    }
}
