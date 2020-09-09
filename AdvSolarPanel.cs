using System;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;

namespace Scripting.AdvSolarPanel
{
    public class Program : MyGridProgram
    {
        class Rotator
        {
            public bool prev_greater = true;
            public int counter = 0;
        }

        #region Fields

        private static float VELOCITY = 0.4f;

        private IMyTextSurface _PanelTextSurface = null;
        private float _lastOutput = 0.0f;
        private bool _dirX = false;

        Rotator _rotator = new Rotator();

        private bool wait_for_rotorX = false;

        private IMyMotorStator _rotorX = null;
        private IMyMotorStator _rotorY = null;
        private List<IMySolarPanel> _solarPanels = new List<IMySolarPanel>();

        private float[] _prev = new float[1000];
        private int _prevIndex = 0;

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

            _rotorX.LowerLimitDeg = -30.0f;
            _rotorX.UpperLimitDeg = 90.0f;
            _rotorX.TargetVelocityRPM = VELOCITY;
            _rotorY.TargetVelocityRPM = VELOCITY;

            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        #endregion

        #region Methods

        public void Main(string argument, UpdateType updateSource)
        {
            float generating = 0.0f;
            _solarPanels.ForEach((IMySolarPanel solarPanel) => { generating += solarPanel.MaxOutput; });
            generating *= 1000.0f;
            generating /= _solarPanels.Count;


            if (wait_for_rotorX)
            {
                float deg_5_in_rad = (float)(5.0 * (Math.PI / 180.0));
                if (_rotorX.Angle >= -deg_5_in_rad || _rotorX.Angle <= deg_5_in_rad)
                {
                    wait_for_rotorX = false;
                    _rotorX.TargetVelocityRPM = VELOCITY;
                }
            }
            else
            {
                var rotor = _dirX ? _rotorX : _rotorY;
                ++_rotator.counter;

                if (generating <= _lastOutput)
                {
                    if (_rotator.prev_greater)
                    {
                        _rotator.counter = 0;
                    }
                    _rotator.prev_greater = false;
                }
                else
                {
                    //if (!_rotator.prev_greater)
                    //{
                        _rotator.counter = 0;
                    //}
                    _rotator.prev_greater = true;
                }


                if (rotor.Angle >= rotor.UpperLimitRad)
                {
                    rotor.TargetVelocityRPM = -2 * VELOCITY;
                    wait_for_rotorX = true;
                }
                else if (rotor.Angle <= rotor.LowerLimitRad)
                {
                    rotor.TargetVelocityRPM = 2 * VELOCITY;
                    wait_for_rotorX = true;
                }
                else if (_rotator.counter == 10)
                {
                    _rotator.counter = 0;
                    _dirX = !_dirX;

                    rotor.TargetVelocityRPM = -rotor.TargetVelocityRPM;
                }
            }

            /*if (generating < _lastOutput)
            {
                ++_Xcounter;
                _Ycounter = 0;
                rotor.TargetVelocityRPM = -rotor.TargetVelocityRPM;
            }
            else
            {
                _Xcounter = 0;
                ++_Ycounter;
            }

            if (_Xcounter == 10 || _Ycounter == 10)
            {
                _Xcounter = 0;
                _Ycounter = 0;
                _dirX = !_dirX;
                if (_dirX)
                {
                    _rotorX.RotorLock = false;
                    _rotorY.RotorLock = true;
                }
                else
                {
                    _rotorX.RotorLock = true;
                    _rotorY.RotorLock = false;
                }
            }*/

            _prev[_prevIndex] = generating;
            _prevIndex = (_prevIndex + 1) % _prev.Length;

            _PanelTextSurface.WriteText(String.Format(
                "Average: {0}\nLast: {1}\nOutput: {2}\nRotorX: {3}\nRotorY: {4}\nWaitForRotorX: {5}\nRotorX Max: {6}\nCounter: {7}",
                _prev.Sum() / _prev.Length,
                _lastOutput,
                generating,
                _rotorX.Angle,
                _rotorY.Angle,
                wait_for_rotorX,
                _rotorX.UpperLimitRad,
                _rotator.counter
                ));

            _lastOutput = generating;
        }

        #endregion
    }
}
