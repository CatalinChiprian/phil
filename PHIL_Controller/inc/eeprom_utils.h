/**
 * eeprom_utils.h
 * 
 * Provides persistent storage functionality using EEPROM.
 * 
 * This module is responsible for saving and loading:
 * - Motor positions (L, R, Z axes)
 * - Current well position
 * - Calibration data
 * - Plate type
 * - Actions and scheduling data
 * 
 * EEPROM is used to maintain system state across restarts.
 * 
 * A "magic byte" system is used to validate stored data and 
 * detect whether EEPROM has been initialized.
 */
#pragma once

#include <EEPROM.h>
#include "../inc/actions.h"


/**
 * MAGIC value used to validate EEPROM sections.
 * If missing or incorrect → data is considered invalid/uninitialized.
 */
constexpr uint8_t MAGIC = 0xCC;


/**
 * Position storage (4 bytes per value)
 * Stores motor positions for:
 * - Left motor (L)
 * - Right motor (R)
 * - Z-axis motors (Z1, Z2)
 */
constexpr uint16_t POS_ADDR_L = 0;
constexpr uint16_t POS_ADDR_R = 4;
constexpr uint16_t POS_ADDR_Z1 = 8;
constexpr uint16_t POS_ADDR_Z2 = 12;
constexpr uint16_t EEPROM_POS_MAGIC_ADDR = 16;

/**
 * Plate type storage
 * Defines the currently active plate configuration (e.g. 96-well, OoC)
 */
constexpr uint16_t EEPROM_PLATE_TYPE_MAGIC_ADDR = 18;
constexpr uint16_t EEPROM_PLATE_TYPE_ADDR = 19;

/**
 * Calibration data storage
 * Stores calibration points and mapping coefficients
 */
constexpr uint16_t EEPROM_CAL_MAGIC_ADDR = 64;

/**
 * Current well position storage
 * Keeps track of last selected well across restarts
 */
constexpr uint16_t EEPROM_WELL_ADDR = 800;

/**
 * Action storage layout
 * 
 * Stores:
 * - Actions array
 * - Action metadata (count, next ID)
 * - Well-to-action mappings
 */
constexpr uint16_t EEPROM_ACTIONS_MAGIC_ADDR = 896;
constexpr uint16_t EEPROM_NEXT_ACTION_ID_ADDR = 900;
constexpr uint16_t EEPROM_ACTION_COUNT_ADDR = 902;
constexpr uint16_t EEPROM_ACTIONS_ADDR = 904;
constexpr uint16_t EEPROM_WELL_ACTIONS_MAGIC_ADDR = 1800;
constexpr uint16_t EEPROM_WELL_ACTIONS_ADDR = 2000;



/**
 * Initialization
 */

/**
 * initPersistentState()
 * 
 * Initializes EEPROM state.
 * 
 * - Validates stored data using magic values
 * - Loads saved data if valid
 * - Initializes defaults if not
 */
void initPersistentState();

/**
 * Motor position persistence
 */
 
/**
 * savePositions()
 * 
 * Stores current motor positions in EEPROM.
 */
void savePositions();

/**
 * loadPositions()
 * 
 * Loads motor positions from EEPROM.
 * 
 * @return true if valid data was loaded
 */
bool loadPositions();

/**
 * Plate type persistence
 */

/**
 * saveWellPlateType()
 * 
 * Saves the current plate type to EEPROM.
 */
void saveWellPlateType();

/**
 * loadWellPlateType()
 * 
 * Loads plate type from EEPROM.
 * 
 * @return true if successful
 */
bool loadWellPlateType();

/**
 * Calibration persistence
 */

/**
 * saveCalibration()
 * 
 * Stores all calibration data (points + mapping coefficients).
 */
void saveCalibration();

/**
 * loadCalibration()
 * 
 * Loads calibration data from EEPROM.
 */
void loadCalibration();

/**
 * clearCalibration()
 * 
 * Resets calibration data in EEPROM and memory.
 */
void clearCalibration();

/**
 * Current well persistence
 */

/**
 * saveCurrentWell(wellIndex)
 * 
 * Saves the currently selected well.
 */
void saveCurrentWell(uint8_t wellIndex);

/**
 * loadCurrentWell()
 * 
 * Loads last selected well from EEPROM.
 */
void loadCurrentWell();

/**
 * Well-action mapping persistence
 */

/**
 * saveWellAction(wa, wellIndex)
 * 
 * Stores action mappings for a single well.
 */
void saveWellAction(WellAction& wa, uint8_t wellIndex);

/**
 * saveWellActions()
 * 
 * Stores all well-to-action mappings.
 */
void saveWellActions();

/**
 * loadWellActions()
 * 
 * Loads all well-action mappings from EEPROM.
 */
void loadWellActions();

/**
 * Action persistence
 */

/**
 * saveAction(action, slot)
 * 
 * Stores a single action at a given slot index.
 */
void saveAction(Action& action, uint8_t slot);

/**
 * loadActions()
 * 
 * Loads all stored actions from EEPROM.
 */
void loadActions();

/**
 * loadActionsSafe()
 * 
 * Loads actions with validation checks 
 * to prevent corrupted data from being used.
 */
void loadActionsSafe();

/**
 * saveActionsState()
 * 
 * Stores global action state (count and next ID).
 */
void saveActionsState();

/**
 * loadActionsState()
 * 
 * Loads global action state from EEPROM.
 */
void loadActionsState();

/**
 * clearAllActions()
 * 
 * Disables/removes all actions and clears mappings.
 */
void clearAllActions();