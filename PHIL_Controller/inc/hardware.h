/**
 * hardware.h
 * 
 * Provides abstraction for all hardware components of the PHIL system.
 * 
 * This module handles:
 * - Stepper motor definitions and control
 * - Limit switch inputs
 * - Motor enable/disable state management
 * - Safety functions (emergency stop, fault checking)
 * - Timing functions using RTC
 * 
 * It acts as the interface between the firmware logic and the
 * physical robot hardware.
 */
#pragma once

#include <Arduino.h>
#include <AccelStepper.h>
#include <MultiStepper.h>
#include <Wire.h>
#include "RTClib.h"

/**
 * Stepper motor definitions
 * 
 * L, R → main arm motors
 * Z1, Z2 → vertical (Z-axis) motors
 * P1, P2 → pump motors
 */
extern AccelStepper stepperL;
extern AccelStepper stepperR;
extern AccelStepper stepperZ1;
extern AccelStepper stepperZ2;
extern AccelStepper stepperP1;
extern AccelStepper stepperP2;


/**
 * Current microstepping configuration
 * 
 * Affects movement precision and speed.
 */
extern const uint8_t currentMicrosteps;


/**
 * Limit switch pins
 * 
 * Used for homing and safety boundary detection.
 */
extern const uint8_t limitSwitchL;
extern const uint8_t limitSwitchR;
extern const uint8_t limitSwitchZ1;
extern const uint8_t limitSwitchZ2;


/**
 * Motor state flags
 * 
 * Track whether motors are currently enabled.
 * Used to avoid redundant enable/disable commands
 * and manage power consumption.
 */
extern bool ZMotorsCurrentlyEnabled;
extern bool LMotorCurrentlyEnabled;
extern bool RMotorCurrentlyEnabled;
extern bool P1MotorCurrentlyEnabled;
extern bool P2MotorCurrentlyEnabled;

/**
 * Initialization
 */

/**
 * initHardware()
 * 
 * Initializes all hardware components:
 * - Configures pins
 * - Initializes motors
 * - Sets default speeds
 */
void initHardware();

/**
 * Speed configuration
 */

/**
 * setSlowMovementSpeed()
 * 
 * Sets reduced speed for precise positioning.
 */
void setSlowMovementSpeed();

/**
 * setNormalMovementSpeed()
 * 
 * Sets standard movement speed.
 */
void setNormalMovementSpeed();

/**
 * setSlowPumpSpeed()
 * 
 * Sets reduced speed for controlled liquid handling.
 */
void setSlowPumpSpeed();

/**
 * setNormalPumpSpeed()
 * 
 * Sets standard pump operating speed.
 */
void setNormalPumpSpeed();

/**
 * Motor control (enable/disable)
 */

/**
 * enableAllMotors(), disableAllMotors()
 * 
 * Enables or disables all motors in the system.
 */
void enableAllMotors();
void disableAllMotors();

/**
 * Individual motor control functions
 */
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

/**
 * autoDisableMotors()
 * 
 * Automatically disables motors after inactivity
 * to reduce heat and power usage.
 */
void autoDisableMotors();
 
/**
 * Safety functions
 */

/**
 * emergencyStop()
 * 
 * Immediately stops all movements and disables motors.
 * Used for critical safety situations.
 */
void emergencyStop();

/**
 * checkFaults()
 * 
 * Monitors system for hardware or runtime faults.
 */
void checkFaults();

/**
 * checkSwitches()
 * 
 * Monitors limit switches and triggers responses
 * when boundaries are reached.
 */
void checkSwitches();

/**
 * Time handling (RTC)
 */

/**
 * adjustTime(unixTime)
 * 
 * Sets the system time using a Unix timestamp.
 */
void adjustTime(uint32_t unixTime);

/**
 * getTime()
 * 
 * Returns current system time as Unix timestamp.
 */
uint32_t getTime();

/**
 * printTime()
 * 
 * Prints current time for debugging or GUI display.
 */
void printTime();