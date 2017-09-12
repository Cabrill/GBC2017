using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Gui;
using GBC2017.GumRuntimes;

namespace GBC2017.Screens
{
    public partial class GameScreen
    {
        private void SetInfoBarControls()
        {
            //Settings button
            InfoBarInstance.SettingsButtonInstanceClick += InfoBarInstanceOnSettingsButtonInstanceClick;
        }

        private void InfoBarInstanceOnSettingsButtonInstanceClick(IWindow window)
        {
            if (IsPaused)
            {
                UnpauseThisScreen();
                InfoBarInstance.CurrentSettingsButtonStateState = InfoBarRuntime.SettingsButtonState.NotHighlighted;
            }
            else
            {
                PauseThisScreen();
                InfoBarInstance.CurrentSettingsButtonStateState = InfoBarRuntime.SettingsButtonState.Highlighted;
            }
        }
    }
}
