#include "../inc/actions.h"
#include "../inc/pumps.h"
#include "../inc/hardware.h"
#include "../inc/well_utils.h"
#include "../inc/eeprom_utils.h"

Action actions[MAX_ACTIONS_TOTAL];
WellAction wellActions[MAX_WELLS];

uint8_t actionCount = 0;
uint16_t nextActionId = 1;

uint8_t hexNibble(char c) {
    if (c >= '0' && c <= '9') return c - '0';
    if (c >= 'A' && c <= 'F') return c - 'A' + 10;
    if (c >= 'a' && c <= 'f') return c - 'a' + 10;
    return INVALID;
}

bool parseWellBitmask(const char* hex, uint8_t mask[12]) {
    if (strlen(hex) != 24) return false;

    for (uint8_t i = 0; i < 12; i++) {
        uint8_t hi = hexNibble(hex[i * 2]);
        uint8_t lo = hexNibble(hex[i * 2 + 1]);

        if (hi == INVALID || lo == INVALID) return false;

        mask[i] = (hi << 4) | lo;
    }

    return true;
}

Action* findActionById(uint16_t id) {
for (uint8_t i = 0; i < MAX_ACTIONS_TOTAL; i++) {
    if (actions[i].id == id && actions[i].enabled) {
    return &actions[i];
    }
}

return nullptr;
}

uint8_t findFreeActionSlot() {
for (uint16_t i = 0; i < MAX_ACTIONS_TOTAL; i++) {
    if (!actions[i].enabled) return i;
}
return INVALID;
}

bool linkActionToWell(uint16_t actionId, uint8_t wellIndex) {
    WellAction &wa = wellActions[wellIndex];

    if (wa.count >= MAX_ACTIONS_PER_WELL) return false;

    for (uint8_t i = 0; i < wa.count; i++) {
        if (wa.actionIds[i] == actionId) return true;
    }

    wa.actionIds[wa.count++] = actionId;

    char row;
    uint8_t col;

    wellIndexToRowCol(wellIndex, row, col);

    saveWellAction(wa, wellIndex);
    Serial.print(F("ACTION_WELL_LINK:"));
    Serial.print(F("Action:")); Serial.print(actionId);
    Serial.print(F(",Well:")); Serial.print(row); Serial.println(col);
    return true;
}

bool unlinkActionFromWell(uint16_t actionId, uint8_t wellIndex) {
    WellAction &wa = wellActions[wellIndex];

    for (uint8_t i = 0; i < wa.count; i++) {
        if (wa.actionIds[i] == actionId) {
        for (uint8_t j = i; j + 1 < wa.count; j++) {
            wa.actionIds[j] = wa.actionIds[j + 1];
        }
        wa.count--;
        saveWellAction(wa, wellIndex);

        char row;
        uint8_t col;

        wellIndexToRowCol(wellIndex, row, col);

        saveWellAction(wa, wellIndex);
        Serial.print(F("ACTION_WELL_UNLINK:"));
        Serial.print(F("Action:")); Serial.print(actionId);
        Serial.print(F(",Well:")); Serial.print(row); Serial.println(col);
        return true;
        }
    }
    return false;
}

void linkActionByMask(uint16_t actionId, const uint8_t mask[12]) {
    for (uint8_t well = 0; well < MAX_WELLS; well++) {
        uint8_t byteIdx = well / 8;
        uint8_t bitIdx  = well % 8;

        if (mask[byteIdx] & (1 << bitIdx)) {
            linkActionToWell(actionId, well);
        }
    }
}

void linkAction(uint16_t id, char* hex) {
	if (!findActionById(id)) return;

	uint8_t mask[12];
	if (!parseWellBitmask(hex, mask)) return;

	linkActionByMask(id, mask);
}

void unlinkActionByMask(uint16_t actionId, const uint8_t mask[12]) {
    for (uint8_t well = 0; well < MAX_WELLS; well++) {
        uint8_t byteIdx = well / 8;
        uint8_t bitIdx  = well % 8;

        if (mask[byteIdx] & (1 << bitIdx)) {
            unlinkActionFromWell(actionId, well);
        }
    }
}

