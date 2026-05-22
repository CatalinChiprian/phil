#pragma once

#include <stdint.h>

bool isInvalidWell(char row, uint8_t col);
void wellIndexToRowCol(uint8_t wellIndex, char& row, uint8_t& col);
uint8_t wellNameToIndex(char row, uint8_t column);
long degToSteps(float deg);
float stepsToDegrees(long steps);
void wellToXY(char row, uint8_t col, float &x, float &y);
void xyToAngles(float x, float y, float &Ldeg, float &Rdeg);