#include "../inc/actions.h"
#include "../inc/pumps.h"
#include "../inc/hardware.h"
#include "../inc/well_utils.h"
#include "../inc/eeprom_utils.h"

Action actions[MAX_ACTIONS_TOTAL];
WellAction wellActions[MAX_WELLS];

uint8_t actionCount = 0;
uint16_t nextActionId = 1;

/**
 * hexNibble(c)
 * 
 * Converts a single hexadecimal character into its numeric value (0–15).
 * 
 * Supports:
 * - '0'–'9'
 * - 'A'–'F'
 * - 'a'–'f'
 * 
 * @param c Hexadecimal character
 * @return value (0–15), or INVALID if not valid
 */
uint8_t hexNibble(char c) {
    if (c >= '0' && c <= '9') return c - '0';
    if (c >= 'A' && c <= 'F') return c - 'A' + 10;
    if (c >= 'a' && c <= 'f') return c - 'a' + 10;
    return INVALID;
}

/**
 * parseWellBitmask(hex, mask)
 * 
 * Converts a 24-character hexadecimal string into a 96-bit mask.
 * Each bit represents whether an action is linked to a well.
 * 
 * Mapping:
 * - 96 wells → 96 bits → 12 bytes
 * - Each byte represents 8 wells
 * 
 * Example:
 * - hex string "FF0000..." → first 8 wells selected
 * 
 * @param hex  Input string (48 hex characters = 96 bits)
 * @param mask Output byte array (12 bytes)
 * 
 * @return true if valid, false if malformed
 */
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

/**
 * findActionById(id)
 * 
 * Searches for an enabled action with a given ID.
 * 
 * @return pointer to action if found, nullptr otherwise
 */
Action* findActionById(uint16_t id) {
    for (uint8_t i = 0; i < MAX_ACTIONS_TOTAL; i++) {
        if (actions[i].id == id && actions[i].enabled) {
            return &actions[i];
        }
    }

    return nullptr;
}

/**
 * findFreeActionSlot()
 * 
 * Finds an unused slot in the actions array.
 * 
 * @return index of free slot, or INVALID if full
 */
uint8_t findFreeActionSlot() {
    for (uint16_t i = 0; i < MAX_ACTIONS_TOTAL; i++) {
        if (!actions[i].enabled) return i;
    }
    return INVALID;
}

/**
 * linkActionToWell(actionId, wellIndex)
 * 
 * Associates an action with a specific well.
 * 
 * Behavior:
 * - Adds action ID to well's action list
 * - Prevents duplicates
 * - Saves mapping to persistent storage
 * 
 * Constraints:
 * - Limited by MAX_ACTIONS_PER_WELL
 * 
 * @return true if successful
 */
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

/**
 * unlinkActionFromWell(actionId, wellIndex)
 * 
 * Removes an action from a specific well.
 * 
 * Behavior:
 * - Searches for action ID
 * - Removes it and compacts the list
 * - Updates persistent storage
 * 
 * @return true if action was found and removed
 */
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

/**
 * linkActionByMask(actionId, mask)
 * 
 * Links an action to multiple wells based on a bitmask.
 * 
 * Iterates through all wells and checks if corresponding
 * bit is set in mask, linking the action where needed.
 */
void linkActionByMask(uint16_t actionId, const uint8_t mask[12]) {
    for (uint8_t well = 0; well < MAX_WELLS; well++) {
        uint8_t byteIdx = well / 8;
        uint8_t bitIdx  = well % 8;

        if (mask[byteIdx] & (1 << bitIdx)) {
            linkActionToWell(actionId, well);
        }
    }
}

/**
 * linkAction(id, hex)
 * 
 * Public interface for linking an action to wells.
 * 
 * Steps:
 * 1. Validate action exists
 * 2. Parse hexadecimal bitmask input
 * 3. Link action to all wells defined in mask
 */
void linkAction(uint16_t id, char* hex) {
	if (!findActionById(id)) return;

	uint8_t mask[12];
	if (!parseWellBitmask(hex, mask)) return;

	linkActionByMask(id, mask);
}

