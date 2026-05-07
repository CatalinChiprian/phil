/* Created by Victoria Shvets
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


  const uint8_t MAX_WELLS = 96;
  const uint8_t MAX_ACTIONS_PER_WELL = 16;
  const uint16_t MAX_ACTIONS_TOTAL = 128;
  const uint8_t INVALID = 0xFF;

  enum State : uint8_t {
    SETUP,
    RUNNING
  } currentState;

  enum ActionType : uint8_t {
    ASPIRATE,
    DISPENSE
  };

  enum TimeUnit : uint8_t {
    HOUR,
    DAY,
  };

  struct Action {
    uint16_t id;
    ActionType type;
    uint8_t pump;
    uint16_t amount_uL;
    uint16_t frequency;
    TimeUnit unit;
    uint32_t startEpoch;
    uint32_t endEpoch;
    bool enabled;
  };

  struct WellAction {
    uint8_t actionIds[MAX_ACTIONS_PER_WELL];
    uint8_t count;
  };

  Action actions[MAX_ACTIONS_TOTAL];
  WellAction wellActions[MAX_WELLS];

  uint8_t actionCount = 0;
  uint16_t nextActionId = 1;

  int myMICROS = 1;
  char Sttngs[][3] = {
    {LOW,  LOW, LOW}, // Full step
    {HIGH,  LOW, LOW}, // Half step
    {LOW, HIGH,  LOW}, // 1/4 step
    {HIGH,  HIGH,  LOW}, // 1/8 step
    {LOW, LOW, HIGH}, // 1/16 step
    {HIGH,  HIGH,  HIGH}
  };

  const float WELL_DX = 9.0;
  const float WELL_DY = 9.0;

  const int MAX_CAL = 32;
  float calX[MAX_CAL], calY[MAX_CAL], calL[MAX_CAL], calR[MAX_CAL];
  uint8_t calCount = 0;

  const uint8_t TERMS = 10;
  float ML[TERMS] = {0};
  float MR[TERMS] = {0};
  bool mapReady = false;

  uint8_t wellIndex;// 0–N‑1 = wells, 255 = HOME

  int MICROoptions[] = {1, 2, 4, 8, 16, 32};

  int M1 = 25; 
  int M2 = 26; 
  int M3 = 27; 

  int P1 = 22;
  int P2 = 23;
  int P3 = 24;

  int ena[] = {44, 47, 50, 53, 13, 10};
  int step[] = {43, 46, 49, 52, 12, 9};
  int dir[] = {42, 45, 48, 51, 11, 8};   

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

  int limitSwitchL = 31; // Target Limit Switch L
  int limitSwitchR = 30; // Target Limit Switch R
  int limitSwitchZ1 = 33; // Target Limit Switch Z
  int limitSwitchZ2 = 32; // Target Limit Switch Z

  int faultR = 37;
  int faultL = 39;

  int microIndex = 3; // 0=full, 1=half, 2=1/4, 3=1/8, 4=1/16, 5=1/32
  int currentMicrosteps = MICROoptions[microIndex]; 

  int16_t times_x10 = 1;
  const long steps = 4 * currentMicrosteps;

  const int16_t ZMotorPumpPosition = -2496;
  const int16_t ZMotorNormalPosition = -384;

  unsigned long lastMotorActivityTime = 0;
  bool ZMotorsCurrentlyEnabled = false;
  bool LMotorCurrentlyEnabled = false;
  bool RMotorCurrentlyEnabled = false;
  bool P1MotorCurrentlyEnabled = false;
  bool P2MotorCurrentlyEnabled = false;
  const unsigned long MOTOR_TIMEOUT = 5000;
  bool emergencyStopRequested = false;

  void setup() {
    Serial.begin(9600);
    Wire.begin();

    
    if (!rtc.begin()) {
        Serial.println("ERROR: RTC not found");
        while (1);
    }

    
    if (rtc.lostPower()) {
      Serial.println("RTC lost power, setting time...");

      rtc.adjust(DateTime(F(__DATE__), F(__TIME__)));
    }

    enableAllMotors();

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
  
    if (!loadPositions()) {
      calibrate();
    }

    loadCurrentWell();

    loadCalibration();

    loadActionsSafe();

    printStepSize();

    emergencyStopRequested = false;
    currentState = RUNNING;
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

  void dispense(int pump, int microliters, String well) {

    if (well.length() > 0) {
      char row = tolower(well.charAt(0));
      int col = well.substring(1).toInt();

      
      if (row >= 'a' || row <= 'h' || col >= 1 || col <= 12) {
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
        while(pumpStepper.distanceToGo() != 0) pumpStepper.run();
    }

    moveZMotors(ZMotorNormalPosition);

    Serial.print("PUMP");
    Serial.print(pump);
    Serial.print(":dispensed="); Serial.print(microliters);
    Serial.println("uL");
    Serial.print("Which is ");
    Serial.print(stepsNeeded);
    Serial.println(" total steps");
  }

  void aspirate(int pump, int microliters, String well) {

    if (well.length() > 0) {
      char row = tolower(well.charAt(0));
      int col = well.substring(1).toInt();

      
      if (row >= 'a' || row <= 'h' || col >= 1 || col <= 12) {
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
      pumpStepper.run();
    }

    moveZMotors(ZMotorNormalPosition);

    Serial.print("PUMP");
    Serial.print(pump);
    Serial.print(":aspirated="); Serial.print(microliters);
    Serial.println("uL");
    Serial.print("Which is ");
    Serial.print(stepsNeeded);
    Serial.println(" total steps");
  }

  void loop() {
    checkFaults();
    basic_controls(); 
    switches(); 
    autoDisableMotors();

    long currentL = stepperL.currentPosition();
    long currentR = stepperR.currentPosition();

    if (currentL != lastL || currentR != lastR) {
    Serial.print("POS:L="); Serial.print(currentL);
    Serial.print(",R="); Serial.print(currentR);
    Serial.print(",Z1="); Serial.print(stepperZ1.currentPosition());
    Serial.print(",Z2="); Serial.println(stepperZ2.currentPosition());
    lastL = currentL;
    lastR = currentR;
}
  }

  void basic_controls() {

    if (Serial.available() > 0) {
      String received = Serial.readStringUntil('\n');
      received.trim();

      if (received.length() < 0) return;

      String tokens[7];

      int count = splitN(received, tokens, 7);

      String cmd = tokens[0];

      if (cmd == "") return;

      if (cmd == MOVE_BACKWARD_CMD) {
        moveBackward();
      }
      else if (cmd == MOVE_FORWARD_CMD) {
        moveForward();
      }
      else if (cmd == MOVE_LEFT_CMD) {
        moveLeft();
      }
      else if (cmd == MOVE_RIGHT_CMD) {
        moveRight();
      }
      else if (cmd == MOVE_UP_CMD) {
        moveUp();
      }
      else if (cmd == MOVE_DOWN_CMD) {
        moveDown();
      }
      else if (cmd == GO_HOME_CMD) {
        goToOrigin();
        disableAllMotors();
      }
      else if (cmd == INC_STEP_CMD) {
        times_x10 += 1;
        printStepSize();
      }
      else if (cmd == DEC_STEP_CMD) {
        times_x10 -= 1;
        printStepSize();
      }
      // ASPIRATE <pump>* <amount>* <well>
      else if (cmd == ASPIRATE_CMD) {
        int pump = tokens[1].toInt();
        int amount = tokens[2].toInt();

        aspirate(pump, amount, tokens[3]);
      }
      // DISPENSE <pump>* <amount>* <well>
      else if (cmd == DISPENSE_CMD) {
        int pump = tokens[1].toInt();
        int amount = tokens[2].toInt();

        dispense(pump, amount, tokens[3]);
      }
      else if (cmd == CALIBRATE_HOME_CMD) {
        calibrate();
      }
      else if (cmd == MOVE_HARD_WELL_CMD) {
        String wellStr = tokens[1];
        if (wellStr == "") return;
        char row = tolower(wellStr.charAt(0));
        int column = wellStr.substring(1).toInt(); 
        goToHardcodedWells(row, column); 
      }
      else if (cmd == MOVE_CALC_WELL_CMD) {
        String wellStr = tokens[1];
        if (wellStr == "") return;
        char row = tolower(wellStr.charAt(0));
        int column = wellStr.substring(1).toInt();  
        goToCalculatedWell(row, column); 
      }
      else if (cmd == RECORD_POINT_CMD) {
        String wellStr = tokens[1];
        if (wellStr == "") return;
        char row = tolower(wellStr.charAt(0));
        int col = wellStr.substring(1).toInt();
        recordCalibrationPoint(row, col);
      }
      else if (cmd == SOLVE_MAP_CMD) {
        solveMapping();
      }
      else if (cmd == DELETE_POINT_CMD) {
        String wellStr = tokens[1];
        if (wellStr == "") return;
        char row = tolower(wellStr.charAt(0));
        int col = wellStr.substring(1).toInt();

        float x = 0;
        float y = 0;
        wellToXY(row, col, x, y);
        
        int foundIdx = -1;
        for (int i = 0; i < calCount; i++) {
            if (fabs(calX[i] - x) < 0.1f && fabs(calY[i] - y) < 0.1f) {
                foundIdx = i;
                break;
            }
        }
        
        if (foundIdx == -1) return;

        // Shift all points after it down by one
        for (int i = foundIdx; i < calCount - 1; i++) {
            calX[i] = calX[i+1];
            calY[i] = calY[i+1];
            calL[i] = calL[i+1];
            calR[i] = calR[i+1];
        }
        calCount--;

        Serial.print("CAL_DELETED:"); Serial.print(row); Serial.print(col);
        Serial.print(",remaining="); Serial.println(calCount);
      }
      else if (cmd == CLEAR_CALIBRATION_CMD) {
        EEPROM.put(EEPROM_CAL_BASE, 0x00);
        mapReady = false;
        for (int i = 0; i < TERMS; i++) { ML[i] = 0; MR[i] = 0; }
        calCount = 0;
        Serial.println("Calibration cleared");
      }
      // CREATE_ACTION <actionId>* <actionType>* <amount>* <frequency>* <frequencyUnit>* <start> <end>
      else if (cmd == CREATE_ACTION_CMD) {
        for (String token : tokens) {
          if (token == "") return;
        }

        ActionType type = (ActionType)tokens[1].toInt();
        uint8_t pump = tokens[2].toInt();
        uint16_t amount = tokens[3].toInt();
        uint16_t frequency = tokens[4].toInt();
        TimeUnit unit = (TimeUnit)tokens[5].toInt();
        uint32_t start = 0;
        uint32_t end = 0;

        if (count > 7) {
          uint32_t start = tokens[6].toInt();
          uint32_t end = tokens[7].toInt();
        }

        createAction(type, pump, amount, frequency, unit, start, end);
      }
      // UPDATE_ACTION <actionId>* <actionType>* <amount>* <frequency>* <frequencyUnit>* <start>* <end>*
      else if (cmd == UPDATE_ACTION_CMD) {
        uint16_t id = tokens[1].toInt();
        ActionType type = (ActionType)tokens[2].toInt();
        uint8_t pump = tokens[3].toInt();
        uint16_t amount = tokens[4].toInt();
        uint16_t frequency = tokens[5].toInt();
        TimeUnit unit = (TimeUnit)tokens[6].toInt();
        uint32_t start = tokens[7].toInt();
        uint32_t end = tokens[8].toInt();

        updateAction(id, type, pump, amount, frequency, unit, start, end);
      }
      // DEL_ACTION ID
      else if (cmd == DEL_ACTION_CMD) {
        uint16_t id = tokens[1].toInt();

        deleteAction(id);
      }
      // LINK_ACTION_WELL <actionId> <96bit_mask_hex>
      else if (cmd == LINK_ACTION_WELL_CMD) {
        if (count < 3) return;
        uint16_t id = tokens[1].toInt();

        if (!findActionById(id)) return;

        uint8_t mask[12];
        if (!parseWellBitmask(tokens[2], mask)) return;

        linkActionByMask(id, mask);
      }
      // UNLINK_ACTION_WELL <actionId> <96bit_mask_hex>
      else if (cmd == UNLINK_ACTION_WELL_CMD) {
        if (count < 3) return;
        uint16_t id = tokens[1].toInt();

        if (!findActionById(id)) return;

        uint8_t mask[12];
        if (!parseWellBitmask(tokens[2], mask)) return;

        unlinkActionByMask(id, mask);
      }
      else if (cmd == PARK_CMD) {
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
      else if (cmd == PRINT_WELL_CMD) {
        printCurrentWell();
      }
      else if (cmd == PRINT_CALIBRATION_CMD) {
        printCalibrationPoints();
      }
      else if (cmd == PRINT_STEPS_CMD) {
        printMicroSteps();
        printStepSize();
      }
      else {
        Serial.println("ERR UNKNOWN_COMMAND");
      }
    }
  }

  void switches() {
    static bool z1WasPressed = false;
    if(digitalRead(limitSwitchZ1) == LOW) {
      if(!z1WasPressed) {  
        Serial.println("LIMIT_PRESSED:AXIS=Z1");
        z1WasPressed = true;
      }
      stepperZ1.setCurrentPosition(stepperZ1.currentPosition());
      stepperZ2.setCurrentPosition(stepperZ2.currentPosition());
    } else {
      if (z1WasPressed) {
          Serial.println("LIMIT_RELEASED:AXIS=Z1");
          z1WasPressed = false;
      }
    }

    static bool z2WasPressed = false;
    if(digitalRead(limitSwitchZ2) == LOW) {
      if(!z2WasPressed) {
        Serial.println("LIMIT_PRESSED:AXIS=Z2");
        z2WasPressed = true;
      }
      stepperZ1.setCurrentPosition(stepperZ1.currentPosition());
      stepperZ2.setCurrentPosition(stepperZ2.currentPosition());
    } else {
      if (z2WasPressed) {
          Serial.println("LIMIT_RELEASED:AXIS=Z2");
          z2WasPressed = false;
      }
    }
    
    
    static bool lWasPressed = false;
    if(digitalRead(limitSwitchL) == LOW) {
      if (!lWasPressed) {
        Serial.println("LIMIT_PRESSED:AXIS=L");
        lWasPressed = true;
      }
      stepperL.stop();
      stepperL.setCurrentPosition(stepperL.currentPosition());
    }
    else {
      if (lWasPressed) {
          Serial.println("LIMIT_RELEASED:AXIS=L");
          lWasPressed = false;
      }
    }
    
    static bool rWasPressed = false;
    if(digitalRead(limitSwitchR) == LOW) {
      if(!rWasPressed) {
        Serial.println("LIMIT_PRESSED:AXIS=R");
        rWasPressed = true;
      }
      stepperR.stop();
      stepperR.setCurrentPosition(stepperR.currentPosition());
    } else {
      if (rWasPressed) {
          Serial.println("LIMIT_RELEASED:AXIS=R");
          rWasPressed = false;
      }
    }
  }
  
  int goToOrigin() {

    moveZMotors(ZMotorNormalPosition);

    enableLMotor();
    enableRMotor();
    stepperL.moveTo(0); 
    stepperR.moveTo(0); 
    
    while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
      stepperR.run();
      stepperL.run();
    }

    saveCurrentWell("HOME");
    savePositions();
  }

  void goToHardcodedWells(char row, int column) {  

    if(emergencyStopRequested) {
      emergencyStopRequested = false;  
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
            Serial.println("Invalid column for row A");
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
            Serial.println("Invalid column for row B");
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
            Serial.println("Invalid column for row C");
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
            Serial.println("Invalid column for row D");
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
            Serial.println("Invalid column for row E");
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
            Serial.println("Invalid column for row F");
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
            Serial.println("Invalid column for row G");
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
            Serial.println("Invalid column for row H");
            break;
        }

        break;  

      default:
        Serial.println("Invalid row");
      break;
    }
  }

  int calibrate() {
    moveZMotors(ZMotorNormalPosition);

    disableAllMotors();

    if(emergencyStopRequested) {
      emergencyStopRequested = false;  
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

    saveCurrentWell("HOME");

    disableAllMotors();

    return 1;
  }

  void moveToWell(long moveL, long moveR, String wellName) {
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

  void emergencyStop() {
    stepperL.stop();
    stepperR.stop();
    stepperZ1.stop();
    stepperZ2.stop();
    stepperP1.stop();
    stepperP2.stop();
    disableAllMotors();
    lastMotorActivityTime = 0;
    emergencyStopRequested = true;
    Serial.println("WARNING:EMERGENCY_STOP,Motors disabled by user");
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
        Serial.println("ERROR:DRIVER_FAULT,Motor driver nFAULT triggered");
        faultLatched = true;
      }
      emergencyStop();
    } else {
      faultLatched = false;
    }
  }

  void enterHomingMode(int homingSpeedL, int homingSpeedR) {
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
      Serial.println("No valid stored positions – doing normal home");
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

    Serial.print("POS:L="); Serial.print(L);
    Serial.print(",R="); Serial.print(R);
    Serial.print(",Z1="); Serial.print(Z1);
    Serial.print(",Z2="); Serial.println(Z2);

    return true;
  }

  void saveCalibration() {
    if (currentState == SETUP) return;

    int addr = EEPROM_CAL_BASE;
    byte magic = MAGIC;
    EEPROM.put(addr, magic);  addr += sizeof(magic);

    EEPROM.put(addr, calCount);  addr += sizeof(uint8_t);

    for (int i = 0; i < calCount; i++) {
      EEPROM.put(addr, calX[i]);  addr += sizeof(float);
      EEPROM.put(addr, calY[i]);  addr += sizeof(float);
      EEPROM.put(addr, calL[i]);  addr += sizeof(float);
      EEPROM.put(addr, calR[i]);  addr += sizeof(float);
    }

    Serial.println("Calibration saved to EEPROM");
    Serial.print("Saved "); Serial.print(calCount); Serial.println(" points");
  }

  void loadCalibration() {
    int addr = EEPROM_CAL_BASE;
    byte magic;
    EEPROM.get(addr, magic);  addr += sizeof(magic);

    if (magic != MAGIC) {
      Serial.println("No valid calibration in EEPROM");
      return;
    }

    EEPROM.get(addr, calCount);  addr += sizeof(uint8_t);
    if (calCount < 0 || calCount > MAX_CAL) {
      Serial.println("Corrupt point count in EEPROM");
      calCount = 0;
      return false;
    }

    for (int i = 0; i < calCount; i++) {
      EEPROM.get(addr, calX[i]);  addr += sizeof(float);
      EEPROM.get(addr, calY[i]);  addr += sizeof(float);
      EEPROM.get(addr, calL[i]);  addr += sizeof(float);
      EEPROM.get(addr, calR[i]);  addr += sizeof(float);
    }

    solveMapping();
    return;
  }

  void wellToXY(char row, int col, float &x, float &y) {
    row = tolower(row);

    if (col < 1 || col > 12 || row < 'a' || row > 'h') {
    Serial.print("ERROR:INVALID_WELL,");
    Serial.print(row); Serial.println(col);
    return;
    }

    int r = row - 'a';  // a=0, b=1, ... h=7
    
    x =  (col - 1) * WELL_DX;
    y =  r * WELL_DY;
  }

  void XYToWell(float x, float y, String &wellName) {
    int col = x / WELL_DX;
    int row = y / WELL_DY;
    char rowChar = 'a' + row;

    wellName = rowChar + String(col + 1);
  }


  void xyToAngles(float x, float y, float &Ldeg, float &Rdeg) {
    if (!mapReady) {
      Serial.println("Angle map not ready!");
      Ldeg = 0; Rdeg = 0;
      return;
    }
    // Basis vector for quadratic model
    float b[TERMS] = { 1.0f, x, y, x*x, x*y, y*y, x*x*x, x*x*y, x*y*y, y*y*y };
    Ldeg = dot10(ML, b);
    Rdeg = dot10(MR, b);
  }

  void recordCalibrationPoint(char row, int col) {
    if (calCount >= MAX_CAL) {
        Serial.print("ERROR:CAL_FULL,Maximum "); 
        Serial.print(MAX_CAL); 
        Serial.println(" calibration points reached");
        return;
    }

    float x = 0;
    float y = 0;
    wellToXY(row, col, x, y);

    calX[calCount] = x;
    calY[calCount] = y;

    calL[calCount] = stepsToDegrees(stepperL.currentPosition());
    calR[calCount] = stepsToDegrees(stepperR.currentPosition());

    Serial.print("CAL_REC:");
    Serial.print("Name="); Serial.print(row); Serial.print(col);
    Serial.print(",X="); Serial.print(x);
    Serial.print(",Y="); Serial.print(y);
    
    Serial.print("CAL_COUNT:"); Serial.println(++calCount);
  }


  inline float dot10(const float a[TERMS], const float b[TERMS]) {
    return a[0]*b[0] + a[1]*b[1] + a[2]*b[2] + a[3]*b[3] + a[4]*b[4]
        + a[5]*b[5] + a[6]*b[6] + a[7]*b[7] + a[8]*b[8] + a[9]*b[9];
  }

  bool solve10(float A[TERMS][TERMS], float b[TERMS], float x[TERMS]) {
    float M[TERMS][TERMS+1];
    for (int i=0;i<TERMS;i++){
      for (int j=0;j<TERMS;j++) M[i][j] = A[i][j];
      M[i][TERMS] = b[i];
    }
    for (int col=0; col<TERMS; col++) {
      int piv = col;
      float best = fabs(M[piv][col]);
      for (int r=col+1; r<TERMS; r++) {
        float v = fabs(M[r][col]);
        if (v > best) { best = v; piv = r; }
      }
      if (best < 1e-9) return false;
      if (piv != col) {
        for (int c=col; c<=TERMS; c++) {
          float tmp = M[col][c];
          M[col][c] = M[piv][c];
          M[piv][c] = tmp;
        }
      }
      float div = M[col][col];
      for (int c=col; c<=TERMS; c++) M[col][c] /= div;
      for (int r=0; r<TERMS; r++) {
        if (r == col) continue;
        float f = M[r][col];
        for (int c=col; c<=TERMS; c++) M[r][c] -= f * M[col][c];
      }
    }
    for (int i=0;i<TERMS;i++) x[i] = M[i][TERMS];
    return true;
  }

  bool solveMapping() {
    if (calCount < TERMS) {
      Serial.print("ERROR:SOLVE_INSUFFICIENT,Need at least ");
      Serial.print(TERMS);
      Serial.println(" calibration points");
      return false;
    }

    // Normal equations: (A^T A) c = (A^T y)
    // A rows are basis b = [1, x, y, x^2, x*y, y^2]
    static float ATA[TERMS][TERMS] = {0};
    static float ATyL[TERMS] = {0};
    static float ATyR[TERMS] = {0};

    for (int i=0; i<calCount; i++) {
      float x = calX[i], y = calY[i];
      float b[TERMS] = { 1.0f, x, y, x*x, x*y, y*y, x*x*x, x*x*y, x*y*y, y*y*y };
      float L = calL[i];
      float R = calR[i];

      // Accumulate ATA = Σ b*b^T
      for (int r=0; r<TERMS; r++) {
        for (int c=0; c<TERMS; c++) {
          ATA[r][c] += b[r]*b[c];
        }
      }
      // Accumulate ATy = Σ b*y
      for (int k=0; k<TERMS; k++) {
        ATyL[k] += b[k]*L;
        ATyR[k] += b[k]*R;
      }
    }

    float MLtmp[TERMS], MRtmp[TERMS];
    bool okL = solve10(ATA, ATyL, MLtmp);
    bool okR = solve10(ATA, ATyR, MRtmp);

    if (!okL || !okR) {
      Serial.println("ERROR:SOLVE_SINGULAR,Calibration matrix singular - add more spread-out points");
      mapReady = false;
      return false;
    }

    // Commit
    for (int i=0;i<TERMS;i++) { ML[i] = MLtmp[i]; MR[i] = MRtmp[i]; }
    mapReady = true;
    saveCalibration();

    Serial.println("=== MAPPING SOLVED (quadratic least-squares) ===");
    printCalibrationPoints();

    return true;
  }

  float clampZero(float v, float eps = 5e-4f) {
      return fabs(v) < eps ? 0.0f : v;
  }

  void printCalibrationPoints() {
    Serial.print("CAL_COUNT:"); Serial.println(calCount);
    float maxErrL = 0, maxErrR = 0, rmsL = 0, rmsR = 0;
    for (int i=0; i<calCount; i++) {
      float x = calX[i], y = calY[i];
      String wellName;
      XYToWell(calX[i], calY[i], wellName);
      float b[TERMS] = { 1.0f, x, y, x*x, x*y, y*y, x*x*x, x*x*y, x*y*y, y*y*y };
      float predL = dot10(ML, b);
      float predR = dot10(MR, b);
      float errL  = calL[i] - predL;
      float errR  = calR[i] - predR;
      rmsL += errL*errL; rmsR += errR*errR;
      if (fabs(errL) > maxErrL) maxErrL = fabs(errL);
      if (fabs(errR) > maxErrR) maxErrR = fabs(errR);
      Serial.print("CAL_PT:");
      Serial.print("Name="); Serial.print(wellName);
      Serial.print(",X="); Serial.print(calX[i], 2);
      Serial.print(",Y="); Serial.print(calY[i], 2);


      if (!mapReady) {
        Serial.println();
        continue;
      }
      
      Serial.print(",ErrorLeft="); Serial.print(clampZero(errL), 3);
      Serial.print(",ErrorRight="); Serial.println(clampZero(errR), 3);
    }
    rmsL = sqrtf(rmsL / calCount);
    rmsR = sqrtf(rmsR / calCount);

    if (!mapReady) return;

    Serial.print("RMS:L="); Serial.print(rmsL, 3);
    Serial.print(",R="); Serial.print(rmsR, 3);
    Serial.print(",MAX_L="); Serial.print(maxErrL, 3);
    Serial.print(",MAX_R="); Serial.println(maxErrR, 3);
  }

  long degToSteps(float deg) {
    float stepsPerRev = 200.0 * currentMicrosteps;
    return lroundf(deg * (stepsPerRev / 360.0));
  }

  float stepsToDegrees(long steps) {
    float stepsPerRev = 200.0f * currentMicrosteps;
    return steps * (360.0f / stepsPerRev);
  }

  uint8_t wellNameToIndex(String wellName) {
      if (wellName == "HOME") return WELL_HOME;

      char row = wellName.charAt(0);
      String columnStr = wellName.substring(1);  
      int column = columnStr.toInt();

      return (row - 'a') * 12 + (column - 1);
  }

  String wellIndexToName(uint8_t wellIndex) {
      if (wellIndex == WELL_HOME) {
          return "HOME";
      }

      char row = 'A' + (wellIndex / 12);
      int  col = (wellIndex % 12) + 1;
      return String(row) + String(col);
  }

  void goToCalculatedWell(char row, int col) {
    if (!mapReady) {
      Serial.println("ERROR:MAP_NOT_READY,Run z solve before moving to wells");
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

    saveCurrentWell(String(row) + String(col));
    savePositions();
    printCurrentWell();
  }

  void saveCurrentWell(String wellName) {  
    wellIndex = wellNameToIndex(wellName);

    EEPROM.put(EEPROM_WELL_BASE, wellIndex);
  }

  void loadCurrentWell() {
    wellIndex = EEPROM.read(EEPROM_WELL_BASE);

    printCurrentWell();
  }

  void printCurrentWell() {
    String wellName = wellIndexToName(wellIndex);

    float x = 0;
    float y = 0;
    float Ldeg = 0;
    float Rdeg = 0;

    if (wellName != "HOME") {
      char row = tolower(wellName[0]);
      if (row < 'a' || row > 'h') return;

      int col = wellName.substring(1).toInt();
      if (col < 1 || col > 12) return;

      
      wellToXY(row, col, x, y);

      Ldeg = stepsToDegrees(stepperL.currentPosition());
      Rdeg = stepsToDegrees(stepperR.currentPosition());
    }

    Serial.print("WELL:"); 
    Serial.print("Name="); Serial.print(wellName);
    Serial.print(",X="); Serial.print(x);
    Serial.print(",Y="); Serial.print(y);
    Serial.print(",L="); Serial.print(Ldeg);
    Serial.print(",R="); Serial.println(Rdeg);
  }

  void printStepSize() {
    Serial.print("STEP_SIZE:");
    Serial.print(times_x10 / 10);
    Serial.print(".");
    Serial.println(times_x10 % 10);
  }

  void printMicroSteps() {
    Serial.print("MICROSTEPS:1/"); Serial.println(currentMicrosteps);
  }

  uint8_t splitN(const String& input, String out[], uint8_t maxTokens) {
    for (uint8_t i = 0; i < maxTokens; i++) {
      out[i] = "";
    }

    String s = input;
    s.trim();

    uint8_t count = 0;
    int start = 0;

    while (start < s.length() && count < maxTokens) {
      int space = s.indexOf(' ', start);
      if (space == -1) space = s.length();

      String token = s.substring(start, space);
      token.trim();

      if (token.length() > 0) {
        out[count++] = token;
      }

      start = space + 1;
    }

    return count;
  }

  uint16_t createAction(ActionType type, uint8_t pump, uint16_t amount, uint16_t frequency, TimeUnit unit, uint32_t start, uint32_t end) {
    if (actionCount >= MAX_ACTIONS_TOTAL) {
      Serial.println("ERROR:FAILED TO CREATE ACTION");
      return 0;
    }
    
    uint8_t slot = findFreeActionSlot();
    if (slot == INVALID) return 0;

    Action &action = actions[slot];

    action.id = nextActionId++;
    action.type = type;
    action.pump = pump;
    action.amount_uL = amount;
    action.frequency = frequency;
    action.unit = unit;
    action.startEpoch = start;
    action.endEpoch = end;
    action.enabled = true;

    saveAction(action, slot);
    actionCount++;
    
    saveActionsState();

    Serial.print("ACTION_CREATED:");
    Serial.println(action.id);

    return action.id;
  }

  void updateAction(uint16_t id, ActionType type, uint8_t pump, uint16_t amount, uint16_t frequency, TimeUnit unit, uint32_t start, uint32_t end) {
    
    Action* action = findActionById(id);
    if (!action) return;


    action->type = type;
    action->pump = pump;
    action->amount_uL = amount;
    action->frequency = frequency;
    action->unit = unit;
    action->startEpoch = start;
    action->endEpoch = end;

    uint8_t index = action - actions;
    saveAction(*action, index);

    Serial.print("ACTION_UPDATED:");
    Serial.println(action->id);
  }

  void deleteAction(uint16_t id) {
    Action* action = findActionById(id);
    if (!action) return;

    action->enabled = false;

    uint8_t index = action - actions;
    saveAction(*action, index);

    Serial.print("ACTION_DELETED:");
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
    for (int i = 0; i < MAX_ACTIONS_TOTAL; i++) {
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
      actions[i].enabled = false;
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
    Serial.print("ACTION:");
    Serial.print("Id=");Serial.print(action.id);
    Serial.print(",ActionType="); Serial.print(action.type);
    Serial.print(",Pump="); Serial.print(action.pump);
    Serial.print(",Amount="); Serial.print(action.amount_uL);
    Serial.print(",Frequency="); Serial.print(action.frequency);
    Serial.print(",Unit="); Serial.print(action.unit);
    Serial.print(",Start="); Serial.print(action.startEpoch);
    Serial.print(",End="); Serial.println(action.endEpoch);
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
    String wellName = wellIndexToName(wellIndex);
    Serial.print("WELL_ACTION:");
    Serial.print("Well="); Serial.print(wellName);
    Serial.print(",Actions=[");
    for (uint8_t i = 0; i < wellAction.count; i++) {
      if (i > 0) Serial.print(',');
      Serial.print(wellAction.actionIds[i]);
    }
    Serial.println("]");
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

  bool parseWellBitmask(const String& hex, uint8_t mask[12]) {
    if (hex.length() != 24) return false;

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
        return true;
      }
    }
    return false;
  }