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


    // calibrate(); 
    // setupCalibration(); 

    // int finalHome = home();
    // if(finalHome == 1) {
    //   Serial.println("Final home complete - position reset to 0,0");
    // } else {
    //   Serial.println("Final home failed");
    //   disableMotors(); 
    //   motorsCurrentlyEnabled = false;
    //   interruptibleDelay(1000);
    // }


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

    stepperL.move(-4 * currentMicrosteps);
    stepperR.move(5 * currentMicrosteps);
  }

  void moveForward() {
    enableMotors();

    stepperL.move(4 * currentMicrosteps);
    stepperR.move(-5 * currentMicrosteps);
  }

  void moveLeft(int times = 1) {
    enableMotors();

    stepperL.move(-4 * times * currentMicrosteps);
    stepperR.move(-3 * times * currentMicrosteps);
  }

  void moveRight(int times = 1) {
    enableMotors();

    stepperL.move(4 * times * currentMicrosteps);
    stepperR.move(3 * times * currentMicrosteps);
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

          case 'q': 
            disableMotors(); 
            motorsCurrentlyEnabled = false;
            lastMotorActivityTime = 0;
            Serial.println("Motors manually disabled");
          break;     

          case 'e': 
            enableMotors(); 
            motorsCurrentlyEnabled = true;
            lastMotorActivityTime = millis();
            Serial.println("Motors manually enabled");
          break;   

          case 't': // Test disable
            Serial.println("Testing motor disable...");
            Serial.println("Setting ENA pins HIGH (should disable motors)");
            for (int i = 0; i < 4; i++) {
              digitalWrite(ena[i], HIGH);
            }
            interruptibleDelay(2000);
            Serial.println("Try to move the robot by hand now - motors should be free");
            interruptibleDelay(3000);
            Serial.println("Setting ENA pins LOW (should enable motors)");
            for (int i = 0; i < 4; i++) {
              digitalWrite(ena[i], LOW);
            }
            Serial.println("Motors should be locked now");
          break;

          case 'm':
            for (char i = 'a'; i <= 'h'; i++) {
              for (int j = 1; j <= 2; j++) {
                wells(i, j);
                interruptibleDelay(500);
                goToOrigin();
                interruptibleDelay(500);
                disableMotors();
                interruptibleDelay(500);
                Serial.println("Went to well ");
                Serial.print(i);
                Serial.print(j);
              }
            }
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

    // int calibResult = calibrate(); 
    // if(calibResult != 1) {
    //   Serial.println("Wells aborted - calibration failed");
    //   return;
    // }
    // calibrate(); 

    // setupCalibration();

    // disableMotors(); 
    
    // interruptibleDelay(60000);

    // enableMotors(); 
    
    Serial.print("Row: ");
    Serial.print(row);
    Serial.print(" | Column: ");
    Serial.println(column);

    //setSlowSpeed(); 

    
    switch(row) {
        case 'a':
          switch(column) {
            
            case 1: // V
              moveToWell(-60, -50.25, "A1"); // Motor L, Motor R, Well name
            break;
            
            case 2: // V
              moveToWell(-43.5, -54.375, "A2"); // Motor L, Motor R, Well name
            break;

            case 3:
              moveToWell(-38, -56, "A3"); // Motor L, Motor R, Well name
            break;

            case 4:
              moveToWell(-33, -60, "A4"); // Motor L, Motor R, Well name
            break;

            case 5:
              moveToWell(-29, -63, "A5"); // Motor L, Motor R, Well name
            break;

            case 6:
              moveToWell(-28, -67, "A6"); // Motor L, Motor R, Well name
            break;

            case 7:
              moveToWell(-20, -70, "A7"); // Motor L, Motor R, Well name
            break;

            case 8:
              moveToWell(-15, -72, "A8"); // Motor L, Motor R, Well name
            break;

            case 9:
              moveToWell(-13, -78, "A9"); // Motor L, Motor R, Well name
            break;

            case 10:
              moveToWell(-10, -81, "A10"); // Motor L, Motor R, Well name
            break;

            case 11:
              moveToWell(-4, -87, "A11"); // Motor L, Motor R, Well name
            break;

            case 12:
              moveLeft(10);

              while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
                stepperR.run();
                stepperL.run();
              }
              //interruptibleDelay(50000);
              moveToWell(31, -88, "A12"); // Motor L, Motor R, Well name
            break;

            default:
            Serial.println("Invalid column for row A");
            break;
          } 
          
        break;

        case 'b' :
        switch(column) {
            
            case 1: // V
              moveToWell(-55, -45.5, "B1"); // Motor L, Motor R, Well name
            break;

            case 2: // V
              moveToWell(-41.5, -50.5, "B2"); // Motor L, Motor R, Well name
            break;

            case 3: // V
              moveToWell(-36, -54.25, "B3"); // Motor L, Motor R, Well name
            break;


            case 8:
              stepperL.move(-15 * currentMicrosteps); 
              stepperR.move(-75 * currentMicrosteps);
              Serial.println("Moved to B8");
            break;

            case 12:
              moveToWell(30, -90, "B12"); // Motor L, Motor R, Well name        
            break;

            default:
            Serial.println("Invalid column for row B");
            break;


            
        }

        break;

        case 'c' :
        switch(column) {
            
            case 1: // V
              moveToWell(-51, -39, "C1"); // Motor L, Motor R, Well name
            break;

            case 2: // V
              moveToWell(-38.75, -46, "C2");
            break;

            case 4:
              stepperL.move(-30 * currentMicrosteps); 
              stepperR.move(-60 * currentMicrosteps);
              Serial.println("Moved to C4");
            break;

            default:
            Serial.println("Invalid column for row C");
            break;
        }

        break;

        case 'd' :
        switch(column) {
            
            case 1: // V
              moveToWell(-52, -34.5, "D1"); // Motor L, Motor R, Well name
            break;

            case 2: // V
              moveToWell(-35.75, -42, "D2");
            break;

            case 3:
              stepperR.move(50 * currentMicrosteps); 
              stepperL.move(-32 * currentMicrosteps);
              Serial.println("Moving to D3");
            break;

            case 7: 
              moveToWell(-14, -62, "D7"); // Motor L, Motor R, Well name
            break;

            case 12:
              moveToWell(43, -84, "D12"); // Motor L, Motor R, Well name        
            break;

            default:
            Serial.println("Invalid column for row D");
            break;
        }

        break;

        case 'e' :
        switch(column) {
            
            case 1: // V
              moveToWell(-45, -29.25, "E1"); 
            break;

            case 2: // V
              moveToWell(-31.5, -38.125, "E2");
            break;

            case 7: 
              moveToWell(-12, -60, "E7"); // Motor L, Motor R, Well name
            break;

            default:
            Serial.println("Invalid column for row E");
            break;
        }

        break;

        case 'f' :
        switch(column) {
            
            case 1: // V
              moveToWell(-39, -24.75, "F1");
            break;

            case 2: // V
              moveToWell(-27, -34.125, "F2");
            break;

            case 3:
              stepperR.move(45 * currentMicrosteps); 
              stepperL.move(-25 * currentMicrosteps);
              Serial.println("Moving to F3");
            break;

            default:
            Serial.println("Invalid column for row F");
            break;
        }

        break;

        case 'g' :
        switch(column) {
            
            case 1: // V
              moveToWell(-29, -21.25, "G1");
            break;

            case 2: // V
              moveToWell(-23.5, -31.25, "G2");
            break;

            case 9:
              stepperR.move(70* currentMicrosteps); 
              stepperL.move(2 * currentMicrosteps);
              Serial.println("Moved to G9");
            break;

            case 12:
              stepperR.move(86 * currentMicrosteps); 
              stepperL.move(58 * currentMicrosteps);
              Serial.println("Moved to G12");
            break;

            default:
            Serial.println("Invalid column for row G");
            break;
        }

        break;

        case 'h' :
        switch(column) {
            case 1: // V
              moveToWell(-24, -18.5, "H1"); // Motor L, Motor R, Well name
            break;

            case 2: // V
              moveToWell(-19, -27.25, "H2");
            break;

            case 6:
              stepperR.move(55* currentMicrosteps); 
              stepperL.move(-5 * currentMicrosteps);
              Serial.println("Moved to H6");
            break;

            case 7:
              moveToWell(-3, -55, "H7"); // Motor L, Motor R, Well name
            break;

            case 8:
              stepperR.move(-60 * currentMicrosteps); 
              Serial.println("Moved to H8");
            break;

            case 10:
              stepperR.move(71 * currentMicrosteps); 
              stepperL.move(40 * currentMicrosteps);
              Serial.println("Moved to H10");
            break;

            case 12:
              //moveToWellWithPreload(58 * currentMicrosteps, -76 * currentMicrosteps, "H12");
              stepperL.move(58 * currentMicrosteps); 
              stepperR.move(-76 * currentMicrosteps);
              Serial.println("Moved to H12");
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

    // move to absolute value (middle)

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

  void setupCalibration(){

    if(emergencyStopRequested) {
      emergencyStopRequested = false;  
      return;
    }
    
    enableMotors(); 


    Serial.println("Initializing...");


    stepperL.move(-14 * currentMicrosteps); 
    stepperR.move(-70 * currentMicrosteps);
  

    while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
      stepperR.run();
      stepperL.run();
    }

    int calibResult = calibrate(); 
    if(calibResult != 1) {
      Serial.println("Aborted - calibration failed");
      return;
    }

    interruptibleDelay(1000);

    stepperL.move(-55 * currentMicrosteps); 
    stepperR.move(-55 * currentMicrosteps);

    while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
      stepperR.run();
      stepperL.run();
    }

    calibResult = calibrate(); 
    if(calibResult != 1) {
      Serial.println("Aborted - calibration failed");
      return;
    }

    interruptibleDelay(1000);


    Serial.println("Calibration complete");

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