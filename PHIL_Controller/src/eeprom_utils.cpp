#include "../inc/eeprom_utils.h"
#include "../inc/movement.h"
#include "../inc/hardware.h"
#include "../inc/calibration.h"
#include "../inc/well_utils.h"

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

void savePositions() {
    EEPROM.put(POS_ADDR_L, (int16_t)stepperL.currentPosition());
    EEPROM.put(POS_ADDR_R, (int16_t)stepperR.currentPosition());
    EEPROM.put(POS_ADDR_Z1, (int16_t)stepperZ1.currentPosition());
    EEPROM.put(POS_ADDR_Z2, (int16_t)stepperZ2.currentPosition());
    EEPROM.put(EEPROM_POS_MAGIC_ADDR, MAGIC);
}

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

bool saveWellPlateType() {
    EEPROM.put(EEPROM_PLATE_TYPE_MAGIC_ADDR, MAGIC);
    EEPROM.put(EEPROM_PLATE_TYPE_ADDR, getCurrentWellplate());
}

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
}

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

void saveCurrentWell(uint8_t wellIndex) {  
    EEPROM.put(EEPROM_WELL_BASE, wellIndex);
}

void loadCurrentWell() {
    wellIndex = EEPROM.read(EEPROM_WELL_BASE);

    printCurrentWell();
}

void saveWellAction(WellAction& wa, uint8_t wellIndex) {
    EEPROM.put(EEPROM_WELL_ACTIONS_ADDR + (uint32_t)wellIndex * sizeof(WellAction), wa);
}

void saveWellActions() {
    EEPROM.put(EEPROM_WELL_ACTIONS_MAGIC_ADDR, MAGIC);
    int addr = EEPROM_WELL_ACTIONS_ADDR;
    for (uint8_t i = 0; i < MAX_WELLS; i++) {
        EEPROM.put(addr, wellActions[i]);
        addr += sizeof(WellAction);
    }
}

void saveAction(Action& action, uint8_t slot) {
    EEPROM.put(EEPROM_ACTIONS_ADDR + (slot) * sizeof(Action), action);
}

void initializeEmptyActions() {
    for (uint8_t i = 0; i < MAX_ACTIONS_TOTAL; i++) {
        actions[i].id = 0;
        actions[i].enabled = 0;
    }

    saveActionsState();
}

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

void loadActions() {
    int addr = EEPROM_ACTIONS_ADDR;
    for (uint8_t i = 0; i < MAX_ACTIONS_TOTAL; i++) {
        EEPROM.get(addr, actions[i]);
        if (actions[i].enabled) printAction(actions[i]);
        addr += sizeof(Action);
    }
}

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

void saveActionsState() {
    EEPROM.put(EEPROM_NEXT_ACTION_ID_ADDR, nextActionId);
    EEPROM.put(EEPROM_ACTION_COUNT_ADDR, actionCount);

    EEPROM.put(EEPROM_ACTIONS_MAGIC_ADDR, MAGIC);
}

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