void unlinkAction(uint16_t id, char* hex) {
    if (!findActionById(id)) return;

	uint8_t mask[12];
	if (!parseWellBitmask(hex, mask)) return;

	unlinkActionByMask(id, mask);
}

uint16_t createAction(int16_t tempId, ActionType type, int8_t pump1, int8_t pump2, uint16_t amount, int8_t frequency, TimeUnit unit, uint32_t start, uint32_t end) {
    if (actionCount >= MAX_ACTIONS_TOTAL) {
        Serial.println(F("ERROR:FAILED TO CREATE ACTION"));
        return 0;
    }

    uint8_t slot = findFreeActionSlot();
    if (slot == INVALID) return 0;

    Action &action = actions[slot];

    action.id = nextActionId++;
    action.type = type;
    action.pump1 = pump1;
    action.pump2 = pump2;
    action.amount_uL = amount;
    action.frequency = frequency;
    action.unit = unit;
    action.startEpoch = start;
    action.endEpoch = end;
    action.lastRunEpoch = 0;
    action.enabled = 1;

    saveAction(action, slot);
    actionCount++;

    saveActionsState();

    Serial.print(F("ACTION_CREATED:"));
    Serial.print(F("TempId=")); Serial.print(tempId);
    Serial.print(F(",Id=")); Serial.println(action.id);

    return action.id;
}

void updateAction(uint16_t id, ActionType type, int8_t pump1, int8_t pump2, uint16_t amount, int8_t frequency, TimeUnit unit, uint32_t start, uint32_t end) {

    Action* action = findActionById(id);
    if (!action) return;


    action->type = type;
    action->pump1 = pump1;
    action->pump2 = pump2;
    action->amount_uL = amount;
    action->frequency = frequency;
    action->unit = unit;
    action->startEpoch = start;
    action->endEpoch = end;

    uint8_t index = action - actions;
    saveAction(*action, index);

    Serial.print(F("ACTION_UPDATED:"));
    Serial.println(action->id);
}

