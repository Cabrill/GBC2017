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

namespace GBC2017.Entities.Structures
{
	public partial class Home
	{
	    public double CurrentMinerals { get; private set; }
        public double MaxMineralsStorage { get; private set; }
        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
        {
            CurrentMinerals = 0;
            MaxMineralsStorage = BaseMineralsStorage;
            CurrentMinerals = MaxMineralsStorage;
            BatteryLevel = InternalBatteryMaxStorage;
        }

		private void CustomActivity()
		{


		}

	    public bool SubtractMinerals(double amount)
	    {
	        if (CurrentMinerals < amount)
	        {
	            return false;
	        }

	        CurrentMinerals -= amount;
	        return true;
	    }

	    public void AddMinerals(double amount)
	    {
	        CurrentMinerals = Math.Min(CurrentMinerals + amount, MaxMineralsStorage);
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
