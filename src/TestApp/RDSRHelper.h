#pragma once

#include <cpprest/json.h>
#include <string>
#include <vector>

using namespace std;

class RDSRHelper
{
public:
	RDSRHelper();
	virtual ~RDSRHelper();

public:
	auto generateContent()->string;

	void putPatientName(wstring value);
	void putPatientID(wstring value);
	void putPatientAge(int value);
	void putPatientGender(wstring value);
	void putPatientBirthDate(wstring value);
	void putTargetPart(wstring value);
	void putPediatric(bool value);
	void putManufacturer(wstring value);
	void putModelName(wstring value);
	void putInstitutionName(wstring value);
	void putInstitutionAddress(wstring value);
	void putStationName(wstring value);
	void putDeviceSerialNumber(wstring value);
	void putSoftwareVersion(wstring value);
	void putSOPInstanceUID(wstring value);
	void putStudyInstanceUID(wstring value);
	void putSeriesInstanceUID(wstring value);
	void putSpecificCharacterSet(wstring value);
	void putSeriesNumber(wstring value);
	void putInstanceNumber(wstring value);
	void putStudyDate(wstring value);
	void putStudyTime(wstring value);
	void putAccessionNumber(wstring value);
	void putDLPAlertThreshold(double value);
	void putCTDIAlertThreshold(double value);
	void putDLPNotificationThreshold(double value);
	void putCTDINotificationThreshold(double value);
	void putDLPUnit(wstring value);
	void putCTDIvolUnit(wstring value);
	void putAccumulatedDose(double dlp, double ctdiVol);
	
	void addIrradiationEventScout(double kVp, double mA, double exposureTime, double dap);
	void addIrradiationEventCT(double kVp, double mA, double exposureTime, double dlp, double ctdiVol);

protected:
	web::json::value m_jsonHeader;
	web::json::value m_jsonSettings;
	web::json::value m_jsonContents;
	web::json::value m_jsonAccumulatedDose;
	vector<web::json::value> m_arrIrradiationEvent;
};

