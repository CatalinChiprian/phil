#include "../inc/pumps.h"
#include "../inc/movement.h"
#include "../inc/hardware.h"
#include "../inc/well_utils.h"

static const int16_t ZMotorPumpPosition = -2304;
static const float UL_PER_STEP = 0.1099f;

long uLToSteps(float microliters) {
    if (UL_PER_STEP <= 0.0f) return 0;
    return lroundf(microliters / UL_PER_STEP);
}

void dispense(uint8_t pump, uint16_t microliters, char* well) {

    if (strlen(well) > 0) {
        char row = tolower(well[0]);
        uint8_t col = atoi(well + 1);

        
        if (row >= 'a' && row <= 'h' && col >= 1 && col <= 12) {
        goToCalculatedWell(row, col);
        moveZMotors(ZMotorPumpPosition);
        }
    }

    AccelStepper pumpStepper;
    switch (pump) {
        case 1:
        enableP1Motor();
        pumpStepper = stepperP1;
        break;
        
        case 2:
        enableP2Motor();
        pumpStepper = stepperP2;
        break;

        default:
        return;
    }

    long stepsNeeded = uLToSteps(microliters);

    if (stepsNeeded > 0) {
        pumpStepper.moveTo(pumpStepper.currentPosition() - stepsNeeded);
        while(pumpStepper.distanceToGo() != 0) {
            if (isEmergencyStopRequest()) emergencyStop();
            
            pumpStepper.run();
        }
    }

    moveZMotors(ZMotorNormalPosition);

    Serial.print(F("PUMP"));
    Serial.print(pump);
    Serial.print(F(":dispensed=")); Serial.print(microliters);
    Serial.println(F("uL"));
}

void aspirate(uint8_t pump, uint16_t microliters, char* well) {

if (strlen(well) > 0) {
        char row = tolower(well[0]);
        uint8_t col = atoi(well + 1);

        
        if (row >= 'a' && row <= 'h' && col >= 1 && col <= 12) {
        moveZMotors(ZMotorNormalPosition);
        goToCalculatedWell(row, col);
        moveZMotors(ZMotorPumpPosition);
        }
    }

    AccelStepper pumpStepper;
    switch (pump) {
        case 1:
        enableP1Motor();
        pumpStepper = stepperP1;
        break;
        
        case 2:
        enableP2Motor();
        pumpStepper = stepperP2;
        break;

        default:
        return;
    }

    long stepsNeeded = uLToSteps(microliters);

    pumpStepper.moveTo(pumpStepper.currentPosition() + stepsNeeded);

    while(pumpStepper.distanceToGo() != 0) {
        if (isEmergencyStopRequest()) emergencyStop();
        
        pumpStepper.run();
    }

    moveZMotors(ZMotorNormalPosition);

    Serial.print(F("PUMP"));
    Serial.print(pump);
    Serial.print(F(":aspirated=")); Serial.print(microliters);
    Serial.println(F("uL"));
}