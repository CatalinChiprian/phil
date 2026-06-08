/**
 * calibration.h
 * 
 * Provides functionality for calibrating the mapping between
 * logical well positions and motor coordinates.
 * 
 * The calibration process records multiple reference points
 * (well positions ↔ motor positions) and computes a polynomial
 * mapping using these points.
 * 
 * This module is responsible for:
 * - Executing origin calibration
 * - Recording calibration points
 * - Solving the mapping model
 * - Managing calibration data
 * 
 * The resulting mapping allows the system to dynamically
 * estimate motor positions for all wells on the plate.
 */
#pragma once

#include <stdint.h>


/**
 * Maximum number of calibration points that can be stored.
 * Limited due to memory constraints on Arduino.
 */
constexpr uint8_t MAX_CAL = 32;

/**
 * Number of polynomial terms used in the mapping model.
 * Defines the complexity of the calibration function.
 */
constexpr uint8_t TERMS = 10;


/**
 * Global calibration state variables
 * 
 * calCount → number of recorded calibration points
 * ML / MR → polynomial coefficients for Left and Right motors
 * calWellIndex → stored well indices
 * calL / calR → recorded motor positions for each calibration point
 * mapReady → indicates whether a valid mapping has been computed
 */
extern uint8_t calCount;
extern float ML[TERMS];
extern float MR[TERMS];
extern uint8_t calWellIndex[MAX_CAL];
extern float calL[MAX_CAL];
extern float calR[MAX_CAL];
extern bool mapReady;

/**
 * Physical spacing between wells (in mm).
 * Standard 96-well plates use 9 mm spacing.
 */
constexpr float WELL_DX = 9.0;
constexpr float WELL_DY = 9.0;

/**
 * calibrateHome()
 * 
 * Performs homing calibration using limit switches.
 * Establishes a reference origin for the system.
 * 
 * @return status code (implementation dependent)
 */
int8_t calibrateHome();

/**
 * recordCalibrationPoint(row, col)
 * 
 * Records the current motor positions for a selected well.
 * These points are later used to compute the mapping function.
 * 
 * @param row   Well row (e.g., 'a'–'h')
 * @param col   Well column (1–12)
 */
void recordCalibrationPoint(char row, uint8_t col);

/**
 * solveMapping()
 * 
 * Computes the polynomial mapping using all recorded
 * calibration points (typically via least-squares fitting).
 * 
 * @return true if mapping was successfully computed
 */
bool solveMapping();

/**
 * deleteCalibrationPoint(row, col)
 * 
 * Removes a previously recorded calibration point.
 * 
 * @param row   Well row
 * @param col   Well column
 */
void deleteCalibrationPoint(char row, uint8_t col);


/**
 * dot10(a, b)
 * 
 * Computes the dot product between two arrays of size TERMS.
 * Used in polynomial evaluation and mapping calculations.
 * 
 * This is manually unrolled for performance optimization
 * on embedded hardware (avoids loop overhead).
 */
inline float dot10(const float a[TERMS], const float b[TERMS]) {
    return a[0]*b[0] + a[1]*b[1] + a[2]*b[2] + a[3]*b[3] + a[4]*b[4]
        + a[5]*b[5] + a[6]*b[6] + a[7]*b[7] + a[8]*b[8] + a[9]*b[9];
}


/**
 * printCalibrationPoints()
 * 
 * Prints all recorded calibration points to the serial output.
 * Useful for debugging and validation.
 */
void printCalibrationPoints();