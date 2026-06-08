#include "../inc/hardware.h"

// Enable pins for all motor drivers (L, R, Z1, Z2, P1, P2)
static const uint8_t ena[]  = { 44, 47, 50, 53, 13, 10 };

// Step pins for all motors
static const uint8_t step[] = { 43, 46, 49, 52, 12, 9 };

// Direction pins for all motors
static const uint8_t dir[] = { 42, 45, 48, 51, 11, 8 };

static const uint8_t M1 = 25;
static const uint8_t M2 = 26;
static const uint8_t M3 = 27;

static const uint8_t P1 = 22;
static const uint8_t P2 = 23;
static const uint8_t P3 = 24;


/**
 * Microstepping configuration
 * 
 * Sttngs defines logic levels for different microstep modes.
 * microIndex selects the active mode.
 * currentMicrosteps is used to scale speed and accuracy.
 */

static const char Sttngs[][3] = {
    {LOW,  LOW, LOW},    // Full step
    {HIGH,  LOW, LOW},   // Half step
    {LOW, HIGH,  LOW},   // 1/4 step
    {HIGH,  HIGH,  LOW}, // 1/8 step
    {LOW, LOW, HIGH},    // 1/16 step
    {HIGH,  HIGH,  HIGH} // Full step
};

static const uint8_t MICROoptions[] = { 1, 2, 4, 8, 16, 32 };

static const uint8_t microIndex = 3; // 0=full, 1=half, 2=1/4, 3=1/8, 4=1/16, 5=1/32
const uint8_t currentMicrosteps = MICROoptions[microIndex]; 

AccelStepper stepperL(1, step[1], dir[1]);
AccelStepper stepperR(1, step[2], dir[2]);
AccelStepper stepperZ1(1, step[0], dir[0]);
AccelStepper stepperZ2(1, step[3], dir[3]);
AccelStepper stepperP1(1, step[4], dir[4]);
AccelStepper stepperP2(1, step[5], dir[5]);

RTC_DS3231 rtc;

const uint8_t limitSwitchL = 31; // Target Limit Switch L
const uint8_t limitSwitchR = 30; // Target Limit Switch R
const uint8_t limitSwitchZ1 = 33; // Target Limit Switch Z
const uint8_t limitSwitchZ2 = 32; // Target Limit Switch Z

bool LMotorCurrentlyEnabled = false;
bool RMotorCurrentlyEnabled = false;
bool ZMotorsCurrentlyEnabled = false;
bool P1MotorCurrentlyEnabled = false;
bool P2MotorCurrentlyEnabled = false;

const uint8_t faultR = 37;
const uint8_t faultL = 39;

uint16_t lastMotorActivityTime = 0;
static const uint16_t MOTOR_TIMEOUT = 5000;

/**
 * initHardware()
 * 
 * Initializes all hardware components at startup.
 * 
 * Steps:
 * 1. Initialize I2C communication (Wire)
 * 2. Initialize RTC module
 *    - If RTC lost power → set to compile time
 * 3. Configure limit switches and fault pins
 * 4. Configure microstepping pins
 * 5. Initialize motor speeds and acceleration
 * 6. Disable all motors (safe startup state)
 * 
 * Notes:
 * - Pump motors are configured with fixed microstepping
 * - Movement motors use configurable microstepping
 */
