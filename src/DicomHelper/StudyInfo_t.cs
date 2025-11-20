using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomHelper
{
    class StudyInfo_t
    {
        public string SOPInstanceUID { set; get; }
        public string StudyInstanceUID { set; get; }
        public string SeriesInstanceUID { set; get; }
        public string SpecificCharacterSet { set; get; }
        public string SeriesNumber { set; get; }
        public string InstanceNumber { set; get; }
        public string StudyDate { set; get; }
        public string StudyTime { set; get; }
        public string AccessionNumber { set; get; }
    }
}
