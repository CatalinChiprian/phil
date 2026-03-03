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

    float plateX0 = 0.0;
    float plateY0 = 0.0;

    const int MAX_CAL = 96;
    float calX[MAX_CAL], calY[MAX_CAL], calL[MAX_CAL], calR[MAX_CAL];
    int calCount = 0;

    const int TERMS = 10;
    float ML[TERMS] = {0};
    float MR[TERMS] = {0};
    bool mapReady = false;


  int MICROoptions[] = {1, 2, 4, 8, 16, 32};

  int M1 = 25; 
  int M2 = 26; 
  int M3 = 27; 

  int ena[] = {10, 47, 50, 13};
  int step[] = {9, 46, 49, 12};
  int dir[] = {8, 45, 48, 11};   

  long lastR;
  long lastL;

  AccelStepper stepperZ1(1, step[0], dir[0]);
  AccelStepper stepperL(1, step[1], dir[1]);
  AccelStepper stepperR(1, step[2], dir[2]);
  AccelStepper stepperZ2(1, step[3], dir[3]);


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

  bool systemInitialized = false;

  unsigned long lastMotorActivityTime = 0;
  bool motorsCurrentlyEnabled = false;
  const unsigned long MOTOR_TIMEOUT = 5000;  // 5 seconds
  bool emergencyStopRequested = false;

  void setup() {
    Serial.begin(9600);

    Serial.println("Hello from Arduino!");
    interruptibleDelay(1000);

    enableMotors();

    //digitalWrite(limitSwitchL, HIGH);
    pinMode(limitSwitchL, INPUT_PULLUP);
    pinMode(limitSwitchR, INPUT_PULLUP);
    //digitalWrite(limitSwitchR, HIGH);
    pinMode(limitSwitchZ1, INPUT_PULLUP);
    //digitalWrite(limitSwitchZ1, HIGH);
    pinMode(limitSwitchZ2, INPUT_PULLUP);
    //digitalWrite(limitSwitchZ2, HIGH);

    pinMode(faultR, INPUT_PULLUP);
    pinMode(faultL, INPUT_PULLUP);
    
    pinMode(M1, OUTPUT); 
    digitalWrite(M1, Sttngs[microIndex][0]); 
    pinMode(M2, OUTPUT); 
    digitalWrite(M2, Sttngs[microIndex][1]); 
    pinMode(M3, OUTPUT); 
    digitalWrite(M3, Sttngs[microIndex][2]);

    stepperL.setMaxSpeed(1000 * currentMicrosteps);
    stepperR.setMaxSpeed(1000 * currentMicrosteps);
    stepperZ1.setMaxSpeed(1000 * currentMicrosteps);
    stepperZ2.setMaxSpeed(1000 * currentMicrosteps);

    stepperL.setAcceleration(500 * currentMicrosteps);
    stepperR.setAcceleration(500 * currentMicrosteps);
    stepperZ1.setAcceleration(500 * currentMicrosteps);
    stepperZ2.setAcceleration(500 * currentMicrosteps);

    Serial.println("System starting - performing initial home...");
    interruptibleDelay(1000);


  
    if (!loadPositions()) {
      home();
    }

    loadCalibration();

    emergencyStopRequested = false; 
    systemInitialized = true;
    Serial.println("System ready!");

    Serial.print("Post-home positions - L: ");
    Serial.print(stepperL.currentPosition());
    Serial.print(" | R: ");
    Serial.println(stepperR.currentPosition());

  }

  void moveBackward() {
    enableMotors();

    // stepperL.move(-4 * currentMicrosteps);
    // stepperR.move(5 * currentMicrosteps);
    long s = steps * times;
    stepperL.moveTo(-s + stepperL.currentPosition());
    stepperR.moveTo(s + stepperR.currentPosition());

      while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
        stepperL.run();
        stepperR.run();
      }
  }

  void moveForward() {
    enableMotors();

    // stepperL.move(4 * currentMicrosteps);
    // stepperR.move(-5 * currentMicrosteps);
    long s = steps * times;
    stepperL.moveTo(s + stepperL.currentPosition());
    stepperR.moveTo(-s + stepperR.currentPosition());

      while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
        stepperL.run();
        stepperR.run();
      }
  }

  void moveLeft() {
    enableMotors();

    // stepperL.move(-4 * times * currentMicrosteps);
    // stepperR.move(-3 * times * currentMicrosteps);
    long s = steps * times;
    stepperL.moveTo(-s + stepperL.currentPosition());
    stepperR.moveTo(-s + stepperR.currentPosition());

      while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
        stepperL.run();
        stepperR.run();
      }
  }

  void moveRight() {
    enableMotors();

    // stepperL.move(4 * times * currentMicrosteps);
    // stepperR.move(3 * times * currentMicrosteps);

    long s = steps * times;
    stepperL.moveTo(s + stepperL.currentPosition());
    stepperR.moveTo(s + stepperR.currentPosition());

      while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
        stepperL.run();
        stepperR.run();
      }
  }

  void loop() {
    checkFaults();
    basic_controls(); 
    switches(); 
    autoDisableMotors();

    long currentL = stepperL.currentPosition();
    long currentR = stepperR.currentPosition();

    if (currentL != lastL) {
      Serial.print("Position L:");
      Serial.println(currentL);
      lastL = currentL;
    }

    if (currentR != lastR) {
      Serial.print("Position R:");
      Serial.println(currentR);
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
          case 'b': // backward
            moveBackward();
            Serial.print("PHIL moved backward \n");
            savePositions();
          break;
      
          case 'f': // forward
            moveForward();
            Serial.print("PHIL moved forward \n");
            savePositions();
          break;
      
          case 'l': // left
            moveLeft();
            Serial.print("PHIL moved left \n");
            savePositions();
          break;
      
          case 'r': // right
            moveRight();
            Serial.print("PHIL moved right \n");
            savePositions();
          break;
      
          case 'u': // Up
            enableMotors();
            stepperZ1.move(6 * currentMicrosteps);
            stepperZ2.move(6 * currentMicrosteps);
            Serial.print("PHIL moved up \n");
            savePositions();
          break;
      
          case 'd': // Down
            enableMotors();
            stepperZ1.move(-6 * currentMicrosteps);
            stepperZ2.move(-6 * currentMicrosteps);
            Serial.print("PHIL moved down \n");
            savePositions();
          break;

          case 'h': // Home
            goToOrigin();
            interruptibleDelay(500);
            disableMotors();
            savePositions();  
          break; 

          case 'p': // Print positions
            Serial.print("Positions - L: ");
            Serial.print(stepperL.currentPosition());
            Serial.print(" | R: ");
            Serial.print(stepperR.currentPosition());
            Serial.print(" | Z1: ");
            Serial.print(stepperZ1.currentPosition());
            Serial.print(" | Z2: ");
            Serial.println(stepperZ2.currentPosition());
          break;

          case 'c':
            calibrate();
          break;

          case 'z':
             // Example commands:
            // z a1
            // z a12
            // z h1
            // z solve
            // z print

            if (received.startsWith("z solve")) {
                solveMapping();
            }
            else if (received.startsWith("z deleteidx")) {
              int idx = received.substring(12).toInt();
              if (idx < 0 || idx >= calCount) {
                Serial.println("Invalid index");
              } else {
                Serial.print("Deleting Pt "); Serial.print(idx);
                Serial.print(": XY("); Serial.print(calX[idx]);
                Serial.print(","); Serial.print(calY[idx]); Serial.println(")");
                for (int i = idx; i < calCount-1; i++) {
                  calX[i]=calX[i+1]; calY[i]=calY[i+1];
                  calL[i]=calL[i+1]; calR[i]=calR[i+1];
                }
                calCount--;
                Serial.print(calCount); Serial.println(" points remaining");
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
                        Serial.print("Point not found for ");
                        Serial.print(row); Serial.println(col);
                    } else {
                        // Shift all points after it down by one
                        for (int i = foundIdx; i < calCount - 1; i++) {
                            calX[i] = calX[i+1];
                            calY[i] = calY[i+1];
                            calL[i] = calL[i+1];
                            calR[i] = calR[i+1];
                        }
                        calCount--;
                        Serial.print("Deleted point ");
                        Serial.print(row); Serial.print(col);
                        Serial.print(" — "); Serial.print(calCount);
                        Serial.println(" points remaining");
                        Serial.println("Run 'z solve' to update the map");
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
                Serial.println("ML (Ldeg) coeffs [1, x, y, x^2, x*y, y^2, x^3, x^2y, xy^2, y^3]:");
                for (int i=0;i<TERMS;i++) { Serial.print(ML[i],5); Serial.print(i<TERMS-1?' ':'\n'); }
                Serial.println("MR (Rdeg) coeffs [1, x, y, x^2, x*y, y^2, x^3, x^2y, xy^2, y^3]:");
                for (int i=0;i<TERMS;i++) { Serial.print(MR[i],5); Serial.print(i<TERMS-1?' ':'\n'); }

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

          case 'w': // Go to well
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
              goToWell(row, column);
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

          case 'o':
            enableMotors();

            stepperL.moveTo(60 * currentMicrosteps); 
            stepperR.moveTo(-5.5 * currentMicrosteps); 
            
            while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
              stepperR.run();
              stepperL.run();
            }

          break;

          case '+':
            times += 0.10;
            Serial.print("Increased to: ");
            Serial.println(times);
          break;

          case '-':
            times -= 0.10;
            Serial.print("Decreased to: ");
            Serial.println(times);
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
        Serial.println("Limit Z1 PRESSED");
        z1WasPressed = true;
      }
      stepperZ1.setCurrentPosition(stepperZ1.currentPosition());
      stepperZ2.setCurrentPosition(stepperZ2.currentPosition());
    } else {
      z1WasPressed = false; 
    }

    static bool z2WasPressed = false;
    if(digitalRead(limitSwitchZ2) == LOW) {
      if(!z2WasPressed) {
        Serial.println("Limit Z2 PRESSED");
        z2WasPressed = true;
      }
      stepperZ1.setCurrentPosition(stepperZ1.currentPosition());
      stepperZ2.setCurrentPosition(stepperZ2.currentPosition());
    } else {
      z2WasPressed = false;
    }
    
    
    static bool lWasPressed = false;
    if(digitalRead(limitSwitchL) == LOW) {
      if (!lWasPressed) {
        Serial.println("Limit L PRESSED");
        lWasPressed = true;
      }
      stepperL.stop();
      stepperL.setCurrentPosition(stepperL.currentPosition());
    }
    else {
      lWasPressed = false;
    }
    
    static bool rWasPressed = false;
    if(digitalRead(limitSwitchR) == LOW) {
      if(!rWasPressed) {
        Serial.println("Limit R PRESSED");
        rWasPressed = true;
      }
      stepperR.stop();
      stepperR.setCurrentPosition(stepperR.currentPosition());
    } else {
      rWasPressed = false;
    }
  }
  
  int goToOrigin() {
    enableMotors();
    stepperL.moveTo(0); 
    stepperR.moveTo(0); 
    
      while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
        stepperR.run();
        stepperL.run();
      }
  }

  int home() {
    if(emergencyStopRequested) {
      emergencyStopRequested = false; 
      return -1;
    }
    enableMotors(); 

    Serial.println("Checking if safe pre-positioning needed...");
    
    long currentL = stepperL.currentPosition();
    long currentR = stepperR.currentPosition();
    
    Serial.print("Current position - L: ");
    Serial.print(currentL);
    Serial.print(" | R: ");
    Serial.println(currentR);
    
    bool needsPrePositioning = false;
    
    if (currentL <= 600 && currentL >= -100 && currentR >= -1800 && currentR <= -1200) {
      Serial.println("Zone 1 detected - moving to safe position");
      stepperL.move(-20 * currentMicrosteps); 
      stepperR.move(20 * currentMicrosteps); 
      needsPrePositioning = true;
    }

    if (needsPrePositioning) {
      while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
        // Add stop check here too
        if(Serial.available() > 0) {
          char c = Serial.read();
          if(c == 's') {
            emergencyStop(); 
            Serial.println("Pre-positioning STOPPED by user");
            return -1;
          }
        }
        stepperR.run();
        stepperL.run();
      }
      interruptibleDelay(1000);
      Serial.println("Safe position reached - starting homing");
    } else {
      Serial.println("Already in safe zone - proceeding with homing");
    }

    Serial.println("Homing - Attempt 1... (send 's' to stop)");
    
    unsigned long overallStartTime = millis();
    unsigned long overallTimeout = 20000;
    
    int result = attemptHome(50 * currentMicrosteps, 100 * currentMicrosteps, 4000, overallStartTime, overallTimeout);
    
    if(result == 1) {
      Serial.println("PHIL homed (Attempt 1)");
      return 1; 
    }
    
    if(result == -1) {
      return -1; 
    }
    
    if(result == -2) {
    Serial.println("Overall timeout reached - stopping. Homing FAILED, manually move needle to the middle and try to home again.");
    return -2; 
    }

    // First attempt timed out (result == 0)
    Serial.println("First attempt timed out - preparing retry...");

    // Reset motor states
    stepperL.stop();
    stepperR.stop();
    stepperL.setSpeed(0);
    stepperR.setSpeed(0);

    interruptibleDelay(1000);

    Serial.println("Moving back for retry...");
    stepperR.move(-20 * currentMicrosteps); 
    stepperL.move(-80 * currentMicrosteps); 

    Serial.print("Distance to go - L: ");
    Serial.print(stepperL.distanceToGo());
    Serial.print(" | R: ");
    Serial.println(stepperR.distanceToGo());

    unsigned long retryMoveStart = millis();
    while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
      if(millis() - retryMoveStart > 5000) {
        Serial.println("Retry movement stuck - aborting");
        return -2;
      }
      
      if(Serial.available() > 0) {
        char c = Serial.read();
        if(c == 's') {
          emergencyStop(); 
          Serial.println("Homing retry STOPPED by user");
          return -1;
        }
      }
      
      stepperR.run();
      stepperL.run();
    }

    Serial.println("Retry position reached");
    interruptibleDelay(1000);
    attemptHome(50 * currentMicrosteps, 100 * currentMicrosteps, 4000, overallStartTime, overallTimeout);
  }

  int attemptHome(int speedR, int speedL, unsigned long timeout, unsigned long overallStartTime, unsigned long overallTimeout) {
    
    enterHomingMode(speedL, speedR); 
    
    unsigned long startTime = millis();
    
    while(digitalRead(limitSwitchR) == HIGH){
      if(millis() - overallStartTime > overallTimeout) {
        Serial.println("Overall timeout, check for problems and obstacles");
        stepperR.setSpeed(0);
        stepperL.setSpeed(0);
        return -2; 
      }
      
      if(Serial.available() > 0) {
        char c = Serial.read();
        if(c == 's') {
          emergencyStop(); 
          Serial.println("Homing STOPPED by user");
          return -1; 
        }
      }
      
      if(millis() - startTime > timeout) {
        Serial.println("Timeout - second attempt...");
        stepperR.setSpeed(0);
        stepperL.setSpeed(0);
        return 0;
      }
      
      stepperR.runSpeed();
      stepperL.runSpeed();
    }
    
    // Success - limit switch pressed
    stepperR.setSpeed(0);
    stepperL.setSpeed(0);
    stepperR.setCurrentPosition(0);
    stepperL.setCurrentPosition(0);

    exitHomingMode();
    return 1; 
  }

  void wells(char row, int column) {  

    if(emergencyStopRequested) {
      emergencyStopRequested = false;  
      return;
    }

    enableMotors();
    
    Serial.print("Row: ");
    Serial.print(row);
    Serial.print(" | Column: ");
    Serial.println(column);
 
    switch(row) {
        case 'a':
          switch(column) {
            
            case 1: 
              moveToWell(12, -40.25, "A1"); // Motor L, Motor R, Well name
            break;

            case 6:
              moveToWell(37.3, -57.5, "A6"); // Motor L, Motor R, Well name
            break;

            case 12:
              moveToWell(66.7, -83.5, "A12"); // Motor L, Motor R, Well name
            break;

            default:
            Serial.println("Invalid column for row A");
            break;
          } 
          
        break;

        case 'd' :
        switch(column) {
            
            case 1: 
              moveToWell(20, -25, "D1"); // Motor L, Motor R, Well name
            break;

            case 6:
              moveToWell(43, -49, "D6"); // Motor L, Motor R, Well name
            break;

            case 12:
              moveToWell(67, -71.5, "D12"); // Motor L, Motor R, Well name        
            break;

            default:
            Serial.println("Invalid column for row D");
            break;
        }

        break;

        case 'h' :
        switch(column) {
            case 1:
              moveToWell(39.875, -8.5, "H1"); // Motor L, Motor R, Well name
            break;

            case 6:
            moveToWell(55.5, -38.5, "H6"); // Motor L, Motor R, Well name
            break;

            case 12:
              moveToWell(80.5, -67.5, "H12"); // Motor L, Motor R, Well name
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

    setNormalSpeed(); 

  }

  int calibrate() {

    if(emergencyStopRequested) {
      emergencyStopRequested = false;  
      return -1;
    }
    enableMotors(); 

    Serial.println("=== CALIBRATION START ===");
    
    int homeResult = home(); 
    if(homeResult != 1) {  
      Serial.println("Calibration aborted - homing failed");
      return homeResult;  
    }
    
    Serial.print("After first home - L: ");
    Serial.print(stepperL.currentPosition());
    Serial.print(" | R: ");
    Serial.println(stepperR.currentPosition());
    
    interruptibleDelay(1000);
    Serial.println("Calibrating - moving L motor...");

    stepperL.setSpeed(10 * currentMicrosteps);
    
    unsigned long pushStart = millis();
    while (millis() - pushStart < 2500) {

        stepperL.runSpeed();
    }

    stepperL.setSpeed(0);

    stepperR.setCurrentPosition(0);
    stepperL.setCurrentPosition(0);

    Serial.println("=== CALIBRATION COMPLETE ===");

    // Move to absolute value (Middle)

    disableMotors();
    interruptibleDelay(500);
    enableMotors();

    stepperR.moveTo(stepperR.currentPosition() - 23.8 * currentMicrosteps);
    stepperL.moveTo(stepperL.currentPosition() - 62.8 * currentMicrosteps);

    while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
      stepperR.run();
      stepperL.run();
    }

    stepperR.setCurrentPosition(0);
    stepperL.setCurrentPosition(0);

    return 1;
  }

  void moveToWell(long moveL, long moveR, String wellName) {

    enableMotors();

    stepperL.moveTo(moveL * currentMicrosteps); 
    stepperR.moveTo(moveR * currentMicrosteps); 
    
    while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
      stepperR.run();
      stepperL.run();
    }

    interruptibleDelay(1000);
    
    Serial.print("Moved to ");
    Serial.println(wellName);

  }


  void setSlowSpeed() {
    stepperL.setMaxSpeed(200 * currentMicrosteps);
    stepperR.setMaxSpeed(200 * currentMicrosteps);
    stepperL.setAcceleration(100 * currentMicrosteps);
    stepperR.setAcceleration(100 * currentMicrosteps);
  }

  void setNormalSpeed() {
    stepperL.setMaxSpeed(1000 * currentMicrosteps);
    stepperR.setMaxSpeed(1000 * currentMicrosteps);
    stepperL.setAcceleration(500 * currentMicrosteps);
    stepperR.setAcceleration(500 * currentMicrosteps);
  }

  void enableMotors() {
    if (motorsCurrentlyEnabled) return;
    for (int i = 0; i < 4; i++) {
      digitalWrite(ena[i], LOW);  // LOW = enabled (motors energized)
    }
    motorsCurrentlyEnabled = true;     
    lastMotorActivityTime = millis();   
  }

  void disableMotors() {
    for (int i = 0; i < 4; i++) {
      digitalWrite(ena[i], HIGH);  // HIGH = disabled (motors off)
    }

    motorsCurrentlyEnabled = false;
  }

  void autoDisableMotors() {
    // Check if any motor is moving
    bool isMoving = (stepperL.distanceToGo() != 0 || 
                    stepperR.distanceToGo() != 0 || 
                    stepperZ1.distanceToGo() != 0 || 
                    stepperZ2.distanceToGo() != 0);
    
    if(isMoving) {
      lastMotorActivityTime = millis();  // Reset timer while moving
      if(!motorsCurrentlyEnabled) {
        enableMotors();
        motorsCurrentlyEnabled = true;
      }
    } else {
      // Motors are idle
      if(motorsCurrentlyEnabled && (millis() - lastMotorActivityTime > MOTOR_TIMEOUT)) {
        disableMotors();
        motorsCurrentlyEnabled = false;
        Serial.println("Motors auto-disabled after timeout");
      }
    }
  }

  void emergencyStop() {
    stepperL.stop();
    stepperR.stop();
    stepperZ1.stop();
    stepperZ2.stop();
    disableMotors();
    motorsCurrentlyEnabled = false;
    lastMotorActivityTime = 0;
    emergencyStopRequested = true;
    Serial.println("EMERGENCY STOP - Motors disabled");
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
        Serial.println("!!! DRIVER FAULT (nFAULT LOW) - stopping motors !!!");
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

  setNormalSpeed();
  }

  void savePositions() {
  EEPROM.put(0, stepperL.currentPosition());
  EEPROM.put(4, stepperR.currentPosition());
  EEPROM.put(8, stepperZ1.currentPosition());
  EEPROM.put(12, stepperZ2.currentPosition());

  byte ok = 123;
  EEPROM.put(16, ok);

  Serial.println("Saved stepper positions to EEPROM");
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

  Serial.print("Loaded L=");
  Serial.print(L);
  Serial.print(" R=");
  Serial.print(R);
  Serial.print(" Z1=");
  Serial.print(Z1);
  Serial.print(" Z2=");
  Serial.println(Z2);

  return true;
  }

