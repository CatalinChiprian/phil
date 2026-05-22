#pragma once

#include <stdint.h>

constexpr uint8_t WELL_HOME = 0xFF;

extern const int16_t ZMotorNormalPosition;

extern int16_t times_x10;
extern uint8_t wellIndex; // 0–N‑1 = wells, 255 = HOME

void moveBackward();
void moveForward();
void moveLeft();
void moveRight();
void moveUp();
void moveDown();
void goToOrigin();
void moveZMotors(int16_t position);
void goToHardcodedWells(char row, uint8_t column);
void goToCalculatedWell(char row, uint8_t col);
void updatePositionState();

void printCurrentWell();
void printPosition(int16_t L, int16_t R, int16_t Z1, int16_t Z2);
void printStepSize();
void printMicroSteps();