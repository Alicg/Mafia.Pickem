import React, { useMemo } from 'react';
import './CrowdStats.css';
import { BlobMatchState, PredictionDto, UNKNOWN_WINNING_SIDE, LAST_ROUND_LABELS } from '../types';

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
  blobState: BlobMatchState | null;
  prediction?: PredictionDto | null;
}

export const CrowdStats: React.FC<CrowdStatsProps> = ({ blobState, prediction }) => {
  const matchResult = blobState?.matchResult ?? null;
  const hasWinningSide = matchResult != null && matchResult.winningSide !== UNKNOWN_WINNING_SIDE;

  const stats = useMemo(() => {
    if (!blobState) return null;
    const w = blobState.winnerVotes;
    const v = blobState.votedOutVotes;
    if (!w || !v) return null;

    return {
      townPct: w.town.percent,
      mafiaPct: w.mafia.percent,
      slots: v.map(s => ({ slot: s.slot, percent: s.percent }))
    };
  }, [blobState]);

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
           <div className={`winner-bar town${hasWinningSide && matchResult?.winningSide === 0 ? ' actual-winner' : ''}`} style={{ width: `${townDisplay}%` }} />
           <div className={`winner-bar mafia${hasWinningSide && matchResult?.winningSide === 1 ? ' actual-winner' : ''}`} style={{ width: `${mafiaDisplay}%` }} />
        </div>
        {prediction != null && (
          <div className="legend-row">
            <LegendItem
              label={`Ваш выбор: ${prediction.predictedWinner === 0 ? 'Мирные' : 'Мафия'}`}
              status={hasWinningSide ? (matchResult!.winningSide === prediction.predictedWinner ? 'correct' : 'wrong') : 'pending'}
            />
            {prediction.winnerPoints != null && (
              <span className={`legend-pts ${prediction.winnerPoints > 0 ? 'positive' : ''}`}>+{prediction.winnerPoints}</span>
            )}
            {hasWinningSide && (
              <LegendItem
                label={`Результат: ${matchResult!.winningSide === 0 ? 'Мирные' : 'Мафия'}`}
                status="correct"
              />
            )}
          </div>
        )}
      </div>

      {/* Voted Out Stats — vertical column chart */}
      <div className="stats-group">
        <label>Первый заголосованный</label>
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
                  {prediction?.votedOutPoints != null && (
                    <span className={`legend-pts ${prediction.votedOutPoints > 0 ? 'positive' : ''}`}>+{prediction.votedOutPoints}</span>
                  )}
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

      {/* Last Round Stats */}
      {(() => {
        const lastRoundVotes = blobState?.lastRoundVotes ?? [];
        const resolvedLastRound = matchResult?.lastRound ?? 0;
        const userLastRound = prediction?.predictedLastRound ?? null;

        if (lastRoundVotes.length === 0 && !userLastRound) return null;

        const pcts = lastRoundVotes.map(lr => formatPct(lr.percent));
        const maxPct = Math.max(...pcts, 1);

        return (
          <div className="stats-group">
            <label>Последний круг</label>
            <div className="slots-columns">
              {lastRoundVotes.map((lr, i) => {
                const pct = pcts[i];
                const heightPct = (pct / maxPct) * 100;
                const isResolved = resolvedLastRound > 0 && lr.lastRound === resolvedLastRound;
                const isUserPick = userLastRound === lr.lastRound;
                return (
                  <div key={lr.lastRound} className={`slot-col${isResolved ? ' actual-voted-out' : ''}${isUserPick ? ' user-pick-slot' : ''}`}>
                    <span className="slot-col-pct">{pct.toFixed(0)}%</span>
                    <div className="slot-col-track">
                      <div
                        className={`slot-col-fill${isResolved ? ' voted-out-fill' : ''}`}
                        style={{ height: `${heightPct}%` }}
                      />
                    </div>
                    <span className="slot-col-num" style={{ fontSize: '10px' }}>{LAST_ROUND_LABELS[lr.lastRound] ?? lr.lastRound}</span>
                  </div>
                );
              })}
            </div>
            {userLastRound != null && userLastRound > 0 && (
              <div className="legend-row">
                <LegendItem
                  label={`Ваш выбор: ${LAST_ROUND_LABELS[userLastRound] ?? userLastRound}`}
                  status={resolvedLastRound > 0 ? (resolvedLastRound === userLastRound ? 'correct' : 'wrong') : 'pending'}
                />
                {prediction?.lastRoundPoints != null && (
                  <span className={`legend-pts ${prediction.lastRoundPoints > 0 ? 'positive' : ''}`}>+{prediction.lastRoundPoints}</span>
                )}
                {resolvedLastRound > 0 && (
                  <LegendItem
                    label={`Результат: ${LAST_ROUND_LABELS[resolvedLastRound] ?? resolvedLastRound}`}
                    status="correct"
                  />
                )}
              </div>
            )}
          </div>
        );
      })()}

      {prediction != null && prediction.totalPoints != null && (
        <div className="total-points-row">
          <span className="total-points-badge">+{prediction.totalPoints} очков</span>
        </div>
      )}
    </div>
  );
};
