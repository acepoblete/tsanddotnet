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

// Workbook types
export interface WorkbookCostCode {
  id: number;
  workbookId: number;
  costCodeId: number;
  cmicCode: string;
  name: string;
  labor: number;
  qty: number;
  materials: number;
  other: number;
  totalCost: number;
}

export interface Workbook {
  id: number;
  name: string;
  description: string | null;
  templateFilePath: string;
  version: number;
  createdAt: string;
  updatedAt: string;
  costCodes: WorkbookCostCode[];
}

export interface WorkbookSummary {
  id: number;
  name: string;
  description: string | null;
  templateFilePath: string;
  version: number;
  createdAt: string;
  updatedAt: string;
  costCodeCount: number;
}

export interface CostCodeUpdateInput {
  cmicCode: string;
  labor?: number;
  qty?: number;
  materials?: number;
  other?: number;
}

export interface CalculationRequest {
  costCodeInputs: CostCodeUpdateInput[];
}

export interface CalculationResult {
  success: boolean;
  error: string | null;
  executionTimeMs: number;
  updatedCostCodes: WorkbookCostCode[];
}