void saveCalibration() {
  int addr = 20;
  byte magic = 0xCC;  // bump magic byte so old saves are invalidated
  EEPROM.put(addr, magic);  addr += sizeof(magic);

  // Save coefficients
  for (int i = 0; i < TERMS; i++) { EEPROM.put(addr, ML[i]); addr += sizeof(float); }
  for (int i = 0; i < TERMS; i++) { EEPROM.put(addr, MR[i]); addr += sizeof(float); }

  // Save point count
  EEPROM.put(addr, calCount);  addr += sizeof(int);

  // Save each point
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
    Serial.println("No valid calibration in EEPROM (or old format)");
    return false;
  }

  // Load coefficients
  for (int i = 0; i < TERMS; i++) { EEPROM.get(addr, ML[i]); addr += sizeof(float); }
  for (int i = 0; i < TERMS; i++) { EEPROM.get(addr, MR[i]); addr += sizeof(float); }

  // Load point count
  EEPROM.get(addr, calCount);  addr += sizeof(int);
  if (calCount < 0 || calCount > MAX_CAL) {
    Serial.println("Corrupt point count in EEPROM");
    calCount = 0;
    return false;
  }

  // Load each point
  for (int i = 0; i < calCount; i++) {
    EEPROM.get(addr, calX[i]);  addr += sizeof(float);
    EEPROM.get(addr, calY[i]);  addr += sizeof(float);
    EEPROM.get(addr, calL[i]);  addr += sizeof(float);
    EEPROM.get(addr, calR[i]);  addr += sizeof(float);
  }

  mapReady = true;
  Serial.print("Calibration loaded: ");
  Serial.print(calCount);
  Serial.println(" points");
  Serial.print("ML: ");
  for (int i = 0; i < TERMS; i++) { Serial.print(ML[i], 4); Serial.print(" "); }
  Serial.println();
  Serial.print("MR: ");
  for (int i = 0; i < TERMS; i++) { Serial.print(MR[i], 4); Serial.print(" "); }
  Serial.println();
  return true;
}

