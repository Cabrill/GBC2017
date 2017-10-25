using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Gui;
using FlatRedBall.Gum;
using FlatRedBall.Math.Geometry;
using GBC2017.Entities.GraphicalElements;
using GBC2017.GumRuntimes;
using GBC2017.ResourceManagers;
using GBC2017.StaticManagers;
using Gum.Converters;
using Gum.DataTypes;
using Microsoft.Xna.Framework.Audio;
using RenderingLibrary.Graphics;
using Layer = FlatRedBall.Graphics.Layer;

namespace GBC2017.Entities.BaseEntities
{
    public partial class BaseStructure
    {
        private static float _maximumY;
        protected float _currentScale;
        private float _startingScale;
        private float _startingLightSpriteScale;
        private float _startingRectangleScaleX;
        private float _startingRectangleScaleY;

        public Action OnBuild;
        public Action OnDestroy;

        public double BatteryLevel { get; protected set; }
        public bool HasSufficientEnergy { get; private set; }
        public bool IsDestroyed => HealthRemaining <= 0;

        public bool HasInternalBattery => InternalBatteryMaxStorage > 0;
        public double EnergyRequestAmount => HasInternalBattery ? Math.Min(MaxReceivableEnergyPerSecond * TimeManager.SecondDifference, EnergyMissing) : EnergyRequiredPerSecond * TimeManager.SecondDifference;
        public float HealthRemaining { get; private set; }
        public double EnergyReceivedLastUpdate { get; private set; }
        public double MineralsReceivedLastUpdate { get; private set; }
        private double _lastUsageUpdate;
        protected double _energyReceivedCurrentUpdate;
        protected double _mineralsReceivedCurrentUpdate;

        private double EnergyMissing => InternalBatteryMaxStorage - BatteryLevel;

        private Layer _hudLayer;

        protected SoundEffectInstance PlacementSound;
        protected SoundEffectInstance DestroyedSound;

        protected float _spriteRelativeY;

        public static void Initialize(float maximumY)
        {
            _maximumY = maximumY;
        }

        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
        private void CustomInitialize()
        {
#if DEBUG
            if (DebugVariables.ShowDebugShapes)
            {
                AxisAlignedRectangleInstance.Visible = true;
            }
            else
#endif
            {
                AxisAlignedRectangleInstance.Visible = false;
            }

            CurrentOnOffState = OnOff.On;

            LightSpriteInstance.Width = SpriteInstance.Width;
            LightSpriteInstance.Height = LightSpriteInstance.Width / 4;

            _startingScale = SpriteInstance.TextureScale;
            _startingLightSpriteScale = LightSpriteInstance.TextureScale;
            _startingRectangleScaleX = AxisAlignedRectangleInstance.ScaleX;
            _startingRectangleScaleY = AxisAlignedRectangleInstance.ScaleY;

            HealthRemaining = MaximumHealth;
            BatteryLevel = 0.6f * InternalBatteryMaxStorage;

            PlacementSound = Structure_Placed.CreateInstance();
            DestroyedSound = Building_Destroyed.CreateInstance();

            _lastUsageUpdate = TimeManager.CurrentTime;
            _spriteRelativeY = GetSpriteRelativeY();

            if (HasInternalBattery)
            {
                EnergyBar.SetRelativeY(SpriteInstance.Height);
                EnergyBar.SetWidth(SpriteInstance.Width);
                EnergyBar.Hide();
            }
            else
            {
                EnergyBar.Hide();
            }

            HealthBar.SetRelativeY(SpriteInstance.Height);
            HealthBar.SetWidth(SpriteInstance.Width);
            HealthBar.Hide();

            CalculateScale();
            UpdateScale();
            UpdateAnimation();
        }

