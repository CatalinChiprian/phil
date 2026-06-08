#include "../inc/eeprom_utils.h"
#include "../inc/movement.h"
#include "../inc/hardware.h"
#include "../inc/calibration.h"
#include "../inc/well_utils.h"

/**
 * initPersistentState()
 * 
 * Initializes all persistent data from EEPROM at startup.
 * 
 * Steps:
 * 1. Load last well position
 * 2. Load calibration data
 * 3. Load actions safely (with validation)
 * 4. Load well-action mappings
 * 5. Attempt to restore motor positions
 * 
 * If stored motor positions are invalid:
 * - Perform full homing calibration
 * 
 * Note:
 * Mechanical drift or startup movement can desynchronize
 * stored positions from real hardware, so homing may be required.
 */
void initPersistentState() {
    loadCurrentWell();
    loadCalibration();
    loadActionsSafe();
    loadWellActions();
    
    if (!loadPositions()) {
      calibrateHome();
    }
    // The pipette might jump on start-up, causing a mismatch between software and mechanical position.
    // On every start-up we must calibrate the home position.
    //calibrateHome();
}

/**
 * savePositions()
 * 
 * Stores current motor positions (L, R, Z axes) into EEPROM.
 * 
 * Also writes a magic value to indicate that valid position
 * data is available for future restoration.
 */
void savePositions() {
    EEPROM.put(POS_ADDR_L, (int16_t)stepperL.currentPosition());
    EEPROM.put(POS_ADDR_R, (int16_t)stepperR.currentPosition());
    EEPROM.put(POS_ADDR_Z1, (int16_t)stepperZ1.currentPosition());
    EEPROM.put(POS_ADDR_Z2, (int16_t)stepperZ2.currentPosition());
    EEPROM.put(EEPROM_POS_MAGIC_ADDR, MAGIC);
}

/**
 * loadPositions()
 * 
 * Restores motor positions from EEPROM.
 * 
 * Uses a magic byte to verify that saved positions are valid.
 * If not valid:
 * - Returns false
 * - System should perform homing
 * 
 * @return true if valid positions were loaded
 */
bool loadPositions() {
    uint8_t ok;
    EEPROM.get(EEPROM_POS_MAGIC_ADDR, ok);

    if (ok != MAGIC) {
        Serial.println(F("No valid stored positions – doing normal home"));
        return false;
    }

    int16_t L, R, Z1, Z2;
    EEPROM.get(POS_ADDR_L, L);
    EEPROM.get(POS_ADDR_R, R);
    EEPROM.get(POS_ADDR_Z1, Z1);
    EEPROM.get(POS_ADDR_Z2, Z2);

    stepperL.setCurrentPosition(L);
    stepperR.setCurrentPosition(R);
    stepperZ1.setCurrentPosition(Z1);
    stepperZ2.setCurrentPosition(Z2);

    return true;
}

/**
 * saveWellPlateType()
 * 
 * Saves the currently selected plate type.
 * 
 * Persists both:
 * - Magic value (validity check)
 * - Plate type enum
 */
void saveWellPlateType() {
    EEPROM.put(EEPROM_PLATE_TYPE_MAGIC_ADDR, MAGIC);
    EEPROM.put(EEPROM_PLATE_TYPE_ADDR, getCurrentWellplate());
}

/**
 * loadWellPlateType()
 * 
 * Loads the stored plate type from EEPROM.
 * 
 * Validates using magic byte.
 * If invalid:
 * - Leaves default plate type
 * 
 * @return true if valid plate type loaded
 */
bool loadWellPlateType() {
    uint8_t ok;
    EEPROM.get(EEPROM_PLATE_TYPE_MAGIC_ADDR, ok);

    if (ok != MAGIC) {
        Serial.println(F("No valid stored WellPlateType"));
        return false;
    }

    WellPlateType plateType;
    EEPROM.get(EEPROM_PLATE_TYPE_ADDR, plateType);

    setCurrentWellplate(plateType);
    return true;
}

/**
 * saveCalibration()
 * 
 * Stores calibration data in EEPROM:
 * - Number of calibration points
 * - Well indices
 * - Motor values for each point
 * 
 * Uses sequential memory layout.
 * 
 * Note:
 * Mapping coefficients are recomputed after loading,
 * rather than stored directly.
 */
