/**
 * Main entry point of the PHIL firmware.
 * 
 * Created by Catalin Chiprian
 * Based on work by Philipp Dettinger:
 * https://github.com/CSDGroup/PHIL.git
 * 
 * Source code available at:
 * https://github.com/CatalinChiprian/phil/tree/main/PHIL_Controller
 * 
 * This file initializes the system and runs the main control loop.
 * The loop continuously processes commands, updates system state,
 * and handles automated actions.
 */
#include "inc/commands.h"
#include "inc/movement.h"
#include "inc/hardware.h"
#include "inc/actions.h"
#include "inc/calibration.h"
#include "inc/eeprom_utils.h"

/**
 * setup()
 * 
 * Runs once at startup.
 * Initializes communication, hardware, and persistent state.
 */
void setup() {
  Serial.begin(9600);
  initHardware();
  initPersistentState();
}


/**
 * loop()
 * 
 * Main execution loop.
 * Runs continuously and coordinates all firmware operations.
 * 
 * Execution order:
 * 1. Safety checks
 * 2. Command processing
 * 3. Hardware state updates
 * 4. Action scheduling
 * 5. Power management
 * 6. State reporting
 */
void loop() {
  checkFaults();
  parseCommands(); 
  checkSwitches();
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