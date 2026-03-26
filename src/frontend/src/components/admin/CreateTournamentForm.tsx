import React, { useEffect, useState } from 'react';
import { adminCreateTournament, adminUpdateTournament } from '../../lib/api';
import {
  cloneTournamentOverlaySettings,
  CreateTournamentRequest,
  OverlayBlockPlacement,
  OverlayStackPanelLayout,
  TournamentDto,
  TournamentOverlaySettings,
  UpdateTournamentRequest,
} from '../../types';
import { hapticFeedback } from '../../lib/telegram';
import './admin.css';

interface CreateTournamentFormProps {
  tournament?: TournamentDto;
  overlaySettingsOnly?: boolean;
  onSuccess: (tournament?: TournamentDto) => void;
  onCancel: () => void;
}

type OverlayBlockKey = 'summaryBlock' | 'firstVoteBlock' | 'lastRoundBlock' | 'footerBlock';
type OverlayPanelKey = 'leftPanel' | 'rightPanel';

const overlayPanelSections: Array<{ key: OverlayPanelKey; title: string; hint: string }> = [
  { key: 'leftPanel', title: 'Левая стек-панель', hint: 'Общий контейнер для блоков слева.' },
  { key: 'rightPanel', title: 'Правая стек-панель', hint: 'Общий контейнер для блоков справа.' },
];

const overlayBlockSections: Array<{ key: OverlayBlockKey; title: string; hint: string }> = [
  { key: 'summaryBlock', title: 'Сводка', hint: 'Блок с процентами по красным и чёрным.' },
  { key: 'firstVoteBlock', title: 'Заголосуют первым', hint: 'Блок с прогнозом первого голосования.' },
  { key: 'lastRoundBlock', title: 'Последний круг', hint: 'Блок с вариантами последнего круга.' },
  { key: 'footerBlock', title: 'Подпись', hint: 'Ссылка на mini app / Telegram.' },
];

const parseNonNegativeNumber = (value: string): number => {
  const parsed = Number.parseInt(value, 10);
  if (Number.isNaN(parsed) || parsed < 0) {
    return 0;
  }

  return parsed;
};

