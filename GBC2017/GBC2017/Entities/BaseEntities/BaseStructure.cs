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
	    public double BatteryLevel { get; protected set; }
	    public bool HasSufficientEnergy { get; private set; }
	    public bool IsDestroyed => HealthRemaining <= 0;

	    public bool HasInternalBattery => InternalBatteryMaxStorage > 0;
        public double EnergyRequestAmount => HasInternalBattery ? EnergyMissing : EnergyRequiredPerSecond;
	    public float HealthRemaining { get; private set; }
        public double EnergyReceivedLastSecond { get; private set; }
	    public double MineralsReceivedLastSecond { get; private set; }
	    private double _lastUsageUpdate;
	    protected double _energyReceivedThisSecond;
        protected double _mineralsReceivedThisSecond;

        private double EnergyMissing => InternalBatteryMaxStorage - BatteryLevel;

	    private Layer _hudLayer;

	    private ResourceBarRuntime _energyBar;
	    private ResourceBarRuntime _healthBar;

        protected SoundEffect PlacementSound;

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

	        HealthRemaining = MaximumHealth;
	        BatteryLevel = 0.6f * InternalBatteryMaxStorage;
	        PlacementSound = Structure_Placed;

	        if (HasInternalBattery)
	        {
	            _energyBar = CreateResourceBar(ResourceBarRuntime.BarType.Energy);
	        }
	        _healthBar = CreateResourceBar(ResourceBarRuntime.BarType.Health);

	        _lastUsageUpdate = TimeManager.CurrentTime;

	    }

	    private void CustomActivity()
	    {
            if (IsBeingPlaced)
		    {
#if DEBUG
		        if (DebugVariables.IgnoreStructureBuildCost)
		        {
		            CheckmarkInstance.CurrentState = Checkmark.VariableState.Enabled;
                } else
#endif
                if (EnergyManager.CanAfford(EnergyBuildCost) && MineralsManager.CanAfford(MineralsBuildCost))
                {
                    CheckmarkInstance.CurrentState = IsValidLocation ? Checkmark.VariableState.Enabled : Checkmark.VariableState.Disabled;
                    CurrentState = IsValidLocation ? VariableState.ValidLocation : VariableState.InvalidLocation;
                }
		        else
		        {
                    CurrentState = VariableState.CantAfford;
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
                if (TimeManager.SecondsSince(_lastUsageUpdate) >= 1)
                {
                    _lastUsageUpdate = TimeManager.CurrentTime;
                    EnergyReceivedLastSecond = _energyReceivedThisSecond;
                    MineralsReceivedLastSecond = _mineralsReceivedThisSecond;

                    _energyReceivedThisSecond = _mineralsReceivedThisSecond = 0;
                }

                if (HasInternalBattery)
                {
                    if (BatteryLevel < InternalBatteryMaxStorage)
                    {
                        _energyBar.UpdateBar(BatteryLevel, InternalBatteryMaxStorage, false);
                        _energyBar.X = (X-Camera.Main.X) * CameraZoomManager.GumCoordOffset;
                        _energyBar.Y = (Y + SpriteInstance.Height - Camera.Main.Y) * CameraZoomManager.GumCoordOffset;
                        _energyBar.Width = SpriteInstance.Width * CameraZoomManager.GumCoordOffset;
                        _energyBar.Height = SpriteInstance.Width / 5 * CameraZoomManager.GumCoordOffset;
                        _energyBar.Visible = true;
                    }
                    else
                    {
                        _energyBar.Visible = false;
                    }
                }

                if (HealthRemaining < MaximumHealth)
                {
                    _healthBar.UpdateBar(HealthRemaining, MaximumHealth, false);
                    _healthBar.X = (X - Camera.Main.X) * CameraZoomManager.GumCoordOffset;
                    _healthBar.Y = (Y + SpriteInstance.Height + _healthBar.Height - Camera.Main.Y) * CameraZoomManager.GumCoordOffset;
                    _healthBar.Width = SpriteInstance.Width * CameraZoomManager.GumCoordOffset;
                    _healthBar.Height = SpriteInstance.Width / 5* CameraZoomManager.GumCoordOffset;
                    _healthBar.Visible = true;
                }
                else
                {
                    _healthBar.Visible = false;
                }
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
	            PlacementSound.Play();
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
	            HasSufficientEnergy = energyAmount >= EnergyRequestAmount;
	        }
	        _energyReceivedThisSecond += energyAmount;
	    }

	    public void DrainEnergy(double energyAmount)
	    {
	        if (HasInternalBattery)
	        {
	            BatteryLevel = Math.Max(BatteryLevel - energyAmount, 0);
	        }
	    }

	    private void PerformDestruction()
	    {
	        Destroy();
	    }

        private void CustomDestroy()
		{
		    _energyBar?.Destroy();
		    _healthBar.Destroy();
        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }

	    private ResourceBarRuntime CreateResourceBar(ResourceBarRuntime.BarType barType)
	    {
	        var newBar = new ResourceBarRuntime
	        {
	            XUnits = GeneralUnitType.PixelsFromMiddle,
	            YUnits = GeneralUnitType.PixelsFromMiddleInverted,
	            XOrigin = HorizontalAlignment.Center,
	            YOrigin = VerticalAlignment.Top,
	            WidthUnits = DimensionUnitType.Absolute,
	            HeightUnits = DimensionUnitType.Absolute,
	            Width = SpriteInstance.Width,
	            Height = SpriteInstance.Width / 5,
	            CurrentBarTypeState = barType,
                Visible = false
	        };
            newBar.AddToManagers();
	        return newBar;
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

	        var frbLayer = GumIdb.AllGumLayersOnFrbLayer(hudLayer).FirstOrDefault();

            if (HasInternalBattery)
	        {
                frbLayer.Remove(_energyBar);
                _energyBar.MoveToLayer(frbLayer);
            }
            frbLayer.Remove(_healthBar);
	        _healthBar.MoveToLayer(frbLayer);
        }
	}
}
