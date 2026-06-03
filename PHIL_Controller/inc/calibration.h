#pragma once

#include <stdint.h>

constexpr uint8_t MAX_CAL = 32;
constexpr uint8_t TERMS = 10;

extern uint8_t calCount;
extern float ML[TERMS];
extern float MR[TERMS];
extern uint8_t calWellIndex[MAX_CAL];
extern float calL[MAX_CAL];
extern float calR[MAX_CAL];
extern bool mapReady;

constexpr float WELL_DX = 9.0;
constexpr float WELL_DY = 9.0;

int8_t calibrateHome();
void recordCalibrationPoint(char row, uint8_t col);
bool solveMapping();
void deleteCalibrationPoint(char row, uint8_t col);

inline float dot10(const float a[TERMS], const float b[TERMS]) {
    return a[0]*b[0] + a[1]*b[1] + a[2]*b[2] + a[3]*b[3] + a[4]*b[4]
        + a[5]*b[5] + a[6]*b[6] + a[7]*b[7] + a[8]*b[8] + a[9]*b[9];
}

void printCalibrationPoints();