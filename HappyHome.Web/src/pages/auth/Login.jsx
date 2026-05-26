import { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

const standardLanding = {
  Klient: '/mina-bokningar',
  Terapeut: '/mina-patienter',
  Admin: '/admin',
};

export default function Login() {
  const [epost, setEpost] = useState('');
  const [lösenord, setLösenord] = useState('');
  const [fel, setFel] = useState(null);
  const [laddar, setLaddar] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const onSubmit = async (e) => {
    e.preventDefault();
    setFel(null);
    setLaddar(true);
    try {
      const user = await login(epost, lösenord);
      const tillbaks = location.state?.från ?? standardLanding[user.roll] ?? '/';
      navigate(tillbaks, { replace: true });
    } catch (err) {
      const msg =
        err.response?.data?.message ??
        'Inloggningen misslyckades. Kontrollera e-post och lösenord.';
      setFel(msg);
    } finally {
      setLaddar(false);
    }
  };

  return (
    <div className="max-w-md mx-auto mt-12">
      <div className="text-center mb-8">
        <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-happy text-white text-2xl font-bold mb-3">
          H
        </div>
        <h1 className="text-3xl font-semibold text-happy">Happy Home</h1>
        <p className="text-happy-ink/60 mt-1">Logga in för att fortsätta</p>
      </div>

      <form
        onSubmit={onSubmit}
        className="bg-white rounded-2xl shadow-sm p-6 border border-happy-light space-y-4"
      >
        <div>
          <label className="block text-sm font-medium text-happy-ink mb-1">
            E-post
          </label>
          <input
            type="email"
            required
            value={epost}
            onChange={(e) => setEpost(e.target.value)}
            className="w-full rounded-md border border-happy-light px-3 py-2 focus:border-happy focus:outline-none"
            placeholder="namn@happyhome.se"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-happy-ink mb-1">
            Lösenord
          </label>
          <input
            type="password"
            required
            value={lösenord}
            onChange={(e) => setLösenord(e.target.value)}
            className="w-full rounded-md border border-happy-light px-3 py-2 focus:border-happy focus:outline-none"
            placeholder="••••••••"
          />
        </div>

        {fel && (
          <div className="text-sm text-red-700 bg-red-50 border border-red-200 rounded-md px-3 py-2">
            {fel}
          </div>
        )}

        <button
          type="submit"
          disabled={laddar}
          className="w-full bg-happy text-white py-2 rounded-md font-medium hover:bg-happy-dark disabled:opacity-50"
        >
          {laddar ? 'Loggar in…' : 'Logga in'}
        </button>

        <div className="text-xs text-happy-ink/50 pt-2 border-t border-happy-light">
          <div className="font-medium text-happy-ink/70 mb-1">Testkonton (alla med lösen <code>Demo123!</code>)</div>
          <ul className="space-y-0.5">
            <li>klient@happyhome.se</li>
            <li>terapeut@happyhome.se</li>
            <li>admin@happyhome.se</li>
          </ul>
        </div>
      </form>
    </div>
  );
}
