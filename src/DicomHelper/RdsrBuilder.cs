using System;
using System.Collections.Generic;
using FellowOakDicom;

namespace DicomHelper
{
    public class ScanData
    {
        public double Kvp;
        public double MA;
        public double ExposureTimeMs;
        public double CTDIvol;
        public double DLP;
        public string Region; // e.g., "Chest"
    }

    class RdsrBuilder
    {
        public static DicomFile CreateXr29CtRdsr(ScanData scout, ScanData upper, ScanData lower)
        {
            var ds = new DicomDataset();

            // 1. Header
            // - Common
            ds.Add(DicomTag.SpecificCharacterSet, "ISO_IR 100");
            // - SOP
            ds.Add(DicomTag.SOPClassUID, "1.2.840.10008.5.1.4.1.1.88.67"); // Radiation Dose SR Storage
            ds.Add(DicomTag.SOPInstanceUID, DicomUID.Generate().UID);
            ds.Add(DicomTag.Modality, "SR");
            // - Patient
            ds.Add(DicomTag.PatientName, "Test^Patient");
            ds.Add(DicomTag.PatientID, "12345");
            ds.Add(DicomTag.PatientBirthDate, "19700101");
            ds.Add(DicomTag.PatientSex, "M");
            // - Product
            ds.Add(DicomTag.Manufacturer, "MyCompany");
            ds.Add(DicomTag.ManufacturerModelName, "ModelX");
            ds.Add(DicomTag.DeviceSerialNumber, "123456");
            ds.Add(DicomTag.SoftwareVersions, "1.0.0");
            // - Study
            ds.Add(DicomTag.StudyInstanceUID, DicomUID.Generate().UID);
            ds.Add(DicomTag.StudyID, "1");
            // - Series
            ds.Add(DicomTag.SeriesInstanceUID, DicomUID.Generate().UID);
            ds.Add(DicomTag.SeriesNumber, "1");
            ds.Add(DicomTag.InstanceNumber, "1");

            // 2. RDSR
            var rootSeq = new DicomSequence(DicomTag.ContentSequence);
            var rootItem = new DicomDataset();
            rootItem.Add(DicomTag.ValueType, "CONTAINER");
            rootItem.Add(new DicomSequence(DicomTag.ConceptNameCodeSequence, CreateCode("113701", "DCM", "Radiation Dose Report")));
            rootItem.Add(DicomTag.ContinuityOfContent, "SEPARATE");

            // - Root ContentSequence
            var contentSeq = new DicomSequence(DicomTag.ContentSequence);
            // - Irration Event
            {
                // : Sample Scout (Localizer) Event ---
                contentSeq.Items.Add(CreateIrradiationEvent("LOCALIZER", "Localizer", scout));

                // : Sample  CT 1
                contentSeq.Items.Add(CreateIrradiationEvent("113822", "CT Acquisition", upper));

                // : Sample  CT 2
                contentSeq.Items.Add(CreateIrradiationEvent("113822", "CT Acquisition", lower));
            }

            // - Accumlated Dose
            contentSeq.Items.Add(CreateAccumulatedDoseContainer(10.5, 350.0));

            rootItem.Add(contentSeq);
            rootSeq.Items.Add(rootItem);
            ds.Add(rootSeq);

            return new DicomFile(ds);
        }

        public static DicomDataset CreateAccumulatedDoseContainer(
            double totalCTDIvol_mGy,
            double totalDLP_mGy_cm
        )
        {
            var container = new DicomDataset();

            container.Add(DicomTag.RelationshipType, "CONTAINS");
            container.Add(DicomTag.ValueType, "CONTAINER");

            // ConceptName for Accumulated Dose
            container.Add(new DicomSequence(
                DicomTag.ConceptNameCodeSequence,
                new DicomDataset {
                { DicomTag.CodeValue, "113812" },
                { DicomTag.CodingSchemeDesignator, "DCM" },
                { DicomTag.CodeMeaning, "CT Accumulated Dose Data" }
                }
            ));

            container.Add(DicomTag.ContinuityOfContent, "SEPARATE");

            // Create content items
            var contentItems = new List<DicomDataset>();

            // 1) Accumulated CTDIvol
            contentItems.Add(
                CreateAccumulatedDoseItem(
                    codeValue: "113830",   // CTDIvol
                    scheme: "DCM",
                    meaning: "CTDIvol",
                    numericValue: totalCTDIvol_mGy.ToString("0.###"),
                    unitCode: "mGy",
                    unitMeaning: "mGy"
                )
            );

            // 2) Accumulated DLP
            contentItems.Add(
                CreateAccumulatedDoseItem(
                    codeValue: "113838",   // DLP
                    scheme: "DCM",
                    meaning: "DLP",
                    numericValue: totalDLP_mGy_cm.ToString("0.###"),
                    unitCode: "mGy.cm",    // UCUM
                    unitMeaning: "mGy.cm"
                )
            );

            // Add into ContentSequence
            container.Add(new DicomSequence(
                DicomTag.ContentSequence,
                contentItems.ToArray()
            ));

            return container;
        }

