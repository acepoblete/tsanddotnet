import { CostCode, FunctionWrapper, ExecutionResult } from '../types';

/**
 * Client-side script executor that mirrors the .NET JintScriptExecutor
 * 
 * This provides the SAME helper functions as the server:
 * - getCostCode(id)
 * - getCostCodes()
 * - getChildren(parentId)
 * - getParent(id)
 * 
 * The function must return a number.
 */
export function executeFunction(wrapper: FunctionWrapper): ExecutionResult {
  const startTime = performance.now();
  
  try {
    // Build lookup map from pre-loaded cost codes (same as .NET)
    const costCodes = wrapper.costCodes;
    const byId = new Map(costCodes.map(c => [c.id, c]));
    
    // Define helper functions (must match .NET exactly!)
    
    /**
     * Get a single cost code by ID
     * Returns null if not found in the pre-loaded set
     */
    const getCostCode = (id: number): CostCode | null => {
      return byId.get(id) ?? null;
    };
    
    /**
     * Get all pre-loaded cost codes
     */
    const getCostCodes = (): CostCode[] => {
      return [...costCodes];
    };
    
    /**
     * Get direct children of a cost code
     */
    const getChildren = (parentId: number): CostCode[] => {
      return costCodes.filter(c => c.parentId === parentId);
    };

    /**
     * Get all descendants of a cost code (recursive)
     */
    const getDescendants = (parentId: number): CostCode[] => {
      const result: CostCode[] = [];
      const collectDescendants = (pid: number) => {
        const children = costCodes.filter(c => c.parentId === pid);
        for (const child of children) {
          result.push(child);
          collectDescendants(child.id);
        }
      };
      collectDescendants(parentId);
      return result;
    };

    /**
     * Get parent of a cost code
     * Returns null if no parent or parent not in pre-loaded set
     */
    const getParent = (id: number): CostCode | null => {
      const code = byId.get(id);
      if (code && code.parentId !== null) {
        return byId.get(code.parentId) ?? null;
      }
      return null;
    };
    
    // Console logging (for debugging)
    const logs: string[] = [];
    const console = {
      log: (...args: unknown[]) => {
        logs.push(args.map(String).join(' '));
      },
    };
    
    // Create the function with helpers injected into scope
    // Using Function constructor to avoid direct eval
    const fn = new Function(
      'getCostCode',
      'getCostCodes',
      'getChildren',
      'getDescendants',
      'getParent',
      'console',
      `"use strict";
       ${wrapper.theFunction}`
    );

    // Execute the function
    const result = fn(getCostCode, getCostCodes, getChildren, getDescendants, getParent, console);
    
    const executionTimeMs = Math.round(performance.now() - startTime);
    
    // Log any console output (for debugging)
    if (logs.length > 0) {
      window.console.log('[Script Output]', logs);
    }
    
    // Validate result is a number
    if (typeof result !== 'number' || Number.isNaN(result)) {
      return {
        success: false,
        result: null,
        error: `Function must return a number, but returned ${typeof result}${Number.isNaN(result) ? ' (NaN)' : ''}`,
        executionTimeMs,
      };
    }
    
    return {
      success: true,
      result,
      error: null,
      executionTimeMs,
    };
    
  } catch (error) {
    const executionTimeMs = Math.round(performance.now() - startTime);
    const errorMessage = error instanceof Error ? error.message : String(error);
    
    return {
      success: false,
      result: null,
      error: `JavaScript Error: ${errorMessage}`,
      executionTimeMs,
    };
  }
}

/**
 * Validate a function string without executing it
 * Matches the .NET validation endpoint
 */
export function validateFunction(theFunction: string): { isValid: boolean; error: string | null } {
  try {
    // Try to create the function (parses without executing)
    new Function(
      'getCostCode',
      'getCostCodes',
      'getChildren',
      'getDescendants',
      'getParent',
      'console',
      `"use strict"; ${theFunction}`
    );
    
    return { isValid: true, error: null };
  } catch (error) {
    const errorMessage = error instanceof Error ? error.message : String(error);
    return { isValid: false, error: `Syntax error: ${errorMessage}` };
  }
}
