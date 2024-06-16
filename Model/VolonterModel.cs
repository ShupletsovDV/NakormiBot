using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NakormiBot.Model
{
    [Serializable]
    public class VolonterModel
    {
        public string FIO;
        public string Email;
        public string Phone;
        public string NikTelegram;
        public string Photo;
        public long IdTelegram;
        public List<VolumeKormModel> VolumeKorm;
       
    }

    [Serializable]
    public class PeredachaKormaModel
    {
        public string From;
        public string To;
        public string NameKorm;
        public string CountKorm;
        public string Photo;
    }

    [Serializable]
    public class LogPointCollectionModel
    {
        public string Point;
        public string FIO;
        public string NameKorm;
        public string CountKorm;
        public string Photo;
        public string date;
    }

    [Serializable]
    public class FeedAnimalModel
    {
        public string FIO;
        public string NameKorm;
        public string CountKorm;
        public string Photo;
        public string date;
    }


}
