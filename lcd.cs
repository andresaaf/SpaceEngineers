using System;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace Scripting.LCD
{
    public class Program : MyGridProgram
    {
        #region Fields

        private IMyTextSurface _PanelTextSurface = null;
        private IMyTextSurface _LCD = null;

        private IMyCargoContainer _TextCargo = null;
        private int _TextIndex = 0;
        private int _TextLength = 0;

        private int _BlinkCounter = 0;
        private int _BlinkCount = 0;

        #endregion

        #region Constructor

        public Program()
        {
            _PanelTextSurface = Me.GetSurface(0);
            _PanelTextSurface.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            _PanelTextSurface.FontSize = 2;
            _PanelTextSurface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            _LCD = GridTerminalSystem.GetBlockWithName("cplcd") as IMyTextSurface;

            _TextCargo = GridTerminalSystem.GetBlockWithName("samme1") as IMyCargoContainer;
            _TextLength = _TextCargo.CustomData.Length;
            _LCD.WriteText("");
        }

        #endregion

        #region Methods
        public void Main(string argument, UpdateType updateSource)
        {
            const int PIXEL_COUNT = 32;
            if (_TextIndex < (_TextLength - PIXEL_COUNT - 1))
            {
                _LCD.WriteText(_TextCargo.CustomData.Substring(_TextIndex, PIXEL_COUNT), true);
                _TextIndex += PIXEL_COUNT;
            }
            else
            {
                if (_BlinkCount < 6)
                {
                    if (_BlinkCounter >= 50)
                    {
                        if (_BlinkCount % 2 == 0)
                        {
                            _LCD.WriteText(_TextCargo.CustomData);
                        }
                        else
                        {
                            _LCD.WriteText(" ");
                        }
                        _BlinkCounter = 0;
                        ++_BlinkCount;
                    }

                    ++_BlinkCounter;
                }
                else
                {
                    _BlinkCount = 0;
                    _TextIndex = 0;
                }
            }

            _PanelTextSurface.WriteText(String.Format(
                "TextIndex: {0}\nTextLength: {1}\nBlinkCount: {2}\nBlinkCounter: {3}\nPPT: {4}",
                _TextIndex,
                _TextLength,
                _BlinkCount,
                _BlinkCounter,
                PIXEL_COUNT
                ));
        }

        #endregion
    }
}
