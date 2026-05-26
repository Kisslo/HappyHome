import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { terapeutApi } from '../../api/terapeut';
import { tidsluckaApi } from '../../api/tidslucka';

const iDag = () => new Date().toISOString().slice(0, 10);

export default function Tidsluckor() {
  const qc = useQueryClient();
  const { data: terapeuter = [] } = useQuery({
    queryKey: ['terapeuter'],
    queryFn: terapeutApi.alla,
  });

  const [terapeutId, setTerapeutId] = useState('');
  const [datum, setDatum] = useState(iDag());
  const [nyTid, setNyTid] = useState('09:00');

  const { data: luckor = [] } = useQuery({
    queryKey: ['lediga', terapeutId, datum],
    queryFn: () => tidsluckaApi.lediga(Number(terapeutId), datum),
    enabled: !!terapeutId,
  });

  const skapa = useMutation({
    mutationFn: () => {
      const start = new Date(`${datum}T${nyTid}:00`);
      const slut = new Date(start.getTime() + 60 * 60 * 1000);
      return tidsluckaApi.skapa({
        terapeutId: Number(terapeutId),
        start: start.toISOString(),
        slut: slut.toISOString(),
      });
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['lediga'] }),
  });

  const taBort = useMutation({
    mutationFn: tidsluckaApi.taBort,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['lediga'] }),
  });

  return (
    <div className="space-y-6 max-w-3xl">
      <h1 className="text-2xl font-semibold text-happy-ink">Tidsluckor</h1>

      <div className="bg-white border border-happy-light rounded-xl p-5 space-y-3">
        <div className="grid md:grid-cols-3 gap-3">
          <label className="block">
            <span className="text-sm font-medium text-happy-ink">Terapeut</span>
            <select
              value={terapeutId}
              onChange={(e) => setTerapeutId(e.target.value)}
              className="mt-1 w-full rounded-md border border-happy-light px-3 py-2 text-sm"
            >
              <option value="">Välj…</option>
              {terapeuter.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.förnamn} {t.efternamn}
                </option>
              ))}
            </select>
          </label>
          <label className="block">
            <span className="text-sm font-medium text-happy-ink">Datum</span>
            <input
              type="date"
              value={datum}
              onChange={(e) => setDatum(e.target.value)}
              className="mt-1 w-full rounded-md border border-happy-light px-3 py-2 text-sm"
            />
          </label>
          <label className="block">
            <span className="text-sm font-medium text-happy-ink">Starttid (timme)</span>
            <input
              type="time"
              value={nyTid}
              onChange={(e) => setNyTid(e.target.value)}
              className="mt-1 w-full rounded-md border border-happy-light px-3 py-2 text-sm"
            />
          </label>
        </div>
        <button
          disabled={!terapeutId || skapa.isPending}
          onClick={() => skapa.mutate()}
          className="bg-happy text-white px-4 py-2 rounded-md hover:bg-happy-dark disabled:opacity-50"
        >
          {skapa.isPending ? 'Skapar…' : 'Lägg till lucka (1 timme)'}
        </button>
      </div>

      {terapeutId && (
        <div>
          <h2 className="text-sm font-medium text-happy-ink mb-2">
            Lediga luckor {datum}
          </h2>
          {luckor.length === 0 ? (
            <p className="text-sm text-happy-ink/60">Inga lediga luckor den dagen.</p>
          ) : (
            <ul className="grid grid-cols-2 md:grid-cols-4 gap-2">
              {luckor.map((l) => (
                <li
                  key={l.id}
                  className="bg-white border border-happy-light rounded-md px-3 py-2 text-sm flex items-center justify-between"
                >
                  <span>
                    {new Date(l.start).toLocaleTimeString('sv-SE', {
                      hour: '2-digit',
                      minute: '2-digit',
                    })}
                  </span>
                  <button
                    onClick={() => {
                      if (window.confirm('Ta bort tidsluckan?')) taBort.mutate(l.id);
                    }}
                    className="text-xs text-red-600 hover:underline"
                  >
                    Ta bort
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  );
}
