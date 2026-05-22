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
	times_x10 += 1;
	printStepSize();
  }
  else if (strcmp_P(cmd, DEC_STEP_CMD) == 0) {
	times_x10 -= 1;
	printStepSize();
  }
  // ASPIRATE <pump>* <amount>* <well>
  else if (strcmp_P(cmd, ASPIRATE_CMD) == 0) {
	uint8_t pump = atoi(tokens[1]);
	uint16_t amount = atoi(tokens[2]);

	aspirate(pump, amount, tokens[3]);
  }
  // DISPENSE <pump>* <amount>* <well>
  else if (strcmp_P(cmd, DISPENSE_CMD) == 0) {
	uint8_t pump = atoi(tokens[1]);
	uint16_t amount = atoi(tokens[2]);

	dispense(pump, amount, tokens[3]);
  }
  else if (strcmp_P(cmd, CALIBRATE_HOME_CMD) == 0) {
	calibrateHome();
  }
  else if (strcmp_P(cmd, MOVE_HARD_WELL_CMD) == 0) {
	char* wellStr = tokens[1];
	if (strlen(wellStr) == 0) return;
	char row = tolower(wellStr[0]);
	uint8_t column = atoi(wellStr + 1);
	goToHardcodedWells(row, column); 
  }
  else if (strcmp_P(cmd, MOVE_CALC_WELL_CMD) == 0) {
	char* wellStr = tokens[1];
	if (strlen(wellStr) == 0) return;
	char row = tolower(wellStr[0]);
	uint8_t column = atoi(wellStr + 1); 
	goToCalculatedWell(row, column); 
  }
  else if (strcmp_P(cmd, RECORD_POINT_CMD) == 0) {
	char* wellStr = tokens[1];
	if (strlen(wellStr) == 0) return;
	char row = tolower(wellStr[0]);
	uint8_t column = atoi(wellStr + 1);
	recordCalibrationPoint(row, column);
  }
  else if (strcmp_P(cmd, SOLVE_MAP_CMD) == 0) {
	solveMapping();
    saveCalibration();
  }
  else if (strcmp_P(cmd, DELETE_POINT_CMD) == 0) {
	char* wellStr = tokens[1];
	if (strlen(wellStr) == 0) return;
	char row = tolower(wellStr[0]);
	uint8_t column = atoi(wellStr + 1);

	float x = 0;
	float y = 0;
	wellToXY(row, column, x, y);
	
	int8_t foundIdx = -1;
	for (uint8_t i = 0; i < calCount; i++) {
		if (fabs(calX[i] - x) < 0.1f && fabs(calY[i] - y) < 0.1f) {
			foundIdx = i;
			break;
		}
	}
	
	if (foundIdx == -1) return;

	for (int8_t i = foundIdx; i < calCount - 1; i++) {
		calX[i] = calX[i+1];
		calY[i] = calY[i+1];
		calL[i] = calL[i+1];
		calR[i] = calR[i+1];
	}
	calCount--;

	Serial.print(F("CAL_DELETED:")); Serial.print(row); Serial.print(column);
	Serial.print(F(",remaining=")); Serial.println(calCount);
  }
  else if (strcmp_P(cmd, CLEAR_CALIBRATION_CMD) == 0) {
	EEPROM.put(EEPROM_CAL_BASE, 0x00);
	mapReady = false;
	for (uint8_t i = 0; i < TERMS; i++) { ML[i] = 0; MR[i] = 0; }
	calCount = 0;
	Serial.println(F("Calibration cleared"));
  }
  // CREATE_ACTION <tempId>* <actionType>* <pump1>* <pump2>* <amount>* <frequency>* <frequencyUnit>* <start> <end>
  else if (strcmp_P(cmd, CREATE_ACTION_CMD) == 0) {
	if (count < 5) return;
	
	// TempId must be signed integer to in order to avoid id tempid-id collisions, rest can be unsigned.
	int16_t tempId = atoi(tokens[1]);
	uint8_t type = atoi(tokens[2]);
	uint8_t pump1 = atoi(tokens[3]);
	uint8_t pump2 = atoi(tokens[4]);
	uint16_t amount = atoi(tokens[5]);
	uint16_t frequency = atoi(tokens[6]);
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
	uint8_t pump1 = atoi(tokens[3]);
	uint8_t pump2 = atoi(tokens[4]);
	uint16_t amount = atoi(tokens[5]);
	uint16_t frequency = atoi(tokens[6]);
	uint8_t unit = atoi(tokens[7]);
	uint32_t start = atoi(tokens[8]);
	uint32_t end = atoi(tokens[9]);

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

	if (!findActionById(id)) return;

	uint8_t mask[12];
	if (!parseWellBitmask(tokens[2], mask)) return;

	linkActionByMask(id, mask);
  }
  // UNLINK_ACTION_WELL <actionId> <96bit_mask_hex>
  else if (strcmp_P(cmd, UNLINK_ACTION_WELL_CMD) == 0) {
	if (count < 3) return;
	uint16_t id = atoi(tokens[1]);

	if (!findActionById(id)) return;

	uint8_t mask[12];
	if (!parseWellBitmask(tokens[2], mask)) return;

	unlinkActionByMask(id, mask);
  }
  else if (strcmp_P(cmd, CLEAR_ACTIONS_CMD) == 0) {
	clearAllActions();
  }
  else if (strcmp_P(cmd, PARK_CMD) == 0) {
	enableLMotor();
	enableRMotor();

	stepperL.moveTo(55 * currentMicrosteps); 
	stepperR.moveTo(-5.5 * currentMicrosteps); 
	
	while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
	  stepperR.run();
	  stepperL.run();
	}

	savePositions();
  }
  else if (strcmp_P(cmd, SET_TIME) == 0) {
	uint32_t unixTime = strtoul(tokens[1], nullptr, 10);
	rtc.adjust(DateTime(unixTime));
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
	for (uint8_t i = 0; i < MAX_ACTIONS_TOTAL; i++) {
	  if (actions[i].enabled) printAction(actions[i]);
	}
  }
  else if (strcmp_P(cmd, PRINT_WELL_ACTIONS_CMD) == 0) {
	for (uint8_t i = 0; i < MAX_WELLS; i++) {
	  if (wellActions[i].count > 0) printWellAction(wellActions[i], i);
	}
  }
  else if (strcmp_P(cmd, PRINT_TIME) == 0) {
	Serial.print(F("TIME=")); Serial.println(rtc.now().unixtime());
  }
  else {
	Serial.println(F("ERROR:UNKNOWN_COMMAND"));
  }
}
}