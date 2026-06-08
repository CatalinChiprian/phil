/**
 * actions.h
 * 
 * Defines the data structures and functions used for automated
 * liquid handling through Actions.
 * 
 * An Action represents a scheduled operation (e.g. aspirate, dispense,
 * or exchange) that can be assigned to one or more wells and executed
 * automatically over time.
 * 
 * This module is responsible for:
 * - Creating and managing actions
 * - Linking actions to wells
 * - Scheduling and executing actions
 * - Providing debugging and status output
 * 
 * Due to memory constraints on Arduino, the number of actions
 * and their associations are limited.
 */
#pragma once

#include <stdint.h>
#include <string.h>


/**
 * ActionType
 * 
 * Defines the type of operation an action performs.
 * 
 * ASPIRATE → removes liquid (IN well)
 * DISPENSE → adds liquid (OUT well)
 * EXCHANGE → combined operation (multi-step for OoC systems)
 */
enum ActionType : uint8_t {
    ASPIRATE, // 96-well: IN. OoC: IN
    DISPENSE, // 96-well: OUT. OoC: OUT
    EXCHANGE // 96-well: N/A. OoC: OUT,IN,OUT,IN
};

/**
 * TimeUnit
 * 
 * Defines the unit used for scheduling frequency.
 */
enum TimeUnit : uint8_t {
    MINUTE,
    HOUR,
    DAY,
};

/**
 * System limits (due to memory constraints)
 * 
 * MAX_WELLS → number of wells (e.g., 96-well plate)
 * MAX_ACTIONS_PER_WELL → max actions assigned to one well
 * MAX_ACTIONS_TOTAL → total number of actions allowed
 */
constexpr uint8_t MAX_WELLS = 96;
constexpr uint8_t MAX_ACTIONS_PER_WELL = 16;
constexpr uint16_t MAX_ACTIONS_TOTAL = 64;


/**
 * Special value representing an invalid or unused entry
 */
constexpr uint16_t INVALID = 0xFF;


/**
 * Action structure
 * 
 * Represents a single scheduled operation.
 * Stored in a compact format to minimize memory usage.
 */
#pragma pack(push, 1)
struct Action {
    uint16_t id;
    ActionType type;
    int8_t pump1;       // 96-well: the pump. OoC: dispense/IN pump
    int8_t pump2;       // 96-well: unused. OoC: aspirate/OUT pump
    uint16_t amount_uL;
    int8_t frequency;
    TimeUnit unit;
    uint32_t startEpoch;
    uint32_t endEpoch;
    uint32_t lastRunEpoch;
    uint8_t enabled;    // Whether action is active. Is used to soft-delete.
};
#pragma pack(pop)


/**
 * WellAction structure
 * 
 * Maps actions to wells.
 * Each well stores a list of associated Action IDs.
 */
#pragma pack(push, 1)
struct WellAction {
    uint8_t actionIds[MAX_ACTIONS_PER_WELL]; // Linked actions
    uint8_t count;                          // Number of active links
};
#pragma pack(pop)


/**
 * Global state
 */
extern Action actions[MAX_ACTIONS_TOTAL]; // All actions in system
extern WellAction wellActions[MAX_WELLS]; // Mapping of wells → actions

extern uint8_t actionCount;               // Number of active actions
extern uint16_t nextActionId;             // ID generator


/**
 * Action management functions
 */

/**
 * createAction()
 * 
 * Creates a new action with provided parameters.
 * 
 * tempId is used by GUI to map temporary client-side actions
 * to firmware-assigned IDs.
 * 
 * @return assigned action ID
 */
uint16_t createAction(int16_t tempId, ActionType type, int8_t pump1, int8_t pump2, uint16_t amount, int8_t frequency, TimeUnit unit, uint32_t start, uint32_t end);

/**
 * updateAction()
 * 
 * Updates parameters of an existing action.
 */
void updateAction(uint16_t id, ActionType type, int8_t pump1, int8_t pump2, uint16_t amount, int8_t frequency, TimeUnit unit, uint32_t start, uint32_t end);

/**
 * deleteAction()
 * 
 * Removes an action from the system and detaches it from all wells.
 */
void deleteAction(uint16_t id);

/**
 * linkAction()
 * 
 * Links an action to one or more wells using a bitmask (hex string).
 */
void linkAction(uint16_t id, char* hex);

/**
 * unlinkAction()
 * 
 * Removes association between an action and selected wells.
 */
void unlinkAction(uint16_t id, char* hex);

/**
 * processActions()
 * 
 * Main scheduler function.
 * 
 * Called in the main loop to:
 * - Check action timing
 * - Execute actions when conditions are met
 * - Update last execution time
 */
void processActions();

/**
 * Debug / print functions
 */

 
/**
 * printAction()
 * 
 * Prints details of a single action.
 */
void printAction(const Action& action);

/**
 * printActions()
 * 
 * Prints all actions in the system.
 */
void printActions();

/**
 * printWellAction()
 * 
 * Prints actions assigned to a specific well.
 */
void printWellAction(const WellAction& wellAction, uint8_t wellIndex);

/**
 * printWellActions()
 * 
 * Prints all well-to-action mappings.
 */
void printWellActions();

/**
 * printMaxActions()
 * 
 * Prints the maximum number of actions supported.
 */
void printMaxActions();

/**
 * printMaxActionsPerWell()
 * 
 * Prints the per-well action limit.
 */
void printMaxActionsPerWell();