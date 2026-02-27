import React, { useState } from 'react';
import { updateNickname } from '../lib/api';
import { hapticFeedback } from '../lib/telegram';
import '../App.css';

interface RegisterPageProps {
  onSuccess: () => void;
}

export const RegisterPage: React.FC<RegisterPageProps> = ({ onSuccess }) => {
  const [nickname, setNickname] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const validateNickname = (val: string): string | null => {
    if (val.length < 2) return 'Минимум 2 символа';
    if (val.length > 30) return 'Максимум 30 символов';
    if (!/^[a-zA-Z0-9_-]+$/.test(val)) return 'Только буквы, цифры, - и _';
    return null;
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value;
    setNickname(val);
    const validationError = validateNickname(val);
    setError(validationError);


  };

  const handleSubmit = async () => {
    if (isSubmitting) return;

    try {
      setIsSubmitting(true);
      hapticFeedback('medium');
      await updateNickname(nickname);
      hapticFeedback('light'); // Success
      onSuccess();
    } catch (err: any) {
      console.error(err);
      setError('Ошибка регистрации. Возможно, ник занят.');
      hapticFeedback('heavy'); // Error
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="container">
      <h1>Добро пожаловать</h1>
      <p className="description">
        Введите ваш игровой никнейм для участия в прогнозах.
      </p>

      <div className="form-group">
        <label htmlFor="nickname">Никнейм</label>
        <input
          id="nickname"
          type="text"
          value={nickname}
          onChange={handleInputChange}
          placeholder="SuperPlayer123"
          autoComplete="off"
          className={error ? 'error' : ''}
          disabled={isSubmitting}
        />
        {error && <div className="error-message">{error}</div>}
      </div>

      <button
        className="inline-submit-btn"
        onClick={handleSubmit}
        disabled={isSubmitting || !!error || nickname.length === 0}
      >
        {isSubmitting ? 'Регистрация...' : 'НАЧАТЬ'}
      </button>

      <div style={{ marginTop: '20px', fontSize: '0.9em', color: 'var(--tg-theme-hint-color)' }}>
        Никнейм будет виден в таблице лидеров.
      </div>
    </div>
  );
};
