using System;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;

namespace Scripting.AdvSolarPanel2
{
    public class Program : MyGridProgram
    {
        #region Fields

        //private float VELOCITY = 0.4f;

        private IMyTextSurface _PanelTextSurface = null;
        private float _lastOutput = 0.0f;

        private IMyMotorStator _rotorX = null;
        private IMyMotorStator _rotorY = null;
        private List<IMySolarPanel> _solarPanels = new List<IMySolarPanel>();

        private float[] _prev = new float[1000];
        private int _prevIndex = 0;

        class PIDController
        {
            public IMyMotorStator rotor;
            public float derivative = 0.0f;
            public float integral = 1.0f;
            public float control = 0.0f;
            public float last_error = 0.0f;
            public float error_sum = 0.0f;

            private const float Kp = 0.001f;
            private const float Kd = 0.0f;

            public PIDController(IMyMotorStator rotor)
            {
                this.rotor = rotor;
            }

            public void PD(float current_power)
            {
                const float TARGET_POWER = 160.0f;

                float error = TARGET_POWER - current_power;

                derivative = error - last_error;

                control = (Kp * error) + (Kd * derivative);

                rotor.TargetVelocityRad = control;

                last_error = error;
            }

            public void PID(float current_power)
            {
                const float TARGET_POWER = 160.0f;

                float error = TARGET_POWER - current_power;

                derivative = error - last_error;

                control = (Kp * error) + (Kd * derivative);

                rotor.TargetVelocityRad = control;

                last_error = error;
            }
        }

        private PIDController _pdX;
        private PIDController _pdY;

        private int pdSwitchCount = 0;
        private bool pdUseX = true;

        #endregion

        #region Constructor

        public Program()
        {
            _PanelTextSurface = GridTerminalSystem.GetBlockWithName("BazLCD") as IMyTextSurface;
            _PanelTextSurface.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            _PanelTextSurface.FontSize = 2;
            _PanelTextSurface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;

            _rotorX = GridTerminalSystem.GetBlockWithName("BazRotorX") as IMyMotorStator;
            _rotorY = GridTerminalSystem.GetBlockWithName("BazRotorY") as IMyMotorStator;
            GridTerminalSystem.GetBlockGroupWithName("Baz Solar").GetBlocksOfType<IMySolarPanel>(_solarPanels);

            //_rotorX.LowerLimitDeg = -30.0f;
            //_rotorX.UpperLimitDeg = 90.0f;
            //_rotorX.TargetVelocityRad = VELOCITY;
            //_rotorY.TargetVelocityRPM = VELOCITY;

            _pdX = new PIDController(_rotorX);
            _pdY = new PIDController(_rotorY);

            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        #endregion

        #region Methods

        public void Main(string argument, UpdateType updateSource)
        {
            float generating = 0.0f;
            _solarPanels.ForEach((IMySolarPanel solarPanel) => { generating += solarPanel.MaxOutput; });
            generating *= 1000.0f;
            generating /= _solarPanels.Count;

            /*if (pdUseX)
            {
                _rotorX.RotorLock = false;
                _pdX.run(generating);
            }
            else
            {
                _rotorY.RotorLock = false;*/
                _pdY.PD(generating);
            //}

            /*++pdSwitchCount;
            if (pdSwitchCount == 10)
            {
                _rotorX.RotorLock = true;
                _rotorY.RotorLock = true;
                pdSwitchCount = 0;
                pdUseX = !pdUseX;
            }*/

            _prev[_prevIndex] = generating;
            _prevIndex = (_prevIndex + 1) % _prev.Length;

            _PanelTextSurface.WriteText(String.Format(
                "Average: {0}\nLast: {1}\nOutput: {2}\nX: I {3:0.00} D {4:0.00} Err {5:0.00} Ctrl {6:0.00}\nY: I {7:0.00} D {8:0.00} Err {9:0.00} Ctrl {10:0.00}",
                _prev.Sum() / _prev.Length,
                _lastOutput,
                generating,

                _pdX.integral,
                _pdX.derivative,
                _pdX.last_error,
                _pdX.control,

                _pdY.integral,
                _pdY.derivative,
                _pdY.last_error,
                _pdY.control
                ));

            _lastOutput = generating;
        }

        #endregion
    }
}