void saveCalibration() {
    int addr = EEPROM_CAL_MAGIC_ADDR;
    uint8_t magic = MAGIC;
    EEPROM.put(addr, magic);  addr += sizeof(magic);

    EEPROM.put(addr, calCount);  addr += sizeof(uint8_t);

    for (uint8_t i = 0; i < calCount; i++) {
        EEPROM.put(addr, calWellIndex[i]);  addr += sizeof(uint8_t);
        
        EEPROM.put(addr, calL[i]); addr += sizeof(float);
        EEPROM.put(addr, calR[i]); addr += sizeof(float);
    }

    Serial.println(F("Calibration saved to EEPROM"));
    Serial.print(F("Saved ")); Serial.print(calCount); Serial.println(F(" points"));
}

/**
 * loadCalibration()
 * 
 * Loads calibration points from EEPROM.
 * 
 * Steps:
 * 1. Validate magic value
 * 2. Load calibration count
 * 3. Validate range (prevent corruption)
 * 4. Load calibration points
 * 5. Recompute mapping (solveMapping)
 * 
 * If data is invalid:
 * - Calibration is reset
 */
void loadCalibration() {
    int addr = EEPROM_CAL_MAGIC_ADDR;
    uint8_t magic;
    EEPROM.get(addr, magic);  addr += sizeof(magic);
    if (magic != MAGIC) {
        Serial.println(F("No valid calibration in EEPROM"));
        return;
    }

    EEPROM.get(addr, calCount);  addr += sizeof(uint8_t);
    if (calCount > MAX_CAL) {
        Serial.println(F("Corrupt point count in EEPROM"));
        calCount = 0;
        return false;
    }

    for (uint8_t i = 0; i < calCount; i++) {
        EEPROM.get(addr, calWellIndex[i]);  addr += sizeof(uint8_t);
        
        EEPROM.get(addr, calL[i]); addr += sizeof(float);
        EEPROM.get(addr, calR[i]); addr += sizeof(float);
    }

    solveMapping();
    return;
}

/**
 * saveCurrentWell(wellIndex)
 * 
 * Stores the currently active well index.
 */
void saveCurrentWell(uint8_t wellIndex) {  
    EEPROM.put(EEPROM_WELL_ADDR, wellIndex);
}

/**
 * loadCurrentWell()
 * 
 * Loads last selected well from EEPROM.
 * 
 * Also prints current well for debugging/GUI sync.
 */
void loadCurrentWell() {
    wellIndex = EEPROM.read(EEPROM_WELL_ADDR);

    printCurrentWell();
}

/**
 * saveWellAction(wa, wellIndex)
 * 
 * Stores the action mappings for a single well.
 * 
 * Writes the WellAction structure directly to EEPROM
 * at an offset based on the well index.
 */
void saveWellAction(WellAction& wa, uint8_t wellIndex) {
    EEPROM.put(EEPROM_WELL_ACTIONS_ADDR + (uint32_t)wellIndex * sizeof(WellAction), wa);
}

/**
 * saveWellActions()
 * 
 * Stores all well-to-action mappings.
 * 
 * Writes:
 * - Magic value (validity check)
 * - Continuous array of WellAction structures
 */
void saveWellActions() {
    EEPROM.put(EEPROM_WELL_ACTIONS_MAGIC_ADDR, MAGIC);
    int addr = EEPROM_WELL_ACTIONS_ADDR;
    for (uint8_t i = 0; i < MAX_WELLS; i++) {
        EEPROM.put(addr, wellActions[i]);
        addr += sizeof(WellAction);
    }
}

/**
 * saveAction(action, slot)
 * 
 * Stores a single action in EEPROM at the given slot index.
 * 
 * Used after:
 * - Creating actions
 * - Updating actions
 * - Executing actions (updating timestamps)
 */
void saveAction(Action& action, uint8_t slot) {
    EEPROM.put(EEPROM_ACTIONS_ADDR + (slot) * sizeof(Action), action);
}

/**
 * initializeEmptyActions()
 * 
 * Clears all actions in memory (soft reset).
 * 
 * Sets:
 * - ID = 0
 * - enabled = 0
 * 
 * Also resets persistent state.
 */
void initializeEmptyActions() {
    for (uint8_t i = 0; i < MAX_ACTIONS_TOTAL; i++) {
        actions[i].id = 0;
        actions[i].enabled = 0;
    }

    saveActionsState();
}

/**
 * loadActionsSafe()
 * 
 * Safely loads actions from EEPROM.
 * 
 * Steps:
 * 1. Check magic value
 * 2. If invalid → initialize empty system
 * 3. Otherwise load actions and state
 * 
 * Prevents usage of corrupted or uninitialized data.
 */
