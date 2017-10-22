using System;
using System.Collections.Generic;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math.Geometry;
using GBC2017.GameClasses;
using GBC2017.StaticManagers;

namespace GBC2017.Entities.Structures.EnergyProducers
{
	public partial class SolarPanels
	{
        private const int kilowattToWatt = 1000;

        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
        private void CustomInitialize()
		{
		    EffectiveEnergyProducedPerSecond = CalculateEnergyPerSecond();

        }

		private void CustomActivity()
		{
		    if (IsTurnedOn)
		    {
		        EffectiveEnergyProducedPerSecond = CalculateEnergyPerSecond();
		    }
		    else
		    {
		        EffectiveEnergyProducedPerSecond = 0;
		    }
		}

	    private float CalculateEnergyPerSecond()
	    {
	        var kwPerHourPerPanel = InsolationFormulas.Instance.CurrentHourlyInsolation * SunlightManager.SunlightEffectiveness * PanelEfficiencyRating;
	        var kwForAllPanels = kwPerHourPerPanel * NumberOfSquareMeterPanels;
	        var wattsPerHour = kwForAllPanels * kilowattToWatt;
	        var wattsPerRealSecond = wattsPerHour / GameFormulas.RealSecondsPerGameHour;

            return wattsPerRealSecond;
	    }

	    public new void AddSpritesToLayers(Layer darknessLayer, Layer hudLayer)
	    {
	        base.AddSpritesToLayers(darknessLayer, hudLayer);

	        if (HasLightSource)
	        {
	            //LayerProvidedByContainer.Remove(LightSpriteInstance);
	            //SpriteManager.AddToLayer(LightSpriteInstance, darknessLayer);
	        }
	    }

        private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
