using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomHelper
{
    class IrradiationEvents_t
    {
        public string Modality { set; get; }
        public double kVp { set; get; }
        public double mA { set; get; }
        public double ExposureTime { set; get; }
        public double DAP { set; get; }
        public double CTDIvol { set; get; }
        public double DLP { set; get; }
    }
}
