import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { bokningApi } from '../../api/bokning';
import { terapeutApi } from '../../api/terapeut';
import { tidsluckaApi } from '../../api/tidslucka';
import { useAuth } from '../../context/AuthContext';

const terapiTyper = [
  { typ: 'Individuell', beskrivning: 'Enskilda samtal kring egna utmaningar.' },
  { typ: 'Par', beskrivning: 'För partners som vill stärka relationen.' },
  { typ: 'Familj', beskrivning: 'Samtal där hela eller delar av familjen deltar.' },
  { typ: 'Grupp', beskrivning: 'Strukturerade möten i mindre grupp.' },
  { typ: 'Kris', beskrivning: 'Akut samtalsstöd vid kris eller chock.' },
  { typ: 'Beroende', beskrivning: 'Specialiserade samtal kring beroende.' },
];

const iDag = () => new Date().toISOString().slice(0, 10);

export default function Boka() {
  const [steg, setSteg] = useState(1);
  const [valdTyp, setValdTyp] = useState(null);
  const [valdTerapeut, setValdTerapeut] = useState(null);
  const [valtDatum, setValtDatum] = useState(iDag());
  const [valdLucka, setValdLucka] = useState(null);
  const [anledning, setAnledning] = useState('');

  const { user } = useAuth();
  const navigate = useNavigate();
  const qc = useQueryClient();

  const { data: terapeuter = [] } = useQuery({
    queryKey: ['terapeuter'],
    queryFn: terapeutApi.alla,
    enabled: steg >= 2,
  });

  const matchande = terapeuter.filter((t) =>
    t.specialiseringar?.includes(valdTyp),
  );

  const { data: luckor = [], isLoading: laddarLuckor } = useQuery({
    queryKey: ['lediga', valdTerapeut?.id, valtDatum],
    queryFn: () => tidsluckaApi.lediga(valdTerapeut.id, valtDatum),
    enabled: steg >= 3 && !!valdTerapeut && !!valtDatum,
  });

  const skapa = useMutation({
    mutationFn: () =>
      bokningApi.skapa({
        klientId: user.klientId,
        tidsluckaId: valdLucka.id,
        terapiTyp: valdTyp,
        anledningTillBesok: anledning,
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['bokningar', user.klientId] });
      navigate('/mina-bokningar');
    },
  });

  const stepBox = 'rounded-xl border bg-white p-5';
  const active = (s) => s === steg ? 'border-happy ring-2 ring-happy/30' : 'border-happy-light';

  return (
    <div className="space-y-6 max-w-3xl">
      <h1 className="text-2xl font-semibold text-happy-ink">Boka tid</h1>

      <div className={`${stepBox} ${active(1)}`}>
        <h2 className="font-medium text-happy-ink mb-3">1. Välj terapityp</h2>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          {terapiTyper.map((t) => (
            <button
              key={t.typ}
              onClick={() => {
                setValdTyp(t.typ);
                setValdTerapeut(null);
                setValdLucka(null);
                setSteg(2);
              }}
              className={`text-left p-3 rounded-lg border transition ${
                valdTyp === t.typ
                  ? 'border-happy bg-happy-light'
                  : 'border-happy-light hover:bg-happy-light/50'
              }`}
            >
              <div className="font-medium text-happy-ink">{t.typ}</div>
              <div className="text-xs text-happy-ink/60 mt-1">{t.beskrivning}</div>
            </button>
          ))}
        </div>
      </div>

      {steg >= 2 && (
        <div className={`${stepBox} ${active(2)}`}>
          <h2 className="font-medium text-happy-ink mb-3">2. Välj terapeut</h2>
          {matchande.length === 0 && (
            <p className="text-sm text-happy-ink/60">
              Ingen terapeut med specialisering på {valdTyp} just nu.
            </p>
          )}
          <ul className="space-y-2">
            {matchande.map((t) => (
              <li key={t.id}>
                <button
                  onClick={() => {
                    setValdTerapeut(t);
                    setValdLucka(null);
                    setSteg(3);
                  }}
                  className={`w-full text-left p-3 rounded-lg border ${
                    valdTerapeut?.id === t.id
                      ? 'border-happy bg-happy-light'
                      : 'border-happy-light hover:bg-happy-light/50'
                  }`}
                >
                  <div className="font-medium text-happy-ink">
                    {t.förnamn} {t.efternamn}
                  </div>
                  <div className="text-xs text-happy-ink/60">
                    {t.roll} — {t.specialiseringar?.join(', ')}
                  </div>
                </button>
              </li>
            ))}
          </ul>
        </div>
      )}

      {steg >= 3 && valdTerapeut && (
        <div className={`${stepBox} ${active(3)}`}>
          <h2 className="font-medium text-happy-ink mb-3">3. Välj tid</h2>
          <input
            type="date"
            value={valtDatum}
            min={iDag()}
            onChange={(e) => {
              setValtDatum(e.target.value);
              setValdLucka(null);
            }}
            className="mb-3 rounded-md border border-happy-light px-3 py-2"
          />
          {laddarLuckor ? (
            <p className="text-sm text-happy-ink/60">Laddar…</p>
          ) : luckor.length === 0 ? (
            <p className="text-sm text-happy-ink/60">
              Inga lediga tider hos {valdTerapeut.förnamn} den dagen.
            </p>
          ) : (
            <div className="grid grid-cols-3 md:grid-cols-4 gap-2">
              {luckor.map((l) => (
                <button
                  key={l.id}
                  onClick={() => {
                    setValdLucka(l);
                    setSteg(4);
                  }}
                  className={`text-sm py-2 rounded-md border ${
                    valdLucka?.id === l.id
                      ? 'border-happy bg-happy-light'
                      : 'border-happy-light hover:bg-happy-light/50'
                  }`}
                >
                  {new Date(l.start).toLocaleTimeString('sv-SE', {
                    hour: '2-digit',
                    minute: '2-digit',
                  })}
                </button>
              ))}
            </div>
          )}
        </div>
      )}

      {steg >= 4 && valdLucka && (
        <div className={`${stepBox} ${active(4)}`}>
          <h2 className="font-medium text-happy-ink mb-3">4. Bekräfta</h2>
          <div className="text-sm text-happy-ink/70 mb-3">
            {valdTyp} med {valdTerapeut.förnamn} {valdTerapeut.efternamn} —{' '}
            {new Date(valdLucka.start).toLocaleString('sv-SE')}
          </div>
          <label className="block text-sm font-medium text-happy-ink mb-1">
            Anledning till besöket (terapeuten ser detta innan mötet)
          </label>
          <textarea
            rows={3}
            value={anledning}
            onChange={(e) => setAnledning(e.target.value)}
            className="w-full rounded-md border border-happy-light px-3 py-2"
            placeholder="Beskriv kort vad du vill prata om."
          />
          {skapa.isError && (
            <div className="mt-3 text-sm text-red-700 bg-red-50 border border-red-200 rounded-md px-3 py-2">
              {skapa.error?.response?.data ?? 'Bokningen kunde inte skapas.'}
            </div>
          )}
          <button
            disabled={skapa.isPending}
            onClick={() => skapa.mutate()}
            className="mt-4 bg-happy text-white px-4 py-2 rounded-md hover:bg-happy-dark disabled:opacity-50"
          >
            {skapa.isPending ? 'Bokar…' : 'Bekräfta bokning'}
          </button>
        </div>
      )}
    </div>
  );
}
