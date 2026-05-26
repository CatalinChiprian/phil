#include "../inc/hardware.h"


static const uint8_t ena[]  = { 44, 47, 50, 53, 13, 10 };
static const uint8_t step[] = { 43, 46, 49, 52, 12, 9 };
static const uint8_t dir[] = { 42, 45, 48, 51, 11, 8 };

static const uint8_t M1 = 25;
static const uint8_t M2 = 26;
static const uint8_t M3 = 27;

static const uint8_t P1 = 22;
static const uint8_t P2 = 23;
static const uint8_t P3 = 24;

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

bool areMotorsCurrentlyEnabled() {
    return ZMotorsCurrentlyEnabled || LMotorCurrentlyEnabled || RMotorCurrentlyEnabled ||
            P1MotorCurrentlyEnabled || P2MotorCurrentlyEnabled;
}

void setSlowMovementSpeed() {
    stepperL.setMaxSpeed(200 * currentMicrosteps);
    stepperR.setMaxSpeed(200 * currentMicrosteps);
    stepperL.setAcceleration(100 * currentMicrosteps);
    stepperR.setAcceleration(100 * currentMicrosteps);
}

void setNormalMovementSpeed() {
    stepperL.setMaxSpeed(1000 * currentMicrosteps);
    stepperR.setMaxSpeed(1000 * currentMicrosteps);
    stepperL.setAcceleration(500 * currentMicrosteps);
    stepperR.setAcceleration(500 * currentMicrosteps);
}

void setSlowPumpSpeed() {
    stepperP1.setMaxSpeed(200);
    stepperP2.setMaxSpeed(200);
    stepperP1.setAcceleration(100);
    stepperP2.setAcceleration(100);
}

void setNormalPumpSpeed() {
    stepperP1.setMaxSpeed(1000);
    stepperP2.setMaxSpeed(1000);
    stepperP1.setAcceleration(500);
    stepperP2.setAcceleration(500);
}

void enableAllMotors() {
    if (areMotorsCurrentlyEnabled()) return;
    enableZMotors();
    enableLMotor();
    enableRMotor();
    enableP1Motor();
    enableP2Motor();
}

void enableZMotors() {
    digitalWrite(ena[0], LOW);
    digitalWrite(ena[3], LOW);

    ZMotorsCurrentlyEnabled = true;     
    lastMotorActivityTime = millis(); 
}

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

void disableZMotors() {
    digitalWrite(ena[0], HIGH);
    digitalWrite(ena[3], HIGH);

    ZMotorsCurrentlyEnabled = false;
    lastMotorActivityTime = millis(); 
}

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


void disableAllMotors() {
    disableLMotor();
    disableRMotor();
    disableZMotors();
    disableP1Motor();
    disableP2Motor();
    }

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

void adjustTime(uint32_t unixTime) {
    rtc.adjust(DateTime(unixTime));
}

uint32_t getTime() {
    return rtc.now().unixtime();
}

void printTime() {
    Serial.print(F("TIME=")); Serial.println(rtc.now().unixtime());
}