        private void CustomActivity()
        {
            UpdateAnimation();

            if (IsBeingPlaced)
            {
                CalculateScale();
                UpdateScale();

                if (IsValidLocation)
                {
#if DEBUG
                    if (DebugVariables.IgnoreStructureBuildCost)
                    {
                        CurrentState = VariableState.ValidLocation;
                        CheckmarkInstance.CurrentState = Checkmark.VariableState.Enabled;
                    }
                    else
#endif
                    if (EnergyManager.CanAfford(EnergyBuildCost) && MineralsManager.CanAfford(MineralsBuildCost))
                    {
                        CurrentState = VariableState.ValidLocation;
                        CheckmarkInstance.CurrentState = Checkmark.VariableState.Enabled;
                    }
                    else
                    {
                        CurrentState = VariableState.CantAfford;
                        CheckmarkInstance.CurrentState = Checkmark.VariableState.Disabled;
                    }
                }
                else
                {
                    CurrentState = VariableState.InvalidLocation;
                    CheckmarkInstance.CurrentState = Checkmark.VariableState.Disabled;
                }

                if (CheckmarkInstance.CurrentState == Checkmark.VariableState.Enabled &&
                    CheckmarkInstance.WasClickedThisFrame(GuiManager.Cursor))
                {
                    BuildStructure();
                }

                if (XCancelInstance.WasClickedThisFrame(GuiManager.Cursor))
                {
                    Destroy();
                }
            }
            else
            {
                EnergyReceivedLastUpdate = _energyReceivedCurrentUpdate;
                MineralsReceivedLastUpdate = _mineralsReceivedCurrentUpdate;

                _energyReceivedCurrentUpdate = _mineralsReceivedCurrentUpdate = 0;

                if (HasInternalBattery)
                {
                    if (BatteryLevel < InternalBatteryMaxStorage)
                    {
                        EnergyBar.Update((float)(BatteryLevel/InternalBatteryMaxStorage));
                    }
                    else
                    {
                        EnergyBar.Hide();
                    }
                }

                if (HealthRemaining < MaximumHealth)
                {
                    HealthBar.Update(HealthRemaining/MaximumHealth);
                }
                else
                {
                    HealthBar.Hide();
                }
            }
        }

        private void CalculateScale()
        {
            _currentScale = 0.3f + (0.4f * (1 - Y / _maximumY));
        }

        protected virtual void UpdateScale()
        {
            SpriteInstance.TextureScale = _startingScale * _currentScale;

            AxisAlignedRectangleInstance.ScaleX = Math.Max(0, _startingRectangleScaleX * _currentScale);
            AxisAlignedRectangleInstance.ScaleY = Math.Max(0, _startingRectangleScaleY * _currentScale);

            AxisAlignedRectangleInstance.RelativeY = AxisAlignedRectangleInstance.Height / 2;

            if (HasLightSource)
            {
                //LightSpriteInstance.TextureScale = _startingLightSpriteScale * _currentScale;
                LightSpriteInstance.Width = SpriteInstance.Width * 1.5f;
                LightSpriteInstance.Height = AxisAlignedRectangleInstance.Height;
            }
            HealthBar.SetWidth(SpriteInstance.Width);
            EnergyBar.SetWidth(SpriteInstance.Width);

            HealthBar.SetRelativeY(SpriteInstance.Height*0.75f);
            EnergyBar.SetRelativeY(SpriteInstance.Height * 0.75f + HealthBar.Height*2);
        }

        protected void UpdateAnimation()
        {
            if (SpriteInstance.CurrentChain == null || SpriteInstance.CurrentChain.Count == 1)
            {
                SpriteInstance.RelativeY = _spriteRelativeY * _currentScale;
            }
            else
            {
                SpriteInstance.UpdateToCurrentAnimationFrame();

                if (SpriteInstance.UseAnimationRelativePosition)
                {
                    SpriteInstance.RelativeX *= (SpriteInstance.FlipHorizontal
                        ? -SpriteInstance.TextureScale
                        : SpriteInstance.TextureScale);
                    SpriteInstance.RelativeY *= (SpriteInstance.FlipVertical
                        ? -SpriteInstance.TextureScale
                        : SpriteInstance.TextureScale);
                }
                SpriteInstance.RelativeY += _spriteRelativeY * _currentScale;
            }
        }

        private void BuildStructure()
        {
            var shouldBuild = EnergyManager.CanAfford(EnergyBuildCost) && MineralsManager.CanAfford(MineralsBuildCost);

            if (shouldBuild)
            {
                EnergyManager.DebitEnergyForBuildRequest(EnergyBuildCost);
                MineralsManager.DebitMinerals(MineralsBuildCost);

                IsBeingPlaced = false;
                CurrentState = VariableState.Built;
                PlayPlacementSound();
                OnBuild?.Invoke();
            }
        }