void deleteAction(uint16_t id) {
    Action* action = findActionById(id);
    if (!action) return;

    action->enabled = 0;

    uint8_t index = action - actions;
    saveAction(*action, index);


    for (uint8_t w = 0; w < MAX_WELLS; w++) {
        unlinkActionFromWell(id, w);
    }

    Serial.print(F("ACTION_DELETED:"));
    Serial.println(action->id);
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

uint32_t unitToSeconds(TimeUnit unit) {
    switch (unit) {
        case MINUTE: return 60; // Test only
        case HOUR: return 60 * 60;
        case DAY: return 24 * 60 * 60;
        default: return 0;
    }
}

bool isActionLinkedToWell(const uint16_t &actionId, const uint8_t &wellIndex) {
    if (wellIndex >= MAX_WELLS) return false;

    const WellAction &wa = wellActions[wellIndex];

    if (wa.count > MAX_ACTIONS_PER_WELL) return false;

    for (uint8_t i = 0; i < wa.count; i++) {
        if (wa.actionIds[i] == actionId) {
        return true;
        }
    }
    return false;
}

void executeAction(Action &action) {
    for (uint8_t well = 0; well < MAX_WELLS; well++) {  
        if (!isActionLinkedToWell(action.id, well)) continue;

        Serial.print(F("EXECUTING_ACTION_ID:")); 
        Serial.print(F("Id=")); Serial.print(action.id);
        Serial.print(F(",LastRun=")); Serial.println(action.lastRunEpoch);

        char row;
        uint8_t col;
        wellIndexToRowCol(well, row, col);

        char wellName[4];
        wellName[0] = row;
        itoa(col, &wellName[1], 10);


        switch (action.type) {
        case ASPIRATE:
            aspirate(action.pump1, action.amount_uL, wellName);
            break;
        case DISPENSE:
            dispense(action.pump1, action.amount_uL, wellName);
            break;
        case EXCHANGE:
        {
            char nxtRow = row + 1;
            uint8_t nxtCol = col + 1;
            char outWellName[4];
            outWellName[0] = nxtRow;
            itoa(nxtCol, &outWellName[1], 10);

            if (isInvalidWell(nxtRow, nxtCol)) return;
            
            if (action.pump2 >= 0) aspirate(action.pump2, action.amount_uL, outWellName);
            if (action.pump1 >= 0) dispense(action.pump1, action.amount_uL, wellName);
            if (action.pump2 >= 0) aspirate(action.pump2, action.amount_uL, outWellName);
            if (action.pump1 >= 0) dispense(action.pump1, action.amount_uL, wellName);
            break;
        }
        }
    }
}

bool isActionCompatible(const Action& action) {
    bool is96 = (getCurrentWellplate() == WELL96);

    if (action.type == EXCHANGE && is96) return false;
    if (action.type != EXCHANGE && !is96) return false;

    return true;
}

void processActions() {
    uint32_t now = getTime();

    for (uint16_t i = 0; i < MAX_ACTIONS_TOTAL; i++) {
        Action &action = actions[i];

        if (!action.enabled) continue;

        if (!isActionCompatible(action)) continue;

        if (now < action.startEpoch) continue;
        if (action.endEpoch != 0 && now > action.endEpoch) continue;

        // ONE-TIME ACTION
        // When Frequency is not set Actions run only once on the specified time
        if (action.frequency < 0) {
            if (action.lastRunEpoch != 0) continue;
            if (now < action.startEpoch) continue;

            action.lastRunEpoch = now;
            executeAction(action);
            saveAction(action, i);
            continue;
        }

        // REPEATING ACTION
        // Frequency is set, thus it must run every TimeUnit
        uint32_t period = action.frequency * unitToSeconds(action.unit);
        if (period == 0) continue;

        uint32_t nextRun = action.lastRunEpoch + period;

        if (action.lastRunEpoch == 0) nextRun = action.startEpoch;

        if (now < nextRun) continue;

        action.lastRunEpoch = nextRun;
        executeAction(action);
        saveAction(action, i);
    }
}

void printAction(const Action& action) {
    Serial.print(F("ACTION:"));
    Serial.print(F("Id="));Serial.print(action.id);
    Serial.print(F(",ActionType=")); Serial.print((uint8_t)action.type);
    Serial.print(F(",Pump1=")); Serial.print(action.pump1);
    Serial.print(F(",Pump2=")); Serial.print(action.pump2);
    Serial.print(F(",Amount=")); Serial.print(action.amount_uL);
    Serial.print(F(",Frequency=")); Serial.print(action.frequency);
    Serial.print(F(",Unit=")); Serial.print((uint8_t)action.unit);
    Serial.print(F(",Start=")); Serial.print(action.startEpoch);
    Serial.print(F(",End=")); Serial.print(action.endEpoch);
    Serial.print(F(",LastRun=")); Serial.print(action.lastRunEpoch);
    Serial.print(F(",Enabled=")); Serial.println(action.enabled);
}

void printActions() {
	for (uint8_t i = 0; i < MAX_ACTIONS_TOTAL; i++) {
	  if (actions[i].enabled) printAction(actions[i]);
	}
    Serial.println(F("END_ACTIONS"));
}

void printWellAction(const WellAction& wellAction, uint8_t wellIndex) {
    char row;
    uint8_t col;
    wellIndexToRowCol(wellIndex, row, col);
    Serial.print(F("WELL_ACTION:"));
    Serial.print(F("Well=")); Serial.print((char)toupper(row)); Serial.print(col);
    Serial.print(F(",Actions=["));
    for (uint8_t i = 0; i < wellAction.count; i++) {
        if (i > 0) Serial.print(',');
        Serial.print(wellAction.actionIds[i]);
    }
    Serial.println(']');
}

void printWellActions() {
	for (uint8_t i = 0; i < MAX_WELLS; i++) {
	  if (wellActions[i].count > 0) printWellAction(wellActions[i], i);
	}
    Serial.println(F("END_WELL_ACTIONS"));
}

void printMaxActions() {
    Serial.print(F("MAX_ACTIONS_TOTAL:")); Serial.println(MAX_ACTIONS_TOTAL);
}

void printMaxActionsPerWell() {
    Serial.print(F("MAX_ACTIONS_PER_WELL:")); Serial.println(MAX_ACTIONS_PER_WELL);
}