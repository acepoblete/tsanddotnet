// Types that mirror the .NET DTOs

export interface CostCode {
  id: number;
  parentId: number | null;
  name: string;
  value: number;
}

export interface FunctionWrapper {
  id: number;
  version: number;
  name: string;
  description: string | null;
  theFunction: string;
  costCodes: CostCode[];
  createdAt: string;
  updatedAt: string;
}

export interface ExecutionResult {
  success: boolean;
  result: number | null;
  error: string | null;
  executionTimeMs: number;
}

export interface ValidateResult {
  isValid: boolean;
  error: string | null;
}

// Request types
export interface CreateFunctionWrapperRequest {
  name: string;
  description?: string;
  theFunction: string;
  selectedCostCodeIds: number[];
}

export interface UpdateFunctionWrapperRequest {
  name: string;
  description?: string;
  theFunction: string;
  selectedCostCodeIds: number[];
}
