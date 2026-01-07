import React, { useState, useEffect } from 'react';
import { useFunctionWrappers, useFunctionWrapper } from '../hooks/useFunctionWrapper';
import { useScriptExecutor } from '../hooks/useScriptExecutor';
import { ExecutionResult } from '../types';

/**
 * Test component that demonstrates:
 * 1. Loading a FunctionWrapper from the API
 * 2. Executing it locally (in browser)
 * 3. Executing it remotely (on server)
 * 4. Comparing results to verify they match
 */
export function FunctionTester() {
  const { wrappers, loading: loadingList } = useFunctionWrappers();
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const { wrapper, loading: loadingWrapper } = useFunctionWrapper(selectedId);
  const { executeLocal, executeRemote, validate, isExecuting } = useScriptExecutor();
  
  const [localResult, setLocalResult] = useState<ExecutionResult | null>(null);
  const [remoteResult, setRemoteResult] = useState<ExecutionResult | null>(null);
  const [validationError, setValidationError] = useState<string | null>(null);

  // Auto-select first wrapper on load
  useEffect(() => {
    if (wrappers.length > 0 && selectedId === null) {
      setSelectedId(wrappers[0].id);
    }
  }, [wrappers, selectedId]);

  // Validate when wrapper changes
  useEffect(() => {
    if (wrapper) {
      const { isValid, error } = validate(wrapper.theFunction);
      setValidationError(isValid ? null : error);
    }
  }, [wrapper, validate]);

  const handleRunLocal = () => {
    if (!wrapper) return;
    const result = executeLocal(wrapper);
    setLocalResult(result);
  };

  const handleRunRemote = async () => {
    if (!wrapper) return;
    const result = await executeRemote(wrapper.id);
    setRemoteResult(result);
  };

  const handleRunBoth = async () => {
    handleRunLocal();
    await handleRunRemote();
  };

  const resultsMatch = 
    localResult?.success && 
    remoteResult?.success && 
    localResult.result === remoteResult.result;

  return (
    <div style={styles.container}>
      <h1 style={styles.title}>Function Executor Tester</h1>
      
      {/* Wrapper Selection */}
      <section style={styles.section}>
        <h2>Select Function Wrapper</h2>
        {loadingList ? (
          <p>Loading...</p>
        ) : (
          <select
            value={selectedId ?? ''}
            onChange={(e) => setSelectedId(Number(e.target.value) || null)}
            style={styles.select}
          >
            <option value="">-- Select a wrapper --</option>
            {wrappers.map((w) => (
              <option key={w.id} value={w.id}>
                {w.name} (v{w.version})
              </option>
            ))}
          </select>
        )}
      </section>

      {/* Wrapper Details */}
      {loadingWrapper && <p>Loading wrapper...</p>}
      
      {wrapper && (
        <>
          <section style={styles.section}>
            <h2>Function Details</h2>
            <div style={styles.details}>
              <p><strong>Name:</strong> {wrapper.name}</p>
              <p><strong>Description:</strong> {wrapper.description || 'N/A'}</p>
              <p><strong>Version:</strong> {wrapper.version}</p>
              <p><strong>Cost Codes Loaded:</strong> {wrapper.costCodes.length}</p>
            </div>
          </section>

          <section style={styles.section}>
            <h2>Function Code</h2>
            {validationError && (
              <div style={styles.errorBanner}>{validationError}</div>
            )}
            <pre style={styles.code}>{wrapper.theFunction}</pre>
          </section>

          <section style={styles.section}>
            <h2>Cost Codes Available</h2>
            <div style={styles.costCodeList}>
              {wrapper.costCodes.map((cc) => (
                <div key={cc.id} style={styles.costCode}>
                  <span style={styles.costCodeId}>#{cc.id}</span>
                  <span>{cc.name}</span>
                  <span style={styles.costCodeValue}>{cc.value}</span>
                  {cc.parentId && (
                    <span style={styles.costCodeParent}>(parent: {cc.parentId})</span>
                  )}
                </div>
              ))}
            </div>
          </section>

          {/* Execution Buttons */}
          <section style={styles.section}>
            <h2>Execute</h2>
            <div style={styles.buttonGroup}>
              <button 
                onClick={handleRunLocal} 
                style={styles.button}
                disabled={!!validationError}
              >
                Run on Client (Browser)
              </button>
              <button 
                onClick={handleRunRemote} 
                style={styles.button}
                disabled={isExecuting}
              >
                {isExecuting ? 'Running...' : 'Run on Server (.NET)'}
              </button>
              <button 
                onClick={handleRunBoth} 
                style={{ ...styles.button, ...styles.primaryButton }}
                disabled={isExecuting || !!validationError}
              >
                Run Both & Compare
              </button>
            </div>
          </section>

          {/* Results */}
          <section style={styles.section}>
            <h2>Results</h2>
            <div style={styles.resultsContainer}>
              <ResultCard title="Client (Browser)" result={localResult} />
              <ResultCard title="Server (.NET)" result={remoteResult} />
            </div>
            
            {localResult && remoteResult && (
              <div style={{
                ...styles.matchBanner,
                backgroundColor: resultsMatch ? '#d4edda' : '#f8d7da',
                color: resultsMatch ? '#155724' : '#721c24',
              }}>
                {resultsMatch 
                  ? '✓ Results match!' 
                  : '✗ Results do not match!'}
              </div>
            )}
          </section>
        </>
      )}
    </div>
  );
}