/**
 * unlinkActionByMask(actionId, mask)
 * 
 * Removes an action from multiple wells using a bitmask.
 * 
 * Iterates through all wells and clears links where
 * corresponding bits are set in the mask.
 */
void unlinkActionByMask(uint16_t actionId, const uint8_t mask[12]) {
    for (uint8_t well = 0; well < MAX_WELLS; well++) {
        uint8_t byteIdx = well / 8;
        uint8_t bitIdx  = well % 8;

        if (mask[byteIdx] & (1 << bitIdx)) {
            unlinkActionFromWell(actionId, well);
        }
    }
}

/**
 * unlinkAction(id, hex)
 * 
 * Public interface for removing action links from wells.
 * 
 * Steps:
 * 1. Validate action exists
 * 2. Parse hexadecimal bitmask
 * 3. Unlink action from all selected wells
 */
void unlinkAction(uint16_t id, char* hex) {
    if (!findActionById(id)) return;

	uint8_t mask[12];
	if (!parseWellBitmask(hex, mask)) return;

	unlinkActionByMask(id, mask);
}

/**
 * createAction(int16_t tempId, ActionType type, int8_t pump1, int8_t pump2, uint16_t amount, int8_t frequency, TimeUnit unit, uint32_t start, uint32_t end)
 * 
 * Creates a new action and assigns it a unique ID.
 * 
 * Steps:
 * 1. Find free slot
 * 2. Populate action fields
 * 3. Store action in EEPROM
 * 4. Increment global counters
 * 
 * tempId:
 * - Temporary identifier used by GUI for synchronization
 * 
 * @return assigned action ID, or 0 if failed
 */
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

/**
 * updateAction(id, ...)
 * 
 * Updates an existing action's parameters.
 * 
 * Behavior:
 * - Finds action by ID
 * - Overwrites all configurable fields
 * - Saves updated action to persistent storage
 * 
 * If action does not exist, function exits silently.
 */
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

/**
 * deleteAction(id)
 * 
 * Disables and removes an action from the system.
 * 
 * Steps:
 * 1. Find action by ID
 * 2. Mark as disabled
 * 3. Persist updated state
 * 4. Unlink action from all wells
 * 
 * This ensures no orphaned references remain.
 */
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

/**
 * unitToSeconds(unit)
 * 
 * Converts time units into seconds.
 * Used for scheduling intervals.
 */
uint32_t unitToSeconds(TimeUnit unit) {
    switch (unit) {
        case MINUTE: return 60;
        case HOUR: return 60 * 60;
        case DAY: return 24 * 60 * 60;
        default: return 0;
    }
}

/**
 * isActionLinkedToWell(actionId, wellIndex)
 * 
 * Checks whether a specific action is assigned to a given well.
 * 
 * @return true if linked, false otherwise
 */
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

/**
 * executeAction(action)
 * 
 * Executes an action across all wells it is linked to.
 * 
 * Process:
 * 1. Iterate through all wells
 * 2. Check if action is linked to well
 * 3. Convert well index → well name (e.g. A1)
 * 4. Execute operation based on action type:
 *      - ASPIRATE
 *      - DISPENSE
 *      - EXCHANGE (multi-step)
 * 
 * Notes:
 * - EXCHANGE operation performs multiple pump actions
 * - Validation ensures well indices are valid
 * 
 * This is the core function that translates
 * scheduled actions into physical robot operations.
 */
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
                
                if (action.pump2 >= 0 && action.pump1 >= 0) {
                    aspirate(action.pump2, action.amount_uL, wellName);
                    dispense(action.pump1, action.amount_uL, outWellName);
                    aspirate(action.pump2, action.amount_uL, outWellName);
                    dispense(action.pump1, action.amount_uL, wellName);
                }
                else if (action.pump2 >= 0) aspirate(action.pump2, action.amount_uL, outWellName);
                else if (action.pump1 >= 0) dispense(action.pump1, action.amount_uL, wellName);

                break;
            }
        }
    }
}

