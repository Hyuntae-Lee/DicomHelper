using System;
using System.Collections.Generic;
using FellowOakDicom;

namespace DicomHelper
{
    using System;
    using FellowOakDicom;
    using System.Collections.Generic;

    public static class CustomDicomTags
    {
        public static readonly DicomTag AcquisitionTypeCodeSequence = new DicomTag(0x0040, 0x0254);
        public static readonly DicomTag ExposureModulationType = new DicomTag(0x0018, 0x9323);
        public static readonly DicomTag DLP = new DicomTag(0x0018, 0x9332);
        public static readonly DicomTag DAP = new DicomTag(0x0018, 0x115E);
        public static readonly DicomTag CTDIvol = new DicomTag(0x0018, 0x9345);
    }

    class RDSRBuilder
    {
        public PatientInfo_t PatientInfo { get { return _patientInfo; } }
        private PatientInfo_t _patientInfo;
        public ProductInfo_t ProductInfo { get { return _productInfo; } }
        private ProductInfo_t _productInfo;
        public DoseInfo_t DoseInfo { get { return _doseInfo_t; } }
        private DoseInfo_t _doseInfo_t;

        public RDSRBuilder()
        {
            _patientInfo = new PatientInfo_t();
            _productInfo = new ProductInfo_t();
            _doseInfo_t = new DoseInfo_t();
        }

