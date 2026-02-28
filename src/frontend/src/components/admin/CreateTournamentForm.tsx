import React, { useState } from 'react';
import { adminCreateTournament } from '../../lib/api';
import { CreateTournamentRequest } from '../../types';
import { hapticFeedback } from '../../lib/telegram';
import './admin.css';

interface CreateTournamentFormProps {
  onSuccess: () => void;
  onCancel: () => void;
}

export const CreateTournamentForm: React.FC<CreateTournamentFormProps> = ({ onSuccess, onCancel }) => {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [imageUrl, setImageUrl] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;

    setIsLoading(true);
    setError(null);
    hapticFeedback();

    try {
      const request: CreateTournamentRequest = {
        name: name.trim(),
        description: description.trim() || undefined,
        imageUrl: imageUrl.trim() || undefined,
      };

      await adminCreateTournament(request);
      hapticFeedback('success');
      onSuccess();
    } catch (err: any) {
      setError(err.message || 'Ошибка создания турнира');
      hapticFeedback('error');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <h2 className="modal-title">Новый турнир</h2>

        {error && <div className="error-message" style={{ color: 'red', marginBottom: '10px' }}>{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">Название *</label>
            <input
              type="text"
              className="form-input"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Например: Кубок Мафии 2026"
              required
              maxLength={300}
            />
          </div>

          <div className="form-group">
            <label className="form-label">Описание</label>
            <textarea
              className="form-input"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Краткое описание турнира"
              rows={3}
              maxLength={1000}
              style={{ resize: 'vertical' }}
            />
          </div>

          <div className="form-group">
            <label className="form-label">URL изображения</label>
            <input
              type="url"
              className="form-input"
              value={imageUrl}
              onChange={(e) => setImageUrl(e.target.value)}
              placeholder="https://..."
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
              disabled={isLoading || !name.trim()}
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
