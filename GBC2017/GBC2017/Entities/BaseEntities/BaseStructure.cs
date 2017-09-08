using System;
using System.Collections.Generic;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Gui;
using FlatRedBall.Math.Geometry;
using GBC2017.Entities.GraphicalElements;

namespace GBC2017.Entities.BaseEntities
{
	public partial class BaseStructure
	{
	    public double BatteryLevel { get; private set; }
	    public bool HasSufficientEnergy { get; private set; }
	    public bool IsDestroyed => _healthRemaining <= 0;

	    private float _healthRemaining;
        private double EnergyMissing => InternalBatteryMaxStorage - BatteryLevel;
	    public bool HasInternalBattery => InternalBatteryMaxStorage > 0;

	    public double EnergyRequestAmount => HasInternalBattery ? EnergyMissing : EnergyRequiredPerSecond;

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
		        CheckmarkInstance.AxisAlignedRectangleInstance.Visible = true;
		        XCancelInstance.AxisAlignedRectangleInstance.Visible = true;
		    }
		    else
#endif
            {
                AxisAlignedRectangleInstance.Visible = false;
                CheckmarkInstance.AxisAlignedRectangleInstance.Visible = false;
		        XCancelInstance.AxisAlignedRectangleInstance.Visible = false;
            }

		    _healthRemaining = MaximumHealth;
		    BatteryLevel = 0.6f * InternalBatteryMaxStorage;
		}

	    private void CustomActivity()
		{
		    if (CheckmarkInstance.CurrentState == Checkmark.VariableState.Enabled && 
                CheckmarkInstance.WasClickedThisFrame(GuiManager.Cursor))
		    {
		        IsBeingPlaced = false;
                CurrentState = VariableState.Built;
		    }
		    if (XCancelInstance.WasClickedThisFrame(GuiManager.Cursor))
		    {
		        Destroy();
		    }
		}

	    public void GetHitBy(BaseEnemyProjectile projectile)
	    {
	        _healthRemaining -= projectile.DamageInflicted;

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


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
