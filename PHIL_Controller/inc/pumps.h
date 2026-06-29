/**
 * pumps.h
 * 
 * Defines high-level pump control functions for liquid handling.
 * 
 * These functions control the aspirate (intake) and dispense (output)
 * operations of the pump motors, optionally targeting a specific well.
 * 
 * Both functions:
 * - Control a selected pump motor
 * - Move a defined volume (in microliters)
 * - Optionally operate at a specific well position
 */

#pragma once

#include <stdint.h>



/**
 * dispense(pump, microliters, well)
 * 
 * Dispenses (expels) liquid from the specified pump.
 * 
 * @param pump         Pump index (e.g., P1, P2)
 * @param microliters  Volume to dispense in µL
 * @param well         Target well (e.g., "A1"), can be empty
 * 
 * Behavior:
 * - Moves pump motor forward to release liquid
 * - If a well is provided, operation is associated with that location
 */
void dispense(uint8_t pump, uint16_t microliters, char* well);

/**
 * prime(pump, microliters)
 *
 * Primes the selected pump by dispensing liquid to the waste container.
 *
 * @param pump         Pump index (e.g., P1, P2)
 * @param microliters  Volume to prime in µL
 */
void prime(uint8_t pump, uint16_t microliters);

/**
 * aspirate(pump, microliters, well)
 * 
 * Aspirates (draws) liquid into the specified pump.
 * 
 * @param pump         Pump index (e.g., P1, P2)
 * @param microliters  Volume to aspirate in µL
 * @param well         Target well (e.g., "A1"), can be empty
 * 
 * Behavior:
 * - Moves pump motor in reverse to draw liquid
 * - If a well is provided, operation is associated with that location
 */
void aspirate(uint8_t pump, uint16_t microliters, char* well);