export const CreateTournamentForm: React.FC<CreateTournamentFormProps> = ({
  tournament,
  overlaySettingsOnly = false,
  onSuccess,
  onCancel,
}) => {
  const isEditMode = Boolean(tournament);
  const isOverlaySettingsOnly = overlaySettingsOnly && isEditMode;
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [imageUrl, setImageUrl] = useState('');
  const [teamsText, setTeamsText] = useState('');
  const [operatorUsernamesText, setOperatorUsernamesText] = useState('');
  const [visibleOnHomePage, setVisibleOnHomePage] = useState(true);
  const [showTeamSelection, setShowTeamSelection] = useState(true);
  const [overlaySettings, setOverlaySettings] = useState<TournamentOverlaySettings>(cloneTournamentOverlaySettings());
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setName(tournament?.name ?? '');
    setDescription(tournament?.description ?? '');
    setImageUrl(tournament?.imageUrl ?? '');
    setTeamsText(tournament?.teams.join('\n') ?? '');
    setOperatorUsernamesText(tournament?.operatorUsernames.join('\n') ?? '');
    setVisibleOnHomePage(tournament?.visibleOnHomePage ?? true);
    setShowTeamSelection(tournament?.showTeamSelection ?? true);
    setOverlaySettings(cloneTournamentOverlaySettings(tournament?.overlaySettings));
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

  const updatePanel = (panelKey: OverlayPanelKey, field: keyof OverlayStackPanelLayout, value: number) => {
    setOverlaySettings(prev => ({
      ...prev,
      [panelKey]: {
        ...prev[panelKey],
        [field]: value,
      },
    }));
  };

  const updateBlock = (blockKey: OverlayBlockKey, field: keyof OverlayBlockPlacement, value: string) => {
    setOverlaySettings(prev => ({
      ...prev,
      [blockKey]: {
        ...prev[blockKey],
        [field]: value,
      },
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;

    if (!isOverlaySettingsOnly && showTeamSelection && parsedTeams.length === 0) {
      setError('Добавьте хотя бы одну команду, если выбор команды включен.');
      return;
    }

    setIsLoading(true);
    setError(null);
    hapticFeedback();

    try {
      const requestBase = {
        name: name.trim(),
        description: description.trim() || undefined,
        imageUrl: imageUrl.trim() || undefined,
        teams: parsedTeams,
        operatorUsernames: parsedOperatorUsernames,
        visibleOnHomePage,
        showTeamSelection,
        overlaySettings: cloneTournamentOverlaySettings(overlaySettings),
      };

      let savedTournament: TournamentDto;
      if (isEditMode && tournament) {
        const request: UpdateTournamentRequest = requestBase;
        savedTournament = await adminUpdateTournament(tournament.id, request);
      } else {
        const request: CreateTournamentRequest = requestBase;
        savedTournament = await adminCreateTournament(request);
      }

      hapticFeedback('success');
      onSuccess(savedTournament);
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
        <h2 className="modal-title">
          {isOverlaySettingsOnly
            ? 'Настройки OBS overlay'
            : isEditMode
              ? 'Редактировать турнир'
              : 'Новый турнир'}
        </h2>

        {error && <div className="error-message" style={{ color: 'red', marginBottom: '10px' }}>{error}</div>}

        <form onSubmit={handleSubmit}>
          {!isOverlaySettingsOnly && (
            <>
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
                <label className="form-label">Команды {showTeamSelection ? '*' : '(необязательно)'}</label>
                <textarea
                  className="form-input"
                  value={teamsText}
                  onChange={(e) => setTeamsText(e.target.value)}
                  placeholder={"Одна команда на строку\nНапример:\nСевер\nЮг\nЗапад"}
                  rows={5}
                  style={{ resize: 'vertical' }}
                />
                <div style={{ marginTop: '8px', fontSize: '12px', color: 'var(--tg-theme-hint-color)' }}>
                  {showTeamSelection
                    ? 'Пользователь выбирает команду один раз и больше не сможет изменить выбор.'
                    : 'Список можно оставить пустым, если в этом турнире выбор команды не нужен.'}
                </div>
              </div>

              <div className="form-group">
                <label className="form-checkbox-label">
                  <input
                    type="checkbox"
                    checked={showTeamSelection}
                    onChange={(e) => setShowTeamSelection(e.target.checked)}
                    disabled={isLoading}
                  />
                  <span>Показывать пользователю выбор команды</span>
                </label>
                <div style={{ marginTop: '8px', fontSize: '12px', color: 'var(--tg-theme-hint-color)' }}>
                  Если снять галочку, пользователи смогут участвовать в турнире без выбора команды.
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
            </>
          )}

          {isOverlaySettingsOnly && (
            <div className="form-section-note" style={{ marginBottom: '16px' }}>
              Здесь можно менять только расположение и палитру OBS overlay. Остальные параметры турнира остаются под управлением администратора.
            </div>
          )}

          <div className="form-section">
            <div className="form-section-title">OBS overlay</div>
            <div className="form-section-note">
              Для каждой стек-панели задаются отступ от края и отступ сверху. Каждый блок можно назначить только в левую или правую панель.
            </div>

            <div className="form-group">
              <label className="form-checkbox-label">
                <input
                  type="checkbox"
                  checked={overlaySettings.hideBlocksByPhase}
                  onChange={(e) => setOverlaySettings(prev => ({ ...prev, hideBlocksByPhase: e.target.checked }))}
                  disabled={isLoading}
                />
                <span>Скрывать блоки по фазе игры</span>
              </label>
              <div className="form-section-note" style={{ marginTop: '8px' }}>
                Если галочка включена, блоки «Заголосуют первым» и «Последний круг» будут переключаться автоматически в зависимости от фазы игры. Если выключена, оба блока отображаются одновременно.
              </div>
            </div>

            <div className="overlay-theme-grid">
              <div className="form-group">
                <label className="form-label">Цвет заливки 1</label>
                <div className="form-color-row">
                  <input
                    type="color"
                    className="form-color-input"
                    value={overlaySettings.theme.fillColorStart}
                    onChange={(e) => setOverlaySettings(prev => ({
                      ...prev,
                      theme: { ...prev.theme, fillColorStart: e.target.value.toUpperCase() },
                    }))}
                    disabled={isLoading}
                  />
                  <input
                    type="text"
                    className="form-input"
                    value={overlaySettings.theme.fillColorStart}
                    onChange={(e) => setOverlaySettings(prev => ({
                      ...prev,
                      theme: { ...prev.theme, fillColorStart: e.target.value },
                    }))}
                    disabled={isLoading}
                    maxLength={7}
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label">Цвет заливки 2</label>
                <div className="form-color-row">
                  <input
                    type="color"
                    className="form-color-input"
                    value={overlaySettings.theme.fillColorEnd}
                    onChange={(e) => setOverlaySettings(prev => ({
                      ...prev,
                      theme: { ...prev.theme, fillColorEnd: e.target.value.toUpperCase() },
                    }))}
                    disabled={isLoading}
                  />
                  <input
                    type="text"
                    className="form-input"
                    value={overlaySettings.theme.fillColorEnd}
                    onChange={(e) => setOverlaySettings(prev => ({
                      ...prev,
                      theme: { ...prev.theme, fillColorEnd: e.target.value },
                    }))}
                    disabled={isLoading}
                    maxLength={7}
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label">Прозрачность</label>
                <input
                  type="number"
                  className="form-input"
                  min={0}
                  max={100}
                  value={overlaySettings.theme.fillOpacity}
                  onChange={(e) => setOverlaySettings(prev => ({
                    ...prev,
                    theme: { ...prev.theme, fillOpacity: parseNonNegativeNumber(e.target.value) },
                  }))}
                  disabled={isLoading}
                />
              </div>

              <div className="form-group">
                <label className="form-checkbox-label">
                  <input
                    type="checkbox"
                    checked={overlaySettings.theme.useGradient}
                    onChange={(e) => setOverlaySettings(prev => ({
                      ...prev,
                      theme: { ...prev.theme, useGradient: e.target.checked },
                    }))}
                    disabled={isLoading}
                  />
                  <span>Использовать градиент</span>
                </label>
              </div>
            </div>

            <div className="overlay-settings-grid">
              {overlayPanelSections.map((section) => {
                const panel = overlaySettings[section.key];

                return (
                  <div key={section.key} className="overlay-settings-card">
                    <div className="overlay-settings-card__title">{section.title}</div>
                    <div className="overlay-settings-card__hint">{section.hint}</div>

                    <div className="overlay-settings-fields">
                      <div className="form-group">
                        <label className="form-label">Отступ от края</label>
                        <input
                          type="number"
                          className="form-input"
                          min={0}
                          value={panel.edgeOffset}
                          onChange={(e) => updatePanel(section.key, 'edgeOffset', parseNonNegativeNumber(e.target.value))}
                          disabled={isLoading}
                        />
                      </div>

                      <div className="form-group">
                        <label className="form-label">Отступ сверху</label>
                        <input
                          type="number"
                          className="form-input"
                          min={0}
                          value={panel.topOffset}
                          onChange={(e) => updatePanel(section.key, 'topOffset', parseNonNegativeNumber(e.target.value))}
                          disabled={isLoading}
                        />
                      </div>
                    </div>
                  </div>
                );
              })}

              {overlayBlockSections.map((section) => {
                const block = overlaySettings[section.key];

                return (
                  <div key={`${section.key}-placement`} className="overlay-settings-card">
                    <div className="overlay-settings-card__title">{section.title}</div>
                    <div className="overlay-settings-card__hint">{section.hint}</div>

                    <div className="overlay-settings-fields">
                      <div className="form-group">
                        <label className="form-label">Панель</label>
                        <select
                          className="form-input"
                          value={block.panel}
                          onChange={(e) => updateBlock(section.key, 'panel', e.target.value)}
                          disabled={isLoading}
                        >
                          <option value="left">Левая</option>
                          <option value="right">Правая</option>
                        </select>
                      </div>
                    </div>
                  </div>
                );
              })}
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
              disabled={isLoading || !name.trim() || (!isOverlaySettingsOnly && showTeamSelection && parsedTeams.length === 0)}
            >
              {isLoading && <span className="btn-spinner" />}
              {isLoading
                ? (isEditMode ? 'Сохранение...' : 'Создание...')
                : isOverlaySettingsOnly
                  ? 'Сохранить overlay'
                  : (isEditMode ? 'Сохранить' : 'Создать')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
