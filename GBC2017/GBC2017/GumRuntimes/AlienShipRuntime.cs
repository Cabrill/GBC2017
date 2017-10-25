using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC2017.GumRuntimes
{
    public partial class AlienShipRuntime
    {
        public void UpdateColoring(float darkness)
        {
            SpriteInstance.Red = (int)(darkness * 255);
            SpriteInstance.Green = (int)(darkness * 255);
            SpriteInstance.Blue = (int)(darkness * 255);
        }
    }
}
