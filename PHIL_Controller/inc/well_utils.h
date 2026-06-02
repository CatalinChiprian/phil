#pragma once

#include <stdint.h>

enum WellPlateType : uint8_t {
    ORGANONCHIP,
    WELL96
};

bool isInvalidWell(char row, uint8_t col);
void wellIndexToRowCol(uint8_t wellIndex, char& row, uint8_t& col);
uint8_t RowColToWellIndex(char row, uint8_t column);
long degToSteps(float deg);
float stepsToDegrees(long steps);
void wellToXY(char row, uint8_t col, float &x, float &y);
void xyToAngles(float x, float y, float &Ldeg, float &Rdeg);
void wellStrToRowCol(char* wellStr, char& row, uint8_t& col);

WellPlateType getCurrentWellplate();
void setCurrentWellplate(WellPlateType t);