        public DicomDataset BuildRDSR()
        {
            var ds = new DicomDataset();

            // Header
            // - Common
            ds.Add(DicomTag.SOPClassUID, DicomUID.XRayRadiationDoseSRStorage.UID);
            ds.Add(DicomTag.SOPInstanceUID, ProductInfo.SOPInstanceUID);
            ds.Add(DicomTag.StudyInstanceUID, ProductInfo.StudyInstanceUID);
            ds.Add(DicomTag.SeriesInstanceUID, ProductInfo.SeriesInstanceUID);
            ds.Add(DicomTag.Modality, "SR");
            ds.Add(DicomTag.InstanceCreationDate, DateTime.UtcNow.ToString("yyyyMMdd")); // by current
            ds.Add(DicomTag.InstanceCreationTime, DateTime.UtcNow.ToString("HHmmss")); // by current
            //"ISO_IR 192"
            // - Patient
            ds.Add(DicomTag.PatientName, PatientInfo.Name);
            ds.Add(DicomTag.PatientID, PatientInfo.ID);
            ds.Add(DicomTag.PatientAge, PatientInfo.Age.ToString("D3") + "Y");
            ds.Add(DicomTag.PatientSex, PatientInfo.Gender);
            ds.Add(DicomTag.PatientBirthDate, PatientInfo.BirthDate);
            // - Product
            ds.Add(DicomTag.Manufacturer, ProductInfo.Manufacturer);
            ds.Add(DicomTag.ManufacturerModelName, ProductInfo.ModelName);
            ds.Add(DicomTag.InstitutionName, ProductInfo.InstitutionName);
            ds.Add(DicomTag.InstitutionAddress, ProductInfo.InstitutionAddress);
            ds.Add(DicomTag.StationName, ProductInfo.StationName);
            ds.Add(DicomTag.DeviceSerialNumber, ProductInfo.DeviceSerialNumber);
            ds.Add(DicomTag.SoftwareVersions, ProductInfo.SoftwareVersion);
            
            // ProtocolName including Pediatric
            if (PatientInfo.Pediatric)
            {
                ds.AddOrUpdate(DicomTag.ProtocolName, "Pediatric Chest Low Dose");
            }
            else
            {
                ds.AddOrUpdate(DicomTag.ProtocolName, "Adult Abdomen Standard");
            }

            // Contents
            var rootContentItems = new List<DicomDataset>();
            // 1) Accumulated Dose container (TID 10012)
            {
                var container = CreateContainer("113800", "DCM", "CT Accumulated Dose Data");

                // Inside Accumulated Dose: add numeric measurements e.g., Accumulated DLP and CTDIvol
                var accDlpItem = CreateNumericItem("113805", "DCM", "Accumulated DLP",
                    DoseInfo.AccDLP, "mGy.cm", "UCUM", "mGy.cm", "CONTAINS");
                var accCtDiItem = CreateNumericItem("113806", "DCM", "Accumulated CTDIvol",
                    DoseInfo.AccCTDIvol, "mGy", "UCUM", "mGy", "CONTAINS");

                //
                container.Add(new DicomSequence(DicomTag.ContentSequence, accDlpItem, accCtDiItem));
                rootContentItems.Add(container);
            }
            // 2) Irradiation Event container (TID 10011)
            {
                var irrEvent = CreateContainer("113820", "DCM", "Irradiation Event"); // Concept name code illustrative
                                                                                      // Add an example numeric for that irradiation event (forward estimates)

                var scout = CreateIrradiationEventScout(50, 100, 500, 250, 30);
                var ct1 = CreateIrradiationEventCT(50, 100, 500, 250, 30);
                var ct2 = CreateIrradiationEventCT(50, 100, 500, 250, 30);


                irrEvent.Add(new DicomSequence(DicomTag.ContentSequence, scout, ct1, ct2));
                rootContentItems.Add(irrEvent);
            }
            // 3) CT Dose Check Details (TID 10015)
            {
                var doseCheckContainer = CreateContainer("113900", "DCM", "Dose Check Alert Details"); // 113900 is the DICOM code for Dose Check Alert Details
                var doseCheckChildren = new List<DicomDataset>();

                // Indicate whether DLP/CTDI alert values are configured (True/False -> use CODE 'Yes'/'No' from DCID230)
                // DICOM uses coded "YES/NO" — here we use SCT/other or DCM '113901' etc per template — adapt codes per DICOM spec.
                bool dlpAlertConfigured = !double.IsNaN(DoseInfo.DLPAlertThreshold);
                bool ctDiAlertConfigured = !double.IsNaN(DoseInfo.CTDIAlertThreshold);
                bool dlpNotiConfigured = !double.IsNaN(DoseInfo.DLPNotificationThreshold);
                bool ctDiNotiConfigured = !double.IsNaN(DoseInfo.CTDINotificationThreshold);

                // CODE items stating whether configured (use DCID 230 'Yes' = (373066001,SCT,"Yes"), 'No' = (373067005,SCT,'No') — these are examples)
                doseCheckChildren.Add(CreateCodeItemContent("113901", "DCM", "DLP Alert Value Configured", dlpAlertConfigured ? "373066001" : "373067005", "SCT", dlpAlertConfigured ? "Yes" : "No", "CONTAINS"));
                doseCheckChildren.Add(CreateCodeItemContent("113902", "DCM", "CTDIvol Alert Value Configured", ctDiAlertConfigured ? "373066001" : "373067005", "SCT", ctDiAlertConfigured ? "Yes" : "No", "CONTAINS"));

                // Add numeric alert values if configured
                if (dlpAlertConfigured)
                    doseCheckChildren.Add(CreateNumericItem("113903", "DCM", "DLP Alert Value", DoseInfo.DLPAlertThreshold, "mGy.cm", "UCUM", "mGy.cm", "CONTAINS"));
                if (ctDiAlertConfigured)
                    doseCheckChildren.Add(CreateNumericItem("113904", "DCM", "CTDIvol Alert Value", DoseInfo.CTDIAlertThreshold, "mGy", "UCUM", "mGy", "CONTAINS"));

                // Indicate whether notification values are configured (TID 10015 also expects Notification items)
                doseCheckChildren.Add(CreateCodeItemContent("113911", "DCM", "DLP Notification Value Configured", dlpNotiConfigured ? "373066001" : "373067005", "SCT", dlpNotiConfigured ? "Yes" : "No", "CONTAINS"));
                doseCheckChildren.Add(CreateCodeItemContent("113912", "DCM", "CTDIvol Notification Value Configured", ctDiNotiConfigured ? "373066001" : "373067005", "SCT", ctDiNotiConfigured ? "Yes" : "No", "CONTAINS"));

                if (dlpNotiConfigured)
                    doseCheckChildren.Add(CreateNumericItem("113913", "DCM", "DLP Notification Value", DoseInfo.DLPNotificationThreshold, "mGy.cm", "UCUM", "mGy.cm", "CONTAINS"));
                if (ctDiNotiConfigured)
                    doseCheckChildren.Add(CreateNumericItem("113914", "DCM", "CTDIvol Notification Value", DoseInfo.CTDINotificationThreshold, "mGy", "UCUM", "mGy", "CONTAINS"));

                // Now record whether forward estimates exceed Notification or Alert values
                bool triggeredDlpNotification = dlpNotiConfigured && DoseInfo.AccDLP > DoseInfo.DLPNotificationThreshold;
                bool triggeredCtDiNotification = ctDiNotiConfigured && DoseInfo.AccCTDIvol > DoseInfo.CTDINotificationThreshold;
                bool triggeredDlpAlert = dlpAlertConfigured && DoseInfo.AccDLP > DoseInfo.DLPAlertThreshold;
                bool triggeredCtDiAlert = ctDiAlertConfigured && DoseInfo.AccCTDIvol > DoseInfo.CTDIAlertThreshold;

                // Add CODE items for triggered/not triggered (Yes/No)
                doseCheckChildren.Add(CreateCodeItemContent("113920", "DCM", "DLP Notification Exceeded", "373066001", "SCT", triggeredDlpNotification ? "Yes" : "No", "CONTAINS"));
                doseCheckChildren.Add(CreateCodeItemContent("113921", "DCM", "CTDIvol Notification Exceeded", "373066001", "SCT", triggeredCtDiNotification ? "Yes" : "No", "CONTAINS"));
                doseCheckChildren.Add(CreateCodeItemContent("113922", "DCM", "DLP Alert Exceeded", "373066001", "SCT", triggeredDlpAlert ? "Yes" : "No", "CONTAINS"));
                doseCheckChildren.Add(CreateCodeItemContent("113923", "DCM", "CTDIvol Alert Exceeded", "373066001", "SCT", triggeredCtDiAlert ? "Yes" : "No", "CONTAINS"));

                doseCheckContainer.Add(new DicomSequence(DicomTag.ContentSequence, doseCheckChildren.ToArray()));
                rootContentItems.Add(doseCheckContainer);
            }
            // 4) AEC
            {
                var aecItem = CreateAECContainer(
                    aecEnabled: true,
                    aecModeDescription: "Scout-based tube current estimation: single frontal scout, lookup table -> recommended mA."
                );

                rootContentItems.Add(aecItem);
            }

            //
            var contentSequenceItems = new List<DicomDataset>(rootContentItems);
            ds.Add(new DicomSequence(DicomTag.ContentSequence, contentSequenceItems.ToArray()));

            return ds;
        }

