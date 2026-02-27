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
    <div className="center-container">
      <div className="spinner"></div>
      <p style={{ marginTop: '1rem' }}>Загрузка{dots}</p>
    </div>
  );
};
