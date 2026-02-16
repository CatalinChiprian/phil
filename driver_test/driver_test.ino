/*
  DRV8824/DRV8825 carrier "logic-side" debug test (no motor, no VMOT).
  Tests:
   - SLEEP/RESET/EN toggling
   - MODE0/1/2 setting
   - STEP pulse generation
   - DIR toggling
   - nFAULT monitoring (expects pull-up; nFAULT commonly open-drain)
  Notes:
   - This does NOT validate output stage current regulation (needs motor/load).
   - nFAULT may behave differently if VMOT is absent; we're mainly checking it's not stuck LOW / floating.
*/

const int PIN_SLEEP = 4;   // nSLEEP
const int PIN_RESET = 5;   // nRESET
const int PIN_EN    = 6;   // ENBL (active LOW on many carriers)
const int PIN_STEP  = 7;   // STEP
const int PIN_DIR   = 8;   // DIR

const int PIN_M0    = 9;   // MODE0
const int PIN_M1    = 10;  // MODE1
const int PIN_M2    = 11;  // MODE2

const int PIN_NFAULT = 12; // nFAULT (active LOW, often open-drain)

void setMicrostepFull() {
  digitalWrite(PIN_M0, LOW);
  digitalWrite(PIN_M1, LOW);
  digitalWrite(PIN_M2, LOW);
}

void setMicrostep32() {
  // Pololu/TI tables commonly show 1/32 uses MODE2=HIGH and MODE0=HIGH, MODE1=LOW on DRV8825 carriers.
  // If your board uses different mapping, adjust accordingly.
  digitalWrite(PIN_M0, HIGH);
  digitalWrite(PIN_M1, LOW);
  digitalWrite(PIN_M2, HIGH);
}

void pulseStep(unsigned long high_us, unsigned long low_us, int count) {
  for (int i = 0; i < count; i++) {
    digitalWrite(PIN_STEP, HIGH);
    delayMicroseconds(high_us);
    digitalWrite(PIN_STEP, LOW);
    delayMicroseconds(low_us);

    // Monitor nFAULT during pulsing
    if (digitalRead(PIN_NFAULT) == LOW) {
      Serial.println("nFAULT LOW during STEP pulses!");
      break;
    }
  }
}

void printPins(const char* label) {
  Serial.print(label);
  Serial.print(" | SLP=");
  Serial.print(digitalRead(PIN_SLEEP));
  Serial.print(" RST=");
  Serial.print(digitalRead(PIN_RESET));
  Serial.print(" EN=");
  Serial.print(digitalRead(PIN_EN));
  Serial.print(" DIR=");
  Serial.print(digitalRead(PIN_DIR));
  Serial.print(" STEP=");
  Serial.print(digitalRead(PIN_STEP));
  Serial.print(" M0=");
  Serial.print(digitalRead(PIN_M0));
  Serial.print(" M1=");
  Serial.print(digitalRead(PIN_M1));
  Serial.print(" M2=");
  Serial.print(digitalRead(PIN_M2));
  Serial.print(" nFAULT=");
  Serial.println(digitalRead(PIN_NFAULT));
}

void setup() {
  Serial.begin(115200);
  Serial.println("DRV8824/DRV8825 logic debug (NO MOTOR, NO VMOT)");

  pinMode(PIN_SLEEP, OUTPUT);
  pinMode(PIN_RESET, OUTPUT);
  pinMode(PIN_EN, OUTPUT);
  pinMode(PIN_STEP, OUTPUT);
  pinMode(PIN_DIR, OUTPUT);

  pinMode(PIN_M0, OUTPUT);
  pinMode(PIN_M1, OUTPUT);
  pinMode(PIN_M2, OUTPUT);

  // nFAULT open-drain needs pull-up; INPUT_PULLUP is fine for a breadboard test
  pinMode(PIN_NFAULT, INPUT_PULLUP); // per TI forum guidance about open-drain nFAULT [4](https://e2e.ti.com/support/motor-drivers-group/motor-drivers/f/motor-drivers-forum/90802/drv8824-fault-condition)

  // Default states
  digitalWrite(PIN_STEP, LOW);
  digitalWrite(PIN_DIR, LOW);

  // Put driver in reset/sleep initially
  digitalWrite(PIN_EN, HIGH);     // disable outputs (active-low enable on many carriers)
  digitalWrite(PIN_RESET, LOW);
  digitalWrite(PIN_SLEEP, LOW);

  setMicrostepFull();
  delay(200);
  printPins("Initial");
}

void loop() {
  // 1) Wake sequence
  Serial.println("\n--- Wake driver (RST=HIGH, SLP=HIGH, EN=LOW) ---");
  digitalWrite(PIN_RESET, HIGH);
  digitalWrite(PIN_SLEEP, HIGH);
  digitalWrite(PIN_EN, LOW);   // enable
  delay(200);
  printPins("Awake");

  // 2) Set microstepping mode
  Serial.println("--- Set microstepping to 1/16 (edit mapping if needed) ---");
  setMicrostep16();
  delay(100);
  printPins("uStep16");

  // 3) Toggle DIR and generate STEP pulses
  Serial.println("--- DIR=0, pulse STEP 200x ---");
  digitalWrite(PIN_DIR, LOW);
  delay(20);
  pulseStep(5, 200, 200); // 5us high, 200us low
  printPins("After pulses DIR0");

  Serial.println("--- DIR=1, pulse STEP 200x ---");
  digitalWrite(PIN_DIR, HIGH);
  delay(20);
  pulseStep(5, 200, 200);
  printPins("After pulses DIR1");

  // 4) Sleep and reset toggles
  Serial.println("--- Put to SLEEP (SLP=LOW) ---");
  digitalWrite(PIN_SLEEP, LOW);
  delay(200);
  printPins("Sleep");

  Serial.println("--- Wake again (SLP=HIGH) ---");
  digitalWrite(PIN_SLEEP, HIGH);
  delay(200);
  printPins("Wake2");

  Serial.println("--- Reset pulse (RST LOW->HIGH) ---");
  digitalWrite(PIN_RESET, LOW);
  delay(50);
  digitalWrite(PIN_RESET, HIGH);
  delay(200);
  printPins("After reset");

  // 5) Disable driver
  Serial.println("--- Disable outputs (EN=HIGH) ---");
  digitalWrite(PIN_EN, HIGH);
  delay(200);
  printPins("Disabled");

  Serial.println("\nCycle complete. Swap driver module and repeat.\n");
  delay(2000);
}