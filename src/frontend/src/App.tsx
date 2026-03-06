import { useState } from 'react';
import './App.css';
import { useAuth } from './hooks/useAuth';
import { TournamentDto } from './types';
import { LoadingPage } from './pages/LoadingPage';
import { RegisterPage } from './pages/RegisterPage';
import { TournamentsListPage } from './pages/TournamentsListPage';
import { TournamentPage } from './pages/TournamentPage';

function App() {
  const { user, isLoading, error, isAuthenticated, completeRegistration } = useAuth();
  const [selectedTournament, setSelectedTournament] = useState<TournamentDto | null>(null);

  if (isLoading) {
    return <LoadingPage />;
  }

  if (error) {
    return (
      <div className="center-container">
        <div style={{ color: 'var(--tg-theme-text-color)', textAlign: 'center' }}>
          <h3>Ошибка</h3>
          <p>{error}</p>
          <button 
            className="button-primary" 
            style={{ marginTop: '20px' }}
            onClick={() => window.location.reload()}
          >
            Обновить
          </button>
        </div>
      </div>
    );
  }

  if (!isAuthenticated || !user) {
    return <div>Authentication failed.</div>;
  }

  if (!user.isRegistered) {
    return <RegisterPage onSuccess={completeRegistration} />;
  }

  if (selectedTournament) {
    return (
      <TournamentPage
        tournament={selectedTournament}
        onBack={() => setSelectedTournament(null)}
      />
    );
  }

  return <TournamentsListPage onSelect={setSelectedTournament} isAdmin={user.isAdmin} />;
}

export default App;
