/**
 * commands.h
 * 
 * Defines all command strings used for communication between the GUI and firmware.
 * Also declares functions responsible for parsing and handling these commands.
 * 
 * Each command corresponds to a specific operation such as movement,
 * calibration, pump control, or action management.
 * 
 * Commands are stored in program memory (PROGMEM) to reduce RAM usage.
 */
#pragma once

#include <Arduino.h>
#include <string.h>
#include <stdlib.h>

const char MOVE_BACKWARD_CMD[] PROGMEM = "MOVE_BACKWARD";
const char MOVE_FORWARD_CMD[] PROGMEM = "MOVE_FORWARD";
const char MOVE_LEFT_CMD[] PROGMEM = "MOVE_LEFT";
const char MOVE_RIGHT_CMD[] PROGMEM = "MOVE_RIGHT";
const char MOVE_UP_CMD[] PROGMEM = "MOVE_UP";
const char MOVE_DOWN_CMD[] PROGMEM = "MOVE_DOWN";
const char GO_HOME_CMD[] PROGMEM = "GO_HOME";
const char INC_STEP_CMD[] PROGMEM = "INC_STEP";
const char DEC_STEP_CMD[] PROGMEM = "DEC_STEP";
const char ASPIRATE_CMD[] PROGMEM = "ASPIRATE";
const char DISPENSE_CMD[] PROGMEM = "DISPENSE";
const char CALIBRATE_HOME_CMD[] PROGMEM = "CALIBRATE_HOME";
const char MOVE_HARD_WELL_CMD[] PROGMEM = "MOVE_HARD_WELL";
const char MOVE_CALC_WELL_CMD[] PROGMEM = "MOVE_CALC_WELL";
const char RECORD_POINT_CMD[] PROGMEM = "RECORD_POINT";
const char SOLVE_MAP_CMD[] PROGMEM = "SOLVE_MAP";
const char DELETE_POINT_CMD[] PROGMEM = "DELETE_POINT";
const char CLEAR_CALIBRATION_CMD[] PROGMEM = "CLEAR_CALIBRATION";
const char GO_WASTE_CMD[] PROGMEM = "GO_WASTE";
const char GO_WASH_CMD[] PROGMEM = "GO_WASH";
const char PRINT_WELL_CMD[] PROGMEM = "PRINT_WELL";
const char PRINT_CALIBRATION_CMD[] PROGMEM = "PRINT_CALIBRATION";
const char PRINT_STEPS_CMD[] PROGMEM = "PRINT_STEPS";
const char CREATE_ACTION_CMD[] PROGMEM = "CREATE_ACTION";
const char UPDATE_ACTION_CMD[] PROGMEM = "UPDATE_ACTION";
const char DEL_ACTION_CMD[] PROGMEM = "DEL_ACTION";
const char LINK_ACTION_WELL_CMD[] PROGMEM = "LINK_ACTION_WELL";
const char UNLINK_ACTION_WELL_CMD[] PROGMEM = "UNLINK_ACTION_WELL";
const char CLEAR_ACTIONS_CMD[] PROGMEM = "CLEAR_ACTIONS";
const char PRINT_ACTIONS_CMD[] PROGMEM = "PRINT_ACTIONS";
const char PRINT_WELL_ACTIONS_CMD[] PROGMEM = "PRINT_WELL_ACTIONS";
const char PRINT_MAX_ACTIONS_CMD[] PROGMEM = "PRINT_MAX_ACTIONS";
const char PRINT_MAX_ACTIONS_PER_WELL_CMD[] PROGMEM = "PRINT_MAX_ACTIONS_PER_WELL";
const char PRINT_TIME_CMD[] PROGMEM = "PRINT_TIME";
const char SET_TIME_CMD[] PROGMEM = "SET_TIME";
const char SET_PLATE_TYPE_CMD[] PROGMEM = "SET_PLATE_TYPE";

const uint8_t COMMAND_STRING_SZ = 11;


/**
 * parseCommands()
 * 
 * Reads incoming serial data from the GUI and executes
 * the corresponding command based on the predefined command list.
 */
void parseCommands();

/**
 * isEmergencyStopRequest()
 * 
 * Checks whether an emergency stop command has been received.
 * 
 * @return true if emergency stop is requested, false otherwise
 */
bool isEmergencyStopRequest();