void wellToXY(char row, int col, float &x, float &y) {
    row = tolower(row);
    int r = row - 'a';  // a=0, b=1, ... h=7
    
    x = plateX0 + (col - 1) * WELL_DX;
    y = plateY0 + r * WELL_DY;

    Serial.print("X: ");
    Serial.println(x);
    Serial.print("Y: ");
    Serial.println(y);
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
        Serial.println("Already have max calibration points!");
        return;
    }

    float x, y;
    wellToXY(row, col, x, y);

    calX[calCount] = x;
    calY[calCount] = y;

    calL[calCount] = stepsToDegrees(stepperL.currentPosition());
    calR[calCount] = stepsToDegrees(stepperR.currentPosition());

    Serial.print("Recorded ");
    Serial.print(row);
    Serial.print(col);
    Serial.print(": XY=(");
    Serial.print(x);
    Serial.print(",");
    Serial.print(y);
    Serial.print(")  Angles=(");
    Serial.print(calL[calCount]);
    Serial.print(",");
    Serial.print(calR[calCount]);
    Serial.println(")");

    calCount++;
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
  if (calCount < TERMS) { // need at least 6 points for 6 unknowns
    Serial.println("Need at least 6 calibration points for quadratic fit");
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
    Serial.println("solveMapping: normal matrix singular — choose non-collinear, well-spread points");
    mapReady = false;
    return false;
  }

  // Commit
  for (int i=0;i<TERMS;i++) { ML[i] = MLtmp[i]; MR[i] = MRtmp[i]; }
  mapReady = true;
  saveCalibration();

  Serial.println("=== MAPPING SOLVED (quadratic least-squares) ===");
  Serial.print("Points used: "); Serial.println(calCount);

  Serial.println("ML (Ldeg) coefficients [1, x, y, x^2, x*y, y^2]:");
  for (int i=0;i<TERMS;i++) { Serial.print(ML[i], 5); Serial.print(i<TERMS-1?' ':'\n'); }

  Serial.println("MR (Rdeg) coefficients [1, x, y, x^2, x*y, y^2]:");
  for (int i=0;i<TERMS;i++) { Serial.print(MR[i], 5); Serial.print(i<TERMS-1?' ':'\n'); }

  // Residuals to assess fit quality
  Serial.println("--- Residuals (deg) ---");
  float maxErrL = 0, maxErrR = 0, rmsL = 0, rmsR = 0;
  for (int i=0; i<calCount; i++) {
    float x = calX[i], y = calY[i];
    float b[TERMS] = { 1.0f, x, y, x*x, x*y, y*y, x*x*x, x*x*y, x*y*y, y*y*y };
    float predL = dot10(ML, b);
    float predR = dot10(MR, b);
    float errL  = calL[i] - predL;
    float errR  = calR[i] - predR;
    rmsL += errL*errL; rmsR += errR*errR;
    if (fabs(errL) > maxErrL) maxErrL = fabs(errL);
    if (fabs(errR) > maxErrR) maxErrR = fabs(errR);
    Serial.print("Pt "); Serial.print(i);
    Serial.print(": errL="); Serial.print(errL, 3);
    Serial.print("  errR="); Serial.println(errR, 3);
  }
  rmsL = sqrtf(rmsL / calCount);
  rmsR = sqrtf(rmsR / calCount);
  Serial.print("Max |err| L="); Serial.print(maxErrL,3);
  Serial.print("  R=");         Serial.print(maxErrR,3);
  Serial.print("   RMS L=");    Serial.print(rmsL,3);
  Serial.print("  R=");         Serial.println(rmsR,3);

  return true;
}

