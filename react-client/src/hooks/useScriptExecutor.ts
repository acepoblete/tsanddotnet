import { useState, useCallback } from 'react';
import { FunctionWrapper, ExecutionResult } from '../types';
import { executeFunction, validateFunction } from '../services/scriptExecutor';
import { executionApi } from '../services/api';

interface UseScriptExecutorResult {
  // Execute locally in browser
  executeLocal: (wrapper: FunctionWrapper) => ExecutionResult;
  
  // Execute on server via API
  executeRemote: (functionWrapperId: number) => Promise<ExecutionResult>;
  
  // Validate function syntax
  validate: (theFunction: string) => { isValid: boolean; error: string | null };
  
  // State
  isExecuting: boolean;
  lastLocalResult: ExecutionResult | null;
  lastRemoteResult: ExecutionResult | null;
}

/**
 * Hook that provides both local and remote script execution
 * 
 * Use this to:
 * - Test functions in the browser (executeLocal)
 * - Run functions on the server (executeRemote)
 * - Compare results to verify consistency
 */
export function useScriptExecutor(): UseScriptExecutorResult {
  const [isExecuting, setIsExecuting] = useState(false);
  const [lastLocalResult, setLastLocalResult] = useState<ExecutionResult | null>(null);
  const [lastRemoteResult, setLastRemoteResult] = useState<ExecutionResult | null>(null);

  /**
   * Execute function locally in the browser
   * Uses the same logic as the .NET server
   */
  const executeLocal = useCallback((wrapper: FunctionWrapper): ExecutionResult => {
    const result = executeFunction(wrapper);
    setLastLocalResult(result);
    return result;
  }, []);

  /**
   * Execute function on the server via API
   */
  const executeRemote = useCallback(async (functionWrapperId: number): Promise<ExecutionResult> => {
    setIsExecuting(true);
    
    try {
      const result = await executionApi.execute(functionWrapperId);
      setLastRemoteResult(result);
      return result;
    } catch (error) {
      const errorResult: ExecutionResult = {
        success: false,
        result: null,
        error: error instanceof Error ? error.message : 'API call failed',
        executionTimeMs: 0,
      };
      setLastRemoteResult(errorResult);
      return errorResult;
    } finally {
      setIsExecuting(false);
    }
  }, []);

  /**
   * Validate function syntax without executing
   */
  const validate = useCallback((theFunction: string) => {
    return validateFunction(theFunction);
  }, []);

  return {
    executeLocal,
    executeRemote,
    validate,
    isExecuting,
    lastLocalResult,
    lastRemoteResult,
  };
}
