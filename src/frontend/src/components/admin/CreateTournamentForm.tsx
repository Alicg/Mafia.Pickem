import React, { useEffect, useState } from 'react';
import { adminCreateTournament, adminUpdateTournament } from '../../lib/api';
import { CreateTournamentRequest, TournamentDto, UpdateTournamentRequest } from '../../types';
import { hapticFeedback } from '../../lib/telegram';
import './admin.css';

interface CreateTournamentFormProps {
  tournament?: TournamentDto;
  onSuccess: () => void;
  onCancel: () => void;
}

export const CreateTournamentForm: React.FC<CreateTournamentFormProps> = ({ tournament, onSuccess, onCancel }) => {
  const isEditMode = Boolean(tournament);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [imageUrl, setImageUrl] = useState('');
  const [teamsText, setTeamsText] = useState('');
  const [operatorUsernamesText, setOperatorUsernamesText] = useState('');
  const [visibleOnHomePage, setVisibleOnHomePage] = useState(true);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setName(tournament?.name ?? '');
    setDescription(tournament?.description ?? '');
    setImageUrl(tournament?.imageUrl ?? '');
    setTeamsText(tournament?.teams.join('\n') ?? '');
    setOperatorUsernamesText(tournament?.operatorUsernames.join('\n') ?? '');
    setVisibleOnHomePage(tournament?.visibleOnHomePage ?? true);
    setError(null);
  }, [tournament]);

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
      if (isEditMode && tournament) {
        const request: UpdateTournamentRequest = {
          name: name.trim(),
          description: description.trim() || undefined,
          imageUrl: imageUrl.trim() || undefined,
          teams: parsedTeams,
          operatorUsernames: parsedOperatorUsernames,
          visibleOnHomePage,
        };

        await adminUpdateTournament(tournament.id, request);
      } else {
        const request: CreateTournamentRequest = {
          name: name.trim(),
          description: description.trim() || undefined,
          imageUrl: imageUrl.trim() || undefined,
          teams: parsedTeams,
          operatorUsernames: parsedOperatorUsernames,
          visibleOnHomePage,
        };

        await adminCreateTournament(request);
      }

      hapticFeedback('success');
      onSuccess();
    } catch (err) {
      setError(err instanceof Error ? err.message : (isEditMode ? 'Ошибка обновления турнира' : 'Ошибка создания турнира'));
      hapticFeedback('error');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <h2 className="modal-title">{isEditMode ? 'Редактировать турнир' : 'Новый турнир'}</h2>

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

          <div className="form-group">
            <label className="form-checkbox-label">
              <input
                type="checkbox"
                checked={visibleOnHomePage}
                onChange={(e) => setVisibleOnHomePage(e.target.checked)}
                disabled={isLoading}
              />
              <span>Показывать турнир зрителям на главной странице</span>
            </label>
            <div style={{ marginTop: '8px', fontSize: '12px', color: 'var(--tg-theme-hint-color)' }}>
              Если снять галочку, турнир останется доступен администраторам и операторам, но исчезнет из общего списка для зрителей.
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
              {isLoading ? (isEditMode ? 'Сохранение...' : 'Создание...') : (isEditMode ? 'Сохранить' : 'Создать')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
