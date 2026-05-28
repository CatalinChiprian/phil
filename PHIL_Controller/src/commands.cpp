#include "../inc/commands.h"
#include "../inc/movement.h"
#include "../inc/hardware.h"
#include "../inc/actions.h"
#include "../inc/calibration.h"
#include "../inc/pumps.h"
#include "../inc/eeprom_utils.h"
#include "../inc/well_utils.h"

void parseCommands() {
	if (Serial.available() > 0) {
		char received[96];
		char* tokens[COMMAND_STRING_SZ];

		size_t len = Serial.readBytesUntil('\n', received, sizeof(received) - 1);
		if (len == 0) return;

		received[len] = '\0';

		uint8_t count = 0;
		char* tok = strtok(received, " ");

		while (tok && count < COMMAND_STRING_SZ) {
			tokens[count++] = tok;
			tok = strtok(nullptr, " ");
		}

		if (count == 0) return;

		char* cmd = tokens[0];

		if (strcmp_P(cmd, MOVE_BACKWARD_CMD) == 0) {
			moveBackward();
		}
		else if (strcmp_P(cmd, MOVE_FORWARD_CMD) == 0) {
			moveForward();
		}
		else if (strcmp_P(cmd, MOVE_LEFT_CMD) == 0) {
			moveLeft();
		}
		else if (strcmp_P(cmd, MOVE_RIGHT_CMD) == 0) {
			moveRight();
		}
		else if (strcmp_P(cmd, MOVE_UP_CMD) == 0) {
			moveUp();
		}
		else if (strcmp_P(cmd, MOVE_DOWN_CMD) == 0) {
			moveDown();
		}
		else if (strcmp_P(cmd, GO_HOME_CMD) == 0) {
			goToOrigin();
			disableAllMotors();
		}
		else if (strcmp_P(cmd, INC_STEP_CMD) == 0) {
			increaseStepSize();
			printStepSize();
		}
		else if (strcmp_P(cmd, DEC_STEP_CMD) == 0) {
			decreaseStepSize();
			printStepSize();
		}
		// ASPIRATE <pump>* <amount>* <well>
		else if (strcmp_P(cmd, ASPIRATE_CMD) == 0) {
			uint8_t pump = atoi(tokens[1]);
			uint16_t amount = atoi(tokens[2]);
			char* well = (char*)"";

			if (count >= 4) {
				well = tokens[3];
			}

			aspirate(pump, amount, well);
		}
		// DISPENSE <pump>* <amount>* <well>
		else if (strcmp_P(cmd, DISPENSE_CMD) == 0) {
			uint8_t pump = atoi(tokens[1]);
			uint16_t amount = atoi(tokens[2]);
			char* well = (char*)"";

			if (count >= 4) {
				well = tokens[3];
			}

			dispense(pump, amount, well);
		}
		else if (strcmp_P(cmd, CALIBRATE_HOME_CMD) == 0) {
			calibrateHome();
		}
		else if (strcmp_P(cmd, MOVE_HARD_WELL_CMD) == 0) {
			char* wellStr = tokens[1];
			if (strlen(wellStr) == 0) return;
			char row; uint8_t column;
			wellStrToRowCol(wellStr, row, column);
			goToHardcodedWells(row, column); 
		}
		else if (strcmp_P(cmd, MOVE_CALC_WELL_CMD) == 0) {
			char* wellStr = tokens[1];
			if (strlen(wellStr) == 0) return;
			char row; uint8_t column;
			wellStrToRowCol(wellStr, row, column);
			goToCalculatedWell(row, column); 
		}
		else if (strcmp_P(cmd, RECORD_POINT_CMD) == 0) {
			char* wellStr = tokens[1];
			if (strlen(wellStr) == 0) return;
			char row; uint8_t column;
			wellStrToRowCol(wellStr, row, column);
			recordCalibrationPoint(row, column);
		}
		else if (strcmp_P(cmd, SOLVE_MAP_CMD) == 0) {
			solveMapping();
			saveCalibration();
		}
		else if (strcmp_P(cmd, DELETE_POINT_CMD) == 0) {
			char* wellStr = tokens[1];
			if (strlen(wellStr) == 0) return;
			char row; uint8_t column;
			wellStrToRowCol(wellStr, row, column);
			deleteCalibrationPoint(row, column);
			saveCalibration();
		}
		else if (strcmp_P(cmd, CLEAR_CALIBRATION_CMD) == 0) {
			clearCalibration();
			saveCalibration();
		}
		// CREATE_ACTION <tempId>* <actionType>* <pump1>* <pump2>* <amount>* <frequency>* <frequencyUnit>* <start> <end>
		else if (strcmp_P(cmd, CREATE_ACTION_CMD) == 0) {
			if (count < 5) return;

			// TempId must be signed integer to in order to avoid id tempid-id collisions, rest can be unsigned.
			int16_t tempId = atoi(tokens[1]);
			uint8_t type = atoi(tokens[2]);
			int8_t pump1 = atoi(tokens[3]);
			int8_t pump2 = atoi(tokens[4]);
			uint16_t amount = atoi(tokens[5]);
			int8_t frequency = atoi(tokens[6]);
			uint8_t unit = atoi(tokens[7]);
			uint32_t start = 0;
			uint32_t end = 0;

			if (count >= 10) {
				start = strtoul(tokens[8], nullptr, 10);
				end = strtoul(tokens[9], nullptr, 10);
			}

			createAction(tempId, type, pump1, pump2, amount, frequency, unit, start, end);
		}
		// UPDATE_ACTION <actionId>* <actionType>* <pump1>* <pump2>* <amount>* <frequency>* <frequencyUnit>* <start>* <end>*
		else if (strcmp_P(cmd, UPDATE_ACTION_CMD) == 0) {
			if (count < 8) return;

			uint16_t id = atoi(tokens[1]);
			uint8_t type = atoi(tokens[2]);
			int8_t pump1 = atoi(tokens[3]);
			int8_t pump2 = atoi(tokens[4]);
			uint16_t amount = atoi(tokens[5]);
			int8_t frequency = atoi(tokens[6]);
			uint8_t unit = atoi(tokens[7]);
			uint32_t start = strtoul(tokens[8], nullptr, 10);
			uint32_t end = strtoul(tokens[9], nullptr, 10);

			updateAction(id, type, pump1, pump2, amount, frequency, unit, start, end);
		}
		// DEL_ACTION ID
		else if (strcmp_P(cmd, DEL_ACTION_CMD) == 0) {
			uint16_t id = atoi(tokens[1]);

			deleteAction(id);
		}
		// LINK_ACTION_WELL <actionId> <96bit_mask_hex>
		else if (strcmp_P(cmd, LINK_ACTION_WELL_CMD) == 0) {
			if (count < 3) return;
			uint16_t id = atoi(tokens[1]);
			char* hex = tokens[2];

			linkAction(id, hex);
		}
		// UNLINK_ACTION_WELL <actionId> <96bit_mask_hex>
		else if (strcmp_P(cmd, UNLINK_ACTION_WELL_CMD) == 0) {
			if (count < 3) return;
			uint16_t id = atoi(tokens[1]);
			char* hex = tokens[2];

			unlinkAction(id, hex);
		}
		else if (strcmp_P(cmd, CLEAR_ACTIONS_CMD) == 0) {
			clearAllActions();
		}
		else if (strcmp_P(cmd, PARK_CMD) == 0) {
			goToWasteContainer();
		}
		else if (strcmp_P(cmd, SET_TIME_CMD) == 0) {
			uint32_t unixTime = strtoul(tokens[1], nullptr, 10);
			adjustTime(unixTime);
		}
		else if (strcmp_P(cmd, SET_PLATE_TYPE_CMD) == 0) {

		}
		else if (strcmp_P(cmd, PRINT_WELL_CMD) == 0) {
			printCurrentWell();
		}
		else if (strcmp_P(cmd, PRINT_CALIBRATION_CMD) == 0) {
			printCalibrationPoints();
		}
		else if (strcmp_P(cmd, PRINT_STEPS_CMD) == 0) {
			printMicroSteps();
			printStepSize();
		}
		else if (strcmp_P(cmd, PRINT_ACTIONS_CMD) == 0) {
			printActions();
		}
		else if (strcmp_P(cmd, PRINT_WELL_ACTIONS_CMD) == 0) {
			printWellActions();
		}
		else if (strcmp_P(cmd, PRINT_TIME_CMD) == 0) {
			printTime();
		}
		else {
			Serial.println(F("ERROR:UNKNOWN_COMMAND"));
		}
	}
}

bool isEmergencyStopRequest() {
    if (Serial.available() <= 0) return false;
    char c = Serial.peek();
    if(c != 's') return false;

    Serial.read();
    return true;
}