import React, { useEffect, useState } from 'react';
import '../App.css';

export const LoadingPage: React.FC = () => {
  const [dots, setDots] = useState('');

  useEffect(() => {
    const interval = setInterval(() => {
      setDots(prev => prev.length >= 3 ? '' : prev + '.');
    }, 500);
    return () => clearInterval(interval);
  }, []);

  return (
    <div className="center-container" style={{ gap: '20px' }}>
      <div style={{ 
        fontSize: '32px', 
        fontWeight: 800, 
        letterSpacing: '-0.03em',
        background: 'linear-gradient(135deg, #4f7cff, #7c5cff)',
        WebkitBackgroundClip: 'text',
        WebkitTextFillColor: 'transparent',
        marginBottom: '4px'
      }}>
        Mafia Pick'em
      </div>
      <div className="spinner"></div>
      <p style={{ 
        color: 'var(--tg-theme-hint-color)', 
        fontSize: '14px',
        fontWeight: 500 
      }}>
        Загрузка{dots}
      </p>
    </div>
  );
};
