import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { bokningApi } from '../../api/bokning';
import { useAuth } from '../../context/AuthContext';

const formatTid = (iso) =>
  new Date(iso).toLocaleString('sv-SE', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    hour: '2-digit',
    minute: '2-digit',
  });

const statusFärg = {
  Bokad: 'bg-happy-light text-happy-dark',
  Genomförd: 'bg-blue-50 text-blue-700',
  Avbokad: 'bg-gray-100 text-gray-600',
  Uteblev: 'bg-amber-50 text-amber-700',
};

export default function MinaBokningar() {
  const { user } = useAuth();
  const qc = useQueryClient();
  const klientId = user?.klientId;

  const { data: bokningar = [], isLoading } = useQuery({
    queryKey: ['bokningar', klientId],
    queryFn: () => bokningApi.förKlient(klientId),
    enabled: !!klientId,
  });

  const avboka = useMutation({
    mutationFn: (id) => bokningApi.ändraStatus(id, 'Avbokad'),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['bokningar', klientId] }),
  });

  const onAvboka = (bokning) => {
    if (!window.confirm('Vill du avboka detta möte? Tidsluckan blir genast bokningsbar för andra.')) return;
    avboka.mutate(bokning.id);
  };

  if (!klientId)
    return <p className="text-happy-ink/60">Din inloggning saknar koppling till en klientprofil.</p>;

  return (
    <div>
      <h1 className="text-2xl font-semibold text-happy-ink mb-6">Mina bokningar</h1>

      {isLoading && <p className="text-happy-ink/60">Laddar…</p>}

      {!isLoading && bokningar.length === 0 && (
        <p className="text-happy-ink/60">Du har inga bokningar just nu.</p>
      )}

      <ul className="space-y-3">
        {bokningar.map((b) => (
          <li
            key={b.id}
            className="bg-white border border-happy-light rounded-xl p-4 flex items-center justify-between"
          >
            <div>
              <div className="text-happy-ink font-medium">
                {b.tidslucka ? formatTid(b.tidslucka.start) : 'Tid saknas'}
              </div>
              <div className="text-sm text-happy-ink/60">
                {b.terapiTyp}
                {b.anledningTillBesok && (
                  <span className="ml-2 italic">"{b.anledningTillBesok}"</span>
                )}
              </div>
            </div>
            <div className="flex items-center gap-3">
              <span
                className={`text-xs px-2 py-1 rounded-full ${statusFärg[b.status] ?? 'bg-gray-100'}`}
              >
                {b.status}
              </span>
              {b.status === 'Bokad' && (
                <button
                  onClick={() => onAvboka(b)}
                  disabled={avboka.isPending}
                  className="text-sm px-3 py-1.5 rounded-md border border-happy text-happy hover:bg-happy-light disabled:opacity-50"
                >
                  Avboka
                </button>
              )}
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}
