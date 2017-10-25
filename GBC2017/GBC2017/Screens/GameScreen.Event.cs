using System;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Specialized;
using System.Linq;
using FlatRedBall.Audio;
using FlatRedBall.Screens;
using GBC2017.Entities.BaseEntities;
using GBC2017.Entities;
using GBC2017.Entities.Enemies;
using GBC2017.Entities.GraphicalElements;
using GBC2017.Entities.Projectiles;
using GBC2017.Entities.Structures.Combat;
using GBC2017.Entities.Structures.EnergyProducers;
using GBC2017.Entities.Structures;
using GBC2017.Entities.Structures.Utility;
using GBC2017.Screens;
using Microsoft.Xna.Framework.Media;

namespace GBC2017.Screens
{
    public partial class GameScreen
    {
        void OnStartButtonInstanceClick (FlatRedBall.Gui.IWindow window)
        {
            StartButtonInstance.Visible = false;
            GameHasStarted = true;
            UpdateMusic();
        }

        void UpdateMusic()
        {
            if (!GameHasStarted || AudioManager.CurrentlyPlayingSong != null) return;

            Song songToPlay = null;

            var randomChoice = FlatRedBallServices.Random.NextDouble();

            if (randomChoice < 0.33)
            {
                songToPlay = anttisinstrumentals_einszweidreiviolins;
            }
            else if (randomChoice < 0.66)
            {
                songToPlay = anttisinstrumentals_harmorsongakaintheyear2000instrumental;
            }
            else
            {
                songToPlay = anttisinstrumentals_hjappiermjusic;
            }

            AudioManager.PlaySong(songToPlay, true, true);
        }
    }
}
