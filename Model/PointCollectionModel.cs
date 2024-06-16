using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NakormiBot.Model
{
    [Serializable]
    public class PointCollectionModel
    { 
        public string Name;
        public string Adres;
        public List<VolumeKormModel> VolumeKorm;

    }
}
