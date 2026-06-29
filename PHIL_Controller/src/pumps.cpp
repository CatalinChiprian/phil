#include "../inc/pumps.h"
#include "../inc/movement.h"
#include "../inc/hardware.h"
#include "../inc/well_utils.h"
#include "../inc/commands.h"

// Z-axis position used during pumping operations (lowered position)
static const int16_t ZMotorPumpPosition = -2304;

// Conversion factor: microliters per motor step
// Defines how much liquid is moved per step of the pump motor
static const float UL_PER_STEP = 0.1099f;

/**
 * uLToSteps(microliters)
 * 
 * Converts a liquid volume (µL) into motor steps.
 * 
 * Uses calibration factor UL_PER_STEP.
 * 
 * @return number of steps required for the given volume
 */
long uLToSteps(float microliters) {
    if (UL_PER_STEP <= 0.0f) return 0;
    return lroundf(microliters / UL_PER_STEP);
}

/**
 * dispense(pump, microliters, well)
 * 
 * Dispenses liquid from the selected pump.
 * 
 * Steps:
 * 1. If a well is specified:
 *    - Validate well (A1–H12)
 *    - Move to well using calibrated movement
 *    - Lower Z-axis to pumping position
 * 
 * 2. Select pump motor (P1 or P2)
 * 3. Convert volume (µL) → steps
 * 4. Move pump motor in negative direction (dispense)
 * 
 * Safety:
 * - Checks for emergency stop during execution
 * 
 * Final:
 * - Raises Z-axis to safe position
 * - Prints operation result over Serial
 */
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
            if (isEmergencyStopRequest()) {
                emergencyStop();
                break;
            }
            
            pumpStepper.run();
        }
    }

    moveZMotors(ZMotorNormalPosition);

    Serial.print(F("PUMP"));
    Serial.print(pump);
    Serial.print(F(":dispensed=")); Serial.print(microliters);
    Serial.println(F("uL"));
}

/**
 * prime(pump, microliters)
 *
 * Primes the selected pump by moving to the waste container,
 * lowering the Z-axis, and dispensing the requested volume.
 */
void prime(uint8_t pump, uint16_t microliters) {

    goToWasteContainer();
    moveZMotors(ZMotorPumpPosition);

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
            if (isEmergencyStopRequest()) {
                emergencyStop();
                break;
            }
            
            pumpStepper.run();
        }
    }

    moveZMotors(ZMotorNormalPosition);

    Serial.print(F("PUMP"));
    Serial.print(pump);
    Serial.print(F(":dispensed=")); Serial.print(microliters);
    Serial.println(F("uL"));
}

/**
 * aspirate(pump, microliters, well)
 * 
 * Aspirates (draws) liquid into the selected pump.
 * 
 * Steps:
 * 1. If a well is specified:
 *    - Move to safe Z height
 *    - Move to well using calibration
 *    - Lower Z-axis to pumping position
 * 
 * 2. Select pump motor
 * 3. Convert volume → steps
 * 4. Move pump motor in positive direction (aspirate)
 * 
 * Safety:
 * - Supports emergency stop during operation
 * 
 * Final:
 * - Raises Z-axis to safe position
 * - Prints confirmation output
 */
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
        if (isEmergencyStopRequest()) {
            emergencyStop();
            break;
        }
        
        pumpStepper.run();
    }

    moveZMotors(ZMotorNormalPosition);

    Serial.print(F("PUMP"));
    Serial.print(pump);
    Serial.print(F(":aspirated=")); Serial.print(microliters);
    Serial.println(F("uL"));
}