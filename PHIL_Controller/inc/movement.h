/**
 * movement.h
 * 
 * Provides high-level movement functions for the PHIL robot.
 * 
 * This module translates user commands and system requests
 * into physical movement of the robot (X, Y, Z axes).
 * 
 * It supports:
 * - Manual directional movement
 * - Movement to specific wells
 * - Homing/origin positioning
 * - Movement to special locations (waste, wash)
 * - Position reporting and debugging
 */
#pragma once

#include <stdint.h>


/**
 * Special well identifiers
 * 
 * WELL_HOME → robot origin (homing position)
 * WELL_UNKNOWN → position not known or not calibrated
 * WELL_CONTAINER → special container position (e.g. waste/wash)
 */
constexpr uint8_t WELL_HOME = 0xFF;
constexpr uint8_t WELL_UNKNOWN = 0xFE;
constexpr uint8_t WELL_CONTAINER = 0xFD;


/**
 * Default Z-axis safe position
 * 
 * Used when moving between wells to avoid collisions.
 */
extern const int16_t ZMotorNormalPosition;


/**
 * Step size control
 * 
 * times_x10 controls movement increments for manual movements.
 * Example: 10 → step size = 1.0, 5 → step size = 0.5
 */
extern int16_t times_x10;

/**
 * Current robot position (well index)
 * 
 * Values:
 * - 0–N → valid wells
 * - WELL_HOME → at origin
 * - WELL_UNKNOWN → undefined position
 */
extern uint8_t wellIndex;


/**
 * Manual movement functions
 * 
 * Move robot in small increments in each direction.
 * Used for calibration and manual adjustment.
 */

void moveBackward();
void moveForward();
void moveLeft();
void moveRight();
void moveUp();
void moveDown();

/**
 * goToOrigin()
 * 
 * Moves robot to the calibrated home position.
 * 
 * Used as reference point for all movements.
 */
void goToOrigin();

/**
 * moveZMotors(position)
 * 
 * Moves Z-axis motors to a specific position.
 * 
 * Used to raise/lower pipette safely.
 */
void moveZMotors(int16_t position);

/**
 * Well-based movement
 */

/**
 * goToHardcodedWells(row, column)
 * 
 * Moves robot to a predefined (hardcoded) well position.
 * 
 * Used before calibration or for testing purposes.
 */
void goToHardcodedWells(char row, uint8_t column);

/**
 * goToCalculatedWell(row, col)
 * 
 * Moves robot to a well using calibration mapping.
 * 
 * Requires calibration to be completed.
 */
void goToCalculatedWell(char row, uint8_t col);


/**
 * Special locations
 */

 
/**
 * goToWasteContainer()
 * 
 * Moves robot to waste container position.
 */
void goToWasteContainer();

/**
 * goToWashContainer()
 * 
 * Moves robot to wash container position.
 */
void goToWashContainer();

/**
 * State updates
 */

/**
 * updatePositionState()
 * 
 * Updates internal position tracking and sends feedback
 * (e.g. current well, motor angles) to the GUI.
 */
void updatePositionState();

/**
 * Step size control
 */

/**
 * increaseStepSize()
 * decreaseStepSize()
 * 
 * Adjust manual movement step size.
 * Used for fine vs coarse positioning.
 */
void increaseStepSize();
void decreaseStepSize();

/**
 * Debug / output functions
 */

/**
 * printCurrentWell()
 * 
 * Prints current well position to Serial.
 */
void printCurrentWell();

/**
 * printPosition(L, R, Z1, Z2)
 * 
 * Prints current motor positions.
 */
void printPosition(int16_t L, int16_t R, int16_t Z1, int16_t Z2);

/**
 * printStepSize()
 * 
 * Prints current manual movement step size.
 */
void printStepSize();

/**
 * printMicroSteps()
 * 
 * Prints current microstepping configuration.
 */
void printMicroSteps();