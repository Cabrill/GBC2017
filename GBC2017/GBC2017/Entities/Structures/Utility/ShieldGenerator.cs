using System;
using System.Collections.Generic;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Glue.StateInterpolation;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math.Geometry;
using GBC2017.Entities.BaseEntities;
using StateInterpolationPlugin;

namespace GBC2017.Entities.Structures.Utility
{
	public partial class ShieldGenerator
	{
	    private float _shieldSpriteScale;
	    private float CurrentShieldHealth;
	    private double _lastRegenerationTime;
	    public bool ShieldIsUp;
	    private bool _shieldWasUp;

        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
        {
            ShieldSpriteInstance.RelativeY = ShieldSpriteInstance.Height / 5;

            RefreshPolygon();

            PolygonShieldInstance.Visible = true;
            ShieldSpriteInstance.Visible = false;

            AfterIsBeingPlacedSet += ShieldGenerator_AfterIsBeingPlacedSet;
            _shieldSpriteScale = ShieldSpriteInstance.TextureScale;
        }

        private void ShieldGenerator_AfterIsBeingPlacedSet(object sender, EventArgs e)
        {
            PolygonShieldInstance.Visible = IsBeingPlaced;
        }

        private void RefreshPolygon()
	    {
	        var leftMost = ShieldSpriteInstance.Width / 2;
	        var rightMost = ShieldSpriteInstance.Width / 2;
	        var topMost = ShieldSpriteInstance.Height / 3;
	        var bottomMost = ShieldSpriteInstance.Height / 3;

	        var polygonFirstPoints = new List<Point>();

	        var leftPoint = new Point(ShieldSpriteInstance.X - leftMost, ShieldSpriteInstance.Y);
	        var topLeftPoint = new Point(ShieldSpriteInstance.X - leftMost * .8f, ShieldSpriteInstance.Y + topMost * 0.7f);
	        var topPoint = new Point(ShieldSpriteInstance.X, ShieldSpriteInstance.Y + topMost);
	        var topRightPoint = new Point(ShieldSpriteInstance.X + rightMost * .8f, ShieldSpriteInstance.Y + topMost * 0.7f);
	        var rightPoint = new Point(ShieldSpriteInstance.X + rightMost, ShieldSpriteInstance.Y);
	        var bottomRightPoint = new Point(ShieldSpriteInstance.X + rightMost * 0.8f, ShieldSpriteInstance.Y - bottomMost * 0.7f);
	        var bottomPoint = new Point(ShieldSpriteInstance.X, ShieldSpriteInstance.Y - bottomMost);
	        var bottomLeftPoint = new Point(ShieldSpriteInstance.X - leftMost * 0.8f, ShieldSpriteInstance.Y - bottomMost * 0.7f);

	        polygonFirstPoints.Add(leftPoint);
	        polygonFirstPoints.Add(topLeftPoint);
	        polygonFirstPoints.Add(topPoint);
	        polygonFirstPoints.Add(topRightPoint);
	        polygonFirstPoints.Add(rightPoint);
	        polygonFirstPoints.Add(bottomRightPoint);
	        polygonFirstPoints.Add(bottomPoint);
	        polygonFirstPoints.Add(bottomLeftPoint);
	        polygonFirstPoints.Add(leftPoint);

	        PolygonShieldInstance.Points = polygonFirstPoints;
        }

	    public void HitShieldWith(BaseEnemyProjectile projectile)
	    {
	        CurrentShieldHealth -= projectile.DamageInflicted;
	        ShieldIsUp = CurrentShieldHealth > 0;

	        if (ShieldIsUp)
	        {
	            UpdateShieldColor(shouldDip: true);
	        }
	        else
	        {
	            PopShield();
            }
        }

	    private void UpdateShieldColor(bool shouldDip)
	    {
	        const float dipSeconds = 0.5f;
	        var percentHealth = CurrentShieldHealth / MaximumShieldHealth;

            if (shouldDip)
	        {
	            var dipLevel = Math.Min(1f, percentHealth * 1.5f);

	            //Immmediately dip 
	            ShieldSpriteInstance.Red = 1 - dipLevel;
	            ShieldSpriteInstance.Green = dipLevel;

	            //Recover to actual level
	            ShieldSpriteInstance.Tween("Red", 1 - percentHealth, dipSeconds, InterpolationType.Bounce, Easing.InOut);
	            ShieldSpriteInstance.Tween("Green", percentHealth, dipSeconds, InterpolationType.Bounce, Easing.InOut);
	        }
	        else
	        {
	            ShieldSpriteInstance.Red = 1 - percentHealth;
	            ShieldSpriteInstance.Green = percentHealth;
            }
	    }

	    private void CustomActivity()
	    {
	        if (TimeManager.SecondsSince(_lastRegenerationTime) >= 1)
	        {
	            _shieldWasUp = ShieldIsUp;

                if (HasSufficientEnergy)
	            {
	                CurrentShieldHealth = Math.Min(CurrentShieldHealth + ShieldRegenerationRatePerSecond, MaximumShieldHealth);
	            }
	            else
	            {
	                CurrentShieldHealth = (float)Math.Max(0,CurrentShieldHealth - EnergyRequiredPerSecond / _energyReceivedThisSecond);
	            }

	            ShieldIsUp = (ShieldIsUp && CurrentShieldHealth > 0) || 
                    CurrentShieldHealth >= MaximumShieldHealth * RequiredEnergyPercentToRaiseShields;

                if (ShieldIsUp)
	            {
	                if (!_shieldWasUp)
	                {
	                    GrowShield();
	                }
	                UpdateShieldColor(shouldDip: false);
	            }
                else if (_shieldWasUp)
                {
                    PopShield();
                }
	            _lastRegenerationTime = TimeManager.CurrentTime;
            }
	    }

	    private void GrowShield()
	    {
	        var secondsToGrow = 1f;
	        ShieldSpriteInstance.Visible = true;
            ShieldSpriteInstance.TextureScale = 0;
	        ShieldSpriteInstance.Alpha = 0;
            ShieldSpriteInstance.Tween("TextureScale", _shieldSpriteScale, secondsToGrow, InterpolationType.Bounce, Easing.InOut);
	        ShieldSpriteInstance.Tween("Alpha", 1, secondsToGrow, InterpolationType.Linear, Easing.InOut);
        }

        private void PopShield()
        {
            var secondsToPop = 2f;
	        ShieldSpriteInstance.TextureScale = _shieldSpriteScale;
            ShieldSpriteInstance.Tween("TextureScale", 0, secondsToPop, InterpolationType.Exponential, Easing.In);
	        ShieldSpriteInstance.Tween("Alpha", 0, secondsToPop, InterpolationType.Linear, Easing.InOut);
	        this.Call(() => ShieldSpriteInstance.Visible = false).After(secondsToPop);
	    }

	    public void NotifyGameStart()
	    {
	        CurrentShieldHealth = MaximumShieldHealth;
	        ShieldIsUp = true;
            GrowShield();
        }

		private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