void initHardware() {
    Wire.begin();

    if (!rtc.begin()) {
        Serial.println(F("ERROR: RTC not found"));
        while (1);
    }

    if (rtc.lostPower()) {
      Serial.println(F("RTC lost power, setting time..."));

      DateTime compileTime = DateTime(F(__DATE__), F(__TIME__));
      rtc.adjust(compileTime);
    }

    Serial.print(F("TIME=")); Serial.println(rtc.now().unixtime());

    pinMode(limitSwitchL, INPUT_PULLUP);
    pinMode(limitSwitchR, INPUT_PULLUP);
    pinMode(limitSwitchZ1, INPUT_PULLUP);
    pinMode(limitSwitchZ2, INPUT_PULLUP);

    pinMode(faultR, INPUT_PULLUP);
    pinMode(faultL, INPUT_PULLUP);
    
    pinMode(M1, OUTPUT); 
    digitalWrite(M1, Sttngs[microIndex][0]); 
    pinMode(M2, OUTPUT); 
    digitalWrite(M2, Sttngs[microIndex][1]); 
    pinMode(M3, OUTPUT); 
    digitalWrite(M3, Sttngs[microIndex][2]);

    // Pumps are hardcoded to full step
    pinMode(P1, OUTPUT); 
    digitalWrite(P1, Sttngs[0][0]); 
    pinMode(P2, OUTPUT); 
    digitalWrite(P2, Sttngs[0][1]); 
    pinMode(P3, OUTPUT); 
    digitalWrite(P3, Sttngs[0][2]);

    stepperZ1.setMaxSpeed(2000 * currentMicrosteps);
    stepperZ2.setMaxSpeed(2000 * currentMicrosteps);
    stepperZ1.setAcceleration(1500 * currentMicrosteps);
    stepperZ2.setAcceleration(1500 * currentMicrosteps);

    setNormalMovementSpeed();
    setNormalPumpSpeed();

    disableAllMotors();
}

/**
 * areMotorsCurrentlyEnabled()
 * 
 * Returns whether any motor in the system is currently enabled.
 * 
 * Used to prevent redundant enable/disable operations.
 */
bool areMotorsCurrentlyEnabled() {
    return ZMotorsCurrentlyEnabled || LMotorCurrentlyEnabled || RMotorCurrentlyEnabled ||
            P1MotorCurrentlyEnabled || P2MotorCurrentlyEnabled;
}

/**
 * setSlowMovementSpeed()
 * 
 * Sets low speed and acceleration for precise movements,
 * typically used during calibration.
 */
void setSlowMovementSpeed() {
    stepperL.setMaxSpeed(200 * currentMicrosteps);
    stepperR.setMaxSpeed(200 * currentMicrosteps);
    stepperL.setAcceleration(100 * currentMicrosteps);
    stepperR.setAcceleration(100 * currentMicrosteps);
}

/**
 * setNormalMovementSpeed()
 * 
 * Sets standard movement speed for normal operation.
 */
void setNormalMovementSpeed() {
    stepperL.setMaxSpeed(1000 * currentMicrosteps);
    stepperR.setMaxSpeed(1000 * currentMicrosteps);
    stepperL.setAcceleration(500 * currentMicrosteps);
    stepperR.setAcceleration(500 * currentMicrosteps);
}

/**
 * setSlowPumpSpeed()
 * 
 * Sets reduced pump speed for controlled liquid handling.
 */
void setSlowPumpSpeed() {
    stepperP1.setMaxSpeed(200);
    stepperP2.setMaxSpeed(200);
    stepperP1.setAcceleration(100);
    stepperP2.setAcceleration(100);
}

/**
 * setNormalPumpSpeed()
 * 
 * Sets standard pump speed for normal operations.
 */
void setNormalPumpSpeed() {
    stepperP1.setMaxSpeed(1000);
    stepperP2.setMaxSpeed(1000);
    stepperP1.setAcceleration(500);
    stepperP2.setAcceleration(500);
}

/**
 * enableAllMotors()
 * 
 * Enables all motors in the system.
 * 
 * Behavior:
 * - Checks if any motor is already enabled
 * - If not, enables all motor groups
 * 
 * Prevents redundant enabling operations.
 */
void enableAllMotors() {
    if (areMotorsCurrentlyEnabled()) return;
    enableZMotors();
    enableLMotor();
    enableRMotor();
    enableP1Motor();
    enableP2Motor();
}

/**
 * enableZMotors()
 * 
 * Enables both Z-axis motors by activating their driver pins.
 * Also updates internal state and activity timer.
 */
