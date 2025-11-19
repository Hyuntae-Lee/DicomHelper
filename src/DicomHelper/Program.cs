using FellowOakDicom;
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
        static void Main()
        {
            var rdsrBuilder = new RDSRBuilder();
            // - patient
            rdsrBuilder.PatientInfo.Name = "Gildong^Hong";
            rdsrBuilder.PatientInfo.ID = "12345";
            rdsrBuilder.PatientInfo.Age = 21;
            rdsrBuilder.PatientInfo.Gender = "M";
            rdsrBuilder.PatientInfo.BirthDate = "19971013";
            rdsrBuilder.PatientInfo.Pediatric = false;
            // - product
            rdsrBuilder.ProductInfo.Manufacturer = "vatech";
            rdsrBuilder.ProductInfo.ModelName = "DCT-01CS";
            rdsrBuilder.ProductInfo.InstitutionName = "";
            rdsrBuilder.ProductInfo.InstitutionAddress = "";
            rdsrBuilder.ProductInfo.StationName = "Smart M Plus";
            rdsrBuilder.ProductInfo.DeviceSerialNumber = "SN111112222";
            rdsrBuilder.ProductInfo.SoftwareVersion = "1.0.0.0";
            rdsrBuilder.ProductInfo.SOPInstanceUID = "1.2.410.200028.20261119121212.1";
            rdsrBuilder.ProductInfo.StudyInstanceUID = "1.2.410.200028.20261119121212.1.1";
            rdsrBuilder.ProductInfo.SeriesInstanceUID = "1.2.410.200028.20261119121212.1.1.1";
            // - dose
            rdsrBuilder.DoseInfo.AccCTDIvol = 150.0;
            rdsrBuilder.DoseInfo.AccDLP = 10.0;
            rdsrBuilder.DoseInfo.DLPNotificationThreshold = 100.0;
            rdsrBuilder.DoseInfo.DLPAlertThreshold = 200.0;
            rdsrBuilder.DoseInfo.CTDINotificationThreshold = 10.0;
            rdsrBuilder.DoseInfo.CTDIAlertThreshold = 25.0;

            // usage
            var ds = rdsrBuilder.BuildRDSR();

            // Save to file
            var file = new DicomFile(ds);

            file.Save(string.Format(@"D:\TTT\RDSR_{0}.dcm", DateTime.UtcNow.ToString("HHmmss")));
        }
    }
}
