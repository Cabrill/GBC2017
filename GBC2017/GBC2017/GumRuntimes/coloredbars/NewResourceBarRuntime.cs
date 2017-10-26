using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC2017.GumRuntimes
{
    public partial class NewResourceBarRuntime
    {
        public void UpdateBar(double current, double total)
        {
            if (TextInstance.Visible)
            {
                if (total > 0)
                {
                    TextInstance.Text = $"{current.ToString("0.#")}/{total.ToString("0")}";
                }
                else
                {
                    TextInstance.Text = $"{current.ToString("#")}";
                }
            }

            var barPct = 1.0;
            if (total > 0)
            {
                barPct = current / total;
            }
            //FillRectangle.Width = (float)(barPct * 100);
        }
    }
}
