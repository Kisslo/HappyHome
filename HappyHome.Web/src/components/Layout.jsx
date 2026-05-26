import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const linkClass = ({ isActive }) =>
  `px-3 py-2 rounded-md text-sm font-medium transition-colors ${
    isActive
      ? 'bg-happy text-white'
      : 'text-happy-ink hover:bg-happy-light'
  }`;

export default function Layout() {
  const { user, logout, inloggad } = useAuth();
  const navigate = useNavigate();

  const meny = (() => {
    if (!inloggad) return [];
    if (user.roll === 'Klient') {
      return [
        { to: '/mina-bokningar', label: 'Mina bokningar' },
        { to: '/boka', label: 'Boka' },
        { to: '/min-journal', label: 'Min journal' },
      ];
    }
    if (user.roll === 'Terapeut') {
      return [
        { to: '/mina-patienter', label: 'Mina patienter' },
        { to: '/ai-diagnos', label: 'AI-diagnos' },
      ];
    }
    if (user.roll === 'Admin') {
      return [
        { to: '/admin', label: 'Dashboard' },
        { to: '/admin/terapeuter', label: 'Terapeuter' },
        { to: '/admin/tidsluckor', label: 'Tidsluckor' },
      ];
    }
    return [];
  })();

  return (
    <div className="min-h-screen flex flex-col">
      <header className="bg-white border-b border-happy-light">
        <div className="max-w-6xl mx-auto flex items-center justify-between px-6 py-3">
          <Link to="/" className="flex items-center gap-2">
            <span className="w-8 h-8 rounded-full bg-happy flex items-center justify-center text-white font-bold">H</span>
            <span className="text-happy font-semibold text-lg tracking-tight">Happy Home</span>
          </Link>

          <nav className="flex items-center gap-1">
            {meny.map((m) => (
              <NavLink key={m.to} to={m.to} className={linkClass}>
                {m.label}
              </NavLink>
            ))}
          </nav>

          {inloggad ? (
            <div className="flex items-center gap-3 text-sm">
              <span className="text-happy-ink/70">
                {user.epost} <span className="text-happy">({user.roll})</span>
              </span>
              <button
                onClick={() => {
                  logout();
                  navigate('/login');
                }}
                className="px-3 py-1.5 rounded-md border border-happy text-happy hover:bg-happy-light"
              >
                Logga ut
              </button>
            </div>
          ) : (
            <Link to="/login" className="px-3 py-1.5 rounded-md bg-happy text-white text-sm">
              Logga in
            </Link>
          )}
        </div>
      </header>

      <main className="flex-1 max-w-6xl w-full mx-auto px-6 py-8">
        <Outlet />
      </main>

      <footer className="border-t border-happy-light bg-white py-3 text-center text-xs text-happy-ink/60">
        Happy Home — fiktivt demosystem för undervisning
      </footer>
    </div>
  );
}
