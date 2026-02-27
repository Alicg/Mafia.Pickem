import './App.css';
import { useAuth } from './hooks/useAuth';
import { LoadingPage } from './pages/LoadingPage';
import { RegisterPage } from './pages/RegisterPage';
import { TournamentPage } from './pages/TournamentPage';

function App() {
  const { user, isLoading, error, isAuthenticated, refreshAuth } = useAuth();

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
    // Should be handled by error state ideally, but fallback
    return <div>Authentication failed.</div>;
  }

  if (!user.isRegistered) {
    return <RegisterPage onSuccess={refreshAuth} />;
  }

  // Main App Content
  return <TournamentPage />;
}

export default App;