        DicomDataset CreateIrradiationEventCT(double kVp, double mA, double exposureTime, double ctdivol, double dlp)
        {
            var ct = new DicomDataset();

            ct.Add(new DicomSequence(
                DicomTag.ConceptNameCodeSequence,
                new DicomDataset
                {
                    { DicomTag.CodeValue, "113701" },
                    { DicomTag.CodingSchemeDesignator, "DCM" },
                    { DicomTag.CodeMeaning, "CT Irradiation Event Data" }
                }));

            ct.AddOrUpdate(DicomTag.IrradiationEventUID, DicomUID.Generate().UID);

            // Acquisition Type = SPIRAL CT (113620, DCM)
            ct.Add(new DicomSequence(
                CustomDicomTags.AcquisitionTypeCodeSequence,
                new DicomDataset
                {
                    { DicomTag.CodeValue, "113620" },
                    { DicomTag.CodingSchemeDesignator, "DCM" },
                    { DicomTag.CodeMeaning, "SPIRAL CT" }
                }));

            ct.AddOrUpdate(DicomTag.KVP, kVp.ToString());
            ct.AddOrUpdate(DicomTag.XRayTubeCurrent, mA.ToString());
            ct.AddOrUpdate(DicomTag.ExposureTime, exposureTime.ToString());
            ct.AddOrUpdate(DicomTag.CTDIvol, ctdivol.ToString());
            ct.AddOrUpdate(CustomDicomTags.DLP, dlp.ToString());

            // AEC indicator (Exposure Modulation Type)
            ct.Add(new DicomSequence(
                DicomTag.ExposureModulationType,
                new DicomDataset
                {
                    { DicomTag.CodeValue, "OTH" },        // Use OTH for "OTHER"
                    { DicomTag.CodingSchemeDesignator, "DCM" },
                    { DicomTag.CodeMeaning, "OTHER" }
                }));

            return ct;
        }

