import { useState, useEffect, useCallback } from 'react';
import { FunctionWrapper } from '../types';
import { functionWrappersApi } from '../services/api';

interface UseFunctionWrapperResult {
  wrapper: FunctionWrapper | null;
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

/**
 * Hook to fetch a FunctionWrapper by ID
 * Includes all pre-loaded costCodes
 */
export function useFunctionWrapper(id: number | null): UseFunctionWrapperResult {
  const [wrapper, setWrapper] = useState<FunctionWrapper | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchWrapper = useCallback(async () => {
    if (id === null) {
      setWrapper(null);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const data = await functionWrappersApi.getById(id);
      setWrapper(data);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to fetch';
      setError(message);
      setWrapper(null);
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchWrapper();
  }, [fetchWrapper]);

  return { wrapper, loading, error, refetch: fetchWrapper };
}

interface UseFunctionWrappersResult {
  wrappers: FunctionWrapper[];
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

/**
 * Hook to fetch all FunctionWrappers
 */
export function useFunctionWrappers(): UseFunctionWrappersResult {
  const [wrappers, setWrappers] = useState<FunctionWrapper[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchWrappers = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const data = await functionWrappersApi.getAll();
      setWrappers(data);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to fetch';
      setError(message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchWrappers();
  }, [fetchWrappers]);

  return { wrappers, loading, error, refetch: fetchWrappers };
}
