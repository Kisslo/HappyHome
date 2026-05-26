import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { bokningApi } from '../../api/bokning';
import { useAuth } from '../../context/AuthContext';

const idagISO = () => new Date().toISOString().slice(0, 10);

const tidEtikett = (iso) =>
  new Date(iso).toLocaleString('sv-SE', {
    weekday: 'long',
    day: 'numeric',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit',
  });

const statusFärg = {
  Bokad: 'bg-happy-light text-happy-dark',
  Genomförd: 'bg-blue-50 text-blue-700',
  Avbokad: 'bg-gray-100 text-gray-600',
  Uteblev: 'bg-amber-50 text-amber-700',
};

export default function MinaPatienter() {
  const { user } = useAuth();
  const terapeutId = user?.terapeutId;

  const { data: bokningar = [], isLoading } = useQuery({
    queryKey: ['bokningar-terapeut', terapeutId],
    queryFn: () => bokningApi.förTerapeut(terapeutId),
    enabled: !!terapeutId,
  });

  if (!terapeutId)
    return <p className="text-happy-ink/60">Din inloggning saknar koppling till en terapeutprofil.</p>;

  const idag = idagISO();
  const idagBokningar = bokningar.filter(
    (b) => b.tidslucka?.start?.startsWith(idag) && b.status === 'Bokad',
  );
  const övriga = bokningar.filter((b) => !idagBokningar.includes(b));

  return (
    <div className="space-y-8">
      <h1 className="text-2xl font-semibold text-happy-ink">Mina patienter</h1>

      {isLoading && <p className="text-happy-ink/60">Laddar…</p>}

      <section>
        <h2 className="text-sm uppercase tracking-wider text-happy-ink/60 mb-2">
          Idag ({idagBokningar.length})
        </h2>
        {idagBokningar.length === 0 ? (
          <p className="text-sm text-happy-ink/60">Inga bokade möten idag.</p>
        ) : (
          <BokningsLista bokningar={idagBokningar} markeraIdag />
        )}
      </section>

      <section>
        <h2 className="text-sm uppercase tracking-wider text-happy-ink/60 mb-2">
          Övriga bokningar ({övriga.length})
        </h2>
        {övriga.length === 0 ? (
          <p className="text-sm text-happy-ink/60">Inga övriga bokningar.</p>
        ) : (
          <BokningsLista bokningar={övriga} />
        )}
      </section>
    </div>
  );
}

function BokningsLista({ bokningar, markeraIdag = false }) {
  return (
    <ul className="bg-white border border-happy-light rounded-xl divide-y divide-happy-light">
      {bokningar.map((b) => (
        <li
          key={b.id}
          className={`flex items-center justify-between px-4 py-3 ${
            markeraIdag ? 'bg-happy-light/40' : ''
          }`}
        >
          <div>
            <div className="font-medium text-happy-ink">
              {b.klient ? `${b.klient.förnamn} ${b.klient.efternamn}` : `Klient #${b.klientId}`}
              {markeraIdag && (
                <span className="ml-2 text-xs bg-happy text-white px-2 py-0.5 rounded-full">
                  IDAG
                </span>
              )}
            </div>
            <div className="text-xs text-happy-ink/60">
              {b.tidslucka ? tidEtikett(b.tidslucka.start) : 'Tid saknas'} · {b.terapiTyp}
              {b.anledningTillBesok && (
                <span className="ml-2 italic">"{b.anledningTillBesok}"</span>
              )}
            </div>
          </div>
          <div className="flex items-center gap-3">
            <span className={`text-xs px-2 py-1 rounded-full ${statusFärg[b.status] ?? 'bg-gray-100'}`}>
              {b.status}
            </span>
            <Link
              to={`/konsultation/${b.klientId}`}
              className="text-sm px-3 py-1.5 rounded-md bg-happy text-white hover:bg-happy-dark"
            >
              Öppna konsultation
            </Link>
          </div>
        </li>
      ))}
    </ul>
  );
}
