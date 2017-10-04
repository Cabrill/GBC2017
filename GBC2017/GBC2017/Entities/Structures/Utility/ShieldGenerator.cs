using System;
using System.Collections.Generic;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Glue.StateInterpolation;
using FlatRedBall.Graphics;
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
	    private float _startingAlpha;
	    public bool ShieldIsUp;
	    private bool _shieldWasUp;

	    Tweener _lastShieldSizeTweener;
	    Tweener _lastShieldColorTweener;

        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
		private void CustomInitialize()
        {
            ShieldSpriteInstance.RelativeY = ShieldSpriteInstance.Height / 5;
            LightSpriteInstance.RelativeY = LightSpriteInstance.Height / 5;

            RefreshPolygon();

            PolygonShieldInstance.Visible = true;
            ShieldSpriteInstance.Visible = false;
            LightSpriteInstance.Visible = false;

            AfterIsBeingPlacedSet += ShieldGenerator_AfterIsBeingPlacedSet;
            _shieldSpriteScale = ShieldSpriteInstance.TextureScale;
            _lastRegenerationTime = TimeManager.CurrentTime;
            _startingAlpha = ShieldSpriteInstance.Alpha;
        }

        private void ShieldGenerator_AfterIsBeingPlacedSet(object sender, EventArgs e)
        {
            PolygonShieldInstance.Visible = IsBeingPlaced;
            ShieldSpriteInstance.Visible = !IsBeingPlaced;

            if (!IsBeingPlaced)
            {
                ShieldIsUp = true;
                CurrentShieldHealth = MaximumShieldHealth;
                GrowShield();
            }
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
	                CurrentShieldHealth = (float)Math.Max(0,CurrentShieldHealth - EnergyRequiredPerSecond / Math.Max(1,_energyReceivedThisSecond));
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

	    private void UpdateShieldColor(bool shouldDip)
	    {
	        const float dipSeconds = 0.25f;
	        var percentHealth = CurrentShieldHealth / MaximumShieldHealth;

	        if (shouldDip)
	        {
	            var dipLevel = Math.Min(1f, percentHealth * 0.6f);

	            //Immmediately dip 
	            ShieldSpriteInstance.Red = (1 - dipLevel)/2;
	            ShieldSpriteInstance.Blue = dipLevel;

	            //Recover to actual level
	            if (_lastShieldColorTweener != null && _lastShieldColorTweener.Running)
	            {
	                _lastShieldColorTweener.Stop();
	                _lastShieldColorTweener = null;
	            }

	            _lastShieldColorTweener =
	                new Tweener(dipLevel, percentHealth, dipSeconds, InterpolationType.Bounce, Easing.InOut)
	                {
	                    PositionChanged = HandleShieldColorPositionSet,
	                    Owner = this
	                };
                _lastShieldColorTweener.Ended += LastShieldColorTweenerOnEnded;

	            TweenerManager.Self.Add(_lastShieldColorTweener);
	            _lastShieldColorTweener.Start();

            }
            else if (_lastShieldColorTweener == null || !_lastShieldColorTweener.Running)
	        {
	            LightSpriteInstance.Red = ShieldSpriteInstance.Red = (1 - percentHealth)/2;
	            LightSpriteInstance.Blue = ShieldSpriteInstance.Blue = percentHealth;
	        }
	    }

	    private void LastShieldColorTweenerOnEnded()
	    {
	        const float dipSeconds = 0.25f;
            var percentHealth = CurrentShieldHealth / MaximumShieldHealth;

            if (_lastShieldColorTweener != null && _lastShieldColorTweener.Running)
	        {
	            _lastShieldColorTweener.Stop();
	            _lastShieldColorTweener = null;
	        }

	        _lastShieldColorTweener =
	            new Tweener(ShieldSpriteInstance.Blue, percentHealth, dipSeconds, InterpolationType.Bounce, Easing.InOut)
	            {
	                PositionChanged = HandleShieldColorPositionSet,
	                Owner = this
	            };
	        _lastShieldColorTweener.Ended += LastShieldColorTweenerOnEnded;

	        TweenerManager.Self.Add(_lastShieldColorTweener);
	        _lastShieldColorTweener.Start();
        }


	    private void HandleShieldColorPositionSet(float newposition)
	    {
	        LightSpriteInstance.Red = ShieldSpriteInstance.Red = (1 - newposition)/2;
	        LightSpriteInstance.Blue = ShieldSpriteInstance.Blue = newposition;
	    }

	    private void GrowShield()
	    {
            var secondsToGrow = 1f;
	        LightSpriteInstance.Visible = ShieldSpriteInstance.Visible = true;
	        LightSpriteInstance.TextureScale = ShieldSpriteInstance.TextureScale = 0f;
	        LightSpriteInstance.Alpha = ShieldSpriteInstance.Alpha = 0f;

            if (_lastShieldSizeTweener != null && _lastShieldSizeTweener.Running)
	        {
	            _lastShieldSizeTweener.Stop();
	            _lastShieldSizeTweener = null;
	        }

	        _lastShieldSizeTweener =
	            new Tweener(0, 1, secondsToGrow, InterpolationType.Bounce, Easing.InOut)
	            {
	                PositionChanged = HandleShieldSizePositionSet,
	                Owner = this
	            };

	        TweenerManager.Self.Add(_lastShieldSizeTweener);
	        _lastShieldSizeTweener.Start();
        }

	    private void HandleShieldSizePositionSet(float newposition)
	    {
	        ShieldSpriteInstance.TextureScale = newposition * _shieldSpriteScale;
	        ShieldSpriteInstance.Alpha = newposition * _startingAlpha;
	        
	        LightSpriteInstance.TextureScale = ShieldSpriteInstance.TextureScale * 1.01f;
	        LightSpriteInstance.Alpha = newposition * _startingAlpha;
        }

	    private void PopShield()
        {
            const float secondsToPop = 1f;
	        ShieldSpriteInstance.TextureScale = _shieldSpriteScale;
            ShieldSpriteInstance.Alpha = _startingAlpha;

            if (_lastShieldSizeTweener != null && _lastShieldSizeTweener.Running)
            {
                _lastShieldSizeTweener.Stop();
                _lastShieldSizeTweener = null;
            }

            _lastShieldSizeTweener = new Tweener(1, 0, secondsToPop, InterpolationType.Quintic, Easing.Out);

            _lastShieldSizeTweener.PositionChanged = HandleShieldSizePositionSet;
            _lastShieldSizeTweener.Ended += () =>
            {
                ShieldSpriteInstance.Visible = false;
                LightSpriteInstance.Visible = false;
            };

            _lastShieldSizeTweener.Owner = this;

            TweenerManager.Self.Add(_lastShieldSizeTweener);
            _lastShieldSizeTweener.Start();
        }

	    public new void AddSpritesToLayers(Layer darknessLayer, Layer hudLayer)
	    {
	        base.AddSpritesToLayers(darknessLayer, hudLayer);

	        LayerProvidedByContainer.Remove(LightSpriteInstance);
	        SpriteManager.AddToLayer(LightSpriteInstance, darknessLayer);

            LayerProvidedByContainer.Remove(PolygonShieldInstance);
            ShapeManager.AddToLayer(PolygonShieldInstance, hudLayer);

	    }

        private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
