using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBC2017.Entities.BaseEntities;
using GBC2017.Performance;

namespace GBC2017.GameClasses.Interfaces
{
    public interface IBuildButton
    {
        bool IsEnabled { get; }
        IEntityFactory BuildingFactory { get; }
        Type BuildingType { get; }

    }
}
