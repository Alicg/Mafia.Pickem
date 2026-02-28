import { useState, useEffect } from 'react';
import { authenticateTelegram, devAuthenticate, setAuthToken, getProfile } from '../lib/api';
import { getInitData, expandApp } from '../lib/telegram';

const isDevAuth = import.meta.env.VITE_DEV_AUTH === 'true';
import { UserProfile } from '../types';
import { isDemoMode } from '../mocks/demo-mode';
import { demoUser } from '../mocks/demo-data';

interface AuthState {
  user: UserProfile | null;
  isLoading: boolean;
  error: string | null;
  isAuthenticated: boolean;
}

export function useAuth() {
  const [state, setState] = useState<AuthState>({
    user: null,
    isLoading: true,
    error: null,
    isAuthenticated: false,
  });

  const refreshAuth = async () => {
    if (isDemoMode) {
      setState({ user: demoUser, isLoading: false, error: null, isAuthenticated: true });
      return;
    }
    try {
      setState(prev => ({ ...prev, isLoading: true, error: null }));
      const user = await getProfile();
      setState({
        user,
        isLoading: false,
        error: null,
        isAuthenticated: true,
      });
    } catch (err) {
      setState(prev => ({
        ...prev,
        isLoading: false,
        error: err instanceof Error ? err.message : 'Unknown error',
      }));
    }
  };

  useEffect(() => {
    if (isDemoMode) {
      setState({ user: demoUser, isLoading: false, error: null, isAuthenticated: true });
      return;
    }

    const initAuth = async () => {
      expandApp(); // Start expanded
      
      try {
        let response;
        if (isDevAuth) {
          // Dev mode: bypass Telegram, authenticate directly
          response = await devAuthenticate();
        } else {
          const initData = getInitData();
          if (!initData) {
            setState({
              user: null,
              isLoading: false,
              error: 'No Telegram initData found. Please open in Telegram.',
              isAuthenticated: false,
            });
            return;
          }
          response = await authenticateTelegram(initData);
        }
        setAuthToken(response.token);
        
        setState({
          user: response.user,
          isLoading: false,
          error: null,
          isAuthenticated: true,
        });
      } catch (err) {
        console.error('Auth failed', err);
        setState({
          user: null,
          isLoading: false,
          error: 'Authentication failed. Please try again.',
          isAuthenticated: false,
        });
      }
    };

    initAuth();
  }, []);

  return { ...state, refreshAuth };
}
