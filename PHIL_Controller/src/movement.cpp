#include "../inc/movement.h"
#include "../inc/hardware.h"
#include "../inc/well_utils.h"
#include "../inc/calibration.h"
#include "../inc/eeprom_utils.h"
#include "../inc/actions.h"

static const int16_t steps = 4 * currentMicrosteps;

const int16_t ZMotorNormalPosition = -128;

int16_t times_x10 = 1;
uint8_t wellIndex = 0;

static long lastR;
static long lastL;

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

  wellIndex = WELL_UNKNOWN;
  saveCurrentWell(wellIndex);
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

  wellIndex = WELL_UNKNOWN;
  saveCurrentWell(wellIndex);
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

  wellIndex = WELL_UNKNOWN;
  saveCurrentWell(wellIndex);
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
  
  wellIndex = WELL_UNKNOWN;
  saveCurrentWell(wellIndex);
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

    wellIndex = WELL_HOME;
    saveCurrentWell(wellIndex);
    savePositions();
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

    char row; uint8_t col;
    wellStrToRowCol(wellName, row, col);

    wellIndex = RowColToWellIndex(row, col);

    saveCurrentWell(wellIndex);
    savePositions();
    printCurrentWell();
}

void goToHardcodedWells(char row, uint8_t column) {  
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

    wellIndex = RowColToWellIndex(row, col);

    saveCurrentWell(wellIndex);
    savePositions();
    printCurrentWell();
}

void updatePositionState() {
  long currentL = stepperL.currentPosition();
  long currentR = stepperR.currentPosition();

  if (currentL != lastL || currentR != lastR) {
    printPosition(currentL, currentR, stepperZ1.currentPosition(), stepperZ2.currentPosition());
    lastL = currentL;
    lastR = currentR;
  }
}

void goToWasteContainer() {
	enableLMotor();
	enableRMotor();

	stepperL.moveTo(55 * currentMicrosteps); 
	stepperR.moveTo(-5.5 * currentMicrosteps); 
	
	while(stepperR.distanceToGo() != 0 || stepperL.distanceToGo() != 0) {
	  stepperR.run();
	  stepperL.run();
	}

  wellIndex = WELL_CONTAINER;
  saveCurrentWell(wellIndex);
	savePositions();
}

void increaseStepSize() {
  times_x10 += 1;
}

void decreaseStepSize() {
  times_x10 -= 1;
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
    if (isInvalidWell(row, col)) return;
    
    wellToXY(row, col, x, y);

    Ldeg = stepsToDegrees(stepperL.currentPosition());
    Rdeg = stepsToDegrees(stepperR.currentPosition());
  }

  Serial.print(F("WELL:"));
  Serial.print(F("Name="));
  if (wellIndex == WELL_HOME) Serial.print(F("HOME"));
  else if (wellIndex == WELL_UNKNOWN) Serial.print(F("UNKNOWN"));
  else if (wellIndex == WELL_CONTAINER) Serial.print(F("CONTAINER"));
  else { Serial.print(row); Serial.print(col); }
  Serial.print(F(",X=")); Serial.print(x);
  Serial.print(F(",Y=")); Serial.print(y);
  Serial.print(F(",L=")); Serial.print(Ldeg);
  Serial.print(F(",R=")); Serial.println(Rdeg);
}

void printPosition(int16_t L, int16_t R, int16_t Z1, int16_t Z2) {
  Serial.print(F("POS:L=")); Serial.print(L);
  Serial.print(F(",R=")); Serial.print(R);
  Serial.print(F(",Z1=")); Serial.print(Z1);
  Serial.print(F(",Z2=")); Serial.println(Z2);
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