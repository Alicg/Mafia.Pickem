import { useEffect, useState, useRef, useCallback } from 'react';
import { BlobMatchState } from '../types';
import { isDemoMode } from '../mocks/demo-mode';
import { demoBlobStates } from '../mocks/demo-data';

const VITE_BLOB_BASE_URL = import.meta.env.VITE_BLOB_BASE_URL || 'https://mafiapickem.blob.core.windows.net/match-state';

interface UseMatchStateResult {
  blobState: BlobMatchState | null;
  isPolling: boolean;
  error: string | null;
}

export function useMatchState(matchId: number | null): UseMatchStateResult {
  const [blobState, setBlobState] = useState<BlobMatchState | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isPolling, setIsPolling] = useState(false);

  // In demo mode, return static demo blob state
  if (isDemoMode) {
    const demoState = matchId ? (demoBlobStates[matchId] ?? null) : null;
    return { blobState: demoState, isPolling: false, error: null };
  }
  
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);
  const lastVersionRef = useRef<number>(-1);
  const mountedRef = useRef(true);
  const matchIdRef = useRef(matchId);

  useEffect(() => {
    matchIdRef.current = matchId;
  }, [matchId]);

  useEffect(() => {
    mountedRef.current = true;
    return () => {
      mountedRef.current = false;
      if (timeoutRef.current) clearTimeout(timeoutRef.current);
    };
  }, []);

  const getInterval = (stateStr?: string) => {
    if (document.hidden) return 20000;
    if (!stateStr) return 10000;

    const s = String(stateStr).toLowerCase();
    if (s === 'open' || s === 'locked' || s === '1' || s === '2') return 2000;
    if (s === 'resolved' || s === '3') return 15000;
    if (s === 'canceled' || s === '4') return 20000;
    return 10000;
  };

  const poll = useCallback(async () => {
    const currentMatchId = matchIdRef.current;
    if (!currentMatchId) return;

    try {
      const controller = new AbortController();
      const signalTimeout = setTimeout(() => controller.abort(), 5000);

      const response = await fetch(`${VITE_BLOB_BASE_URL}/match-state-${currentMatchId}.json?t=${Date.now()}`, {
        signal: controller.signal
      });
      clearTimeout(signalTimeout);

      let nextInterval = 10000;

      if (response.ok) {
        const data: BlobMatchState = await response.json();
        
        if (mountedRef.current && matchIdRef.current === currentMatchId) {
          if (data.version !== lastVersionRef.current) {
            lastVersionRef.current = data.version;
            setBlobState(data);
          }
          setError(null);
          nextInterval = getInterval(data.state);
        }
      } else {
        // If 404 or error, retry with default interval
        if (response.status !== 404) {
             console.warn(`Polling status ${response.status} for match ${currentMatchId}`);
        }
      }

      if (mountedRef.current && matchIdRef.current === currentMatchId) {
        timeoutRef.current = setTimeout(poll, nextInterval);
      }
    } catch (err: unknown) {
        if (err instanceof Error && err.name !== 'AbortError') {
             console.error('Polling error:', err);
        }
        // Retry on error
        if (mountedRef.current && matchIdRef.current === currentMatchId) {
            timeoutRef.current = setTimeout(poll, 15000);
        }
    }
  }, []);

  // Start/Stop polling
  useEffect(() => {
    if (matchId) {
      setIsPolling(true);
      // Reset version on new match logic if needed, but keeping cache across same matchId is usually fine
      // If switching matches we might want to reset though
      lastVersionRef.current = -1;  
      setBlobState(null);
      setError(null);
      
      poll();
    } else {
      setIsPolling(false);
      if (timeoutRef.current) clearTimeout(timeoutRef.current);
      setBlobState(null);
    }
  }, [matchId, poll]);

  // Handle visibility
  useEffect(() => {
    const handleVisibilityChange = () => {
      if (!document.hidden && matchIdRef.current && mountedRef.current) {
        if (timeoutRef.current) clearTimeout(timeoutRef.current);
        poll();
      }
    };
    
    document.addEventListener('visibilitychange', handleVisibilityChange);
    return () => {
        document.removeEventListener('visibilitychange', handleVisibilityChange);
    };
  }, [poll]);

  return { blobState, isPolling, error };
}
