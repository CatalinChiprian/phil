#pragma once

#include <Arduino.h>
#include <AccelStepper.h>
#include <MultiStepper.h>
#include <Wire.h>
#include "RTClib.h"

extern AccelStepper stepperL;
extern AccelStepper stepperR;
extern AccelStepper stepperZ1;
extern AccelStepper stepperZ2;
extern AccelStepper stepperP1;
extern AccelStepper stepperP2;

extern RTC_DS3231 rtc;

extern const uint8_t currentMicrosteps;

extern const uint8_t limitSwitchL;
extern const uint8_t limitSwitchR;
extern const uint8_t limitSwitchZ1;
extern const uint8_t limitSwitchZ2;

extern bool ZMotorsCurrentlyEnabled;
extern bool LMotorCurrentlyEnabled;
extern bool RMotorCurrentlyEnabled;
extern bool P1MotorCurrentlyEnabled;
extern bool P2MotorCurrentlyEnabled;
extern bool eStopRequested;

void initHardware();
void setSlowMovementSpeed();
void setNormalMovementSpeed();
void setSlowPumpSpeed();
void setNormalPumpSpeed();
void enableAllMotors();
void enableZMotors();
void enableP1Motor();
void enableP2Motor();
void enableLMotor();
void enableRMotor();
void disableZMotors();
void disableP1Motor();
void disableP2Motor();
void disableLMotor();
void disableRMotor();
void disableAllMotors();
void autoDisableMotors();
bool isEmergencyStopRequest();
void emergencyStop();
void checkFaults();
void checkSwitches();