void enableZMotors() {
    digitalWrite(ena[0], LOW);
    digitalWrite(ena[3], LOW);

    ZMotorsCurrentlyEnabled = true;     
    lastMotorActivityTime = millis(); 
}

/**
 * Individual motor enable functions
 * 
 * Each function:
 * - Activates motor driver (LOW signal)
 * - Updates internal state flag
 * - Resets motor activity timer
 */

void enableP1Motor() {
    digitalWrite(ena[4], LOW);

    P1MotorCurrentlyEnabled = true;
    lastMotorActivityTime = millis(); 
}

void enableP2Motor() {
    digitalWrite(ena[5], LOW);

    P2MotorCurrentlyEnabled = true;
    lastMotorActivityTime = millis(); 
}

void enableLMotor() {
    digitalWrite(ena[1], LOW);

    LMotorCurrentlyEnabled = true;
    lastMotorActivityTime = millis(); 
}

void enableRMotor() {
    digitalWrite(ena[2], LOW);

    RMotorCurrentlyEnabled = true;
    lastMotorActivityTime = millis(); 
}

/**
 * disableZMotors()
 * 
 * Disables both Z-axis motors and updates state flags.
 */
void disableZMotors() {
    digitalWrite(ena[0], HIGH);
    digitalWrite(ena[3], HIGH);

    ZMotorsCurrentlyEnabled = false;
    lastMotorActivityTime = millis(); 
}

/**
 * Individual motor disable functions
 * 
 * Each function:
 * - Deactivates motor driver (HIGH signal)
 * - Updates internal state flag
 * - Resets activity timer
 */

void disableP1Motor() {
    digitalWrite(ena[4], HIGH);

    P1MotorCurrentlyEnabled = false;
    lastMotorActivityTime = millis(); 
}

void disableP2Motor() {
    digitalWrite(ena[5], HIGH);

    P2MotorCurrentlyEnabled = false;
    lastMotorActivityTime = millis(); 
}

void disableLMotor() {
    digitalWrite(ena[1], HIGH);

    LMotorCurrentlyEnabled = false;
    lastMotorActivityTime = millis(); 
}

void disableRMotor() {
    digitalWrite(ena[2], HIGH);

    RMotorCurrentlyEnabled = false;
    lastMotorActivityTime = millis(); 
}

/**
 * disableAllMotors()
 * 
 * Disables all motors in the system.
 * 
 * Used:
 * - After movement completion
 * - During safety events
 */
void disableAllMotors() {
    disableLMotor();
    disableRMotor();
    disableZMotors();
    disableP1Motor();
    disableP2Motor();
    }

/**
 * autoDisableMotors()
 * 
 * Automatically disables motors after a period of inactivity.
 * 
 * Behavior:
 * - Checks if any motor is currently moving
 * - If idle and timeout exceeded → disable all motors
 * 
 * Purpose:
 * - Reduce power consumption
 * - Prevent motor overheating
 */
void autoDisableMotors() {
    // Check if any motor is moving
    bool isMoving = (stepperL.distanceToGo() != 0 || 
                    stepperR.distanceToGo() != 0 || 
                    stepperZ1.distanceToGo() != 0 || 
                    stepperZ2.distanceToGo() != 0 ||
                    stepperP1.distanceToGo() != 0 ||
                    stepperP2.distanceToGo() != 0);

    if(!isMoving) {
        if(areMotorsCurrentlyEnabled() && (millis() - lastMotorActivityTime > MOTOR_TIMEOUT)) {
        disableAllMotors();
        }
    }
}

/**
 * emergencyStop()
 * 
 * Immediately stops all motor movement and disables all motors.
 * 
 * Also:
 * - Resets activity timer
 * - Sends warning message over Serial
 * 
 * Used for:
 * - User-triggered stop
 * - Critical fault conditions
 */
void emergencyStop() {
    stepperL.stop();
    stepperR.stop();
    stepperZ1.stop();
    stepperZ2.stop();
    stepperP1.stop();
    stepperP2.stop();
    disableAllMotors();
    lastMotorActivityTime = 0;
    Serial.println(F("WARNING:EMERGENCY_STOP,Motors disabled by user"));
}

