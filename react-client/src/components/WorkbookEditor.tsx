import React, { useState, useEffect, useCallback } from 'react';
import { workbooksApi } from '../services/api';
import { Workbook, WorkbookSummary, WorkbookCostCode, CostCodeUpdateInput, CalculationResult } from '../types';

export function WorkbookEditor() {
  const [workbooks, setWorkbooks] = useState<WorkbookSummary[]>([]);
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [workbook, setWorkbook] = useState<Workbook | null>(null);
  const [editedCostCodes, setEditedCostCodes] = useState<Map<string, CostCodeUpdateInput>>(new Map());
  const [loading, setLoading] = useState(false);
  const [calculating, setCalculating] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastResult, setLastResult] = useState<CalculationResult | null>(null);

  // Load workbooks list
  useEffect(() => {
    workbooksApi.getAll()
      .then(setWorkbooks)
      .catch((err) => setError(err.message));
  }, []);

  // Auto-select first workbook
  useEffect(() => {
    if (workbooks.length > 0 && selectedId === null) {
      setSelectedId(workbooks[0].id);
    }
  }, [workbooks, selectedId]);

  // Load selected workbook
  useEffect(() => {
    if (selectedId === null) {
      setWorkbook(null);
      return;
    }

    setLoading(true);
    setError(null);
    workbooksApi.getById(selectedId)
      .then((wb) => {
        setWorkbook(wb);
        setEditedCostCodes(new Map());
        setLastResult(null);
      })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, [selectedId]);

  const handleValueChange = useCallback((costCode: WorkbookCostCode, field: keyof CostCodeUpdateInput, value: string) => {
    const numValue = parseFloat(value) || 0;

    setEditedCostCodes((prev) => {
      const newMap = new Map(prev);
      const existing = newMap.get(costCode.cmicCode) || { cmicCode: costCode.cmicCode };
      newMap.set(costCode.cmicCode, { ...existing, [field]: numValue });
      return newMap;
    });
  }, []);

  const getValue = (costCode: WorkbookCostCode, field: 'labor' | 'qty' | 'materials' | 'other'): number => {
    const edited = editedCostCodes.get(costCode.cmicCode);
    if (edited && edited[field] !== undefined) {
      return edited[field]!;
    }
    return costCode[field];
  };

  const hasChanges = editedCostCodes.size > 0;

  const handleCalculate = async () => {
    if (!workbook) return;

    setCalculating(true);
    setError(null);

    try {
      const updates = Array.from(editedCostCodes.values());
      const result = await workbooksApi.calculate(workbook.id,
        updates.length > 0 ? { costCodeInputs: updates } : undefined
      );

      setLastResult(result);

      if (result.success) {
        // Update the workbook with new calculated values
        setWorkbook((prev) => prev ? {
          ...prev,
          costCodes: result.updatedCostCodes,
        } : null);
        setEditedCostCodes(new Map());
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Calculation failed');
    } finally {
      setCalculating(false);
    }
  };

  const handleReset = () => {
    setEditedCostCodes(new Map());
  };

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 2,
    }).format(value);
  };

  return (
    <div style={styles.container}>
      <h1 style={styles.title}>Workbook Cost Code Editor</h1>

      {/* Workbook Selection */}
      <section style={styles.section}>
        <h2>Select Workbook</h2>
        <select
          value={selectedId ?? ''}
          onChange={(e) => setSelectedId(Number(e.target.value) || null)}
          style={styles.select}
        >
          <option value="">-- Select a workbook --</option>
          {workbooks.map((wb) => (
            <option key={wb.id} value={wb.id}>
              {wb.name} ({wb.costCodeCount} cost codes)
            </option>
          ))}
        </select>
      </section>

      {error && (
        <div style={styles.errorBanner}>{error}</div>
      )}

      {loading && <p>Loading workbook...</p>}

      {workbook && (
        <>
          {/* Workbook Details */}
          <section style={styles.section}>
            <h2>Workbook Details</h2>
            <div style={styles.details}>
              <p><strong>Name:</strong> {workbook.name}</p>
              <p><strong>Description:</strong> {workbook.description || 'N/A'}</p>
              <p><strong>Template:</strong> {workbook.templateFilePath}</p>
              <p><strong>Version:</strong> {workbook.version}</p>
            </div>
          </section>

          {/* Action Buttons */}
          <section style={styles.section}>
            <div style={styles.buttonGroup}>
              <button
                onClick={handleCalculate}
                style={{ ...styles.button, ...styles.primaryButton }}
                disabled={calculating}
              >
                {calculating ? 'Calculating...' : 'Calculate with Excel Template'}
              </button>
              {hasChanges && (
                <button
                  onClick={handleReset}
                  style={styles.button}
                  disabled={calculating}
                >
                  Reset Changes
                </button>
              )}
            </div>
            {hasChanges && (
              <p style={styles.changesNote}>
                {editedCostCodes.size} cost code(s) modified
              </p>
            )}
          </section>

          {/* Calculation Result */}
          {lastResult && (
            <section style={{
              ...styles.section,
              backgroundColor: lastResult.success ? '#d4edda' : '#f8d7da',
            }}>
              <h3 style={{ margin: 0 }}>
                {lastResult.success ? 'Calculation Successful' : 'Calculation Failed'}
              </h3>
              {lastResult.error && <p style={{ color: '#721c24' }}>{lastResult.error}</p>}
              <p style={{ margin: '8px 0 0' }}>Execution time: {lastResult.executionTimeMs}ms</p>
            </section>
          )}

          {/* Cost Codes Table */}
          <section style={styles.section}>
            <h2>Cost Codes</h2>
            <div style={styles.tableContainer}>
              <table style={styles.table}>
                <thead>
                  <tr>
                    <th style={styles.th}>CMIC Code</th>
                    <th style={styles.th}>Name</th>
                    <th style={styles.thNumber}>Labor</th>
                    <th style={styles.thNumber}>Qty</th>
                    <th style={styles.thNumber}>Materials</th>
                    <th style={styles.thNumber}>Other</th>
                    <th style={styles.thNumber}>Total Cost</th>
                  </tr>
                </thead>
                <tbody>
                  {workbook.costCodes.map((cc) => {
                    const isEdited = editedCostCodes.has(cc.cmicCode);
                    return (
                      <tr key={cc.id} style={isEdited ? styles.editedRow : undefined}>
                        <td style={styles.td}>{cc.cmicCode}</td>
                        <td style={styles.td}>{cc.name}</td>
                        <td style={styles.tdNumber}>
                          <input
                            type="number"
                            step="0.01"
                            value={getValue(cc, 'labor')}
                            onChange={(e) => handleValueChange(cc, 'labor', e.target.value)}
                            style={styles.input}
                          />
                        </td>
                        <td style={styles.tdNumber}>
                          <input
                            type="number"
                            step="0.01"
                            value={getValue(cc, 'qty')}
                            onChange={(e) => handleValueChange(cc, 'qty', e.target.value)}
                            style={styles.input}
                          />
                        </td>
                        <td style={styles.tdNumber}>
                          <input
                            type="number"
                            step="0.01"
                            value={getValue(cc, 'materials')}
                            onChange={(e) => handleValueChange(cc, 'materials', e.target.value)}
                            style={styles.input}
                          />
                        </td>
                        <td style={styles.tdNumber}>
                          <input
                            type="number"
                            step="0.01"
                            value={getValue(cc, 'other')}
                            onChange={(e) => handleValueChange(cc, 'other', e.target.value)}
                            style={styles.input}
                          />
                        </td>
                        <td style={{ ...styles.tdNumber, fontWeight: 'bold' }}>
                          {formatCurrency(cc.totalCost)}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </section>
        </>
      )}
    </div>
  );
}

const styles: Record<string, React.CSSProperties> = {
  container: {
    maxWidth: '1200px',
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
  changesNote: {
    marginTop: '8px',
    color: '#856404',
    fontStyle: 'italic',
  },
  errorBanner: {
    backgroundColor: '#f8d7da',
    color: '#721c24',
    padding: '12px',
    borderRadius: '4px',
    marginBottom: '16px',
  },
  tableContainer: {
    overflowX: 'auto',
  },
  table: {
    width: '100%',
    borderCollapse: 'collapse',
    backgroundColor: 'white',
  },
  th: {
    padding: '12px 8px',
    textAlign: 'left',
    borderBottom: '2px solid #dee2e6',
    backgroundColor: '#e9ecef',
  },
  thNumber: {
    padding: '12px 8px',
    textAlign: 'right',
    borderBottom: '2px solid #dee2e6',
    backgroundColor: '#e9ecef',
  },
  td: {
    padding: '8px',
    borderBottom: '1px solid #dee2e6',
  },
  tdNumber: {
    padding: '8px',
    borderBottom: '1px solid #dee2e6',
    textAlign: 'right',
  },
  editedRow: {
    backgroundColor: '#fff3cd',
  },
  input: {
    width: '100px',
    padding: '4px 8px',
    fontSize: '14px',
    borderRadius: '4px',
    border: '1px solid #ced4da',
    textAlign: 'right',
  },
};
