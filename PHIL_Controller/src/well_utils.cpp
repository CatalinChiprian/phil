#include "../inc/well_utils.h"
#include "../inc/hardware.h"
#include "../inc/calibration.h"
#include <math.h>

WellPlateType selectedPlateType;

/**
 * isInvalidWell(row, col)
 * 
 * Validates a well position against plate boundaries.
 * 
 * Valid range:
 * - Rows: 'a'–'h'
 * - Columns: 1–12
 * 
 * @return true if outside valid range
 */
bool isInvalidWell(char row, uint8_t col) {
    return (col < 1 || col > 12 || row < 'a' || row > 'h');
}

/**
 * wellIndexToRowCol(wellIndex, row, col)
 * 
 * Converts a linear well index into row and column.
 * 
 * Example:
 * - 0 → A1
 * - 13 → B2
 */
void wellIndexToRowCol(uint8_t wellIndex, char& row, uint8_t& col) {
    row = 'a' + (wellIndex / 12);
    col = (wellIndex % 12) + 1;
}

/**
 * wellIndexToXY(wellIndex, x, y)
 * 
 * Converts a well index into physical coordinates (x, y).
 * 
 * Internally:
 * - Converts index → row/col
 * - Converts row/col → coordinates
 */
void wellIndexToXY(uint8_t wellIndex, float& x, float& y) {
    char row; uint8_t col;
    wellIndexToRowCol(wellIndex, row, col);
    wellToXY(row, col, x, y);
}

/**
 * rowColToWellIndex(row, column)
 * 
 * Converts row/column into a linear well index.
 * 
 * Example:
 * - A1 → 0
 * - B1 → 12
 */
uint8_t rowColToWellIndex(char row, uint8_t column) {
    return (row - 'a') * 12 + (column - 1);
}

/**
 * degToSteps(de rotation in degrees into stepper motor steps. * degToSteps(deg)
 * 
 * Uses:
 * - 200 steps per revolution
 * - current microstepping factor
 */
long degToSteps(float deg) {
    float stepsPerRev = 200.0 * currentMicrosteps;
    return lroundf(deg * (stepsPerRev / 360.0));
}

/**
 * stepsToDegrees(steps)
 * 
 * Converts stepper motor steps back into degrees.
 * 
 * Useful for reporting and debugging.
 */
float stepsToDegrees(long steps) {
    float stepsPerRev = 200.0f * currentMicrosteps;
    return steps * (360.0f / stepsPerRev);
}

/**
 * wellToXY(row, col, x, y)
 * 
 * Converts a well position into physical (x, y) coordinates.
 * 
 * Uses:
 * - WELL_DX → horizontal spacing
 * - WELL_DY → vertical spacing
 * 
 * Coordinates are relative to plate origin.
 * 
 * Invalid inputs are detected and reported.
 */
void wellToXY(char row, uint8_t col, float &x, float &y) {
    row = tolower(row);

    if (isInvalidWell(row, col)) {
    Serial.print(F("ERROR:INVALID_WELL,"));
    Serial.print(row); Serial.println(col);
    return;
    }

    uint8_t r = row - 'a';

    x =  (col - 1) * WELL_DX;
    y =  r * WELL_DY;
}

/**
 * xyToAngles(x, y, Ldeg, Rdeg)
 * 
 * Converts physical coordinates (x, y) into motor angles.
 * 
 * Uses calibration model:
 * - Polynomial mapping (10 terms)
 * - Coefficients ML and MR (left/right motors)
 * 
 * Steps:
 * 1. Build basis vector from x, y
 * 2. Compute dot product with calibration coefficients
 * 
 * Requires:
 * - mapReady = true
 * 
 * If calibration is not available:
 * - Returns (0, 0) with warning
 * 
 * This is the core function that enables calibrated movement.
 */
void xyToAngles(float x, float y, float &Ldeg, float &Rdeg) {
    if (!mapReady) {
        Serial.println(F("Angle map not ready!"));
        Ldeg = 0; Rdeg = 0;
        return;
    }
    // Basis vector for quadratic model
    float b[TERMS] = { 1.0f, x, y, x*x, x*y, y*y, x*x*x, x*x*y, x*y*y, y*y*y };
    Ldeg = dot10(ML, b);
    Rdeg = dot10(MR, b);
}

/**
 * wellStrToRowCol(wellStr, row, col)
 * 
 * Parses a string representation of a well (e.g. "A1")
 * into row and column values.
 * 
 * Example:
 * - "B12" → row='b', col=12
 */
void wellStrToRowCol(char* wellStr, char& row, uint8_t& col) {
	row = tolower(wellStr[0]);
	col = atoi(wellStr + 1);
}

/**
 * getCurrentWellplate()
 * 
 * Returns the currently selected well plate type.
 */
WellPlateType getCurrentWellplate() {
    return selectedPlateType;
}

/**
 * setCurrentWellplate(t)
 * 
 * Sets the active well plate type.
 * 
 * This may affect:
 * - coordinate calculations
 * - valid well ranges
 */
void setCurrentWellplate(WellPlateType t) {
    selectedPlateType = t;
}