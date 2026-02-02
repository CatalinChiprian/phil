/* Created by Victoria Shvets
Based on Phillip Dettinger work availible on https://github.com/CSDGroup/PHIL.git */
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

AccelStepper stepperZ1(1, step[0], dir[0]);
AccelStepper stepperL(1, step[1], dir[1]);
AccelStepper stepperR(1, step[2], dir[2]);
AccelStepper stepperZ2(1, step[3], dir[3]);


int limitSwitchR = 30; // Target Limit Switch R
int limitSwitchZ1 = 33; // Target Limit Switch Z
int limitSwitchZ2 = 32; // Target Limit Switch Z

int microIndex = 4; // 0=full, 1=half, 2=1/4, 3=1/8, 4=1/16, 5=1/32
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

  for (int i = 0; i < 4; i++) {
    pinMode(ena[i], OUTPUT);
    digitalWrite(ena[i], LOW);
    pinMode(step[i], OUTPUT);
    pinMode(dir[i], OUTPUT);
  }

  //digitalWrite(limitSwitchL, HIGH);
  pinMode(limitSwitchR, INPUT_PULLUP);
  //digitalWrite(limitSwitchR, HIGH);
  pinMode(limitSwitchZ1, INPUT_PULLUP);
  //digitalWrite(limitSwitchZ1, HIGH);
  pinMode(limitSwitchZ2, INPUT_PULLUP);
  //digitalWrite(limitSwitchZ2, HIGH);
  
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
  calibrate(); 
  setupCalibration(); 

  int finalHome = home();
  if(finalHome == 1) {
    Serial.println("Final home complete - position reset to 0,0");
  } else {
    Serial.println("Final home failed");
    disableMotors(); 
    motorsCurrentlyEnabled = false;
    interruptibleDelay(1000);
  }


  emergencyStopRequested = false; 
  systemInitialized = true;
  Serial.println("System ready!");

  Serial.print("Post-home positions - L: ");
  Serial.print(stepperL.currentPosition());
  Serial.print(" | R: ");
  Serial.println(stepperR.currentPosition());

}

void loop() {
  basic_controls(); 
  switches(); 
  autoDisableMotors();
}

void basic_controls(){

  if (Serial.available() > 0) {
    String received = Serial.readStringUntil('\n');
    received.trim(); 
    Serial.print("Arduino received: ");
    Serial.println(received);
    
    if (received.length() > 0) {
      char cmd = received.charAt(0); 
      
      switch (cmd) {
        case 'b': // backward
          enableMotors();
          motorsCurrentlyEnabled = true;        
          lastMotorActivityTime = millis();     
          stepperR.move(5 * currentMicrosteps);
          stepperL.move(-4 * currentMicrosteps);
          Serial.print("PHIL moved backward \n");
        break;
    
        case 'f': // forward
          enableMotors();
          motorsCurrentlyEnabled = true;
          lastMotorActivityTime = millis();
          stepperR.move(-5 * currentMicrosteps);
          stepperL.move(4 * currentMicrosteps);
          Serial.print("PHIL moved forward \n");
        break;
    
        case 'l': // left
          enableMotors();
          motorsCurrentlyEnabled = true;
          lastMotorActivityTime = millis();
          stepperL.move(-4 * currentMicrosteps);
          stepperR.move(-3 * currentMicrosteps);
          Serial.print("PHIL moved left \n");
        break;
    
        case 'r': // right
          enableMotors();
          motorsCurrentlyEnabled = true;
          lastMotorActivityTime = millis();
          stepperL.move(4 * currentMicrosteps);
          stepperR.move(3 * currentMicrosteps);
          Serial.print("PHIL moved right \n");
        break;
    
        case 'u': // Up
          enableMotors();
          motorsCurrentlyEnabled = true;
          lastMotorActivityTime = millis();
          stepperZ1.move(6 * currentMicrosteps);
          stepperZ2.move(6 * currentMicrosteps);
          Serial.print("PHIL moved up \n");
        break;
    
        case 'd': // Down
          enableMotors();
          motorsCurrentlyEnabled = true;
          lastMotorActivityTime = millis();
          stepperZ1.move(-6 * currentMicrosteps);
          stepperZ2.move(-6 * currentMicrosteps);
          Serial.print("PHIL moved down \n");
        break;

        case 'h': // Home
          home();
          calibrate();  
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
            wells(row, received);  
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
      }
    }
  }

  stepperL.run();
  stepperR.run();
  stepperZ1.run();
  stepperZ2.run();
}

