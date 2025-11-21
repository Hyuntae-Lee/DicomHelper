#include "RDSRHelper.h"
#include <fstream>
#include <iostream>

RDSRHelper::RDSRHelper()
{
}

RDSRHelper::~RDSRHelper()
{
}

auto RDSRHelper::generateContent()->string
{
	// Contents
	web::json::value contents;
	// - AccumulatedDose
	contents[U("AccumulatedDose")] = m_jsonAccumulatedDose;
	// - IrradiationEvents
	{
		web::json::value jsonIrradiationEvent = web::json::value::array(m_arrIrradiationEvent.size());
		for (size_t i = 0; i < m_arrIrradiationEvent.size(); ++i)
		{
			jsonIrradiationEvent[i] = m_arrIrradiationEvent[i];
		}

		contents[U("IrradiationEvents")] = jsonIrradiationEvent;
	}

	// root
	web::json::value root;

	root[U("Header")] = m_jsonHeader;
	root[U("Settings")] = m_jsonSettings;
	root[U("Contents")] = contents;

	return utility::conversions::to_utf8string(root.serialize());
}

void RDSRHelper::putPatientName(wstring value)
{
	m_jsonHeader[U("PatientName")] = web::json::value::string(value);
}

void RDSRHelper::putPatientID(wstring value)
{
	m_jsonHeader[U("PatientID")] = web::json::value::string(value);
}

void RDSRHelper::putPatientAge(int value)
{
	m_jsonHeader[U("PatientAge")] = value;
}

void RDSRHelper::putPatientGender(wstring value)
{
	m_jsonHeader[U("PatientGender")] = web::json::value::string(value);
}

void RDSRHelper::putPatientBirthDate(wstring value)
{
	m_jsonHeader[U("PatientBirthDate")] = web::json::value::string(value);
}

void RDSRHelper::putTargetPart(wstring value)
{
	m_jsonHeader[U("TargetPart")] = web::json::value::string(value);
}

void RDSRHelper::putPediatric(bool value)
{
	m_jsonHeader[U("Pediatric")] = value;
}

void RDSRHelper::putManufacturer(wstring value)
{
	m_jsonHeader[U("Manufacturer")] = web::json::value::string(value);
}

void RDSRHelper::putModelName(wstring value)
{
	m_jsonHeader[U("ModelName")] = web::json::value::string(value);
}

void RDSRHelper::putInstitutionName(wstring value)
{
	m_jsonHeader[U("InstitutionName")] = web::json::value::string(value);
}

void RDSRHelper::putInstitutionAddress(wstring value)
{
	m_jsonHeader[U("InstitutionAddress")] = web::json::value::string(value);
}

void RDSRHelper::putStationName(wstring value)
{
	m_jsonHeader[U("StationName")] = web::json::value::string(value);
}

void RDSRHelper::putDeviceSerialNumber(wstring value)
{
	m_jsonHeader[U("DeviceSerialNumber")] = web::json::value::string(value);
}

void RDSRHelper::putSoftwareVersion(wstring value)
{
	m_jsonHeader[U("SoftwareVersion")] = web::json::value::string(value);
}

void RDSRHelper::putSOPInstanceUID(wstring value)
{
	m_jsonHeader[U("SOPInstanceUID")] = web::json::value::string(value);
}

void RDSRHelper::putStudyInstanceUID(wstring value)
{
	m_jsonHeader[U("StudyInstanceUID")] = web::json::value::string(value);
}

void RDSRHelper::putSeriesInstanceUID(wstring value)
{
	m_jsonHeader[U("SeriesInstanceUID")] = web::json::value::string(value);
}

void RDSRHelper::putSpecificCharacterSet(wstring value)
{
	m_jsonHeader[U("SpecificCharacterSet")] = web::json::value::string(value);
}

void RDSRHelper::putSeriesNumber(wstring value)
{
	m_jsonHeader[U("SeriesNumber")] = web::json::value::string(value);
}

void RDSRHelper::putInstanceNumber(wstring value)
{
	m_jsonHeader[U("InstanceNumber")] = web::json::value::string(value);
}

void RDSRHelper::putStudyDate(wstring value)
{
	m_jsonHeader[U("StudyDate")] = web::json::value::string(value);
}

void RDSRHelper::putStudyTime(wstring value)
{
	m_jsonHeader[U("StudyTime")] = web::json::value::string(value);
}

void RDSRHelper::putAccessionNumber(wstring value)
{
	m_jsonHeader[U("AccessionNumber")] = web::json::value::string(value);
}

void RDSRHelper::putDLPAlertThreshold(double value)
{
	m_jsonSettings[U("DLPAlertThreshold")] = value;
}

void RDSRHelper::putCTDIAlertThreshold(double value)
{
	m_jsonSettings[U("CTDIAlertThreshold")] = value;
}

void RDSRHelper::putDLPNotificationThreshold(double value)
{
	m_jsonSettings[U("DLPNotificationThreshold")] = value;
}

void RDSRHelper::putCTDINotificationThreshold(double value)
{
	m_jsonSettings[U("CTDINotificationThreshold")] = value;
}

void RDSRHelper::putDLPUnit(wstring value)
{
	m_jsonSettings[U("DLPUnit")] = web::json::value::string(value);
}

void RDSRHelper::putCTDIvolUnit(wstring value)
{
	m_jsonSettings[U("CTDIvolUnit")] = web::json::value::string(value);
}

void RDSRHelper::putAccumulatedDose(double dlp, double ctdiVol)
{
	m_jsonAccumulatedDose[U("DLP")] = dlp;
	m_jsonAccumulatedDose[U("CTDIvol")] = ctdiVol;
}

void RDSRHelper::addIrradiationEventScout(double kVp, double mA, double exposureTime, double dap)
{
	web::json::value ev;

	ev[U("Modality")] = web::json::value::string(U("Scout"));
	ev[U("kVp")] = kVp;
	ev[U("mA")] = mA;
	ev[U("ExposureTime")] = exposureTime;
	ev[U("DAP")] = dap;
	ev[U("CTDIvol")] = 0;
	ev[U("DLP")] = 0;

	m_arrIrradiationEvent.push_back(ev);
}

void RDSRHelper::addIrradiationEventCT(double kVp, double mA, double exposureTime, double dlp, double ctdiVol)
{
	web::json::value ev;

	ev[U("Modality")] = web::json::value::string(U("CT"));
	ev[U("kVp")] = kVp;
	ev[U("mA")] = mA;
	ev[U("ExposureTime")] = exposureTime;
	ev[U("DAP")] = 0;
	ev[U("CTDIvol")] = dlp;
	ev[U("DLP")] = ctdiVol;

	m_arrIrradiationEvent.push_back(ev);
}