        private static DicomDataset CreateIrradiationEvent(string codeValue, string meaning, ScanData data)
        {
            var evt = new DicomDataset();
            evt.Add(DicomTag.ValueType, "CONTAINER");
            evt.Add(new DicomSequence(DicomTag.ConceptNameCodeSequence, CreateCode(codeValue, "DCM", meaning)));
            evt.Add(DicomTag.ContinuityOfContent, "SEPARATE");

            var seq = new DicomSequence(DicomTag.ContentSequence);

            // Numeric items
            seq.Items.Add(CreateNumericItem("113733", "DCM", "KVP", data.Kvp, "kV"));
            seq.Items.Add(CreateNumericItem("113735", "DCM", "X-Ray Tube Current", data.MA, "mA"));
            seq.Items.Add(CreateNumericItem("113736", "DCM", "Exposure Time", data.ExposureTimeMs, "ms"));

            if (meaning == "CT Acquisition")
            {
                seq.Items.Add(CreateNumericItem("113830", "DCM", "CTDIvol", data.CTDIvol, "mGy"));
                seq.Items.Add(CreateNumericItem("113838", "DCM", "DLP", data.DLP, "mGy.cm"));
            }

            evt.Add(seq);
            return evt;
        }

        public static DicomDataset CreateAccumulatedDoseItem(
            string codeValue,
            string scheme,
            string meaning,
            string numericValue,
            string unitCode,       // e.g., "mGy" or "mGy.cm"
            string unitMeaning      // e.g., "mGy" or "mGy.cm"
        )
        {
            var item = new DicomDataset();

            item.Add(DicomTag.RelationshipType, "CONTAINS");
            item.Add(DicomTag.ValueType, "NUM");

            // Concept name
            item.Add(new DicomSequence(
                DicomTag.ConceptNameCodeSequence,
                new DicomDataset {
                { DicomTag.CodeValue, codeValue },
                { DicomTag.CodingSchemeDesignator, scheme },
                { DicomTag.CodeMeaning, meaning }
                }
            ));

            // Value
            item.Add(DicomTag.NumericValue, numericValue);

            // Units
            item.Add(new DicomSequence(
                DicomTag.MeasurementUnitsCodeSequence,
                new DicomDataset {
                { DicomTag.CodeValue, unitCode },
                { DicomTag.CodingSchemeDesignator, "UCUM" },
                { DicomTag.CodeMeaning, unitMeaning }
                }
            ));

            return item;
        }

        private static DicomDataset CreateNumericItem(string codeValue, string scheme, string meaning, double value, string unit)
        {
            var ds = new DicomDataset();
            ds.Add(DicomTag.ValueType, "NUM");
            ds.Add(new DicomSequence(DicomTag.ConceptNameCodeSequence, CreateCode(codeValue, scheme, meaning)));
            ds.Add(DicomTag.NumericValue, value.ToString("F2"));
            ds.Add(new DicomSequence(DicomTag.MeasurementUnitsCodeSequence, CreateCode(unit, "UCUM", unit)));
            return ds;
        }

        private static DicomDataset CreateCode(string codeValue, string scheme, string meaning)
        {
            var ds = new DicomDataset();
            ds.Add(DicomTag.CodeValue, codeValue);
            ds.Add(DicomTag.CodingSchemeDesignator, scheme);
            ds.Add(DicomTag.CodeMeaning, meaning);
            return ds;
        }
    }
}
