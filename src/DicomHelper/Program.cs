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
        static void Main(string[] args)
        {
            if (args.Count() != 2)
            {
                return;
            }

            var jsonPath = args[0];
            var dcmPath = args[1];

            //
            PatientInfo_t patientInfo = new PatientInfo_t();
            ProductInfo_t productInfo = new ProductInfo_t();
            DoseInfo_t doseInfo = new DoseInfo_t();
            StudyInfo_t studyInfo = new StudyInfo_t();
            List<IrradiationEvents_t> irrEventList = new List<IrradiationEvents_t>();

            // get input
            if (!InputParser.ParseInput(ref patientInfo, ref productInfo, ref doseInfo,
                ref studyInfo, ref irrEventList, jsonPath))
            {
                return;
            }

            // create builder
            var rdsrBuilder = new RDSRBuilder();

            // set input to builder
            rdsrBuilder.PatientInfo = patientInfo;
            rdsrBuilder.ProductInfo = productInfo;
            rdsrBuilder.DoseInfo = doseInfo;
            rdsrBuilder.StudyInfo = studyInfo;
            rdsrBuilder.IrrEventList.AddRange(irrEventList);

            // build data set
            var ds = rdsrBuilder.BuildRDSR();

            // save to file
            var file = new DicomFile(ds);
            file.Save(dcmPath);
        }
    }
}
