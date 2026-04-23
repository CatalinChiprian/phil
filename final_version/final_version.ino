/* Created by Victoria Shvets
  Based on Phillip Dettinger work availible on https://github.com/CSDGroup/PHIL.git */

  #include <EEPROM.h>
  #include <AccelStepper.h>
  #include <MultiStepper.h>

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
  int calCount = 0;

  const int TERMS = 10;
  float ML[TERMS] = {0};
  float MR[TERMS] = {0};
  bool mapReady = false;

  const int WELL_NAME_ADDR = 500;
  const int WELL_NAME_MAX = 5;

  struct {
    String wellName;
    float x;
    float y;
    float lDeg;
    float rDeg;
  } currentWell;

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

  float UL_PER_STEP = 0.1099f;

  int limitSwitchL = 31; // Target Limit Switch L
  int limitSwitchR = 30; // Target Limit Switch R
  int limitSwitchZ1 = 33; // Target Limit Switch Z
  int limitSwitchZ2 = 32; // Target Limit Switch Z

  int faultR = 37;
  int faultL = 39;

  int microIndex = 3; // 0=full, 1=half, 2=1/4, 3=1/8, 4=1/16, 5=1/32
  int currentMicrosteps = MICROoptions[microIndex]; 

  float times = 0.10;
  const long steps = 4 * currentMicrosteps;

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

    printStepSize();

    emergencyStopRequested = false;
  }

  void moveBackward() {
    enableLMotor();
    enableRMotor();

    // stepperL.move(-4 * currentMicrosteps);
    // stepperR.move(5 * currentMicrosteps);
    long s = steps * times;
    stepperL.moveTo(s + stepperL.currentPosition());
    stepperR.moveTo(-s + stepperR.currentPosition());

    while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
      stepperL.run();
      stepperR.run();
    }
  }

  void moveForward() {
    enableLMotor();
    enableRMotor();

    // stepperL.move(4 * currentMicrosteps);
    // stepperR.move(-5 * currentMicrosteps);
    long s = steps * times;
    stepperL.moveTo(-s + stepperL.currentPosition());
    stepperR.moveTo(s + stepperR.currentPosition());

    while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
      stepperL.run();
      stepperR.run();
    }
  }

  void moveLeft() {
    enableLMotor();
    enableRMotor();

    bool lLimitHit = false;

    // stepperL.move(-4 * times * currentMicrosteps);
    // stepperR.move(-3 * times * currentMicrosteps);
    long s = steps * times;
    stepperL.moveTo(s + stepperL.currentPosition());
    stepperR.moveTo(s + stepperR.currentPosition());

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
  }

  void moveRight() {
    enableLMotor();
    enableRMotor();

    bool rLimitHit = false;

    // stepperL.move(4 * times * currentMicrosteps);
    // stepperR.move(3 * times * currentMicrosteps);

    long s = steps * times;
    stepperL.moveTo(-s + stepperL.currentPosition());
    stepperR.moveTo(-s + stepperR.currentPosition());

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
  }

  void moveUp() {
    enableZMotors();
    
    bool z1LimitHit = false;
    bool z2LimitHit = false;

    long s = 8 * currentMicrosteps;
    stepperZ1.moveTo(-s + stepperZ1.currentPosition());
    stepperZ2.moveTo(-s + stepperZ2.currentPosition());

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
  }

  void moveDown() {
    enableZMotors();

    long s = 8 * currentMicrosteps;
    stepperZ1.moveTo(s + stepperZ1.currentPosition());
    stepperZ2.moveTo(s + stepperZ2.currentPosition());

    while(stepperZ1.distanceToGo() != 0 || stepperZ2.distanceToGo() != 0) {
      stepperZ1.run();
      stepperZ2.run();
    }
  }

  long uLToSteps(float microliters) {
    return (long)(microliters / UL_PER_STEP);
  }

  void aspirateP1(int microliters = 20) {
    enableP1Motor();

    long stepsNeeded = uLToSteps(microliters);

    stepperP1.moveTo(stepperP1.currentPosition() - stepsNeeded);

    while(stepperP1.distanceToGo() != 0) {
      stepperP1.run();
    }

    Serial.print("PUMP1:aspirated="); Serial.print(microliters);
    Serial.println("uL");
    Serial.print("Which is ");
    Serial.print(stepsNeeded);
    Serial.println(" steps");
  }

  void dispenseP1(int microliters = 20) {
    enableP1Motor();

    long stepsNeeded = uLToSteps(microliters);

    if (stepsNeeded > 0) {
        stepperP1.moveTo(stepperP1.currentPosition() + stepsNeeded);
        while(stepperP1.distanceToGo() != 0) stepperP1.run();
    }

    Serial.print("PUMP1:dispensed="); Serial.print(microliters);
    Serial.println("uL");
    Serial.print("Which is ");
    Serial.print(stepsNeeded);
    Serial.println(" total steps");
  }

  void aspirateP2(int amount = 20) {
    enableP2Motor();

    stepperP2.moveTo(stepperP2.currentPosition() - steps);

    while(stepperP2.distanceToGo() != 0) {
      stepperP2.run();
    }
  }

  void dispenseP2(int microliters = 20) {
    enableP2Motor();
    
    long stepsNeeded = uLToSteps(microliters);

    if (stepsNeeded > 0) {
        stepperP2.moveTo(stepperP2.currentPosition() + stepsNeeded);
        while(stepperP2.distanceToGo() != 0) stepperP2.run();
    }

    Serial.print("PUMP2:dispensed="); Serial.print(microliters);
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
      Serial.print("Arduino received: ");
      Serial.println(received);
      
      if (received.length() > 0) {
        char cmd = received.charAt(0); 
        
        switch (cmd) {
          case 'b':
            moveBackward();
            Serial.print("PHIL moved backward \n");
            savePositions();
          break;
      
          case 'f':
            moveForward();
            Serial.print("PHIL moved forward \n");
            savePositions();
          break;
      
          case 'l':
            moveLeft();
            Serial.print("PHIL moved left \n");
            savePositions();
          break;
      
          case 'r':
            moveRight();
            Serial.print("PHIL moved right \n");
            savePositions();
          break;
      
          case 'u':
            moveUp();
            Serial.print("PHIL moved up \n");
            savePositions();
          break;
      
          case 'd':
            moveDown();
            Serial.print("PHIL moved down \n");
            savePositions();
          break;

          case 'i':
          {
            char arg = received.charAt(1);
            String amountChar = received.substring(2);
            int amount = amountChar.toInt();
            if (arg == '1') aspirateP1(amount);
            else if (arg == '2') aspirateP2(amount);
          }
          break;

          case 'o':
          {
            char arg = received.charAt(1);
            String amountChar = received.substring(2);
            int amount = amountChar.toInt();
            if (arg == '1') dispenseP1(amount);
            else if (arg == '2') dispenseP2(amount);
          }
          break;

          case 'h': // Home
            goToOrigin();
            interruptibleDelay(500);
            disableAllMotors();
            savePositions();  
          break; 

          case 'p':
          {
            char arg = received.charAt(1);
            if (arg == 'w')
              printCurrentWell();
            else if (arg == 'm')
              printCalibrationPoints();
            else if (arg == 's') {
              printMicroSteps();
              printStepSize();
            }
          }
          break;

          case 'c':
            calibrate();
          break;

          case 'z':
             // Example commands:
            // z a1
            // z solve
            // z print

            if (received.startsWith("z solve")) {
                solveMapping();
            }
            else if (received.startsWith("z deleteidx")) {
              int idx = received.substring(12).toInt();
              if (idx < 0 || idx >= calCount) {
                Serial.println("ERROR:CAL_INDEX,Index out of range");
              } else {
                Serial.print("Deleting Pt "); Serial.print(idx);
                Serial.print(": XY("); Serial.print(calX[idx]);
                Serial.print(","); Serial.print(calY[idx]); Serial.println(")");
                for (int i = idx; i < calCount-1; i++) {
                  calX[i]=calX[i+1]; calY[i]=calY[i+1];
                  calL[i]=calL[i+1]; calR[i]=calR[i+1];
                }
                calCount--;
                Serial.print("CAL_DELETED_IDX:"); Serial.print(idx);
                Serial.print(",remaining="); Serial.println(calCount);
              }
            }
            else if (received.startsWith("z delete")) {
                // Format: "z delete a1"
                if (received.length() >= 10) {
                    char row = received.charAt(9);
                    int col = received.substring(10).toInt();
                    
                    // Find the point
                    float x, y;
                    wellToXY(row, col, x, y);
                    
                    int foundIdx = -1;
                    for (int i = 0; i < calCount; i++) {
                        if (fabs(calX[i] - x) < 0.1f && fabs(calY[i] - y) < 0.1f) {
                            foundIdx = i;
                            break;
                        }
                    }
                    
                    if (foundIdx == -1) {
                        Serial.println("ERROR:CAL_NOT_FOUND,No calibration point at that well");
                    } else {
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
                } else {
                    Serial.println("Usage: z delete a1");
                }
            }
            else if (received.startsWith("z print")) {
              Serial.println("--- Calibration Data ---");
              for (int i=0; i<calCount; i++) {
                Serial.print("Pt "); Serial.print(i);
                Serial.print(": XY("); Serial.print(calX[i],2);
                Serial.print(",");     Serial.print(calY[i],2);
                Serial.print(") Ang(");Serial.print(calL[i],2);
                Serial.print(",");     Serial.print(calR[i],2);
                Serial.println(")");
              }
                if (mapReady && calCount > 0) {
                  float rmsL=0, rmsR=0, maxErrL=0, maxErrR=0;
                  Serial.println("--- Residuals ---");
                  for (int i=0; i<calCount; i++) {
                    float x = calX[i], y = calY[i];
                    float b[TERMS] = { 1.0f, x, y, x*x, x*y, y*y, x*x*x, x*x*y, x*y*y, y*y*y };
                    float predL = dot10(ML, b);
                    float predR = dot10(MR, b);
                    float errL  = calL[i] - predL;
                    float errR  = calR[i] - predR;
                    rmsL += errL*errL;
                    rmsR += errR*errR;
                    if (fabs(errL) > maxErrL) maxErrL = fabs(errL);
                    if (fabs(errR) > maxErrR) maxErrR = fabs(errR);
                    Serial.print("Pt "); Serial.print(i);
                    Serial.print(": errL="); Serial.print(errL, 3);
                    Serial.print("  errR="); Serial.println(errR, 3);
                  }
                  rmsL = sqrtf(rmsL / calCount);
                  rmsR = sqrtf(rmsR / calCount);
                  Serial.print("RMS  L="); Serial.print(rmsL, 3);
                  Serial.print("  R=");    Serial.println(rmsR, 3);
                  Serial.print("Max  L="); Serial.print(maxErrL, 3);
                  Serial.print("  R=");    Serial.println(maxErrR, 3);
                } else {
                  Serial.println("(no map solved yet - run 'z solve' first)");
                }
            }

            else {
                // Example: "z a1"
                char row = received.charAt(2);
                int col = received.substring(3).toInt();
                recordCalibrationPoint(row, col);
            }
          break;

          case 'w': // Go to hardcoded well
            if(received.length() >= 3) {
              char row = received.charAt(1);
              String columnStr = received.substring(2);  
              int column = columnStr.toInt();
              wells(row, column);  
              savePositions();
            } else {
              Serial.println("Invalid well command. Use format: wa1, wb2, wa10, etc.");
            }
          break;  

          case 'q': // Go to calculated well
            if (received.length() >= 3) {
              char row = received.charAt(1);
              String columnStr = received.substring(2);  
              int column = columnStr.toInt();
              goToCalculatedWell(row, column);
              savePositions();
            }
          break;


          case 'x':
            EEPROM.put(20, 0x00);
            mapReady = false;
            for (int i = 0; i < TERMS; i++) { ML[i] = 0; MR[i] = 0; }
            calCount = 0;
            Serial.println("Calibration cleared");
          break;

          case 'k':
            enableLMotor();
            enableRMotor();

            stepperL.moveTo(55 * currentMicrosteps); 
            stepperR.moveTo(-5.5 * currentMicrosteps); 
            
            while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
              stepperR.run();
              stepperL.run();
            }

            savePositions();

          break;

          case '+':
            times += 0.10;
            printStepSize();
          break;

          case '-':
            times -= 0.10;
            printStepSize();
          break;
        }
      }
    }

    stepperL.run();
    stepperR.run();
    stepperZ1.run();
    stepperZ2.run();
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
    enableLMotor();
    enableRMotor();
    stepperL.moveTo(0); 
    stepperR.moveTo(0); 
    
    while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
      stepperR.run();
      stepperL.run();
    }

    saveCurrentWell("HOME", 0, 0, 0, 0);
  }

  void wells(char row, int column) {  

    if(emergencyStopRequested) {
      emergencyStopRequested = false;  
      return;
    }

    enableLMotor();
    enableRMotor();
 
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

    while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
      if(Serial.available() > 0) {
          char c = Serial.read();
          if(c == 's') {
            emergencyStop(); 
            Serial.println("STOPPED by user");
            return;  
          }
      }
      stepperR.run();
      stepperL.run();
    }

    setNormalMovementSpeed(); 
  }

  int calibrate() {
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

    stepperR.move(350);

    while (stepperR.distanceToGo() != 0) {
      stepperR.run();
    }

    stepperR.setCurrentPosition(0);

    saveCurrentWell("HOME", 0, 0, 0, 0);

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

    interruptibleDelay(1000);
    
    char row = wellName.charAt(0);
    String columnStr = wellName.substring(1);  
    int col = columnStr.toInt();

    float x, y;
    wellToXY(row, col, x, y);

    float Ldeg = stepsToDegrees(stepperL.currentPosition());
    float Rdeg = stepsToDegrees(stepperR.currentPosition());

    saveCurrentWell(wellName, x, y, Ldeg, Rdeg);
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
    EEPROM.put(0, stepperL.currentPosition());
    EEPROM.put(4, stepperR.currentPosition());
    EEPROM.put(8, stepperZ1.currentPosition());
    EEPROM.put(12, stepperZ2.currentPosition());

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

    long L, R, Z1, Z2;
    EEPROM.get(0, L);
    EEPROM.get(4, R);
    EEPROM.get(8, Z1);
    EEPROM.get(12, Z2);

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
    int addr = 20;
    byte magic = 0xCC;
    EEPROM.put(addr, magic);  addr += sizeof(magic);

    for (int i = 0; i < TERMS; i++) { EEPROM.put(addr, ML[i]); addr += sizeof(float); }
    for (int i = 0; i < TERMS; i++) { EEPROM.put(addr, MR[i]); addr += sizeof(float); }

    EEPROM.put(addr, calCount);  addr += sizeof(int);

    for (int i = 0; i < calCount; i++) {
      EEPROM.put(addr, calX[i]);  addr += sizeof(float);
      EEPROM.put(addr, calY[i]);  addr += sizeof(float);
      EEPROM.put(addr, calL[i]);  addr += sizeof(float);
      EEPROM.put(addr, calR[i]);  addr += sizeof(float);
    }

    Serial.println("Calibration saved to EEPROM");
    Serial.print("Saved "); Serial.print(calCount); Serial.println(" points");
  }

  bool loadCalibration() {
    int addr = 20;
    byte magic;
    EEPROM.get(addr, magic);  addr += sizeof(magic);

    if (magic != 0xCC) {
      Serial.println("No valid calibration in EEPROM");
      return false;
    }

    for (int i = 0; i < TERMS; i++) { EEPROM.get(addr, ML[i]); addr += sizeof(float); }
    for (int i = 0; i < TERMS; i++) { EEPROM.get(addr, MR[i]); addr += sizeof(float); }

    EEPROM.get(addr, calCount);  addr += sizeof(int);
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

    mapReady = true;
    Serial.print("CAL_COUNT:"); Serial.println(calCount);
    for (int i = 0; i < calCount; i++) {
      Serial.print("CAL_PT:");
      Serial.print(i); Serial.print(",");
      Serial.print(calX[i], 2); Serial.print(",");
      Serial.print(calY[i], 2); Serial.print(",");
      Serial.print(calL[i], 2); Serial.print(",");
      Serial.println(calR[i], 2);
    }
    Serial.print("CAL_COEFFS_L:");
    for (int i = 0; i < TERMS; i++) { 
      Serial.print(ML[i], 5); 
      if (i < TERMS-1) Serial.print(","); 
    }
    Serial.println();
    Serial.print("CAL_COEFFS_R:");
    for (int i = 0; i < TERMS; i++) { 
      Serial.print(MR[i], 5); 
      if (i < TERMS-1) Serial.print(","); 
    }
    Serial.println();
    return true;
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

    float x, y;
    wellToXY(row, col, x, y);

    calX[calCount] = x;
    calY[calCount] = y;

    calL[calCount] = stepsToDegrees(stepperL.currentPosition());
    calR[calCount] = stepsToDegrees(stepperR.currentPosition());

    Serial.print("CAL_REC:");
    Serial.print(row); Serial.print(col); Serial.print(",");
    //XY
    Serial.print(x, 2); Serial.print(",");
    Serial.print(y, 2); Serial.println();
    
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
    float ATA[TERMS][TERMS] = {0};
    float ATyL[TERMS] = {0};
    float ATyR[TERMS] = {0};

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

    // Residuals to assess fit quality
    Serial.println("--- Residuals (deg) ---");


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
      Serial.print(wellName); Serial.print(",");
      Serial.print(calX[i], 2); Serial.print(",");
      Serial.print(calY[i], 2); 

      if (!mapReady) {
        Serial.println();
        continue;
      }
      
      Serial.print(",");
      Serial.print(clampZero(errL), 3); Serial.print(",");
      Serial.println(clampZero(errR), 3);
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
    return (long)(deg * (stepsPerRev / 360.0));
  }

  float stepsToDegrees(long steps) {
    float stepsPerRev = 200.0f * currentMicrosteps;
    return (float)steps * (360.0f / stepsPerRev);
  }

  void goToCalculatedWell(char row, int col) {
    if (!mapReady) {
      Serial.println("ERROR:MAP_NOT_READY,Run z solve before moving to wells");
      return;
    }

    enableLMotor();
    enableRMotor();

    float x, y;
    wellToXY(row, col, x, y);

    float Ldeg, Rdeg;
    xyToAngles(x, y, Ldeg, Rdeg);

    long Lsteps = degToSteps(Ldeg);
    long Rsteps = degToSteps(Rdeg);

    stepperL.moveTo(Lsteps);
    stepperR.moveTo(Rsteps);

    while (stepperL.distanceToGo() != 0 || stepperR.distanceToGo() != 0) {
        stepperL.run();
        stepperR.run();
    }

    saveCurrentWell(String(row) + String(col), x, y, Ldeg, Rdeg);
    printCurrentWell();
  }

  void saveCurrentWell(String wellName, float x, float y, float Ldeg, float Rdeg) {
    for (int i = 0; i < WELL_NAME_MAX; i++) {
        EEPROM.write(WELL_NAME_ADDR + i, i < wellName.length() ? wellName.charAt(i) : '\0');
    }

    currentWell.wellName = wellName;
    currentWell.x = x;
    currentWell.y = y;
    currentWell.lDeg = Ldeg;
    currentWell.rDeg = Rdeg;
  }

  void loadCurrentWell() {
    char name[WELL_NAME_MAX];
    for (int i = 0; i < WELL_NAME_MAX; i++) {
        name[i] = EEPROM.read(WELL_NAME_ADDR + i);
    }
    name[WELL_NAME_MAX - 1] = '\0';

    if (strcmp(name, "HOME") == 0) {
      currentWell.wellName = "HOME";
      currentWell.x = 0;
      currentWell.y = 0;
      currentWell.lDeg = 0;
      currentWell.rDeg = 0;

      Serial.println("WELL:HOME");
    return;
  }

    if (name[0] < 'a' || name[0] > 'h') return;

    char row = name[0];
    int col = String(name + 1).toInt();
    if (col < 1 || col > 12) return;

    float x, y;
    wellToXY(row, col, x, y);

    float Ldeg = stepsToDegrees(stepperL.currentPosition());
    float Rdeg = stepsToDegrees(stepperR.currentPosition());

    currentWell.wellName = String(row) + String(col);
    currentWell.x = x;
    currentWell.y = y;
    currentWell.lDeg = Ldeg;
    currentWell.rDeg = Rdeg;

    printCurrentWell();
  }

  void printCurrentWell() {
    Serial.print("WELL:"); Serial.print(currentWell.wellName);
    Serial.print(",X="); Serial.print(currentWell.x, 2);
    Serial.print(",Y="); Serial.print(currentWell.y, 2);
    Serial.print(",L="); Serial.print(currentWell.lDeg, 2);
    Serial.print(",R="); Serial.println(currentWell.rDeg, 2);
  }

  void printStepSize() {
    Serial.print("STEP_SIZE:"); Serial.println(times, 2);
  }

  void printMicroSteps() {
    Serial.print("MICROSTEPS:1/"); Serial.println(currentMicrosteps);
  }