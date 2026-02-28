import { useEffect, useState, useRef, useCallback } from 'react';
import { BlobMatchState } from '../types';
import { isDemoMode } from '../mocks/demo-mode';
import { demoBlobStates } from '../mocks/demo-data';

const VITE_BLOB_BASE_URL = import.meta.env.VITE_BLOB_BASE_URL || 'https://mafiapickem.blob.core.windows.net/match-state';

interface MatchPollEntry {
  matchId: number;
  blobState: BlobMatchState | null;
  lastVersion: number;
  timeoutId?: ReturnType<typeof setTimeout>;
}

export interface UseMatchStatesResult {
  /** Map of matchId → BlobMatchState (null if not yet loaded) */
  states: Record<number, BlobMatchState | null>;
  /** Whether the initial load is complete for a given matchId */
  isLoading: (matchId: number) => boolean;
  /** Force-refetch a single match's blob state immediately */
  refetchMatch: (matchId: number) => Promise<void>;
}

function getInterval(stateStr?: string): number {
  if (document.hidden) return 20000;
  if (!stateStr) return 10000;
  const s = String(stateStr).toLowerCase();
  if (s === 'open' || s === 'locked' || s === '1' || s === '2') return 2000;
  if (s === 'resolved' || s === '3') return 15000;
  if (s === 'canceled' || s === '4') return 20000;
  return 10000;
}

/**
 * Polls match-state-<id>.json from blob storage for multiple matches simultaneously.
 * Each match polls at its own interval based on the match state.
 */
export function useMatchStates(matchIds: number[]): UseMatchStatesResult {
  const [states, setStates] = useState<Record<number, BlobMatchState | null>>({});
  const entriesRef = useRef<Map<number, MatchPollEntry>>(new Map());
  const mountedRef = useRef(true);
  const matchIdsRef = useRef(matchIds);

  // Demo mode: return static states
  if (isDemoMode) {
    const demoStates: Record<number, BlobMatchState | null> = {};
    for (const id of matchIds) {
      demoStates[id] = demoBlobStates[id] ?? null;
    }
    return {
      states: demoStates,
      isLoading: (id) => !(id in demoBlobStates),
      refetchMatch: async () => {},
    };
  }

  matchIdsRef.current = matchIds;

  const pollMatch = useCallback(async (matchId: number) => {
    if (!mountedRef.current) return;

    const entry = entriesRef.current.get(matchId);
    if (!entry) return;

    try {
      const controller = new AbortController();
      const signalTimeout = setTimeout(() => controller.abort(), 5000);

      const response = await fetch(
        `${VITE_BLOB_BASE_URL}/match-state-${matchId}.json?t=${Date.now()}`,
        { signal: controller.signal }
      );
      clearTimeout(signalTimeout);

      let nextInterval = 10000;

      if (response.ok) {
        const data: BlobMatchState = await response.json();
        if (mountedRef.current && entriesRef.current.has(matchId)) {
          if (data.version !== entry.lastVersion) {
            entry.lastVersion = data.version;
            entry.blobState = data;
            setStates(prev => ({ ...prev, [matchId]: data }));
          }
          nextInterval = getInterval(data.state);
        }
      } else if (response.status !== 404) {
        console.warn(`Polling status ${response.status} for match ${matchId}`);
      }

      if (mountedRef.current && entriesRef.current.has(matchId)) {
        entry.timeoutId = setTimeout(() => pollMatch(matchId), nextInterval);
      }
    } catch (err: unknown) {
      if (err instanceof Error && err.name !== 'AbortError') {
        console.error(`Polling error for match ${matchId}:`, err);
      }
      if (mountedRef.current && entriesRef.current.has(matchId)) {
        entry.timeoutId = setTimeout(() => pollMatch(matchId), 15000);
      }
    }
  }, []);

  useEffect(() => {
    mountedRef.current = true;
    return () => {
      mountedRef.current = false;
      // Clear all polling timers
      for (const entry of entriesRef.current.values()) {
        if (entry.timeoutId) clearTimeout(entry.timeoutId);
      }
      entriesRef.current.clear();
    };
  }, []);

  // Reconcile active match IDs: start new polls, stop removed ones
  useEffect(() => {
    const currentIds = new Set(matchIds);
    const existingIds = new Set(entriesRef.current.keys());

    // Stop polling for removed matches
    for (const id of existingIds) {
      if (!currentIds.has(id)) {
        const entry = entriesRef.current.get(id);
        if (entry?.timeoutId) clearTimeout(entry.timeoutId);
        entriesRef.current.delete(id);
        setStates(prev => {
          const next = { ...prev };
          delete next[id];
          return next;
        });
      }
    }

    // Start polling for new matches
    for (const id of currentIds) {
      if (!existingIds.has(id)) {
        const entry: MatchPollEntry = { matchId: id, blobState: null, lastVersion: -1 };
        entriesRef.current.set(id, entry);
        setStates(prev => ({ ...prev, [id]: null }));
        pollMatch(id);
      }
    }
  }, [matchIds.join(','), pollMatch]);

  // Handle visibility change — restart polling immediately when tab becomes visible
  useEffect(() => {
    const handleVisibilityChange = () => {
      if (!document.hidden && mountedRef.current) {
        for (const [matchId, entry] of entriesRef.current.entries()) {
          if (entry.timeoutId) clearTimeout(entry.timeoutId);
          pollMatch(matchId);
        }
      }
    };
    document.addEventListener('visibilitychange', handleVisibilityChange);
    return () => document.removeEventListener('visibilitychange', handleVisibilityChange);
  }, [pollMatch]);

  const refetchMatch = useCallback(async (matchId: number): Promise<void> => {
    const entry = entriesRef.current.get(matchId);
    if (!entry) return;

    // Cancel existing timer to avoid double-polling
    if (entry.timeoutId) clearTimeout(entry.timeoutId);

    try {
      const response = await fetch(
        `${VITE_BLOB_BASE_URL}/match-state-${matchId}.json?t=${Date.now()}`
      );
      if (response.ok) {
        const data: BlobMatchState = await response.json();
        if (mountedRef.current && entriesRef.current.has(matchId)) {
          entry.lastVersion = data.version;
          entry.blobState = data;
          setStates(prev => ({ ...prev, [matchId]: data }));
        }
      }
    } catch (err) {
      console.error(`Refetch error for match ${matchId}:`, err);
    }

    // Resume normal polling
    if (mountedRef.current && entriesRef.current.has(matchId)) {
      const interval = getInterval(entry.blobState?.state);
      entry.timeoutId = setTimeout(() => pollMatch(matchId), interval);
    }
  }, [pollMatch]);

  return {
    states,
    isLoading: (id: number) => states[id] === undefined || states[id] === null,
    refetchMatch,
  };
}
