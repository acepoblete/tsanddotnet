import {
  CostCode,
  FunctionWrapper,
  ExecutionResult,
  ValidateResult,
  CreateFunctionWrapperRequest,
  UpdateFunctionWrapperRequest,
} from '../types';

const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

/**
 * Generic fetch wrapper with error handling
 */
async function fetchApi<T>(
  endpoint: string,
  options?: RequestInit
): Promise<T> {
  const response = await fetch(`${API_BASE}${endpoint}`, {
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
    ...options,
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ error: 'Unknown error' }));
    throw new Error(error.error || `HTTP ${response.status}`);
  }

  // Handle 204 No Content
  if (response.status === 204) {
    return undefined as T;
  }

  return response.json();
}

// ============================================================
// Cost Codes API
// ============================================================

export const costCodesApi = {
  getAll: () => fetchApi<CostCode[]>('/cost-codes'),
  
  getById: (id: number) => fetchApi<CostCode>(`/cost-codes/${id}`),
  
  getWithDescendants: (id: number) => 
    fetchApi<CostCode[]>(`/cost-codes/${id}/with-descendants`),
  
  getChildren: (id: number) => 
    fetchApi<CostCode[]>(`/cost-codes/${id}/children`),
};

// ============================================================
// Function Wrappers API
// ============================================================

export const functionWrappersApi = {
  getAll: () => fetchApi<FunctionWrapper[]>('/function-wrappers'),
  
  getById: (id: number) => 
    fetchApi<FunctionWrapper>(`/function-wrappers/${id}`),
  
  create: (request: CreateFunctionWrapperRequest) =>
    fetchApi<FunctionWrapper>('/function-wrappers', {
      method: 'POST',
      body: JSON.stringify(request),
    }),
  
  update: (id: number, request: UpdateFunctionWrapperRequest) =>
    fetchApi<FunctionWrapper>(`/function-wrappers/${id}`, {
      method: 'PUT',
      body: JSON.stringify(request),
    }),
  
  delete: (id: number) =>
    fetchApi<void>(`/function-wrappers/${id}`, {
      method: 'DELETE',
    }),
};

// ============================================================
// Execution API
// ============================================================

export const executionApi = {
  /**
   * Execute a function wrapper on the server
   */
  execute: (functionWrapperId: number) =>
    fetchApi<ExecutionResult>(`/execute/${functionWrapperId}`, {
      method: 'POST',
    }),
  
  /**
   * Validate a function string without executing
   */
  validate: (theFunction: string) =>
    fetchApi<ValidateResult>('/execute/validate', {
      method: 'POST',
      body: JSON.stringify({ theFunction }),
    }),
};
