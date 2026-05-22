#include "../inc/calibration.h"
#include "../inc/hardware.h"
#include "../inc/movement.h"
#include "../inc/well_utils.h"
#include "../inc/eeprom_utils.h"

uint8_t calCount = 0;
float ML[TERMS] = {0};
float MR[TERMS] = {0};
float calX[MAX_CAL] = {0};
float calY[MAX_CAL] = {0};
float calL[MAX_CAL] = {0};
float calR[MAX_CAL] = {0};
bool mapReady = false;

void XYToWell(float x, float y, char& row, uint8_t& col) {
    uint8_t rowInt = y / WELL_DY;
    col = x / WELL_DX;
    row = 'a' + rowInt;
}

int8_t calibrateHome() {
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

    Serial.println(F("=== MAPPING SOLVED (quadratic least-squares) ==="));
    printCalibrationPoints();

    return true;
}

void clearCalibration() {
	EEPROM.put(EEPROM_CAL_BASE, 0x00);
	mapReady = false;

	for (uint8_t i = 0; i < TERMS; i++)
    {
        ML[i] = 0;
        MR[i] = 0;
    }

	calCount = 0;
	Serial.println(F("Calibration cleared"));
}

void deleteCalibrationPoint(char row, uint8_t col) {
	float x = 0;
	float y = 0;
	wellToXY(row, col, x, y);
	
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

	Serial.print(F("CAL_DELETED:")); Serial.print(row); Serial.print(col);
	Serial.print(F(",remaining=")); Serial.println(calCount);
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