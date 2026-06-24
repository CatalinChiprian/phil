#include "../inc/calibration.h"
#include "../inc/hardware.h"
#include "../inc/movement.h"
#include "../inc/well_utils.h"
#include "../inc/eeprom_utils.h"
#include "../inc/commands.h"

uint8_t calCount = 0;
float ML[TERMS] = {0};
float MR[TERMS] = {0};
uint8_t calWellIndex[MAX_CAL] = {0};
float calL[MAX_CAL] = {0};
float calR[MAX_CAL] = {0};
bool mapReady = false;


/**
 * calibrateHome()
 * 
 * Performs origin calibration using both limit switches.
 * 
 * Procedure:
 * 1. Move Z-axis to a safe position to avoid collision
 * 2. Move right motor (R) until right limit switch is triggered
 * 3. Back off slightly and set position as reference
 * 4. Move left motor (L) until left limit switch is triggered
 * 5. Set both motor positions to zero
 * 6. Move to a predefined offset (center reference)
 * 7. Store the origin position and persist state
 * 
 * Safety:
 * - Continuously checks for emergency stop requests
 * - Stops immediately if triggered
 * 
 * Result:
 * Establishes a reliable origin (0,0) for all future movements
 * 
 * @return 1 on success, 0 if interrupted
 */
int8_t calibrateHome() {
    moveZMotors(ZMotorNormalPosition);

    disableAllMotors();

    enableRMotor();

    stepperR.setSpeed(10 * currentMicrosteps);
    while(digitalRead(limitSwitchR) == HIGH){
        if (isEmergencyStopRequest()) {
            emergencyStop();
            return 0;
        }
        
        stepperR.runSpeed();
    }

    stepperR.setSpeed(-10 * currentMicrosteps);

    stepperR.moveTo(stepperR.currentPosition() - 550);
    while(stepperR.distanceToGo() != 0) {
        if (isEmergencyStopRequest()) {
            emergencyStop();
            return 0;
        }

        stepperR.run();
    }

    stepperR.setCurrentPosition(0);

    enableLMotor();

    stepperL.setSpeed(-10 * currentMicrosteps);
    while(digitalRead(limitSwitchL) == HIGH){
        if (isEmergencyStopRequest()) {
            emergencyStop();
            return 0;
        }

        stepperL.runSpeed();
    }

    stepperL.setSpeed(10 * currentMicrosteps);

    stepperL.setCurrentPosition(0);

    disableLMotor();

    stepperR.move(405);

    while (stepperR.distanceToGo() != 0) {
        if (isEmergencyStopRequest()) {
            emergencyStop();
            return 0;
        }

        stepperR.run();
    }

    stepperR.setCurrentPosition(0);

    wellIndex = WELL_HOME;
    saveCurrentWell(wellIndex);
    savePositions();

    disableAllMotors();

    return 1;
}

/**
 * recordCalibrationPoint(row, col)
 * 
 * Records a calibration point linking a well position to motor positions.
 * 
 * Steps:
 * 1. Convert well (row, col) → well index
 * 2. Convert index → physical (x, y) coordinates
 * 3. Check for duplicates
 * 4. Store:
 *    - well index
 *    - motor positions (converted to degrees)
 * 
 * Constraints:
 * - Maximum number of calibration points is limited (MAX_CAL)
 * - Duplicate wells are not allowed
 * 
 * Notes:
 * - Raw motor steps are printed for debugging
 * - Stored values are converted to degrees for mapping
 * 
 * Output:
 * - Prints confirmation + updated calibration count
 */
