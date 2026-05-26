import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { authApi } from '../api/auth';

const AuthContext = createContext(null);

const lagra = (token, användare) => {
  localStorage.setItem('happyhome.token', token);
  localStorage.setItem('happyhome.user', JSON.stringify(användare));
};

const städaInloggning = () => {
  localStorage.removeItem('happyhome.token');
  localStorage.removeItem('happyhome.user');
};

const läsLagradAnvändare = () => {
  try {
    const raw = localStorage.getItem('happyhome.user');
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
};

export function AuthProvider({ children }) {
  // Initialt tillstånd kommer från localStorage så att en sidladdning inte
  // loggar ut oss. Det är en avvägning: bekvämt för användaren, något mer
  // exponering om en angripare kommer åt browsern.
  const [user, setUser] = useState(läsLagradAnvändare);
  const [token, setToken] = useState(() => localStorage.getItem('happyhome.token'));

  // Vid mount: om vi har token, verifiera mot /me. Annars är vi utloggade.
  useEffect(() => {
    if (!token) return;
    let avbruten = false;
    authApi
      .me()
      .then((me) => {
        if (!avbruten) setUser(me);
      })
      .catch(() => {
        if (!avbruten) {
          städaInloggning();
          setUser(null);
          setToken(null);
        }
      });
    return () => {
      avbruten = true;
    };
  }, [token]);

  const login = useCallback(async (epost, lösenord) => {
    const data = await authApi.login(epost, lösenord);
    lagra(data.token, data.användare);
    setToken(data.token);
    setUser(data.användare);
    return data.användare;
  }, []);

  const logout = useCallback(() => {
    städaInloggning();
    setToken(null);
    setUser(null);
  }, []);

  const value = useMemo(
    () => ({ user, token, login, logout, inloggad: !!user }),
    [user, token, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth måste användas inuti <AuthProvider>');
  return ctx;
}
