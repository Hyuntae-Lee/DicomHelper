using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomHelper
{
    class DoseInfo_t
    {
        public double AccDLP { set; get; }
        public double AccCTDIvol { set; get; }
        public string DLPUnit { set; get; }
        public string CTDIlvolUnit { set; get; }
        public double DLPNotificationThreshold { set; get; }
        public double DLPAlertThreshold { set; get; }
        public double CTDINotificationThreshold { set; get; }
        public double CTDIAlertThreshold { set; get; }
        public bool AECEnabled { set; get; }
        public bool CTDIAlertEnabled { set; get; }
        public bool CTDINotificationEnabled { set; get; }
        public bool DLPAlertEnabled { set; get; }
        public bool DLPNotificationEnabled { set; get; }
    }
}
