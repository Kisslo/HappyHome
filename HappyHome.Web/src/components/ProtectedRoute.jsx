import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

// Wrap:ar en route. Om användaren inte är inloggad → skicka till /login.
// Om roller anges och användaren saknar dem → /no-access.
export function ProtectedRoute({ children, roller }) {
  const { user, inloggad } = useAuth();
  const location = useLocation();

  if (!inloggad) {
    return <Navigate to="/login" state={{ från: location.pathname }} replace />;
  }

  if (roller && roller.length > 0 && !roller.includes(user?.roll)) {
    return <Navigate to="/no-access" replace />;
  }

  return children;
}