long degToSteps(float deg) {
  // Adjust for your microstepping + gearbox if you have one
  float stepsPerRev = 200.0 * currentMicrosteps;   // Example: 16x microstepping
  return (long)(deg * (stepsPerRev / 360.0));
}

void goToWell(char row, int col) {
  enableMotors();

    float x, y;
    wellToXY(row, col, x, y);

    float Ldeg, Rdeg;
    xyToAngles(x, y, Ldeg, Rdeg);

    Serial.print("L deg");
    Serial.println(Ldeg);
    Serial.print("R deg");
    Serial.println(Rdeg);

    long Lsteps = degToSteps(Ldeg);
    long Rsteps = degToSteps(Rdeg);

    stepperL.moveTo(Lsteps);
    stepperR.moveTo(Rsteps);

    while (stepperL.distanceToGo() != 0 || stepperR.distanceToGo() != 0) {
        stepperL.run();
        stepperR.run();
    }

    Serial.print("Moved to well ");
    Serial.print(row);
    Serial.println(col);
}

float stepsToDegrees(long steps) {
    float stepsPerRev = 200.0f * currentMicrosteps;
    return (float)steps * (360.0f / stepsPerRev);
}

long degToStepsCurrent(float deg) {
  const float stepsPerRev = 200.0f * currentMicrosteps;
  return (long)(deg * (stepsPerRev / 360.0f));
}