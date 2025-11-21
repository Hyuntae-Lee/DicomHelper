// TestApp.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <cpprest/json.h>
#include <iostream>
#include "RDSRHelper.h"

using namespace std;

int main()
{
    RDSRHelper builder;

    builder.putPatientName(L"DOE^JONE");
    builder.putPatientID(L"P123456");
    builder.putPatientAge(24);
    builder.putPatientGender(L"M");
    builder.putPatientBirthDate(L"19700101");
    builder.putTargetPart(L"L-Spine");
    builder.putPediatric(false);
    builder.putManufacturer(L"vatech");
    builder.putModelName(L"DCT-01CS");
    builder.putInstitutionName(L"");
    builder.putInstitutionAddress(L"Dontan");
    builder.putStationName(L"Smart M Plus");
    builder.putDeviceSerialNumber(L"SN111112222");
    builder.putSoftwareVersion(L"1.2.3.4");
    builder.putSOPInstanceUID(L"1.2.840.10008.5.1.4.1.1.88.67.99");
    builder.putStudyInstanceUID(L"1.2.840.10008.5.1.4.1.1.88.67.999");
    builder.putSeriesInstanceUID(L"1.2.840.10008.5.1.4.1.1.88.67.9999");
    builder.putSpecificCharacterSet(L"ISO_IR 192");
    builder.putSeriesNumber(L"1");
    builder.putInstanceNumber(L"1");
    builder.putStudyDate(L"19970302");
    builder.putStudyTime(L"163759");
    builder.putAccessionNumber(L"A-0001");
    builder.putDLPAlertThreshold(10.0);
    builder.putCTDIAlertThreshold(10.0);
    builder.putDLPNotificationThreshold(10.0);
    builder.putCTDINotificationThreshold(10.0);
    builder.putDLPUnit(L"mGy.cm");
    builder.putCTDIvolUnit(L"mGy");
    builder.putAccumulatedDose(350.0, 10.5);
    builder.addIrradiationEventScout(10.0, 120.0, 13.0, 103.0);
    builder.addIrradiationEventCT(10.0, 120.0, 13.0, 104.0, 105.0);
    builder.addIrradiationEventCT(11.0, 121.0, 14.0, 105.0, 106.0);

    auto contents = utility::conversions::to_utf8string(builder.generateContent());

    // save
    std::ofstream file(L"D:\\TTT\\RDSR_T.json", std::ios::out | std::ios::trunc);

    file << contents;

    return 0;
}

void setStringValue(web::json::value& json, wstring value)
{

}

