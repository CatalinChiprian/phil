#pragma once

#include <stdint.h>
#include <string.h>

enum ActionType : uint8_t {
    ASPIRATE, // 96-well: IN. OoC: IN
    DISPENSE, // 96-well: OUT. OoC: OUT
    EXCHANGE // 96-well: N/A. OoC: OUT,IN,OUT,IN
};

enum TimeUnit : uint8_t {
    MINUTE, // debug only
    HOUR,
    DAY,
};

constexpr uint8_t MAX_WELLS = 96;
constexpr uint8_t MAX_ACTIONS_PER_WELL = 28;
constexpr uint16_t MAX_ACTIONS_TOTAL = 64;

constexpr uint16_t INVALID = 0xFF;

struct Action {
    uint16_t id;
    ActionType type;
    int8_t pump1; // 96-well: the pump. OoC: dispense/IN pump
    int8_t pump2; // 96-well: unused. OoC: aspirate/OUT pump
    uint16_t amount_uL;
    int8_t frequency;
    TimeUnit unit;
    uint32_t startEpoch;
    uint32_t endEpoch;
    uint32_t lastRunEpoch;
    uint8_t enabled;
};

struct WellAction {
    uint8_t actionIds[MAX_ACTIONS_PER_WELL];
    uint8_t count;
};

extern Action actions[MAX_ACTIONS_TOTAL];
extern WellAction wellActions[MAX_WELLS];

extern uint8_t actionCount;
extern uint16_t nextActionId;

uint16_t createAction(int16_t tempId, ActionType type, int8_t pump1, int8_t pump2, uint16_t amount, int8_t frequency, TimeUnit unit, uint32_t start, uint32_t end);
void updateAction(uint16_t id, ActionType type, int8_t pump1, int8_t pump2, uint16_t amount, int8_t frequency, TimeUnit unit, uint32_t start, uint32_t end);
void deleteAction(uint16_t id);
void linkAction(uint16_t id, char* hex);
void unlinkAction(uint16_t id, char* hex);
void clearAllActions();
void processActions();
void changePlateType();


void printAction(const Action& action);
void printActions();
void printWellAction(const WellAction& wellAction, uint8_t wellIndex);
void printWellActions();
void printMaxActions();
void printMaxActionsPerWell();