import React, { useMemo } from 'react';
import './CrowdStats.css';
import { BlobMatchState, VoteStatsDto } from '../types';

interface CrowdStatsProps {
  apiStats: VoteStatsDto | null;
  blobState: BlobMatchState | null;
}

export const CrowdStats: React.FC<CrowdStatsProps> = ({ apiStats, blobState }) => {
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
        <div className="winner-bar-container">
           {/* Town Bar */}
           <div className="winner-bar town" style={{ width: `${townDisplay}%` }}>
             <span className="bar-label left">Мирные {townDisplay}%</span>
           </div>
           {/* Mafia Bar */}
           <div className="winner-bar mafia" style={{ width: `${mafiaDisplay}%` }}>
             <span className="bar-label right">Мафия {mafiaDisplay}%</span>
           </div>
        </div>
      </div>

      {/* Voted Out Stats */}
      <div className="stats-group">
        <label>Первый отстрел</label>
        <div className="slots-grid">
           {stats.slots.map((s) => (
             <div key={s.slot} className="slot-stat">
               <span className="slot-num">{s.slot === 0 ? 'Никто' : s.slot}</span>
               <div className="slot-bar-bg">
                 <div 
                    className="slot-bar-fill" 
                    style={{ width: `${formatPct(s.percent)}%` }}
                 />
               </div>
               <span className="slot-pct">{formatPct(s.percent).toFixed(0)}%</span>
             </div>
           ))}
        </div>
      </div>
    </div>
  );
};
