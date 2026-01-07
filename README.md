# Function Executor MVP

Execute the same JavaScript functions on both .NET (via Jint) and React (native JS), with guaranteed identical results.

## Quick Start

```bash
# Clone and install
git clone <repo-url>
cd costcodes_poc

# Backend
cd dotnet-api
dotnet restore
dotnet run &

# Frontend (new terminal)
cd react-client
yarn install
yarn dev
```

Open http://localhost:3000 - API at http://localhost:5000/swagger

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    FunctionWrapper                          │
│  {                                                          │
│    id: 1,                                                   │
│    costCodes: [ ... pre-loaded ... ],                       │
│    theFunction: "return getCostCode(1).value * 2;"          │
│  }                                                          │
└─────────────────────────────────────────────────────────────┘
                          │
          ┌───────────────┴───────────────┐
          ▼                               ▼
   ┌─────────────────┐           ┌─────────────────┐
   │   .NET + Jint   │           │     React       │
   │                 │           │                 │
   │ getCostCode()   │           │ getCostCode()   │
   │ getCostCodes()  │           │ getCostCodes()  │
   │ getChildren()   │           │ getChildren()   │
   │ getParent()     │           │ getParent()     │
   └────────┬────────┘           └────────┬────────┘
            │                             │
            └──────────┬──────────────────┘
                       ▼
                 Returns: number
                 (Must be identical!)
```

## File Structure

```
mvp/
├── dotnet-api/
│   ├── Models/
│   │   ├── CostCode.cs           # Cost code entity
│   │   ├── FunctionWrapper.cs    # Function + associated cost codes
│   │   ├── ExecutionResult.cs    # Result of script execution
│   │   └── Dto.cs                # DTOs for API
│   ├── Data/
│   │   └── AppDbContext.cs       # EF Core context + seeding
│   ├── Repositories/
│   │   ├── ICostCodeRepository.cs
│   │   ├── CostCodeRepository.cs
│   │   ├── IFunctionWrapperRepository.cs
│   │   └── FunctionWrapperRepository.cs
│   ├── Services/
│   │   ├── IScriptExecutor.cs
│   │   ├── JintScriptExecutor.cs      # ⭐ Core: Jint JS execution
│   │   ├── IFunctionWrapperService.cs
│   │   └── FunctionWrapperService.cs
│   ├── Controllers/
│   │   ├── CostCodesController.cs
│   │   ├── FunctionWrappersController.cs
│   │   └── ExecutionController.cs
│   ├── Program.cs
│   └── FunctionExecutor.csproj
│
└── react-client/
    ├── src/
    │   ├── types/
    │   │   └── index.ts           # TypeScript types
    │   ├── services/
    │   │   ├── api.ts             # API client
    │   │   └── scriptExecutor.ts  # ⭐ Core: Browser JS execution
    │   ├── hooks/
    │   │   ├── useFunctionWrapper.ts
    │   │   └── useScriptExecutor.ts
    │   ├── components/
    │   │   └── FunctionTester.tsx # Test UI
    │   ├── App.tsx
    │   └── main.tsx
    ├── index.html
    ├── package.json
    ├── tsconfig.json
    └── vite.config.ts
```

## Helper Functions Available in User Scripts

Both .NET and React provide these identical functions:

| Function | Returns | Description |
|----------|---------|-------------|
| `getCostCode(id)` | `CostCode \| null` | Get a cost code by ID from pre-loaded set |
| `getCostCodes()` | `CostCode[]` | Get all pre-loaded cost codes |
| `getChildren(parentId)` | `CostCode[]` | Get direct children of a cost code |
| `getDescendants(parentId)` | `CostCode[]` | Get all descendants recursively |
| `getParent(id)` | `CostCode \| null` | Get parent of a cost code |

### CostCode Object Shape

```typescript
{
  id: number;
  parentId: number | null;
  name: string;
  value: number;
}
```

## Example User Functions

```javascript
// Simple: return a single value doubled
const code = getCostCode(5);
return code ? code.value * 2 : 0;

// Sum all direct children
const children = getChildren(1);
let total = 0;
for (const c of children) {
  total += c.value;
}
return total;

// Sum entire hierarchy using getDescendants
const root = getCostCode(1);
const descendants = getDescendants(1);
let total = root.value;
for (const c of descendants) {
  total += c.value;
}
return total;

// Percentage calculation
const item = getCostCode(5);
const all = getCostCodes();
const total = all.reduce((s, c) => s + c.value, 0);
return total > 0 ? (item.value / total) * 100 : 0;
```

## Running the MVP

### Backend (.NET)

```bash
cd dotnet-api

# Restore packages
dotnet restore

# Run (creates SQLite DB automatically)
dotnet run

# API will be at http://localhost:5000
# Swagger UI at http://localhost:5000/swagger
```

### Frontend (React)

```bash
cd react-client

# Install dependencies
npm install

# Run dev server
npm run dev

# App will be at http://localhost:3000
```

## API Endpoints

### Cost Codes
- `GET /api/cost-codes` - List all
- `GET /api/cost-codes/{id}` - Get one
- `GET /api/cost-codes/{id}/with-descendants` - Get with all children
- `GET /api/cost-codes/{id}/children` - Get direct children only

### Function Wrappers
- `GET /api/function-wrappers` - List all
- `GET /api/function-wrappers/{id}` - Get one (includes costCodes)
- `POST /api/function-wrappers` - Create
- `PUT /api/function-wrappers/{id}` - Update
- `DELETE /api/function-wrappers/{id}` - Delete

### Execution
- `POST /api/execute/{functionWrapperId}` - Execute on server
- `POST /api/execute/validate` - Validate syntax

## Key Design Decisions

1. **Pre-loaded Cost Codes**: Users select root cost codes when creating a FunctionWrapper. All descendants are automatically loaded. No runtime fetching.

2. **Must Return Number**: All user functions must return a number. This ensures predictable results and easy comparison.

3. **Identical Helper Functions**: The exact same functions are available on both .NET and React, with identical behavior.

4. **Sandboxed Execution**: 
   - .NET: Jint with timeouts, statement limits, recursion limits
   - React: `new Function()` with strict mode

5. **Version Tracking**: FunctionWrappers have a version number that increments on update.

## Testing Consistency

The FunctionTester component lets you:
1. Select a FunctionWrapper
2. Run it on the browser (React)
3. Run it on the server (.NET)
4. Compare results to verify they match

If results don't match, there's a bug in either executor!