        DicomDataset CreateIrradiationEventScout(double kVp, double mA, double exposureTime, double ctdivol, double dlp)
        {
            var irrEvent = CreateContainer("113701", "DCM", "CT Irradiation Event Data");


            var scout = new DicomDataset();

            // ConceptNameCodeSequence: CT Irradiation Event Data (113701, DCM)
            scout.Add(new DicomSequence(
                DicomTag.ConceptNameCodeSequence,
                new DicomDataset
                {
                    { DicomTag.CodeValue, "113701" },
                    { DicomTag.CodingSchemeDesignator, "DCM" },
                    { DicomTag.CodeMeaning, "CT Irradiation Event Data" }
                }));

            scout.AddOrUpdate(DicomTag.IrradiationEventUID, DicomUID.Generate().UID);

            // Acquisition Type = LOCALIZER (113622, DCM)
            scout.Add(new DicomSequence(
                CustomDicomTags.AcquisitionTypeCodeSequence,
                new DicomDataset
                {
                    { DicomTag.CodeValue, "113622" },
                    { DicomTag.CodingSchemeDesignator, "DCM" },
                    { DicomTag.CodeMeaning, "LOCALIZER" }
                }));

            scout.AddOrUpdate(DicomTag.KVP, kVp.ToString());
            scout.AddOrUpdate(DicomTag.XRayTubeCurrent, mA.ToString());
            scout.AddOrUpdate(DicomTag.ExposureTime, exposureTime.ToString());
            scout.AddOrUpdate(DicomTag.CTDIvol, ctdivol.ToString());
            scout.AddOrUpdate(CustomDicomTags.DLP, dlp.ToString());

            return scout;
        }

        void AddConceptNameCodeSequence(DicomDataset parent, string codeValue, string scheme, string meaning)
        {
            var seqItem = new DicomDataset();
            seqItem.Add(DicomTag.CodeValue, codeValue);
            seqItem.Add(DicomTag.CodingSchemeDesignator, scheme);
            seqItem.Add(DicomTag.CodeMeaning, meaning);
            var seq = new DicomSequence(DicomTag.ConceptNameCodeSequence, seqItem);
            parent.Add(seq);
        }

        DicomDataset CreateCodeItem(string codeValue, string codingScheme, string codeMeaning)
        {
            var ds = new DicomDataset();
            ds.Add(DicomTag.CodeValue, codeValue);
            ds.Add(DicomTag.CodingSchemeDesignator, codingScheme);
            ds.Add(DicomTag.CodeMeaning, codeMeaning);
            return ds;
        }

        DicomDataset CreateContainer(string containerNameCodeValue, string scheme, string meaning, string relationshipType = "CONTAINS")
        {
            var item = new DicomDataset();
            item.Add(DicomTag.ValueType, "CONTAINER");                      // (0040,A040)
            AddConceptNameCodeSequence(item, containerNameCodeValue, scheme, meaning); // (0040,A043)
            item.Add(DicomTag.RelationshipType, relationshipType);          // (0040,A010)
                                                                            // Optionally add a template id sequence or continuity etc.
            return item;
        }

        DicomDataset CreateNumericItem(string nameCodeValue, string nameScheme, string nameMeaning,
                                              double value, string unitsCodeValue, string unitsScheme, string unitsMeaning,
                                              string relationshipType = "HAS CONCEPT MOD")
        {
            var item = new DicomDataset();
            item.Add(DicomTag.ValueType, "NUM");                            // (0040,A040)
            AddConceptNameCodeSequence(item, nameCodeValue, nameScheme, nameMeaning);

            // Measured Value Sequence (0040,A300) containing Numeric Value (0040,A30A)
            var measured = new DicomDataset();
            measured.Add(DicomTag.NumericValue, value.ToString("G"));       // (0040,A30A) Decimal String
                                                                            // Measurement Units Code Sequence (0040,08EA)
            var units = new DicomDataset();
            units.Add(DicomTag.CodeValue, unitsCodeValue);
            units.Add(DicomTag.CodingSchemeDesignator, unitsScheme);
            units.Add(DicomTag.CodeMeaning, unitsMeaning);
            measured.Add(new DicomSequence(DicomTag.MeasurementUnitsCodeSequence, units));
            item.Add(new DicomSequence(DicomTag.MeasuredValueSequence, measured));
            item.Add(DicomTag.RelationshipType, relationshipType);          // (0040,A010)
            return item;
        }

        DicomDataset CreateCodeItemContent(string nameCodeValue, string nameScheme, string nameMeaning,
                                                  string valueCodeValue, string valueScheme, string valueMeaning,
                                                  string relationshipType = "HAS CONCEPT MOD")
        {
            var item = new DicomDataset();
            item.Add(DicomTag.ValueType, "CODE");
            AddConceptNameCodeSequence(item, nameCodeValue, nameScheme, nameMeaning);

            var value = new DicomDataset();
            value.Add(DicomTag.CodeValue, valueCodeValue);
            value.Add(DicomTag.CodingSchemeDesignator, valueScheme);
            value.Add(DicomTag.CodeMeaning, valueMeaning);
            item.Add(new DicomSequence(DicomTag.ConceptCodeSequence, value)); // (0040,A168) Concept Code Sequence
            item.Add(DicomTag.RelationshipType, relationshipType);
            return item;
        }

