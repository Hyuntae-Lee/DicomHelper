using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomHelper
{
    class ProductInfo_t
    {
        public string Manufacturer { set; get; }
        public string ModelName { set; get; }
        public string InstitutionName { set; get; }
        public string InstitutionAddress { set; get; }
        public string StationName { set; get; }
        public string DeviceSerialNumber { set; get; }
        public string SoftwareVersion { set; get; }
        public string SOPInstanceUID { set; get; }
        public string StudyInstanceUID { set; get; }
        public string SeriesInstanceUID { set; get; }
    }
}