/**
 * handleAction(action, index, now)
 * 
 * Executes an action and updates its runtime state.
 * 
 * Steps:
 * - Update last execution timestamp
 * - Execute action
 * - Persist updated state
 */
void handleAction(Action &action, uint16_t index, uint32_t now) {
    action.lastRunEpoch = now;
    executeAction(action);
    saveAction(action, index);
}

/**
 * isActionCompatible(action)
 * 
 * Ensures that the action type matches the selected plate type.
 * 
 * Example:
 * - EXCHANGE only allowed for OoC system
 * - ASPIRATE/DISPENSE only for 96-well plates
 * 
 * @return true if compatible
 */
bool isActionCompatible(const Action& action) {
    bool is96 = (getCurrentWellplate() == WELL96);

    if (action.type == EXCHANGE && is96) return false;
    if (action.type != EXCHANGE && !is96) return false;

    return true;
}

/**
 * processActions()
 * 
 * Main scheduler for all automated actions.
 * Called continuously in the main loop.
 * 
 * Logic:
 * 1. Get current time
 * 2. Iterate through all actions
 * 3. Skip invalid or inactive actions
 * 4. Check:
 *    - Plate compatibility
 *    - Start and end time
 * 
 * Two execution modes:
 * 
 * 1. ONE-TIME ACTION:
 *    - frequency < 0
 *    - Runs once at start time
 * 
 * 2. REPEATING ACTION:
 *    - Runs periodically based on frequency and unit
 *    - Uses lastRunEpoch to determine next execution time
 * 
 * Scheduling:
 * - Uses Unix timestamps for precise timing
 * - Prevents repeated execution within same interval
 * 
 * This function defines the automated behavior of the system.
 */
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

            handleAction(action, i, now);
            continue;
        }

        // REPEATING ACTION
        // Frequency is set, thus it must run every TimeUnit
        uint32_t period = action.frequency * unitToSeconds(action.unit);
        if (period == 0) continue;

        int32_t baseTime = (action.lastRunEpoch == 0)
        ? (action.startEpoch == 0 ? now : action.startEpoch)
        : action.lastRunEpoch;

        uint32_t nextRun = baseTime + (action.lastRunEpoch == 0 ? 0 : period);

        if (now < nextRun) continue;

        handleAction(action, i, now);
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

/**
 * printActions()
 * 
 * Prints all enabled actions for debugging and GUI display.
 * Ends with "END_ACTIONS" marker.
 */
void printActions() {
	for (uint8_t i = 0; i < MAX_ACTIONS_TOTAL; i++) {
	  if (actions[i].enabled) printAction(actions[i]);
	}
    Serial.println(F("END_ACTIONS"));
}

/**
 * printWellAction(wellAction, wellIndex)
 * 
 * Prints all actions linked to a specific well.
 * 
 * Output format:
 * WELL_ACTION: Well=A1, Actions=[1,2,3]
 */
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

/**
 * printWellActions()
 * 
 * Prints all wells that have at least one linked action.
 * Ends with "END_WELL_ACTIONS" marker.
 */
void printWellActions() {
	for (uint8_t i = 0; i < MAX_WELLS; i++) {
	  if (wellActions[i].count > 0) printWellAction(wellActions[i], i);
	}
    Serial.println(F("END_WELL_ACTIONS"));
}

/**
 * printMaxActions()
 * 
 * Prints the maximum number of actions supported by the system.
 */
void printMaxActions() {
    Serial.print(F("MAX_ACTIONS_TOTAL:")); Serial.println(MAX_ACTIONS_TOTAL);
}

/**
 * printMaxActionsPerWell()
 * 
 * Prints the maximum number of actions allowed per well.
 */
void printMaxActionsPerWell() {
    Serial.print(F("MAX_ACTIONS_PER_WELL:")); Serial.println(MAX_ACTIONS_PER_WELL);
}