        DicomDataset CreateAECContainer(bool aecEnabled, string aecModeDescription)
        {
            var aecContainer = CreateContainer("113950", "DCM", "Automatic Exposure Control Details");

            // 1) AEC Enabled
            var aecEnabledItem = new DicomDataset();
            aecEnabledItem.Add(DicomTag.ValueType, "TEXT");
            AddConceptNameCodeSequence(aecEnabledItem, "113951", "DCM", "AEC Enabled");
            aecEnabledItem.Add(DicomTag.TextValue, aecEnabled ? "ON" : "OFF");
            aecEnabledItem.Add(DicomTag.RelationshipType, "HAS OBS CONTEXT");

            // 2) AEC Mode Description
            var aecModeDescItem = new DicomDataset();
            aecModeDescItem.Add(DicomTag.ValueType, "TEXT");
            AddConceptNameCodeSequence(aecModeDescItem, "113952", "DCM", "AEC Mode Description");
            aecModeDescItem.Add(DicomTag.TextValue, aecModeDescription ?? "Scout-based mA estimation using standard scout + mA table");
            aecModeDescItem.Add(DicomTag.RelationshipType, "HAS CONCEPT MOD");

            //// 3) Estimated mA (NUM) - this is the mA selected/estimated from the scout
            //var estMAItem = CreateNumericItem("113953", "DCM", "Estimated mA from Scout", estimatedMA, "mA", "UCUM", "mA", "CONTAINS");

            //// 4) Control variable used by AEC - use DICOM code 111641 "Patient Equivalent Thickness" (control variable)
            ////    Code 111641 is defined in DICOM Controlled Terminology as "Patient Equivalent Thickness".
            //var controlVarItem = new DicomDataset();
            //controlVarItem.Add(DicomTag.ValueType, "NUM");
            //AddConceptNameCodeSequence(controlVarItem, "111641", "DCM", "Patient Equivalent Thickness");
            //// measured value sequence
            //var measured = new DicomDataset();
            //measured.Add(DicomTag.NumericValue, patientEquivalentThickness.ToString("G"));
            //var units = new DicomDataset();
            //units.Add(DicomTag.CodeValue, "mm");                 // UCUM code token for millimeter
            //units.Add(DicomTag.CodingSchemeDesignator, "UCUM");
            //units.Add(DicomTag.CodeMeaning, "mm");
            //measured.Add(new DicomSequence(DicomTag.MeasurementUnitsCodeSequence, units));
            //controlVarItem.Add(new DicomSequence(DicomTag.MeasuredValueSequence, measured));
            //controlVarItem.Add(DicomTag.RelationshipType, "CONTAINS");

            // 5) X-Ray Modulation type (CODE) - example values: ANGULAR, LONGITUDINAL, NONE
            //    Use DICOM concept "X-Ray Modulation Type" (controlled term code 113845 exists in DICOM CT terminology).
            //    Here we store the actual modulation used (for your device we can store NONE since you said "No dynamic modulation during rotation").
            var modulationItem = new DicomDataset();
            modulationItem.Add(DicomTag.ValueType, "CODE");
            AddConceptNameCodeSequence(modulationItem, "113845", "DCM", "X-Ray Modulation Type");
            // If your device uses NO modulation (you said system does not modify exposure during rotation), use value "NONE".
            var modulationValue = new DicomDataset();
            modulationValue.Add(DicomTag.CodeValue, "NONE");
            modulationValue.Add(DicomTag.CodingSchemeDesignator, "DCM");
            modulationValue.Add(DicomTag.CodeMeaning, "None");
            modulationItem.Add(new DicomSequence(DicomTag.ConceptCodeSequence, modulationValue));
            modulationItem.Add(DicomTag.RelationshipType, "CONTAINS");

            // Group them into the AEC container's ContentSequence
            aecContainer.Add(new DicomSequence(DicomTag.ContentSequence,
                aecEnabledItem,
                aecModeDescItem,
                modulationItem));

            return aecContainer;
        }
    }
}
