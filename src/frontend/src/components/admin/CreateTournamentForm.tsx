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
  const [teamsText, setTeamsText] = useState('');
  const [operatorUsernamesText, setOperatorUsernamesText] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const parsedTeams = teamsText
    .split(/[\n,;]/)
    .map(team => team.trim())
    .filter(Boolean)
    .filter((team, index, all) => all.findIndex(item => item.toLowerCase() === team.toLowerCase()) === index);

  const parsedOperatorUsernames = operatorUsernamesText
    .split(/[\n,;\s]+/)
    .map(username => username.trim())
    .filter(Boolean)
    .map(username => username.startsWith('@') ? username : `@${username}`)
    .filter((username, index, all) => all.findIndex(item => item.toLowerCase() === username.toLowerCase()) === index);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim() || parsedTeams.length === 0) return;

    setIsLoading(true);
    setError(null);
    hapticFeedback();

    try {
      const request: CreateTournamentRequest = {
        name: name.trim(),
        description: description.trim() || undefined,
        imageUrl: imageUrl.trim() || undefined,
        teams: parsedTeams,
        operatorUsernames: parsedOperatorUsernames,
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

          <div className="form-group">
            <label className="form-label">Команды *</label>
            <textarea
              className="form-input"
              value={teamsText}
              onChange={(e) => setTeamsText(e.target.value)}
              placeholder={"Одна команда на строку\nНапример:\nСевер\nЮг\nЗапад"}
              rows={5}
              style={{ resize: 'vertical' }}
            />
            <div style={{ marginTop: '8px', fontSize: '12px', color: 'var(--tg-theme-hint-color)' }}>
              Пользователь выбирает команду один раз и больше не сможет изменить выбор.
            </div>
          </div>

          <div className="form-group">
            <label className="form-label">Операторы турнира</label>
            <textarea
              className="form-input"
              value={operatorUsernamesText}
              onChange={(e) => setOperatorUsernamesText(e.target.value)}
              placeholder={"Добавьте Telegram usernames через @userName\nНапример:\n@ivan_admin\n@marina_ops"}
              rows={4}
              style={{ resize: 'vertical' }}
            />
            <div style={{ marginTop: '8px', fontSize: '12px', color: 'var(--tg-theme-hint-color)' }}>
              Операторы смогут создавать игры и менять статусы игр этого турнира. Пользователь должен хотя бы один раз открыть mini app.
            </div>
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
              disabled={isLoading || !name.trim() || parsedTeams.length === 0}
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
