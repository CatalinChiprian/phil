/**
 * well_utils.h
 * 
 * Provides utility functions for converting between different
 * representations of well positions and robot coordinates.
 * 
 * This module handles transformations between:
 * - Well identifiers (e.g., A1, B3)
 * - Well indices (0–95)
 * - Physical coordinates (x, y in mm)
 * - Motor space (angles and steps)
 * 
 * It serves as the core mapping layer between logical plate positions
 * and physical robot movement.
 */
#pragma once

#include <stdint.h>


/**
 * Well plate types
 * 
 * Defines supported plate configurations.
 * 
 * ORGANONCHIP → special microfluidic layout
 * WELL96 → standard 96-well plate
 */
enum WellPlateType : uint8_t {
    ORGANONCHIP,
    WELL96
};


/**
 * Validation
 */

/**
 * isInvalidWell(row, col)
 * 
 * Checks whether a given well coordinate is outside valid bounds.
 * 
 * @return true if invalid
 */
bool isInvalidWell(char row, uint8_t col);

/**
 * Well index ↔ row/column conversions
 */

/**
 * wellIndexToRowCol(wellIndex, row, col)
 * 
 * Converts a linear well index into row and column.
 * 
 * Example:
 * index 0 → A1
 */
void wellIndexToRowCol(uint8_t wellIndex, char& row, uint8_t& col);

/**
 * rowColToWellIndex(row, column)
 * 
 * Converts row/column notation into a linear index.
 * 
 * Example:
 * A1 → 0
 */
uint8_t rowColToWellIndex(char row, uint8_t column);

/**
 * Well → physical coordinates
 */

/**
 * wellIndexToXY(wellIndex, x, y)
 * 
 * Converts well index into physical coordinates (x, y).
 * 
 * Used for calibrated movement calculations.
 */
void wellIndexToXY(uint8_t wellIndex, float& x, float& y);

/**
 * wellToXY(row, col, x, y)
 * 
 * Converts well row/column into physical coordinates.
 * 
 * Wrapper combining coordinate conversion logic.
 */
void wellToXY(char row, uint8_t col, float &x, float &y);

/**
 * Coordinate ↔ motor conversions
 */

/**
 * xyToAngles(x, y, Ldeg, Rdeg)
 * 
 * Converts physical (x, y) coordinates into motor angles.
 * 
 * Used after calibration to determine required motor rotation.
 */
void xyToAngles(float x, float y, float &Ldeg, float &Rdeg);

/**
 * degToSteps(deg)
 * 
 * Converts motor angle (degrees) into stepper motor steps.
 */
long degToSteps(float deg);

/**
 * stepsToDegrees(steps)
 * 
 * Converts motor steps back into degrees.
 */
float stepsToDegrees(long steps);

/**
 * String parsing
 */

/**
 * wellStrToRowCol(wellStr, row, col)
 * 
 * Parses a well string (e.g., "A1") into row and column values.
 */
void wellStrToRowCol(char* wellStr, char& row, uint8_t& col);

/**
 * Plate configuration
 */

/**
 * getCurrentWellplate()
 * 
 * Returns currently selected well plate type.
 */
WellPlateType getCurrentWellplate();

/**
 * setCurrentWellplate(t)
 * 
 * Sets the active well plate type.
 * 
 * Affects coordinate calculations and valid well ranges.
 */
void setCurrentWellplate(WellPlateType t);