using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomHelper
{
    class Program
    {
        const string RDSRInofPath = @"D:\TTT\RDSR.json";

        static void Main(string[] args)
        {
            var scout = new ScanData { Kvp = 120, MA = 50, ExposureTimeMs = 150 };
            var upper = new ScanData { Kvp = 120, MA = 150, ExposureTimeMs = 500, CTDIvol = 15.2, DLP = 550 };
            var lower = new ScanData { Kvp = 120, MA = 160, ExposureTimeMs = 520, CTDIvol = 16.0, DLP = 580 };

            var dicomFile = RdsrBuilder.CreateXr29CtRdsr(scout, upper, lower);
            dicomFile.Save(@"D:\TTT\xr29_ct_rdsr.dcm");
        }
    }
}
