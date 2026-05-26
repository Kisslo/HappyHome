import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const standardLanding = {
  Klient: '/mina-bokningar',
  Terapeut: '/mina-patienter',
  Admin: '/admin',
};

export default function Hem() {
  const { user, inloggad } = useAuth();
  if (!inloggad) return <Navigate to="/login" replace />;
  const dest = standardLanding[user?.roll] ?? '/login';
  return <Navigate to={dest} replace />;
}