void switches(){
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
  
  
  static bool rWasPressed = false;
  if(digitalRead(limitSwitchR) == LOW) {
    if(!rWasPressed) {
      Serial.println("Limit R PRESSED");
      rWasPressed = true;
    }
    stepperR.setCurrentPosition(stepperR.currentPosition());
  } else {
    rWasPressed = false;
  }
}

int home(){

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
  
  int result = attemptHome(100* currentMicrosteps, 200 * currentMicrosteps, 4000, overallStartTime, overallTimeout);
  
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
  attemptHome(100* currentMicrosteps, 200 * currentMicrosteps, 4000, overallStartTime, overallTimeout);
}

int attemptHome(int speedR, int speedL, unsigned long timeout, unsigned long overallStartTime, unsigned long overallTimeout){
  
  stepperL.setSpeed(speedR);
  stepperR.setSpeed(speedL);  
  
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
  return 1; 
}

void wells(char row, String columnNum){  

  if(emergencyStopRequested) {
    emergencyStopRequested = false;  
    return;
  }

  enableMotors();

  int calibResult = calibrate(); 
  if(calibResult != 1) {
    Serial.println("Wells aborted - calibration failed");
    return;
  }
  calibrate(); 

  setupCalibration();

  // disableMotors(); 
  
  // interruptibleDelay(60000);

  // enableMotors(); 

  String columnStr = columnNum.substring(2);  
  int column = columnStr.toInt();
  
  Serial.print("Row: ");
  Serial.print(row);
  Serial.print(" | Column: ");
  Serial.println(column);

  setSlowSpeed(); 

  
  switch(row) {
      case 'a':
        switch(column) {
          case 1: 
            stabilizeDown(); 
            moveToWell(-60, -50, "A1"); // Motor L, Motor R, Well name
          break;
          
          case 2:
            stabilizeDown(); 
            moveToWell(-43, -52, "A2"); // Motor L, Motor R, Well name
          break;

          case 3:
            stabilizeDown(); 
            moveToWell(-38, -56, "A3"); // Motor L, Motor R, Well name
          break;

          case 4:
            stabilizeDown(); 
            moveToWell(-33, -60, "A4"); // Motor L, Motor R, Well name
          break;

          case 5:
            stabilizeDown(); 
            moveToWell(-29, -63, "A5"); // Motor L, Motor R, Well name
          break;

          case 6:
            stabilizeDown(); 
            moveToWell(-28, -67, "A6"); // Motor L, Motor R, Well name
          break;

          case 7:
            stabilizeDown(); 
            moveToWell(-20, -70, "A7"); // Motor L, Motor R, Well name
          break;

          case 8:
            stabilizeUp();
            moveToWell(-15, -72, "A8"); // Motor L, Motor R, Well name
          break;

          case 9:
            stabilizeUp();
            moveToWell(-13, -79, "A9"); // Motor L, Motor R, Well name
          break;

          case 10:
            stabilizeUp();
            moveToWell(-10, -81, "A10"); // Motor L, Motor R, Well name
          break;

          case 11:
            stabilizeUp();
            moveToWell(-4, -87, "A11"); // Motor L, Motor R, Well name
          break;

          case 12:
            stabilizeUp();
            moveToWell(22, -91, "A12"); // Motor L, Motor R, Well name
          break;

          default:
          Serial.println("Invalid column for row A");
          break;
        } 
        
      break;

      case 'b' :
      switch(column) {
          
          case 1: 
            stabilizeDown(); 
            moveToWell(-60, -45, "B1"); // Motor L, Motor R, Well name
          break;

          case 2: 
            stabilizeDown(); 
            moveToWell(-38, -49, "B2"); // Motor L, Motor R, Well name
          break;


          case 8:
            //stabilizeUp();
            stepperL.move(-15 * currentMicrosteps); 
            stepperR.move(-75 * currentMicrosteps);
            Serial.println("Moved to B8");
          break;

          case 12:
            stabilizeUp();  
            moveToWell(30, -90, "B12"); // Motor L, Motor R, Well name        
          break;

          default:
          Serial.println("Invalid column for row B");
          break;


          
      }

      break;

      case 'c' :
      switch(column) {
          
          case 1:
            stabilizeDown(); 
            moveToWell(-43, -41, "C1"); // Motor L, Motor R, Well name
          break;

          case 2:
            //stabilizeDown();
            stepperR.move(48 * currentMicrosteps); 
            stepperL.move(-40 * currentMicrosteps);
            Serial.println("Moving to D2");
          break;

          case 4:
            //stabilizeUp();
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
          
          case 1:
            stabilizeDown(); 
            moveToWell(-50, -33, "D1"); // Motor L, Motor R, Well name
          break;

          case 2:
            //stabilizeDown();
            stepperR.move(45 * currentMicrosteps); 
            stepperL.move(-36 * currentMicrosteps);
            Serial.println("Moving to D2");
          break;

          case 3:
            ///stabilizeDown();
            stepperR.move(50 * currentMicrosteps); 
            stepperL.move(-32 * currentMicrosteps);
            Serial.println("Moving to D3");
          break;

          case 7: 
            stabilizeUp(); 
            moveToWell(-14, -62, "D7"); // Motor L, Motor R, Well name
          break;

          case 12:
            stabilizeUp();
            moveToWell(43, -84, "D12"); // Motor L, Motor R, Well name        
          break;

          default:
          Serial.println("Invalid column for row D");
          break;
      }

      break;

      case 'e' :
      switch(column) {
          
          case 1:
            //stabilizeDown();
            stepperR.move(37 * currentMicrosteps); 
            stepperL.move(-14 * currentMicrosteps); 
            Serial.println("Moved to E1");
          break;

          case 2:
            //stabilizeDown();
            stepperR.move(43 * currentMicrosteps); 
            stepperL.move(-10 * currentMicrosteps);
            Serial.println("Moving to E2");
          break;

          case 7: 
            stabilizeUp(); 
            moveToWell(-12, -60, "E7"); // Motor L, Motor R, Well name
          break;

          default:
          Serial.println("Invalid column for row E");
          break;
      }

      break;

      case 'f' :
      switch(column) {
          
          case 1:
            //stabilizeDown();
            stepperR.move(30 * currentMicrosteps); 
            stepperL.move(-35 * currentMicrosteps);
            Serial.println("Moving to F1");
          break;

          case 2:
            //stabilizeDown();
            stepperR.move(40 * currentMicrosteps); 
            stepperL.move(-6 * currentMicrosteps);
            Serial.println("Moving to F2");
          break;

          case 3:
            //stabilizeDown();
            stepperR.move(45 * currentMicrosteps); 
            stepperL.move(-25 * currentMicrosteps);
            Serial.println("Moving to H2");
          break;

          default:
          Serial.println("Invalid column for row F");
          break;
      }

      break;

      case 'g' :
      switch(column) {
          
          case 1:
            //stabilizeDown();
            stepperR.move(29 * currentMicrosteps); 
            stepperL.move(-6 * currentMicrosteps); 
            Serial.println("Moved to G1");
          break;

          case 2:
            //stabilizeDown();
            stepperR.move(36 * currentMicrosteps); 
            stepperL.move(-2 * currentMicrosteps);
            Serial.println("Moving to G2");
          break;

          case 9:
            //stabilizeUp();
            stepperR.move(70* currentMicrosteps); 
            stepperL.move(2 * currentMicrosteps);
            Serial.println("Moved to G9");
          break;

          case 12:
            //stabilizeUp();
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
          
          case 1:
            stabilizeDown(); 
            moveToWell(-24, -19, "H1"); // Motor L, Motor R, Well name
            
          break;

          case 2:
            //stabilizeDown();
            stepperR.move(33 * currentMicrosteps); 
            stepperL.move(-20 * currentMicrosteps);
            Serial.println("Moving to H2");
          
          break;

          case 6:
            //stabilizeDown();
            stepperR.move(55* currentMicrosteps); 
            stepperL.move(-5 * currentMicrosteps);
            Serial.println("Moved to H6");
          break;

          case 7:
            stabilizeUp(); 
            moveToWell(-3, -55, "H7"); // Motor L, Motor R, Well name
          break;

          case 8:
            //stabilizeUp();
            stepperR.move(-60 * currentMicrosteps); 
            Serial.println("Moved to H8");
          break;

          case 10:
            //stabilizeUp();
            stepperR.move(71 * currentMicrosteps); 
            stepperL.move(40 * currentMicrosteps);
            Serial.println("Moved to H10");
          break;

          case 12:
            //stabilizeUp();
            stepperL.move(67* currentMicrosteps); 
            stepperR.move(-85* currentMicrosteps);
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

int calibrate(){

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

  stepperL.move(100 * currentMicrosteps);
 
  while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
    if(Serial.available() > 0) {
        char c = Serial.read();
        if(c == 's') {
          emergencyStop(); 
          Serial.println("STOPPED by user");
          return -1;
        }
    }
    stepperR.run();
    stepperL.run();
  }

  Serial.print("After calibration move - L: ");
  Serial.print(stepperL.currentPosition());
  Serial.print(" | R: ");
  Serial.println(stepperR.currentPosition());

  interruptibleDelay(1000);

  Serial.println("Homing again to reset position...");
  homeResult = home(); 
  if(homeResult != 1) {
    Serial.println("Calibration aborted - second homing failed");
    return homeResult;
  }
  
  Serial.print("After second home - L: ");
  Serial.print(stepperL.currentPosition());
  Serial.print(" | R: ");
  Serial.println(stepperR.currentPosition());
  
  interruptibleDelay(1000);

  Serial.println("=== CALIBRATION COMPLETE ===");
  return 1;
}


void moveToWell(int moveL, int moveR, String wellName) {

  enableMotors();

  stepperL.move(moveL * currentMicrosteps); 
  stepperR.move(moveR * currentMicrosteps); 
  
  while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
    stepperR.run();
    stepperL.run();
  }

  interruptibleDelay(1000);
  
  Serial.print("Moved to ");
  Serial.println(wellName);

}

void stabilizeDown(){

  if(emergencyStopRequested) {
    emergencyStopRequested = false;  
    return;
  }
  
  enableMotors(); 

  int calibResult = calibrate(); 
  if(calibResult != 1) {
    Serial.println("Aborted - calibration failed");
    return;
  }

  stepperL.move(-14 * currentMicrosteps); 
  stepperR.move(-70 * currentMicrosteps);

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


  Serial.println("Stabilizing...");

  interruptibleDelay(1000);

  calibResult = calibrate(); 
  if(calibResult != 1) {
    Serial.println("Aborted - calibration failed");
    return;
  }


}

void stabilizeUp(){

  if(emergencyStopRequested) {
    emergencyStopRequested = false;  
    return;
  }

  enableMotors(); 

  int calibResult = calibrate(); 
  if(calibResult != 1) {
    Serial.println("Aborted - calibration failed");
    return;
  }

  stepperL.move(-55 * currentMicrosteps); 
  stepperR.move(-55 * currentMicrosteps);

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

  Serial.println("Stabilizing...");

  interruptibleDelay(1000);

  calibResult = calibrate(); 
  if(calibResult != 1) {
    Serial.println("Aborted - calibration failed");
    return;
  }


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