/**
 * checkFaults()
 * 
 * Monitors motor driver fault pins.
 * 
 * Behavior:
 * - Detects nFAULT signals from drivers
 * - Prints error once (latched)
 * - Triggers emergency stop
 * 
 * Ensures safe shutdown in case of hardware failure.
 */
void checkFaults() {
    static bool faultLatched = false;

    if (digitalRead(faultR) == LOW || digitalRead(faultL) == LOW) {
        if (!faultLatched) {
        Serial.println(F("ERROR:DRIVER_FAULT,Motor driver nFAULT triggered"));
        faultLatched = true;
        }
        emergencyStop();
    } else {
        faultLatched = false;
    }
}

/**
 * checkSwitches()
 * 
 * Monitors all limit switches in the system.
 * 
 * For each axis:
 * - Detects press/release transitions
 * - Sends status updates over Serial
 * - Stops or stabilizes corresponding motors
 * 
 * Behavior:
 * - Z-axis: maintains current position when triggered
 * - L/R: stops movement when limit reached
 * 
 * Uses edge detection to avoid repeated messages.
 */
void checkSwitches() {
    static bool z1WasPressed = false;
    if(digitalRead(limitSwitchZ1) == LOW) {
        if(!z1WasPressed) {  
        Serial.println(F("LIMIT_PRESSED:AXIS=Z1"));
        z1WasPressed = true;
        }
        stepperZ1.setCurrentPosition(stepperZ1.currentPosition());
        stepperZ2.setCurrentPosition(stepperZ2.currentPosition());
    } else {
        if (z1WasPressed) {
            Serial.println(F("LIMIT_RELEASED:AXIS=Z1"));
            z1WasPressed = false;
        }
    }

    static bool z2WasPressed = false;
    if(digitalRead(limitSwitchZ2) == LOW) {
        if(!z2WasPressed) {
        Serial.println(F("LIMIT_PRESSED:AXIS=Z2"));
        z2WasPressed = true;
        }
        stepperZ1.setCurrentPosition(stepperZ1.currentPosition());
        stepperZ2.setCurrentPosition(stepperZ2.currentPosition());
    } else {
        if (z2WasPressed) {
            Serial.println(F("LIMIT_RELEASED:AXIS=Z2"));
            z2WasPressed = false;
        }
    }


    static bool lWasPressed = false;
    if(digitalRead(limitSwitchL) == LOW) {
        if (!lWasPressed) {
        Serial.println(F("LIMIT_PRESSED:AXIS=L"));
        lWasPressed = true;
        }
        stepperL.stop();
        stepperL.setCurrentPosition(stepperL.currentPosition());
    }
    else {
        if (lWasPressed) {
            Serial.println(F("LIMIT_RELEASED:AXIS=L"));
            lWasPressed = false;
        }
    }

    static bool rWasPressed = false;
    if(digitalRead(limitSwitchR) == LOW) {
        if(!rWasPressed) {
        Serial.println(F("LIMIT_PRESSED:AXIS=R"));
        rWasPressed = true;
        }
        stepperR.stop();
        stepperR.setCurrentPosition(stepperR.currentPosition());
    } else {
        if (rWasPressed) {
            Serial.println(F("LIMIT_RELEASED:AXIS=R"));
            rWasPressed = false;
        }
    }
}

/**
 * adjustTime(unixTime)
 * 
 * Sets RTC time using a Unix timestamp.
 */
void adjustTime(uint32_t unixTime) {
    rtc.adjust(DateTime(unixTime));
}

/**
 * getTime()
 * 
 * Returns current system time as Unix timestamp.
 */
uint32_t getTime() {
    return rtc.now().unixtime();
}

/**
 * printTime()
 * 
 * Prints current system time for debugging or GUI synchronization.
 */
void printTime() {
    Serial.print(F("TIME:")); Serial.println(rtc.now().unixtime());
}