void recordCalibrationPoint(char row, uint8_t col) {
    if (calCount >= MAX_CAL) {
        Serial.print(F("ERROR:CAL_FULL,Maximum ")); 
        Serial.print(MAX_CAL); 
        Serial.println(F(" calibration points reached"));
        return;
    }

    float x = 0;
    float y = 0;
    uint8_t wellIndex = rowColToWellIndex(row, col);
    wellIndexToXY(wellIndex, x, y);

    for (uint8_t i = 0; i < calCount; i++) {
        if (calWellIndex[i] == wellIndex) {
            Serial.println(F("ERROR:CAL_DUPLICATE"));
            return;
        }
    }

    calWellIndex[calCount] = wellIndex;

    Serial.print("RAW L steps: "); Serial.println(stepperL.currentPosition());
    Serial.print("RAW R steps: "); Serial.println(stepperR.currentPosition());

    calL[calCount] = stepsToDegrees(stepperL.currentPosition());
    calR[calCount] = stepsToDegrees(stepperR.currentPosition());

    Serial.print(F("CAL_REC:"));
    Serial.print(F("Name=")); Serial.print(row); Serial.print(col);
    Serial.print(F(",X=")); Serial.print(x);
    Serial.print(F(",Y=")); Serial.println(y);

    Serial.print(F("CAL_COUNT:")); Serial.println(++calCount);
}

/**
 * solve10(A, b, x)
 * 
 * Solves a linear system of equations:
 *     A * x = b
 * 
 * Uses Gaussian elimination with partial pivoting.
 * 
 * Purpose:
 * - Used internally for solving the least-squares mapping system
 * - Computes polynomial coefficients for calibration
 * 
 * Steps:
 * 1. Build augmented matrix [A | b]
 * 2. Perform row pivoting to improve numerical stability
 * 3. Normalize rows and eliminate variables
 * 4. Perform back substitution
 * 
 * Safety:
 * - Detects singular matrices (non-invertible)
 * - Returns false if solution cannot be computed
 * 
 * @return true if solution found, false otherwise
 */
bool solve10(float A[TERMS][TERMS], float b[TERMS], float x[TERMS]) {
    float M[TERMS][TERMS+1];
    for (uint8_t i = 0; i < TERMS; i++){
        for (uint8_t j = 0; j < TERMS; j++) M[i][j] = A[i][j];
        M[i][TERMS] = b[i];
    }
    for (uint8_t col = 0; col < TERMS; col++) {
        uint8_t piv = col;
        float best = fabs(M[piv][col]);
        for (uint8_t r = col + 1; r < TERMS; r++) {
        float v = fabs(M[r][col]);
        if (v > best) { best = v; piv = r; }
        }
        if (best < 1e-9) return false;
        if (piv != col) {
        for (uint8_t c = col; c <= TERMS; c++) {
            float tmp = M[col][c];
            M[col][c] = M[piv][c];
            M[piv][c] = tmp;
        }
        }
        float div = M[col][col];
        for (uint8_t c = col; c <= TERMS; c++) M[col][c] /= div;
        for (uint8_t r = 0; r < TERMS; r++) {
        if (r == col) continue;
        float f = M[r][col];
        for (uint8_t c=col; c <= TERMS; c++) M[r][c] -= f * M[col][c];
        }
    }
    for (uint8_t i = 0; i < TERMS; i++) x[i] = M[i][TERMS];
    return true;
}

/**
 * solveMapping()
 * 
 * Computes the polynomial mapping from well coordinates (x, y)
 * to motor positions (L, R).
 * 
 * Method:
 * - Uses least-squares fitting via normal equations:
 *       (A^T A) c = (A^T y)
 * - Polynomial model with TERMS (10) basis functions:
 *       [1, x, y, x², xy, y², x³, x²y, xy², y³]
 * 
 * Steps:
 * 1. Validate sufficient calibration points
 * 2. Build normal equation matrices (ATA, ATy)
 * 3. Solve for coefficients ML (left) and MR (right)
 * 4. Store results and mark mapping as ready
 * 
 * Output:
 * - Prints mapping status and calibration summary
 * 
 * Errors:
 * - Insufficient points → rejected
 * - Singular matrix → suggests better spatial calibration
 * 
 * @return true if mapping successfully computed
 */