        private void PlayPlacementSound()
        {
            try
            {
                PlacementSound.Play();
            }
            catch (Exception)
            {
            }
        }

        public void TakeMeleeDamage(BaseEnemy enemy)
        {
            HealthRemaining -= enemy.MeleeAttackDamage;

            if (IsDestroyed)
            {
                PerformDestruction();
            }
        }

        public void GetHitBy(BaseEnemyProjectile projectile)
        {
            HealthRemaining -= projectile.DamageInflicted;
            projectile?.PlayHitTargetSound();

            if (IsDestroyed)
            {
                PerformDestruction();
            }
        }

        public void ReceiveEnergy(double energyAmount)
        {
            if (HasInternalBattery)
            {
                BatteryLevel = Math.Min(BatteryLevel + energyAmount, InternalBatteryMaxStorage);
            }
            else
            {
                HasSufficientEnergy = energyAmount >= EnergyRequestAmount*TimeManager.SecondDifference;
            }
            _energyReceivedCurrentUpdate += energyAmount;
        }

        public void DrainEnergy(double energyAmount)
        {
            if (HasInternalBattery)
            {
                BatteryLevel = Math.Max(BatteryLevel - energyAmount, 0);
            }
        }

        private float GetSpriteRelativeY()
        {
            if (SpriteInstance.CurrentChain == null || SpriteInstance.CurrentChain.Count == 1)
            {
                return SpriteInstance.Height / 2;
            }
            else
            {
                var maxHeight = 0f;
                foreach (var frame in SpriteInstance.CurrentChain)
                {
                    maxHeight = Math.Max(maxHeight,
                        (frame.BottomCoordinate - frame.TopCoordinate) * frame.Texture.Height *
                        SpriteInstance.TextureScale);
                }
                return maxHeight / 2;
            }
        }

        private void PerformDestruction()
        {
            ;
            OnDestroy?.Invoke();

            Destroy();
        }

        private void CustomDestroy()
        {
            if (PlacementSound != null && !PlacementSound.IsDisposed)
            {
                PlacementSound.Stop(true);
                PlacementSound.Dispose();
            }
            if (DestroyedSound != null && !DestroyedSound.IsDisposed)
            {
                DestroyedSound.Stop(true);
                DestroyedSound.Dispose();
            }
        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }

        protected void AddSpritesToLayers(Layer darknessLayer, Layer hudLayer)
        {
            _hudLayer = hudLayer;

            LayerProvidedByContainer.Remove(CheckmarkInstance.SpriteInstance);
            CheckmarkInstance.MoveToLayer(hudLayer);

            LayerProvidedByContainer.Remove(XCancelInstance.SpriteInstance);
            XCancelInstance.MoveToLayer(hudLayer);

            LayerProvidedByContainer.Remove(AxisAlignedRectangleInstance);
            ShapeManager.AddToLayer(AxisAlignedRectangleInstance, hudLayer);

            LayerProvidedByContainer.Remove(SpriteInstance);
            FlatRedBall.SpriteManager.AddToLayer(SpriteInstance, hudLayer);

            LayerProvidedByContainer.Remove(LightSpriteInstance);
            FlatRedBall.SpriteManager.AddToLayer(LightSpriteInstance, darknessLayer);

            HealthBar.MoveToLayer(hudLayer);
            EnergyBar.MoveToLayer(hudLayer);
        }

        public void DestroyByRequest()
        {
            HealthRemaining = 0;
            PerformDestruction();
        }

        public void Repair()
        {
            HealthRemaining = MaximumHealth;
        }

        public double GetEnergyRepairCost()
	    {
	        if (HealthRemaining >= MaximumHealth)
	        {
	            return 0;
	        }
	        else
	        {
	            return EnergyBuildCost * (HealthRemaining / MaximumHealth);
	        }
	    }

	    public double GetMineralRepairCost()
	    {
	        if (HealthRemaining >= MaximumHealth)
	        {
	            return 0f;
	        }
	        else
	        {
	            return MineralsBuildCost * (HealthRemaining / MaximumHealth);
            }
        }
	}
}
