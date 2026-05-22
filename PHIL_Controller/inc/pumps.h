#pragma once

#include <stdint.h>

void dispense(uint8_t pump, uint16_t microliters, char* well);
void aspirate(uint8_t pump, uint16_t microliters, char* well);