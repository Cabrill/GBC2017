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

		    HealthRemaining = MaximumHealth;
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

		private void CustomDestroy()
		{


		}

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
	}
}