void loadActionsSafe() {
    uint8_t magic;
    EEPROM.get(EEPROM_ACTIONS_MAGIC_ADDR, magic);

    if (magic != MAGIC) {
        initializeEmptyActions();
        return;
    }

    loadActions();
    loadActionsState();
}

/**
 * loadActions()
 * 
 * Loads all actions from EEPROM into memory.
 * 
 * Prints enabled actions for debugging/verification.
 */
void loadActions() {
    int addr = EEPROM_ACTIONS_ADDR;
    for (uint8_t i = 0; i < MAX_ACTIONS_TOTAL; i++) {
        EEPROM.get(addr, actions[i]);
        if (actions[i].enabled) printAction(actions[i]);
        addr += sizeof(Action);
    }
}

/**
 * loadWellActions()
 * 
 * Loads well-to-action mappings from EEPROM.
 * 
 * Validation:
 * - Checks magic value
 * - Ensures count does not exceed limits
 * 
 * Invalid entries are reset to prevent corruption.
 */
void loadWellActions() {
    uint8_t ok;
    EEPROM.get(EEPROM_WELL_ACTIONS_MAGIC_ADDR, ok);

    if (ok != MAGIC) {
        Serial.println(F("No valid WellActions"));

        for (uint8_t i = 0; i < MAX_WELLS; i++) {
            wellActions[i].count = 0;
        }
        return;
    }

    int addr = EEPROM_WELL_ACTIONS_ADDR;
    for (uint8_t i = 0; i < MAX_WELLS; i++) {
        EEPROM.get(addr, wellActions[i]);
        
        if (wellActions[i].count > MAX_ACTIONS_PER_WELL) {
            wellActions[i].count = 0;
        }

        if (wellActions[i].count > 0) printWellAction(wellActions[i], i);
        addr += sizeof(WellAction);
    }
}

/**
 * saveActionsState()
 * 
 * Stores global action metadata:
 * - nextActionId (ID generator)
 * - actionCount (number of active actions)
 * 
 * Also writes magic value for validation.
 */
void saveActionsState() {
    EEPROM.put(EEPROM_NEXT_ACTION_ID_ADDR, nextActionId);
    EEPROM.put(EEPROM_ACTION_COUNT_ADDR, actionCount);

    EEPROM.put(EEPROM_ACTIONS_MAGIC_ADDR, MAGIC);
}

/**
 * loadActionsState()
 * 
 * Loads global action metadata from EEPROM.
 * 
 * Includes safety checks:
 * - Resets invalid nextActionId
 * - Resets invalid actionCount
 */
void loadActionsState() {
    EEPROM.get(EEPROM_NEXT_ACTION_ID_ADDR, nextActionId);
    EEPROM.get(EEPROM_ACTION_COUNT_ADDR, actionCount);

    if (nextActionId == 0 || nextActionId == 0xFFFF) {
        nextActionId = 1;
    }
    if (actionCount > MAX_ACTIONS_TOTAL) {
        actionCount = 0;
    }
}

/**
 * clearAllActions()
 * 
 * Completely resets all actions and their associations.
 * 
 * Steps:
 * 1. Overwrite all action slots with empty entries
 * 2. Clear all well-action mappings
 * 3. Reset counters and ID tracking
 * 4. Save cleared state to EEPROM
 * 
 * Note:
 * This performs a full cleanup (not soft delete).
 */
void clearAllActions() {
    Action empty;
    memset(&empty, 0, sizeof(Action));
    empty.id = 0;
    empty.enabled = 0;

    for (uint8_t i = 0; i < MAX_ACTIONS_TOTAL; i++) {
        actions[i] = empty;
        EEPROM.put(EEPROM_ACTIONS_ADDR + i * sizeof(Action), empty);
    }

    for (uint8_t w = 0; w < MAX_WELLS; w++) {
        wellActions[w].count = 0;
    }

    actionCount = 0;
    nextActionId = 1;

    saveActionsState();
    saveWellActions();

    Serial.println(F("Actions Cleared"));
}

/**
 * clearCalibration()
 * 
 * Clears all calibration data:
 * - Invalidates EEPROM data
 * - Resets mapping coefficients
 * - Resets calibration count
 * 
 * After this, a new calibration must be performed.
 */
void clearCalibration() {
	EEPROM.put(EEPROM_CAL_MAGIC_ADDR, 0x00);
	mapReady = false;

	for (uint8_t i = 0; i < TERMS; i++)
    {
        ML[i] = 0;
        MR[i] = 0;
    }

	calCount = 0;
	Serial.println(F("Calibration cleared"));
}