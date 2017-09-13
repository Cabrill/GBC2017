using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBC2017.GumRuntimes
{
    public partial class EnemyInfoRuntime
    {
        public Action OnClose;

        partial void CustomInitialize()
        {
            CloseButton.Click += CloseButton_Click;
        }

        private void CloseButton_Click(FlatRedBall.Gui.IWindow window)
        {
            Visible = false;
            OnClose?.Invoke();
        }
    }
}
