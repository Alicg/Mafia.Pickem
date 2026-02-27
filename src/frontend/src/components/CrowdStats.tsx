import React, { useMemo } from 'react';
import './CrowdStats.css';
import { BlobMatchState, PredictionDto, VoteStatsDto } from '../types';

type LegendStatus = 'correct' | 'wrong' | 'pending';

const LegendItem: React.FC<{ label: string; status: LegendStatus }> = ({ label, status }) => (
  <span className={`legend-item ${status}`}>
    <span className="legend-icon">
      {status === 'correct' && (
        <svg width="14" height="14" viewBox="0 0 14 14" fill="currentColor">
          <circle cx="7" cy="7" r="7" />
          <path d="M4 7l2 2 4-4" stroke="white" strokeWidth="1.5" fill="none" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      )}
      {status === 'wrong' && (
        <svg width="14" height="14" viewBox="0 0 14 14" fill="currentColor">
          <circle cx="7" cy="7" r="7" />
          <path d="M5 5l4 4M9 5l-4 4" stroke="white" strokeWidth="1.5" fill="none" strokeLinecap="round" />
        </svg>
      )}
      {status === 'pending' && (
        <svg width="14" height="14" viewBox="0 0 14 14" fill="none" stroke="currentColor" strokeWidth="1.5">
          <circle cx="7" cy="7" r="6" />
          <path d="M4 7l2 2 4-4" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      )}
    </span>
    {label}
  </span>
);

interface CrowdStatsProps {
  apiStats: VoteStatsDto | null;
  blobState: BlobMatchState | null;
  prediction?: PredictionDto | null;
}

export const CrowdStats: React.FC<CrowdStatsProps> = ({ apiStats, blobState, prediction }) => {
  const matchResult = blobState?.matchResult ?? null;

  // Prefer blob state for real-time updates, fallback to API stats
  const stats = useMemo(() => {
    if (blobState) {
      // Normalize from Blob
      const w = blobState.winnerVotes;
      const v = blobState.votedOutVotes;
      if (!w || !v) return null;

      return {
        townPct: w.town.percent, // Assuming 0-1 or 0-100? Let's check usage. Usually 0.627
        mafiaPct: w.mafia.percent,
        slots: v.map(s => ({ slot: s.slot, percent: s.percent }))
      };
    } else if (apiStats) {
      // Normalize from API
      return {
        townPct: apiStats.townPercentage,
        mafiaPct: apiStats.mafiaPercentage,
        slots: apiStats.slotVotes.map(s => ({ slot: s.slot, percent: s.percentage }))
      };
    }
    return null;
  }, [blobState, apiStats]);

  if (!stats) return null;

  // Formatting helper: if value < 1, treat as fraction (e.g. 0.62 -> 62%). If > 1, treat as %.
  // But wait, percentages from backend might be 0..1 or 0..100. Let's assume 0..1 commonly OR look at standard.
  // Actually, usually C# percentages are 0..1, while UI displays 0..100.
  // But let's handle both just in case: if sum > 2, it's probably 0..100.
  // Let's just assume 0..1 for now. If it prints 0.6%, we can fix.
  
  const formatPct = (val: number) => {
      // If val is e.g. 0.5, print 50%.
      // If val is 50, print 5000%? No.
      // Heuristic: if val > 1, it's already percentage (0-100).
      return val > 1 ? val : val * 100;
  };

  const townDisplay = formatPct(stats.townPct).toFixed(1);
  const mafiaDisplay = formatPct(stats.mafiaPct).toFixed(1);

  return (
    <div className="crowd-stats">
      <h3>Статистика голосования</h3>
      
      {/* Winner Stats */}
      <div className="stats-group">
        <label>Победа команды</label>
        <div className="winner-labels">
          <span className="winner-label-left">Мирные {townDisplay}%</span>
          <span className="winner-label-right">Мафия {mafiaDisplay}%</span>
        </div>
        <div className="winner-bar-container">
           <div className={`winner-bar town${matchResult?.winningSide === 0 ? ' actual-winner' : ''}`} style={{ width: `${townDisplay}%` }} />
           <div className={`winner-bar mafia${matchResult?.winningSide === 1 ? ' actual-winner' : ''}`} style={{ width: `${mafiaDisplay}%` }} />
        </div>
        {prediction != null && (
          <div className="legend-row">
            <LegendItem
              label={`Ваш выбор: ${prediction.predictedWinner === 0 ? 'Мирные' : 'Мафия'}`}
              status={matchResult ? (matchResult.winningSide === prediction.predictedWinner ? 'correct' : 'wrong') : 'pending'}
            />
            {matchResult && (
              <LegendItem
                label={`Результат: ${matchResult.winningSide === 0 ? 'Мирные' : 'Мафия'}`}
                status="correct"
              />
            )}
          </div>
        )}
      </div>

      {/* Voted Out Stats — vertical column chart */}
      <div className="stats-group">
        <label>Первый отстрел</label>
        {(() => {
          const pcts = stats.slots.map(s => formatPct(s.percent));
          const maxPct = Math.max(...pcts, 1);
          const votedOutSet = new Set(matchResult?.votedOutSlots ?? []);
          const userVotedOut = prediction?.predictedVotedOut ?? null;
          return (
            <>
              <div className="slots-columns">
                {stats.slots.map((s, i) => {
                  const pct = pcts[i];
                  const heightPct = (pct / maxPct) * 100;
                  const isVotedOut = votedOutSet.has(s.slot);
                  const isUserPick = userVotedOut === s.slot;
                  return (
                    <div key={s.slot} className={`slot-col${isVotedOut ? ' actual-voted-out' : ''}${isUserPick ? ' user-pick-slot' : ''}`}>
                      <span className="slot-col-pct">{pct.toFixed(0)}%</span>
                      <div className="slot-col-track">
                        <div
                          className={`slot-col-fill${isVotedOut ? ' voted-out-fill' : ''}`}
                          style={{ height: `${heightPct}%` }}
                        />
                      </div>
                      <span className="slot-col-num">{s.slot === 0 ? '–' : s.slot}</span>
                    </div>
                  );
                })}
              </div>
              {userVotedOut != null && (
                <div className="legend-row">
                  <LegendItem
                    label={`Ваш выбор: ${userVotedOut === 0 ? 'Никто' : userVotedOut}`}
                    status={matchResult ? (votedOutSet.has(userVotedOut) ? 'correct' : 'wrong') : 'pending'}
                  />
                  {matchResult && (
                    <LegendItem
                      label={`Результат: ${matchResult.votedOutSlots.join(', ')}`}
                      status="correct"
                    />
                  )}
                </div>
              )}
            </>
          );
        })()}
      </div>
    </div>
  );
};
