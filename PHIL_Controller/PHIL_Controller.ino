/* Created by Catalin Chiprian
  Based on Phillip Dettinger work availible on https://github.com/CSDGroup/PHIL.git */

  #include <EEPROM.h>
  #include <AccelStepper.h>
  #include <MultiStepper.h>
  #include <Wire.h>
  #include "RTClib.h"

  #define WELL_HOME   0xFF
  #define MAGIC       0xCC

  #define POS_ADDR_L                 0
  #define POS_ADDR_R                 4
  #define POS_ADDR_Z1                8
  #define POS_ADDR_Z2                12
  #define EEPROM_CAL_BASE            64
  #define EEPROM_WELL_BASE           800
  #define EEPROM_ACTIONS_MAGIC_ADDR  896
  #define EEPROM_NEXT_ACTION_ID_ADDR 900
  #define EEPROM_ACTION_COUNT_ADDR   902
  #define EEPROM_ACTIONS_ADDR        904
  #define EEPROM_WELL_ACTIONS_ADDR   2000

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
  const char PARK_CMD[] PROGMEM = "PARK";
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
  const char PRINT_TIME[] PROGMEM = "PRINT_TIME";
  const char SET_TIME[] PROGMEM = "SET_TIME";


  constexpr uint8_t MAX_WELLS = 96;
  constexpr uint8_t MAX_ACTIONS_PER_WELL = 16;
  constexpr uint16_t MAX_ACTIONS_TOTAL = 64;
  constexpr uint16_t INVALID = 0xFF;

  enum State : uint8_t {
    SETUP,
    RUNNING
  } currentState;

  // enum ActionType : uint8_t {
  //   ASPIRATE, // 96-well: IN. OoC: IN
  //   DISPENSE, // 96-well: OUT. OoC: OUT
  //   EXCHANGE // 96-well: N/A. OoC: OUT,IN,OUT,IN
  // };

  // enum TimeUnit : uint8_t {
  //   HOUR,
  //   DAY,
  // };

  struct Action {
    uint16_t id;
    uint8_t type;
    uint8_t pump1; // 96-well: the pump. OoC: dispense/IN pump
    uint8_t pump2; // 96-well: unused. OoC: aspirate/OUT pump
    uint16_t amount_uL;
    uint16_t frequency;
    uint8_t unit;
    uint32_t startEpoch;
    uint32_t endEpoch;
    uint32_t lastRunEpoch;
    uint8_t enabled;
  };

  struct WellAction {
    uint8_t actionIds[MAX_ACTIONS_PER_WELL];
    uint8_t count;
  };

  Action actions[MAX_ACTIONS_TOTAL];
  WellAction wellActions[MAX_WELLS];

  uint8_t actionCount = 0;
  uint16_t nextActionId = 1;

  static uint32_t lastSchedulerRun = 0;

  const uint8_t myMICROS = 1;
  const char Sttngs[][3] = {
    {LOW,  LOW, LOW}, // Full step
    {HIGH,  LOW, LOW}, // Half step
    {LOW, HIGH,  LOW}, // 1/4 step
    {HIGH,  HIGH,  LOW}, // 1/8 step
    {LOW, LOW, HIGH}, // 1/16 step
    {HIGH,  HIGH,  HIGH} // Full step
  };

  const float WELL_DX = 9.0;
  const float WELL_DY = 9.0;

  const uint8_t MAX_CAL = 32;
  float calX[MAX_CAL], calY[MAX_CAL], calL[MAX_CAL], calR[MAX_CAL];
  uint8_t calCount = 0;

  const uint8_t TERMS = 10;
  float ML[TERMS] = {0};
  float MR[TERMS] = {0};
  bool mapReady = false;

  uint8_t wellIndex;// 0–N‑1 = wells, 255 = HOME

  const uint8_t MICROoptions[] = {1, 2, 4, 8, 16, 32};

  const uint8_t M1 = 25; 
  const uint8_t M2 = 26; 
  const uint8_t M3 = 27; 

  const uint8_t P1 = 22;
  const uint8_t P2 = 23;
  const uint8_t P3 = 24;

  const uint8_t ena[]  = {44, 47, 50, 53, 13, 10};
  const uint8_t step[] = {43, 46, 49, 52, 12, 9};
  const uint8_t dir[] = {42, 45, 48, 51, 11, 8};   

  long lastR;
  long lastL;

  AccelStepper stepperZ1(1, step[0], dir[0]);
  AccelStepper stepperL(1, step[1], dir[1]);
  AccelStepper stepperR(1, step[2], dir[2]);
  AccelStepper stepperZ2(1, step[3], dir[3]);

  AccelStepper stepperP1(1, step[4], dir[4]);
  AccelStepper stepperP2(1, step[5], dir[5]);

  RTC_DS3231 rtc;

  const float UL_PER_STEP = 0.1099f;

  const uint8_t limitSwitchL = 31; // Target Limit Switch L
  const uint8_t limitSwitchR = 30; // Target Limit Switch R
  const uint8_t limitSwitchZ1 = 33; // Target Limit Switch Z
  const uint8_t limitSwitchZ2 = 32; // Target Limit Switch Z

  const uint8_t faultR = 37;
  const uint8_t faultL = 39;

  const uint8_t microIndex = 3; // 0=full, 1=half, 2=1/4, 3=1/8, 4=1/16, 5=1/32
  const uint8_t currentMicrosteps = MICROoptions[microIndex]; 

  int16_t times_x10 = 1;
  const int16_t steps = 4 * currentMicrosteps;

  const int16_t ZMotorPumpPosition = -2496;
  const int16_t ZMotorNormalPosition = -384;

  uint16_t lastMotorActivityTime = 0;
  bool ZMotorsCurrentlyEnabled = false;
  bool LMotorCurrentlyEnabled = false;
  bool RMotorCurrentlyEnabled = false;
  bool P1MotorCurrentlyEnabled = false;
  bool P2MotorCurrentlyEnabled = false;
  const uint16_t MOTOR_TIMEOUT = 5000;
  bool eStopRequested = false;

  void setup() {
    Serial.begin(9600);
    Wire.begin();

    
    if (!rtc.begin()) {
        Serial.println(F("ERROR: RTC not found"));
        while (1);
    }

    
    if (rtc.lostPower()) {
      Serial.println(F("RTC lost power, setting time..."));

      DateTime compileTime = DateTime(F(__DATE__), F(__TIME__));
      rtc.adjust(compileTime);

    }

    Serial.print(F("TIME=")); Serial.println(rtc.now().unixtime());

    pinMode(limitSwitchL, INPUT_PULLUP);
    pinMode(limitSwitchR, INPUT_PULLUP);
    pinMode(limitSwitchZ1, INPUT_PULLUP);
    pinMode(limitSwitchZ2, INPUT_PULLUP);

    pinMode(faultR, INPUT_PULLUP);
    pinMode(faultL, INPUT_PULLUP);
    
    pinMode(M1, OUTPUT); 
    digitalWrite(M1, Sttngs[microIndex][0]); 
    pinMode(M2, OUTPUT); 
    digitalWrite(M2, Sttngs[microIndex][1]); 
    pinMode(M3, OUTPUT); 
    digitalWrite(M3, Sttngs[microIndex][2]);

    // Pumps are hardcoded to full step
    pinMode(P1, OUTPUT); 
    digitalWrite(P1, Sttngs[0][0]); 
    pinMode(P2, OUTPUT); 
    digitalWrite(P2, Sttngs[0][1]); 
    pinMode(P3, OUTPUT); 
    digitalWrite(P3, Sttngs[0][2]);

    stepperZ1.setMaxSpeed(2000 * currentMicrosteps);
    stepperZ2.setMaxSpeed(2000 * currentMicrosteps);
    stepperZ1.setAcceleration(1500 * currentMicrosteps);
    stepperZ2.setAcceleration(1500 * currentMicrosteps);

    setNormalMovementSpeed();
    setNormalPumpSpeed();

    disableAllMotors();
  
    if (!loadPositions()) {
      calibrate();
    }

    printStepSize();

    loadCurrentWell();

    loadCalibration();

    loadActionsSafe();
    loadWellActions();

    eStopRequested = false;
    currentState = RUNNING;

    // The pipette might jump on start-up, causing a mismatch between software and mechanical position.
    // On every start-up we must calibrate the home position.
    //calibrate();
  }

  void moveBackward() {
    enableLMotor();
    enableRMotor();

    long s = steps * times_x10 / 10;
    stepperL.moveTo(-s + stepperL.currentPosition());
    stepperR.moveTo(s + stepperR.currentPosition());

    while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
      stepperL.run();
      stepperR.run();
    }

    savePositions();
  }

  void moveForward() {
    enableLMotor();
    enableRMotor();

    long s = steps * times_x10 / 10;
    stepperL.moveTo(s + stepperL.currentPosition());
    stepperR.moveTo(-s + stepperR.currentPosition());

    while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
      stepperL.run();
      stepperR.run();
    }

    savePositions();
  }

  void moveLeft() {
    enableLMotor();
    enableRMotor();

    bool lLimitHit = false;

    long s = steps * times_x10 / 10;
    stepperL.moveTo(-s + stepperL.currentPosition());
    stepperR.moveTo(-s + stepperR.currentPosition());

    while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {

      if (digitalRead(limitSwitchL) == LOW) {
          stepperL.stop();
          lLimitHit = true;
      }

      if (lLimitHit) {
          stepperL.stop();
          stepperR.stop();
          break;
      }

      stepperL.run();
      stepperR.run();
    }

    savePositions();
  }

  void moveRight() {
    enableLMotor();
    enableRMotor();

    bool rLimitHit = false;

    long s = steps * times_x10 / 10;
    stepperL.moveTo(s + stepperL.currentPosition());
    stepperR.moveTo(s + stepperR.currentPosition());

    while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
      
      if (digitalRead(limitSwitchR) == LOW) {
          stepperR.stop();
          rLimitHit = true;
      }

      if (rLimitHit) {
          stepperR.stop();
          stepperL.stop();
          break;
      }

      stepperL.run();
      stepperR.run();
    }

    savePositions();
  }

  void moveUp() {
    enableZMotors();
    
    bool z1LimitHit = false;
    bool z2LimitHit = false;

    long s = 16 * currentMicrosteps;
    stepperZ1.moveTo(s + stepperZ1.currentPosition());
    stepperZ2.moveTo(s + stepperZ2.currentPosition());

    while (stepperZ1.distanceToGo() != 0 || stepperZ2.distanceToGo() != 0) {

      if (digitalRead(limitSwitchZ1) == LOW) {
          stepperZ1.stop();
          z1LimitHit = true;
      }
      if (digitalRead(limitSwitchZ2) == LOW) {
          stepperZ2.stop();
          z2LimitHit = true;
      }

      if (z1LimitHit || z2LimitHit) {
          stepperZ1.stop();
          stepperZ1.setCurrentPosition(0);
          stepperZ2.stop();
          stepperZ2.setCurrentPosition(0);
          break;
      }

      stepperZ1.run();
      stepperZ2.run();
    }

    savePositions();
  }

  void moveDown() {
    enableZMotors();

    long s = 16 * currentMicrosteps;
    stepperZ1.moveTo(-s + stepperZ1.currentPosition());
    stepperZ2.moveTo(-s + stepperZ2.currentPosition());

    while(stepperZ1.distanceToGo() != 0 || stepperZ2.distanceToGo() != 0) {
      stepperZ1.run();
      stepperZ2.run();
    }

    savePositions();
  }

  void moveZMotors(int16_t position) {
    enableZMotors();

    long pos = position;
    stepperZ1.moveTo(pos);
    stepperZ2.moveTo(pos);

    while(stepperZ1.distanceToGo() != 0 || stepperZ2.distanceToGo() != 0) {
      stepperZ1.run();
      stepperZ2.run();
    }
  }

  long uLToSteps(float microliters) {
    if (UL_PER_STEP <= 0.0f) return 0;
    return lroundf(microliters / UL_PER_STEP);
  }

  void dispense(uint8_t pump, uint16_t microliters, char* well) {

    if (strlen(well) > 0) {
      char row = tolower(well[0]);
      uint8_t col = atoi(well + 1);

      
      if (row >= 'a' && row <= 'h' && col >= 1 && col <= 12) {
        moveZMotors(ZMotorNormalPosition);
        goToCalculatedWell(row, col);
        moveZMotors(ZMotorPumpPosition);
      }
    }

    AccelStepper pumpStepper;
    switch (pump) {
      case 1:
        enableP1Motor();
        pumpStepper = stepperP1;
      break;
      
      case 2:
        enableP2Motor();
        pumpStepper = stepperP2;
      break;

      default:
      return;
    }

    long stepsNeeded = uLToSteps(microliters);

    if (stepsNeeded > 0) {
        pumpStepper.moveTo(pumpStepper.currentPosition() - stepsNeeded);
        while(pumpStepper.distanceToGo() != 0) {
          if (isEmergencyStopRequest()) emergencyStop();
          
          pumpStepper.run();
        }
    }

    moveZMotors(ZMotorNormalPosition);

    Serial.print(F("PUMP"));
    Serial.print(pump);
    Serial.print(F(":dispensed=")); Serial.print(microliters);
    Serial.println(F("uL"));
  }

  void aspirate(uint8_t pump, uint16_t microliters, char* well) {

    if (strlen(well) > 0) {
      char row = tolower(well[0]);
      uint8_t col = atoi(well + 1);

      
      if (row >= 'a' && row <= 'h' && col >= 1 && col <= 12) {
        moveZMotors(ZMotorNormalPosition);
        goToCalculatedWell(row, col);
        moveZMotors(ZMotorPumpPosition);
      }
    }

    AccelStepper pumpStepper;
    switch (pump) {
      case 1:
        enableP1Motor();
        pumpStepper = stepperP1;
      break;
      
      case 2:
        enableP2Motor();
        pumpStepper = stepperP2;
      break;

      default:
      return;
    }

    long stepsNeeded = uLToSteps(microliters);

    pumpStepper.moveTo(pumpStepper.currentPosition() + stepsNeeded);

    while(pumpStepper.distanceToGo() != 0) {
      if (isEmergencyStopRequest()) emergencyStop();
      
      pumpStepper.run();
    }

    moveZMotors(ZMotorNormalPosition);

    Serial.print(F("PUMP"));
    Serial.print(pump);
    Serial.print(F(":aspirated=")); Serial.print(microliters);
    Serial.println(F("uL"));
  }

  void loop() {
    checkFaults();
    parse_commands(); 
    switches();

    uint32_t now = rtc.now().unixtime();
    if (now != lastSchedulerRun) {
      processActions();
      lastSchedulerRun = now;
    }
    autoDisableMotors();

    long currentL = stepperL.currentPosition();
    long currentR = stepperR.currentPosition();

    if (currentL != lastL || currentR != lastR) {
    Serial.print(F("POS:L=")); Serial.print(currentL);
    Serial.print(F(",R=")); Serial.print(currentR);
    Serial.print(F(",Z1=")); Serial.print(stepperZ1.currentPosition());
    Serial.print(F(",Z2=")); Serial.println(stepperZ2.currentPosition());
    lastL = currentL;
    lastR = currentR;
}
  }

  void parse_commands() {

    if (Serial.available() > 0) {
      char received[96];
      char* tokens[9];
      
      size_t len = Serial.readBytesUntil('\n', received, sizeof(received) - 1);
      if (len == 0) return;

      received[len] = '\0';
      
      uint8_t count = 0;
      char* tok = strtok(received, " ");

      while (tok && count < 8) {
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
        calibrate();
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
      // CREATE_ACTION <actionType>* <pump1>* <pump2>* <amount>* <frequency>* <frequencyUnit>* <start> <end>
      else if (strcmp_P(cmd, CREATE_ACTION_CMD) == 0) {
        if (count < 5) return;

        uint8_t type = atoi(tokens[1]);
        uint8_t pump1 = atoi(tokens[2]);
        uint8_t pump2 = atoi(tokens[3]);
        uint16_t amount = atoi(tokens[4]);
        uint16_t frequency = atoi(tokens[5]);
        uint8_t unit = atoi(tokens[6]);
        uint32_t start = 0;
        uint32_t end = 0;

        if (count >= 9) {
          start = strtoul(tokens[7], nullptr, 10);
          end = strtoul(tokens[8], nullptr, 10);
        }

        createAction(type, pump1, pump2, amount, frequency, unit, start, end);
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

  void switches() {
    static bool z1WasPressed = false;
    if(digitalRead(limitSwitchZ1) == LOW) {
      if(!z1WasPressed) {  
        Serial.println(F("LIMIT_PRESSED:AXIS=Z1"));
        z1WasPressed = true;
      }
      stepperZ1.setCurrentPosition(stepperZ1.currentPosition());
      stepperZ2.setCurrentPosition(stepperZ2.currentPosition());
    } else {
      if (z1WasPressed) {
          Serial.println(F("LIMIT_RELEASED:AXIS=Z1"));
          z1WasPressed = false;
      }
    }

    static bool z2WasPressed = false;
    if(digitalRead(limitSwitchZ2) == LOW) {
      if(!z2WasPressed) {
        Serial.println(F("LIMIT_PRESSED:AXIS=Z2"));
        z2WasPressed = true;
      }
      stepperZ1.setCurrentPosition(stepperZ1.currentPosition());
      stepperZ2.setCurrentPosition(stepperZ2.currentPosition());
    } else {
      if (z2WasPressed) {
          Serial.println(F("LIMIT_RELEASED:AXIS=Z2"));
          z2WasPressed = false;
      }
    }
    
    
    static bool lWasPressed = false;
    if(digitalRead(limitSwitchL) == LOW) {
      if (!lWasPressed) {
        Serial.println(F("LIMIT_PRESSED:AXIS=L"));
        lWasPressed = true;
      }
      stepperL.stop();
      stepperL.setCurrentPosition(stepperL.currentPosition());
    }
    else {
      if (lWasPressed) {
          Serial.println(F("LIMIT_RELEASED:AXIS=L"));
          lWasPressed = false;
      }
    }
    
    static bool rWasPressed = false;
    if(digitalRead(limitSwitchR) == LOW) {
      if(!rWasPressed) {
        Serial.println(F("LIMIT_PRESSED:AXIS=R"));
        rWasPressed = true;
      }
      stepperR.stop();
      stepperR.setCurrentPosition(stepperR.currentPosition());
    } else {
      if (rWasPressed) {
          Serial.println(F("LIMIT_RELEASED:AXIS=R"));
          rWasPressed = false;
      }
    }
  }
  
  void goToOrigin() {

    moveZMotors(ZMotorNormalPosition);

    enableLMotor();
    enableRMotor();
    stepperL.moveTo(0); 
    stepperR.moveTo(0); 
    
    while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
      stepperR.run();
      stepperL.run();
    }

    saveCurrentWell(WELL_HOME);
    savePositions();
  }

  void goToHardcodedWells(char row, uint8_t column) {  

    if(eStopRequested) {
      eStopRequested = false;  
      return;
    }

    moveZMotors(ZMotorNormalPosition);
 
    switch(row) {
        case 'a':
          switch(column) {
            
            case 1: 
              moveToWell(-4.125, -30.375, "a1"); // Motor L, Motor R, Well name
            break;

            case 5:
              moveToWell(17.375, -44.625, "a5"); // Motor L, Motor R, Well name
            break;

            case 7:
              moveToWell(25.625, -51.375, "a7"); // Motor L, Motor R, Well name
            break;

            case 11:
              moveToWell(43.625, -67.875, "a11"); // Motor L, Motor R, Well name
            break;

            default:
            Serial.println(F("Invalid column for row A"));
            break;
          } 
        break;

        case 'b':
          switch(column) {

            case 2: 
              moveToWell(5.125, -30.375, "b2"); // Motor L, Motor R, Well name
            break;

            case 6:
              moveToWell(23.75, -45, "b6"); // Motor L, Motor R, Well name
            break;

            case 8:
              moveToWell(31.625, -52.125, "b8"); // Motor L, Motor R, Well name
            break;

            case 12:
              moveToWell(50.375, -70.875, "b12"); // Motor L, Motor R, Well name
            break;

            default:
            Serial.println(F("Invalid column for row B"));
            break;
          } 
        break;

        case 'c':
          switch(column) {
            
            case 1: 
              moveToWell(2.125, -20.625, "c1"); // Motor L, Motor R, Well name
            break;

            case 5: 
              moveToWell(21.5, -38.5, "c5"); // Motor L, Motor R, Well name
            break;

            case 7:
              moveToWell(29.375, -45.625, "c7"); // Motor L, Motor R, Well name
            break;

            case 11:
              moveToWell(46.125, -62.375, "c11"); // Motor L, Motor R, Well name
            break;

            default:
            Serial.println(F("Invalid column for row C"));
            break;
          } 
        break;

        case 'd' :
        switch(column) {

            case 2: 
              moveToWell(11.125, -21.875, "d2"); // Motor L, Motor R, Well name
            break;

            case 6:
              moveToWell(28.875, -39.125, "d6"); // Motor L, Motor R, Well name
            break;

            case 8:
              moveToWell(36.375, -46.625, "d8"); // Motor L, Motor R, Well name
            break;

            case 12:
              moveToWell(54.75, -65.75, "d12"); // Motor L, Motor R, Well name        
            break;

            default:
            Serial.println(F("Invalid column for row D"));
            break;
        }
        break;

        case 'e' :
        switch(column) {
            
            case 1: 
              moveToWell(10.5, -10.25, "e1"); // Motor L, Motor R, Well name
            break;

            case 5:
              moveToWell(27.75, -32.75, "e5"); // Motor L, Motor R, Well name
            break;

            case 7:
              moveToWell(34.875, -40.625, "e7"); // Motor L, Motor R, Well name
            break;

            case 11:
              moveToWell(51, -57.5, "e11"); // Motor L, Motor R, Well name        
            break;

            default:
            Serial.println(F("Invalid column for row E"));
            break;
        }
        break;

        case 'f' :
        switch(column) {

            case 2: 
              moveToWell(18.75, -14.5, "2"); // Motor L, Motor R, Well name
            break;

            case 6:
              moveToWell(34.5, -34.75, "f6"); // Motor L, Motor R, Well name
            break;

            case 8:
              moveToWell(40.875, -42.625, "f8"); // Motor L, Motor R, Well name
            break;

            case 12:
              moveToWell(59.25, -61.75, "f12"); // Motor L, Motor R, Well name        
            break;

            default:
            Serial.println(F("Invalid column for row F"));
            break;
        }
        break;

        case 'g' :
        switch(column) {
            
            case 1: 
              moveToWell(19.5, -4.25, "g1"); // Motor L, Motor R, Well name
            break;

            case 5: 
              moveToWell(33, -27.5, "g5"); // Motor L, Motor R, Well name
            break;

            case 7:
              moveToWell(40.5, -36.5, "g7"); // Motor L, Motor R, Well name
            break;

            case 11:
              moveToWell(55.875, -53.375, "g11"); // Motor L, Motor R, Well name        
            break;

            default:
            Serial.println(F("Invalid column for row G"));
            break;
        }
        break;

        case 'h' :
        switch(column) {
          
            case 2:
              moveToWell(27.25, -9.25, "h2"); // Motor L, Motor R, Well name
            break;

            case 6:
              moveToWell(40, -30.25, "h6"); // Motor L, Motor R, Well name
            break;

            case 8:
              moveToWell(46.75, -39.25, "h8"); // Motor L, Motor R, Well name
            break;

            case 12:
              moveToWell(66.25, -58.75, "h12"); // Motor L, Motor R, Well name
            break;

            default:
            Serial.println(F("Invalid column for row H"));
            break;
        }

        break;  

      default:
        Serial.println(F("Invalid row"));
      break;
    }
  }

  int8_t calibrate() {
    moveZMotors(ZMotorNormalPosition);

    disableAllMotors();

    if(eStopRequested) {
      eStopRequested = false;  
      return -1;
    }

    enableRMotor();

    stepperR.setSpeed(10 * currentMicrosteps);
    while(digitalRead(limitSwitchR) == HIGH){
      stepperR.runSpeed();
    }

    stepperR.setSpeed(-10 * currentMicrosteps);

    stepperR.moveTo(stepperR.currentPosition() - 500);
    while(stepperR.distanceToGo() != 0) {
      stepperR.run();
    }

    stepperR.setCurrentPosition(0);

    enableLMotor();

    stepperL.setSpeed(-10 * currentMicrosteps);
    while(digitalRead(limitSwitchL) == HIGH){
      stepperL.runSpeed();
    }

    stepperL.setSpeed(10 * currentMicrosteps);

    stepperL.setCurrentPosition(0);

    disableLMotor();

    stepperR.move(345);

    while (stepperR.distanceToGo() != 0) {
      stepperR.run();
    }

    stepperR.setCurrentPosition(0);

    saveCurrentWell(WELL_HOME);

    disableAllMotors();

    return 1;
  }

  void moveToWell(long moveL, long moveR, char* wellName) {
    enableLMotor();
    enableRMotor();

    stepperL.moveTo(moveL * currentMicrosteps); 
    stepperR.moveTo(moveR * currentMicrosteps); 
    
    while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
      stepperR.run();
      stepperL.run();
    }

    saveCurrentWell(wellName);
    savePositions();
    printCurrentWell();
  }


  void setSlowMovementSpeed() {
    stepperL.setMaxSpeed(200 * currentMicrosteps);
    stepperR.setMaxSpeed(200 * currentMicrosteps);
    stepperL.setAcceleration(100 * currentMicrosteps);
    stepperR.setAcceleration(100 * currentMicrosteps);
  }

  void setNormalMovementSpeed() {
    stepperL.setMaxSpeed(1000 * currentMicrosteps);
    stepperR.setMaxSpeed(1000 * currentMicrosteps);
    stepperL.setAcceleration(500 * currentMicrosteps);
    stepperR.setAcceleration(500 * currentMicrosteps);
  }

  void setSlowPumpSpeed() {
    stepperP1.setMaxSpeed(200);
    stepperP2.setMaxSpeed(200);
    stepperP1.setAcceleration(100);
    stepperP2.setAcceleration(100);
  }

  void setNormalPumpSpeed() {
    stepperP1.setMaxSpeed(1000);
    stepperP2.setMaxSpeed(1000);
    stepperP1.setAcceleration(500);
    stepperP2.setAcceleration(500);
  }

  void enableAllMotors() {
    if (areMotorsCurrentlyEnabled()) return;
    enableZMotors();
    enableLMotor();
    enableRMotor();
    enableP1Motor();
    enableP2Motor();
  }

  void enableZMotors() {
    digitalWrite(ena[0], LOW);
    digitalWrite(ena[3], LOW);

    ZMotorsCurrentlyEnabled = true;     
    lastMotorActivityTime = millis(); 
  }

  void enableP1Motor() {
    digitalWrite(ena[4], LOW);

    P1MotorCurrentlyEnabled = true;
    lastMotorActivityTime = millis(); 
  }

  void enableP2Motor() {
    digitalWrite(ena[5], LOW);

    P2MotorCurrentlyEnabled = true;
    lastMotorActivityTime = millis(); 
  }

  void enableLMotor() {
    digitalWrite(ena[1], LOW);

    LMotorCurrentlyEnabled = true;
    lastMotorActivityTime = millis(); 
  }

  void enableRMotor() {
    digitalWrite(ena[2], LOW);

    RMotorCurrentlyEnabled = true;
    lastMotorActivityTime = millis(); 
  }

  void disableZMotors() {
    digitalWrite(ena[0], HIGH);
    digitalWrite(ena[3], HIGH);

    ZMotorsCurrentlyEnabled = false;
    lastMotorActivityTime = millis(); 
  }

  void disableP1Motor() {
    digitalWrite(ena[4], HIGH);

    P1MotorCurrentlyEnabled = false;
    lastMotorActivityTime = millis(); 
  }

  void disableP2Motor() {
    digitalWrite(ena[5], HIGH);

    P2MotorCurrentlyEnabled = false;
    lastMotorActivityTime = millis(); 
  }

  void disableLMotor() {
    digitalWrite(ena[1], HIGH);

    LMotorCurrentlyEnabled = false;
    lastMotorActivityTime = millis(); 
  }

  void disableRMotor() {
    digitalWrite(ena[2], HIGH);

    RMotorCurrentlyEnabled = false;
    lastMotorActivityTime = millis(); 
  }


  void disableAllMotors() {
    disableLMotor();
    disableRMotor();
    disableZMotors();
    disableP1Motor();
    disableP2Motor();
  }

  void autoDisableMotors() {
    // Check if any motor is moving
    bool isMoving = (stepperL.distanceToGo() != 0 || 
                    stepperR.distanceToGo() != 0 || 
                    stepperZ1.distanceToGo() != 0 || 
                    stepperZ2.distanceToGo() != 0 ||
                    stepperP1.distanceToGo() != 0 ||
                    stepperP2.distanceToGo() != 0);
    
    if(!isMoving) {
      if(areMotorsCurrentlyEnabled() && (millis() - lastMotorActivityTime > MOTOR_TIMEOUT)) {
        disableAllMotors();
      }
    }
  }

  bool areMotorsCurrentlyEnabled() {
    return ZMotorsCurrentlyEnabled || LMotorCurrentlyEnabled || RMotorCurrentlyEnabled ||
           P1MotorCurrentlyEnabled || P2MotorCurrentlyEnabled;
  }

  bool isEmergencyStopRequest() {
    if (Serial.available() < 0) return false;
    char c = Serial.read();
    if(c != 's') return false;

    return true;
  }

  void emergencyStop() {
    stepperL.stop();
    stepperR.stop();
    stepperZ1.stop();
    stepperZ2.stop();
    stepperP1.stop();
    stepperP2.stop();
    disableAllMotors();
    lastMotorActivityTime = 0;
    eStopRequested = true;
    Serial.println(F("WARNING:EMERGENCY_STOP,Motors disabled by user"));
  }

  void interruptibleDelay(unsigned long ms) {
    unsigned long startTime = millis();
    while(millis() - startTime < ms) {
      if(Serial.available() > 0) {
        char c = Serial.read();
        if(c == 's') {
          emergencyStop();
          return;
        }
      }
      delay(10);  
    }
  }

  void checkFaults() {
    static bool faultLatched = false;

    if (digitalRead(faultR) == LOW || digitalRead(faultL) == LOW) {
      if (!faultLatched) {
        Serial.println(F("ERROR:DRIVER_FAULT,Motor driver nFAULT triggered"));
        faultLatched = true;
      }
      emergencyStop();
    } else {
      faultLatched = false;
    }
  }

  void enterHomingMode(uint16_t homingSpeedL, uint16_t homingSpeedR) {
    stepperL.stop();
    stepperR.stop();

    stepperL.setSpeed(homingSpeedL);
    stepperR.setSpeed(homingSpeedR);
  }

  void exitHomingMode() {
    stepperL.setSpeed(0);
    stepperR.setSpeed(0);

    setNormalMovementSpeed();
  }

  void savePositions() {
    EEPROM.put(POS_ADDR_L, (int16_t)stepperL.currentPosition());
    EEPROM.put(POS_ADDR_R, (int16_t)stepperR.currentPosition());
    EEPROM.put(POS_ADDR_Z1, (int16_t)stepperZ1.currentPosition());
    EEPROM.put(POS_ADDR_Z2, (int16_t)stepperZ2.currentPosition());

    byte ok = 123;
    EEPROM.put(16, ok);
  }

  bool loadPositions() {
    byte ok;
    EEPROM.get(16, ok);

    if (ok != 123) {
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

    Serial.print(F("POS:L=")); Serial.print(L);
    Serial.print(F(",R=")); Serial.print(R);
    Serial.print(F(",Z1=")); Serial.print(Z1);
    Serial.print(F(",Z2=")); Serial.println(Z2);

    return true;
  }

  void saveCalibration() {
    if (currentState == SETUP) return;

    int addr = EEPROM_CAL_BASE;
    byte magic = MAGIC;
    EEPROM.put(addr, magic);  addr += sizeof(magic);

    EEPROM.put(addr, calCount);  addr += sizeof(uint8_t);

    for (uint8_t i = 0; i < calCount; i++) {
      EEPROM.put(addr, calX[i]);  addr += sizeof(float);
      EEPROM.put(addr, calY[i]);  addr += sizeof(float);
      EEPROM.put(addr, calL[i]);  addr += sizeof(float);
      EEPROM.put(addr, calR[i]);  addr += sizeof(float);
    }

    Serial.println(F("Calibration saved to EEPROM"));
    Serial.print(F("Saved ")); Serial.print(calCount); Serial.println(F(" points"));
  }

  void loadCalibration() {
    int addr = EEPROM_CAL_BASE;
    byte magic;
    EEPROM.get(addr, magic);  addr += sizeof(magic);
    if (magic != MAGIC) {
      Serial.println(F("No valid calibration in EEPROM"));
      return;
    }

    EEPROM.get(addr, calCount);  addr += sizeof(uint8_t);
    if (calCount < 0 || calCount > MAX_CAL) {
      Serial.println(F("Corrupt point count in EEPROM"));
      calCount = 0;
      return false;
    }

    for (uint8_t i = 0; i < calCount; i++) {
      EEPROM.get(addr, calX[i]);  addr += sizeof(float);
      EEPROM.get(addr, calY[i]);  addr += sizeof(float);
      EEPROM.get(addr, calL[i]);  addr += sizeof(float);
      EEPROM.get(addr, calR[i]);  addr += sizeof(float);
    }

    solveMapping();
    return;
  }

  bool isInvalidWell(char row, uint8_t col) {
    return (col < 1 || col > 12 || row < 'a' || row > 'h');
  }

  void wellToXY(char row, uint8_t col, float &x, float &y) {
    row = tolower(row);

    if (isInvalidWell(row, col)) {
    Serial.print(F("ERROR:INVALID_WELL,"));
    Serial.print(row); Serial.println(col);
    return;
    }

    uint8_t r = row - 'a';
    
    x =  (col - 1) * WELL_DX;
    y =  r * WELL_DY;
  }

  void XYToWell(float x, float y, char& row, uint8_t& col) {
    uint8_t rowInt = y / WELL_DY;
    col = x / WELL_DX;
    row = 'a' + rowInt;
  }


  void xyToAngles(float x, float y, float &Ldeg, float &Rdeg) {
    if (!mapReady) {
      Serial.println(F("Angle map not ready!"));
      Ldeg = 0; Rdeg = 0;
      return;
    }
    // Basis vector for quadratic model
    float b[TERMS] = { 1.0f, x, y, x*x, x*y, y*y, x*x*x, x*x*y, x*y*y, y*y*y };
    Ldeg = dot10(ML, b);
    Rdeg = dot10(MR, b);
  }

  void recordCalibrationPoint(char row, uint8_t col) {
    if (calCount >= MAX_CAL) {
        Serial.print(F("ERROR:CAL_FULL,Maximum ")); 
        Serial.print(MAX_CAL); 
        Serial.println(F(" calibration points reached"));
        return;
    }

    float x = 0;
    float y = 0;
    wellToXY(row, col, x, y);

    calX[calCount] = x;
    calY[calCount] = y;

    calL[calCount] = stepsToDegrees(stepperL.currentPosition());
    calR[calCount] = stepsToDegrees(stepperR.currentPosition());

    Serial.print(F("CAL_REC:"));
    Serial.print(F("Name=")); Serial.print(row); Serial.print(col);
    Serial.print(F(",X=")); Serial.print(x);
    Serial.print(F(",Y=")); Serial.print(y);
    
    Serial.print(F("CAL_COUNT:")); Serial.println(++calCount);
  }


  inline float dot10(const float a[TERMS], const float b[TERMS]) {
    return a[0]*b[0] + a[1]*b[1] + a[2]*b[2] + a[3]*b[3] + a[4]*b[4]
        + a[5]*b[5] + a[6]*b[6] + a[7]*b[7] + a[8]*b[8] + a[9]*b[9];
  }

  bool solve10(float A[TERMS][TERMS], float b[TERMS], float x[TERMS]) {
    float M[TERMS][TERMS+1];
    for (uint8_t i=0;i<TERMS;i++){
      for (uint8_t j=0;j<TERMS;j++) M[i][j] = A[i][j];
      M[i][TERMS] = b[i];
    }
    for (uint8_t col=0; col<TERMS; col++) {
      uint8_t piv = col;
      float best = fabs(M[piv][col]);
      for (uint8_t r=col+1; r<TERMS; r++) {
        float v = fabs(M[r][col]);
        if (v > best) { best = v; piv = r; }
      }
      if (best < 1e-9) return false;
      if (piv != col) {
        for (uint8_t c=col; c<=TERMS; c++) {
          float tmp = M[col][c];
          M[col][c] = M[piv][c];
          M[piv][c] = tmp;
        }
      }
      float div = M[col][col];
      for (uint8_t c=col; c<=TERMS; c++) M[col][c] /= div;
      for (uint8_t r=0; r<TERMS; r++) {
        if (r == col) continue;
        float f = M[r][col];
        for (uint8_t c=col; c<=TERMS; c++) M[r][c] -= f * M[col][c];
      }
    }
    for (uint8_t i=0;i<TERMS;i++) x[i] = M[i][TERMS];
    return true;
  }

  bool solveMapping() {
    if (calCount < TERMS) {
      Serial.print(F("ERROR:SOLVE_INSUFFICIENT,Need at least "));
      Serial.print(TERMS);
      Serial.println(F(" calibration points"));
      return false;
    }

    // Normal equations: (A^T A) c = (A^T y)
    // A rows are basis b = [1, x, y, x^2, x*y, y^2]
    static float ATA[TERMS][TERMS] = {0};
    static float ATyL[TERMS] = {0};
    static float ATyR[TERMS] = {0};

    for (uint8_t i=0; i<calCount; i++) {
      float x = calX[i], y = calY[i];
      float b[TERMS] = { 1.0f, x, y, x*x, x*y, y*y, x*x*x, x*x*y, x*y*y, y*y*y };
      float L = calL[i];
      float R = calR[i];

      // Accumulate ATA = Σ b*b^T
      for (uint8_t r=0; r<TERMS; r++) {
        for (uint8_t c=0; c<TERMS; c++) {
          ATA[r][c] += b[r]*b[c];
        }
      }
      // Accumulate ATy = Σ b*y
      for (uint8_t k=0; k<TERMS; k++) {
        ATyL[k] += b[k]*L;
        ATyR[k] += b[k]*R;
      }
    }

    float MLtmp[TERMS], MRtmp[TERMS];
    bool okL = solve10(ATA, ATyL, MLtmp);
    bool okR = solve10(ATA, ATyR, MRtmp);

    if (!okL || !okR) {
      Serial.println(F("ERROR:SOLVE_SINGULAR,Calibration matrix singular - add more spread-out points"));
      mapReady = false;
      return false;
    }

    // Commit
    for (uint8_t i=0;i<TERMS;i++) { ML[i] = MLtmp[i]; MR[i] = MRtmp[i]; }
    mapReady = true;
    saveCalibration();

    Serial.println(F("=== MAPPING SOLVED (quadratic least-squares) ==="));
    printCalibrationPoints();

    return true;
  }

  float clampZero(float v, float eps = 5e-4f) {
      return fabs(v) < eps ? 0.0f : v;
  }

  void printCalibrationPoints() {
    Serial.print(F("CAL_COUNT:")); Serial.println(calCount);
    float maxErrL = 0, maxErrR = 0, rmsL = 0, rmsR = 0;
    for (uint8_t i=0; i<calCount; i++) {
      float x = calX[i], y = calY[i];
      char row;
      uint8_t col;
      XYToWell(calX[i], calY[i], row, col);
      float b[TERMS] = { 1.0f, x, y, x*x, x*y, y*y, x*x*x, x*x*y, x*y*y, y*y*y };
      float predL = dot10(ML, b);
      float predR = dot10(MR, b);
      float errL  = calL[i] - predL;
      float errR  = calR[i] - predR;
      rmsL += errL*errL; rmsR += errR*errR;
      if (fabs(errL) > maxErrL) maxErrL = fabs(errL);
      if (fabs(errR) > maxErrR) maxErrR = fabs(errR);
      Serial.print(F("CAL_PT:"));
      Serial.print(F("Name=")); Serial.print(row); Serial.print(col);
      Serial.print(F(",X=")); Serial.print(calX[i], 2);
      Serial.print(F(",Y=")); Serial.print(calY[i], 2);


      if (!mapReady) {
        Serial.println();
        continue;
      }
      
      Serial.print(F(",ErrorLeft=")); Serial.print(clampZero(errL), 3);
      Serial.print(F(",ErrorRight=")); Serial.println(clampZero(errR), 3);
    }
    rmsL = sqrtf(rmsL / calCount);
    rmsR = sqrtf(rmsR / calCount);

    if (!mapReady) return;

    Serial.print(F("RMS:L=")); Serial.print(rmsL, 3);
    Serial.print(F(",R=")); Serial.print(rmsR, 3);
    Serial.print(F(",MAX_L=")); Serial.print(maxErrL, 3);
    Serial.print(F(",MAX_R=")); Serial.println(maxErrR, 3);
  }

  long degToSteps(float deg) {
    float stepsPerRev = 200.0 * currentMicrosteps;
    return lroundf(deg * (stepsPerRev / 360.0));
  }

  float stepsToDegrees(long steps) {
    float stepsPerRev = 200.0f * currentMicrosteps;
    return steps * (360.0f / stepsPerRev);
  }

  uint8_t wellNameToIndex(char row, uint8_t column) {
      return (row - 'a') * 12 + (column - 1);
  }

  void wellIndexToRowCol(uint8_t wellIndex, char& row, uint8_t& col) {
      row = 'A' + (wellIndex / 12);
      col = (wellIndex % 12) + 1;
  }

  void goToCalculatedWell(char row, uint8_t col) {
    if (!mapReady) {
      Serial.println(F("ERROR:MAP_NOT_READY,Run z solve before moving to wells"));
      return;
    }

    moveZMotors(ZMotorNormalPosition);

    enableLMotor();
    enableRMotor();

    float x = 0;
    float y = 0;
    wellToXY(row, col, x, y);

    float Ldeg = 0;
    float Rdeg = 0;
    xyToAngles(x, y, Ldeg, Rdeg);

    long Lsteps = degToSteps(Ldeg);
    long Rsteps = degToSteps(Rdeg);

    stepperL.moveTo(Lsteps);
    stepperR.moveTo(Rsteps);

    while (stepperL.distanceToGo() != 0 || stepperR.distanceToGo() != 0) {
        stepperL.run();
        stepperR.run();
    }

    uint8_t wellIndex = wellNameToIndex(row, col);

    saveCurrentWell(wellIndex);
    savePositions();
    printCurrentWell();
  }

  void saveCurrentWell(uint8_t wellIndex) {  
    EEPROM.put(EEPROM_WELL_BASE, wellIndex);
  }

  void loadCurrentWell() {
    wellIndex = EEPROM.read(EEPROM_WELL_BASE);

    printCurrentWell();
  }

  void printCurrentWell() {
    char row;
    uint8_t col;

    float x = 0;
    float y = 0;
    float Ldeg = 0;
    float Rdeg = 0;

    if (wellIndex != WELL_HOME) {
      wellIndexToRowCol(wellIndex, row, col);
      if (row < 'a' || row > 'h') return;
      if (col < 1 || col > 12) return;
      
      wellToXY(row, col, x, y);

      Ldeg = stepsToDegrees(stepperL.currentPosition());
      Rdeg = stepsToDegrees(stepperR.currentPosition());
    }

    Serial.print(F("WELL:"));
    Serial.print(F("Name="));
    if (wellIndex == WELL_HOME) Serial.print(F("HOME"));
    else { Serial.print(row); Serial.print(col); }
    Serial.print(F(",X=")); Serial.print(x);
    Serial.print(F(",Y=")); Serial.print(y);
    Serial.print(F(",L=")); Serial.print(Ldeg);
    Serial.print(F(",R=")); Serial.println(Rdeg);
  }

  void printStepSize() {
    Serial.print(F("STEP_SIZE:"));
    Serial.print(times_x10 / 10);
    Serial.print(F("."));
    Serial.println(times_x10 % 10);
  }

  void printMicroSteps() {
    Serial.print(F("MICROSTEPS:1/")); Serial.println(currentMicrosteps);
  }

  uint16_t createAction(uint8_t type, uint8_t pump1, uint8_t pump2, uint16_t amount, uint16_t frequency, uint8_t unit, uint32_t start, uint32_t end) {
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
    Serial.println(action.id);

    return action.id;
  }

  void updateAction(uint16_t id, uint8_t type, uint8_t pump1, uint8_t pump2, uint16_t amount, uint16_t frequency, uint8_t unit, uint32_t start, uint32_t end) {
    
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

  void saveAction(Action& action, uint8_t slot) {
    EEPROM.put(EEPROM_ACTIONS_ADDR + (slot) * sizeof(Action), action);
  }

  void loadActionsSafe() {
    byte magic;
    EEPROM.get(EEPROM_ACTIONS_MAGIC_ADDR, magic);

    if (magic != MAGIC) {
      initializeEmptyActions();
      return;
    }

    loadActions();
    loadActionsState();
  }

  void initializeEmptyActions() {
    for (uint8_t i = 0; i < MAX_ACTIONS_TOTAL; i++) {
      actions[i].id = 0;
      actions[i].enabled = 0;
    }

    for (uint8_t w = 0; w < MAX_WELLS; w++) {
      wellActions[w].count = 0;
    }

    saveActionsState();
    saveWellActions();
  }

  void loadActions() {
    int addr = EEPROM_ACTIONS_ADDR;
    for (uint8_t i = 0; i < MAX_ACTIONS_TOTAL; i++) {
      EEPROM.get(addr, actions[i]);
      if (actions[i].enabled) printAction(actions[i]);
      addr += sizeof(Action);
    }
  }

  void printAction(const Action& action) {
    Serial.print(F("ACTION:"));
    Serial.print(F("Id="));Serial.print(action.id);
    Serial.print(F(",ActionType=")); Serial.print(action.type);
    Serial.print(F(",Pump1=")); Serial.print(action.pump1);
    Serial.print(F(",Pump2=")); Serial.print(action.pump2);
    Serial.print(F(",Amount=")); Serial.print(action.amount_uL);
    Serial.print(F(",Frequency=")); Serial.print(action.frequency);
    Serial.print(F(",Unit=")); Serial.print(action.unit);
    Serial.print(F(",Start=")); Serial.print(action.startEpoch);
    Serial.print(F(",End=")); Serial.print(action.endEpoch);
    Serial.print(F(",LastRun=")); Serial.print(action.lastRunEpoch);
    Serial.print(F(",Enabled=")); Serial.println(action.enabled);
  }

  void saveWellAction(WellAction& wa, uint8_t wellIndex) {
    EEPROM.put(EEPROM_WELL_ACTIONS_ADDR + (uint32_t)wellIndex * sizeof(WellAction), wa);
  }

  void saveWellActions() {
    int addr = EEPROM_WELL_ACTIONS_ADDR;
    for (uint8_t i = 0; i < MAX_WELLS; i++) {
      EEPROM.put(addr, wellActions[i]);
      addr += sizeof(WellAction);
    }
  }

  void loadWellActions() {
    int addr = EEPROM_WELL_ACTIONS_ADDR;
    for (uint8_t i = 0; i < MAX_WELLS; i++) {
      EEPROM.get(addr, wellActions[i]);
      if (wellActions[i].count > 0) printWellAction(wellActions[i], i);
      addr += sizeof(WellAction);
    }
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

  void saveActionsState() {
    EEPROM.put(EEPROM_NEXT_ACTION_ID_ADDR, nextActionId);
    EEPROM.put(EEPROM_ACTION_COUNT_ADDR, actionCount);

    byte magic = MAGIC;
    EEPROM.put(EEPROM_ACTIONS_MAGIC_ADDR, magic);
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

  void linkActionByMask(uint16_t actionId, const uint8_t mask[12]) {
    for (uint8_t well = 0; well < MAX_WELLS; well++) {
      uint8_t byteIdx = well / 8;
      uint8_t bitIdx  = well % 8;

      if (mask[byteIdx] & (1 << bitIdx)) {
        linkActionToWell(actionId, well);
      }
    }
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

  uint32_t unitToSeconds(uint8_t unit) {
    switch (unit) {
      case 0: return 60;           // Test only Minutes
      case 1: return 3600;        // Hour
      case 2: return 86400;      // Day
      default: return 0;
    }
  }

  void processActions() {
    uint32_t now = rtc.now().unixtime();

    for (uint16_t i = 0; i < MAX_ACTIONS_TOTAL; i++) {
      Action &action = actions[i];

      if (!action.enabled) continue;

      if (now < action.startEpoch) continue;
      if (action.endEpoch != 0 && now > action.endEpoch) continue;

      uint32_t period = action.frequency * unitToSeconds(action.unit);
      if (period == 0) continue;

      uint32_t nextRun = action.lastRunEpoch + period;

      if (action.lastRunEpoch == 0) nextRun = action.startEpoch;

      if (now < nextRun) continue;

      executeAction(action);
      action.lastRunEpoch = now;
      saveAction(action, i);
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

      Serial.print(F("Executing Action")); Serial.println(action.id);

      char row;
      uint8_t col;
      wellIndexToRowCol(well, row, col);

      char wellName[4];
      wellName[0] = row;
      itoa(col, &wellName[1], 10);


      switch (action.type) {
        case 0:
          aspirate(action.pump1, action.amount_uL, wellName);
          break;
        case 1:
          dispense(action.pump1, action.amount_uL, wellName);
          break;
        case 2:
        {
          char nxtRow = row + 1;
          uint8_t nxtCol = col + 1;
          char outWellName[4];
          outWellName[0] = nxtRow;
          itoa(nxtCol, &outWellName[1], 10);

          if (isInvalidWell(nxtRow, nxtCol)) return;

          aspirate(action.pump2, action.amount_uL, outWellName);
          dispense(action.pump1, action.amount_uL, wellName);
          aspirate(action.pump2, action.amount_uL, outWellName);
          dispense(action.pump1, action.amount_uL, wellName);
          break;
        }
      }
    }
  }