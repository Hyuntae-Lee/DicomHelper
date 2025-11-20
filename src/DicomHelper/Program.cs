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
        const string RDSRInfoPath = @"D:\Projects\Utilities\DicomHelper\git\src\SampleData\RDSR.json";

        static void Main()
        {
            PatientInfo_t patientInfo = new PatientInfo_t();
            ProductInfo_t productInfo = new ProductInfo_t();
            DoseInfo_t doseInfo = new DoseInfo_t();
            StudyInfo_t studyInfo = new StudyInfo_t();
            List<IrradiationEvents_t> irrEventList = new List<IrradiationEvents_t>();

            // get input
            if (!InputParser.ParseInput(ref patientInfo, ref productInfo, ref doseInfo, ref studyInfo, ref irrEventList,
                RDSRInfoPath))
            {
                return;
            }

            // create builder
            var rdsrBuilder = new RDSRBuilder();

            // set input to builder
            rdsrBuilder.PatientInfo = patientInfo;
            rdsrBuilder.ProductInfo = productInfo;
            rdsrBuilder.DoseInfo = doseInfo;
            rdsrBuilder.IrrEventList.AddRange(irrEventList);

            // build data set
            var ds = rdsrBuilder.BuildRDSR();

            // save to file
            var file = new DicomFile(ds);
            file.Save(@"D:\TTT\RDSR_T.dcm");
        }
    }
}
