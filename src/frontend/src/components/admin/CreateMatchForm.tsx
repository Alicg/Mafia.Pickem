import React, { useState } from 'react';
import { adminCreateMatch } from '../../lib/api';
import { CreateMatchRequest } from '../../types';
import { hapticFeedback } from '../../lib/telegram';
import './admin.css';

interface CreateMatchFormProps {
  tournamentId: number;
  onSuccess: () => void;
  onCancel: () => void;
}

export const CreateMatchForm: React.FC<CreateMatchFormProps> = ({ tournamentId, onSuccess, onCancel }) => {
  const [gameNumber, setGameNumber] = useState('');
  const [tableNumber, setTableNumber] = useState('');
  const [externalMatchRef, setExternalMatchRef] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!gameNumber) return;
    
    setIsLoading(true);
    setError(null);
    hapticFeedback();

    try {
      const request: CreateMatchRequest = {
        tournamentId,
        gameNumber: Number(gameNumber),
        tableNumber: tableNumber ? Number(tableNumber) : undefined,
        externalMatchRef: externalMatchRef || undefined
      };

      await adminCreateMatch(request);
      hapticFeedback('success');
      onSuccess();
    } catch (err: any) {
      setError(err.message || 'Ошибка создания игры');
      hapticFeedback('error');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <h2 className="modal-title">Создать игру</h2>
        
        {error && <div className="error-message" style={{color: 'red', marginBottom: '10px'}}>{error}</div>}
        
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">Номер игры *</label>
            <input 
              type="number" 
              className="form-input"
              value={gameNumber}
              onChange={(e) => setGameNumber(e.target.value)}
              required
              min="1"
            />
          </div>
          
          <div className="form-group">
            <label className="form-label">Номер стола</label>
            <input 
              type="number" 
              className="form-input"
              value={tableNumber}
              onChange={(e) => setTableNumber(e.target.value)}
              min="1"
            />
          </div>
          
          <div className="form-group">
            <label className="form-label">Внешний ID (опционально)</label>
            <input 
              type="text" 
              className="form-input"
              value={externalMatchRef}
              onChange={(e) => setExternalMatchRef(e.target.value)}
            />
          </div>
          
          <div className="form-actions">
            <button 
              type="button" 
              className="btn btn-secondary" 
              onClick={onCancel}
              disabled={isLoading}
            >
              Отмена
            </button>
            <button 
              type="submit" 
              className="btn btn-primary"
              disabled={isLoading}
            >
              {isLoading && <span className="btn-spinner" />}
              {isLoading ? 'Создание...' : 'Создать'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