/**
 * Result display card
 */
function ResultCard({ title, result }: { title: string; result: ExecutionResult | null }) {
  if (!result) {
    return (
      <div style={styles.resultCard}>
        <h3>{title}</h3>
        <p style={styles.noResult}>Not run yet</p>
      </div>
    );
  }

  return (
    <div style={{
      ...styles.resultCard,
      borderColor: result.success ? '#28a745' : '#dc3545',
    }}>
      <h3>{title}</h3>
      <div style={styles.resultContent}>
        <p>
          <strong>Status:</strong>{' '}
          <span style={{ color: result.success ? '#28a745' : '#dc3545' }}>
            {result.success ? 'Success' : 'Error'}
          </span>
        </p>
        {result.success ? (
          <p><strong>Result:</strong> {result.result}</p>
        ) : (
          <p style={styles.errorText}><strong>Error:</strong> {result.error}</p>
        )}
        <p><strong>Time:</strong> {result.executionTimeMs}ms</p>
      </div>
    </div>
  );
}

// Simple inline styles for the demo
const styles: Record<string, React.CSSProperties> = {
  container: {
    maxWidth: '900px',
    margin: '0 auto',
    padding: '20px',
    fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
  },
  title: {
    borderBottom: '2px solid #333',
    paddingBottom: '10px',
  },
  section: {
    marginBottom: '24px',
    padding: '16px',
    backgroundColor: '#f8f9fa',
    borderRadius: '8px',
  },
  select: {
    width: '100%',
    padding: '8px 12px',
    fontSize: '16px',
    borderRadius: '4px',
    border: '1px solid #ced4da',
  },
  details: {
    display: 'grid',
    gap: '8px',
  },
  code: {
    backgroundColor: '#282c34',
    color: '#abb2bf',
    padding: '16px',
    borderRadius: '4px',
    overflow: 'auto',
    fontSize: '14px',
    lineHeight: '1.5',
  },
  errorBanner: {
    backgroundColor: '#f8d7da',
    color: '#721c24',
    padding: '8px 12px',
    borderRadius: '4px',
    marginBottom: '8px',
  },
  costCodeList: {
    display: 'flex',
    flexDirection: 'column',
    gap: '4px',
    maxHeight: '200px',
    overflow: 'auto',
  },
  costCode: {
    display: 'flex',
    gap: '12px',
    padding: '4px 8px',
    backgroundColor: 'white',
    borderRadius: '4px',
    fontSize: '14px',
  },
  costCodeId: {
    color: '#6c757d',
    fontFamily: 'monospace',
  },
  costCodeValue: {
    marginLeft: 'auto',
    fontWeight: 'bold',
  },
  costCodeParent: {
    color: '#6c757d',
    fontSize: '12px',
  },
  buttonGroup: {
    display: 'flex',
    gap: '12px',
    flexWrap: 'wrap',
  },
  button: {
    padding: '10px 20px',
    fontSize: '14px',
    borderRadius: '4px',
    border: '1px solid #ced4da',
    backgroundColor: 'white',
    cursor: 'pointer',
  },
  primaryButton: {
    backgroundColor: '#007bff',
    color: 'white',
    border: 'none',
  },
  resultsContainer: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))',
    gap: '16px',
  },
  resultCard: {
    padding: '16px',
    backgroundColor: 'white',
    borderRadius: '8px',
    border: '2px solid #ced4da',
  },
  resultContent: {
    display: 'grid',
    gap: '4px',
  },
  noResult: {
    color: '#6c757d',
    fontStyle: 'italic',
  },
  errorText: {
    color: '#dc3545',
  },
  matchBanner: {
    marginTop: '16px',
    padding: '12px',
    borderRadius: '4px',
    textAlign: 'center',
    fontWeight: 'bold',
  },
};
