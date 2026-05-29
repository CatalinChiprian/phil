#include "../inc/well_utils.h"
#include "../inc/hardware.h"
#include "../inc/calibration.h"
#include <math.h>

WellPlateType selectedPlateType;

bool isInvalidWell(char row, uint8_t col) {
    return (col < 1 || col > 12 || row < 'a' || row > 'h');
}

void wellIndexToRowCol(uint8_t wellIndex, char& row, uint8_t& col) {
    row = 'a' + (wellIndex / 12);
    col = (wellIndex % 12) + 1;
}

uint8_t wellNameToIndex(char row, uint8_t column) {
    return (row - 'a') * 12 + (column - 1);
}

long degToSteps(float deg) {
    float stepsPerRev = 200.0 * currentMicrosteps;
    return lroundf(deg * (stepsPerRev / 360.0));
}

float stepsToDegrees(long steps) {
    float stepsPerRev = 200.0f * currentMicrosteps;
    return steps * (360.0f / stepsPerRev);
}

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

void wellStrToRowCol(char* wellStr, char& row, uint8_t& col) {
	row = tolower(wellStr[0]);
	col = atoi(wellStr + 1);
}

WellPlateType getCurrentWellplate() {
    return selectedPlateType;
}

void setCurrentWellplate(WellPlateType t) {
    selectedPlateType = t;
}