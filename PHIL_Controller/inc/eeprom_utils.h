#pragma once

#include <EEPROM.h>
#include "../inc/actions.h"

constexpr uint8_t MAGIC = 0xCC;

constexpr uint16_t POS_ADDR_L = 0;
constexpr uint16_t POS_ADDR_R = 4;
constexpr uint16_t POS_ADDR_Z1 = 8;
constexpr uint16_t POS_ADDR_Z2 = 12;

constexpr uint16_t EEPROM_CAL_BASE = 64;
constexpr uint16_t EEPROM_WELL_BASE = 800;

constexpr uint16_t EEPROM_ACTIONS_MAGIC_ADDR = 896;
constexpr uint16_t EEPROM_NEXT_ACTION_ID_ADDR = 900;
constexpr uint16_t EEPROM_ACTION_COUNT_ADDR = 902;
constexpr uint16_t EEPROM_ACTIONS_ADDR = 904;
constexpr uint16_t EEPROM_WELL_ACTIONS_ADDR = 2000;

void initPersistentState();
void savePositions();
bool loadPositions();
void saveCalibration();
void loadCalibration();
void saveCurrentWell(uint8_t wellIndex);
void loadCurrentWell();
void saveWellAction(WellAction& wa, uint8_t wellIndex);
void saveWellActions();
void saveAction(Action& action, uint8_t slot);
void loadActionsSafe();
void loadActions();
void loadWellActions();
void saveActionsState();
void loadActionsState();