/* Created by Catalin Chiprian
  Based on Phillip Dettinger work availible on https://github.com/CSDGroup/PHIL.git */

#include "inc/commands.h"
#include "inc/movement.h"
#include "inc/hardware.h"
#include "inc/actions.h"
#include "inc/calibration.h"
#include "inc/eeprom_utils.h"

void setup() {
  Serial.begin(9600);
  initHardware();
  initPersistentState();

  // The pipette might jump on start-up, causing a mismatch between software and mechanical position.
  // On every start-up we must calibrate the home position.
  //calibrateHome();
}

void loop() {
  checkFaults();

  parseCommands(); 

  switches();

  processActions();

  autoDisableMotors();
  
  updatePositionState();
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
  }
}