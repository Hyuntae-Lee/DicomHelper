using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json.Nodes;

namespace DicomHelper
{
    class InputParser
    {
        public static bool ParseInput(ref PatientInfo_t patientInfo, ref ProductInfo_t productInfo,
            ref DoseInfo_t doseInfo, ref StudyInfo_t studyInfo, ref List<IrradiationEvents_t> irrEventList,
            string path)
        {
            string json = File.ReadAllText(path);

            var root = JsonNode.Parse(json);
            var header = root["Header"];
            var settings = root["Settings"];
            var contents = root["Contents"];
            var addDose = contents["AccumulatedDose"];
            var irrEvents = contents["IrradiationEvents"];

            if (!ParseHeader(ref patientInfo, ref productInfo, ref studyInfo, header))
            {
                return false;
            }

            if (!ParseDoseInfo(ref doseInfo, settings))
            {
                return false;
            }

            if (!ParseAccumulatedDose(ref doseInfo, addDose))
            {
                return false;
            }

            if (!ParseIrradiationEvents(ref irrEventList, irrEvents.AsArray()))
            {
                return false;
            }

            return true;
        }

        protected static bool ParseIrradiationEvents(ref List<IrradiationEvents_t> irrEventList, JsonArray jsonArray)
        {
            foreach (JsonNode json in jsonArray)
            {
                IrradiationEvents_t item = new IrradiationEvents_t();

                item.Modality = json["Modality"].GetValue<string>();
                item.kVp = json["kVp"].GetValue<double>();
                item.mA = json["mA"].GetValue<double>();
                item.ExposureTime = json["ExposureTime"].GetValue<double>();
                item.DAP = json["DAP"].GetValue<double>();
                item.CTDIvol = json["CTDIvol"].GetValue<double>();
                item.DLP = json["DLP"].GetValue<double>();

                irrEventList.Add(item);
            }

            return true;
        }

        protected static bool ParseAccumulatedDose(ref DoseInfo_t doseInfo, JsonNode json)
        {
            doseInfo.AccCTDIvol = json["CTDIvol"].GetValue<double>();
            doseInfo.AccDLP = json["DLP"].GetValue<double>();

            return true;
        }

        protected static bool ParseHeader(ref PatientInfo_t patientInfo, ref ProductInfo_t productInfo,
            ref StudyInfo_t studyInfo, JsonNode json)
        {
            patientInfo.Name = json["PatientName"].GetValue<string>();
            patientInfo.ID = json["PatientID"].GetValue<string>();
            patientInfo.Age = json["PatientAge"].GetValue<int>();
            patientInfo.Gender = json["PatientGender"].GetValue<string>();
            patientInfo.BirthDate = json["PatientBirthDate"].GetValue<string>();
            patientInfo.TargetPart = json["TargetPart"].GetValue<string>();
            patientInfo.Pediatric = json["Pediatric"].GetValue<bool>();

            productInfo.Manufacturer = json["Manufacturer"].GetValue<string>();
            productInfo.ModelName = json["ModelName"].GetValue<string>();
            productInfo.InstitutionName = json["InstitutionName"].GetValue<string>();
            productInfo.InstitutionAddress = json["InstitutionAddress"].GetValue<string>();
            productInfo.StationName = json["StationName"].GetValue<string>();
            productInfo.DeviceSerialNumber = json["DeviceSerialNumber"].GetValue<string>();
            productInfo.SoftwareVersion = json["SoftwareVersion"].GetValue<string>();

            studyInfo.SOPInstanceUID = json["SOPInstanceUID"].GetValue<string>();
            studyInfo.StudyInstanceUID = json["StudyInstanceUID"].GetValue<string>();
            studyInfo.SeriesInstanceUID = json["SeriesInstanceUID"].GetValue<string>();
            studyInfo.SpecificCharacterSet = json["SpecificCharacterSet"].GetValue<string>();
            studyInfo.SeriesNumber = json["SeriesNumber"].GetValue<string>();
            studyInfo.InstanceNumber = json["InstanceNumber"].GetValue<string>();
            studyInfo.StudyDate = json["StudyDate"].GetValue<string>();
            studyInfo.StudyTime = json["StudyTime"].GetValue<string>();
            studyInfo.AccessionNumber = json["AccessionNumber"].GetValue<string>();

            return true;
        }

        protected static bool ParseDoseInfo(ref DoseInfo_t doseInfo, JsonNode json)
        {
            doseInfo.DLPAlertThreshold = json["DLPAlertThreshold"].GetValue<double>();
            doseInfo.CTDIAlertThreshold = json["CTDIAlertThreshold"].GetValue<double>();
            doseInfo.DLPNotificationThreshold = json["DLPNotificationThreshold"].GetValue<double>();
            doseInfo.CTDINotificationThreshold = json["CTDINotificationThreshold"].GetValue<double>();
            doseInfo.DLPUnit = json["DLPUnit"].GetValue<string>();
            doseInfo.CTDIlvolUnit = json["CTDIvolUnit"].GetValue<string>();

            return true;
        }
    }
}