bool solveMapping() {
    if (calCount < TERMS) {
        Serial.print(F("ERROR:SOLVE_INSUFFICIENT,Need at least "));
        Serial.print(TERMS);
        Serial.println(F(" calibration points"));
        return false;
    }

    // Normal equations: (A^T A) c = (A^T y)
    // A rows are basis b = [1, x, y, x^2, x*y, y^2]
    float ATA[TERMS][TERMS] = {0};
    float ATyL[TERMS] = {0};
    float ATyR[TERMS] = {0};

    for (uint8_t i = 0; i < calCount; i++) {
        uint8_t wellIndex = calWellIndex[i];
        float x, y;
        wellIndexToXY(wellIndex, x, y);
        float b[TERMS] = { 1.0f, x, y, x*x, x*y, y*y, x*x*x, x*x*y, x*y*y, y*y*y };
        float L = calL[i];
        float R = calR[i];

        // Accumulate ATA = Σ b*b^T
        for (uint8_t r = 0; r < TERMS; r++) {
        for (uint8_t c = 0; c < TERMS; c++) {
            ATA[r][c] += b[r]*b[c];
        }
        }
        // Accumulate ATy = Σ b*y
        for (uint8_t k = 0; k < TERMS; k++) {
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
    for (uint8_t i = 0; i < TERMS; i++) { ML[i] = MLtmp[i]; MR[i] = MRtmp[i]; }
    mapReady = true;

    Serial.println(F("=== MAPPING SOLVED (quadratic least-squares) ==="));
    printCalibrationPoints();

    return true;
}

/**
 * deleteCalibrationPoint(row, col)
 * 
 * Removes a calibration point corresponding to a specific well.
 * 
 * Steps:
 * 1. Find calibration entry matching well
 * 2. Shift all following entries (array compaction)
 * 3. Decrease calibration count
 * 
 * Notes:
 * - If point does not exist, function exits silently
 * - Maintains continuous array structure
 * 
 * Output:
 * - Prints deletion confirmation and remaining count
 */
void deleteCalibrationPoint(char row, uint8_t col) {
    uint8_t targetIndex = rowColToWellIndex(row, col);
	
	int8_t foundIdx = -1;

    for (uint8_t i = 0; i < calCount; i++) {
        if (calWellIndex[i] == targetIndex) {
            foundIdx = i;
            break;
        }
    }

	if (foundIdx == -1) return;

	for (int8_t i = foundIdx; i < calCount - 1; i++) {
		calWellIndex[i] = calWellIndex[i+1];
		calL[i] = calL[i+1];
		calR[i] = calR[i+1];
	}
	calCount--;

	Serial.print(F("CAL_DEL:")); 
    Serial.print(F("Name=")); Serial.print(row); Serial.print(col);
	Serial.print(F(",Remaining=")); Serial.println(calCount);
}

/**
 * clampZero(v, eps)
 * 
 * Clamps very small values to zero.
 * 
 * Purpose:
 * - Improve readability of printed floating-point errors
 * - Avoid showing very small numerical noise (e.g. 1e-6)
 * 
 * @param v   Value to clamp
 * @param eps Threshold below which value is treated as zero
 * 
 * @return 0 if |v| < eps, otherwise v
 */
float clampZero(float v, float eps = 5e-4f) {
    return fabs(v) < eps ? 0.0f : v;
}

/**
 * printCalibrationPoints()
 * 
 * Outputs all calibration points and mapping error metrics.
 * 
 * For each calibration point:
 * - Prints well name, coordinates (x, y)
 * - If mapping is ready:
 *     - Computes predicted motor values
 *     - Calculates error (actual vs predicted)
 * 
 * Metrics:
 * - RMS error (Left/Right): average calibration accuracy
 * - Max error (Left/Right): worst-case deviation
 * 
 * Purpose:
 * - Used by GUI to display calibration quality
 * - Helps user assess calibration accuracy
 * 
 * Output format:
 * - CAL_PT → individual points
 * - RMS → overall error metrics
 * - END_CAL → termination marker
 */
void printCalibrationPoints() {
    Serial.print(F("CAL_COUNT:")); Serial.println(calCount);
    float maxErrL = 0, maxErrR = 0, rmsL = 0, rmsR = 0;

    if (calCount == 0) {
        Serial.println(F("END_CAL"));
        return;
    }

    for (uint8_t i=0; i<calCount; i++) {
        uint8_t wellIndex = calWellIndex[i];
        float x, y;
        wellIndexToXY(wellIndex, x, y);
        char row;
        uint8_t col;
        wellIndexToRowCol(wellIndex, row, col);
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
        Serial.print(F(",X=")); Serial.print(x, 2);
        Serial.print(F(",Y=")); Serial.print(y, 2);

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
    
    Serial.println(F("END_CAL"));
}