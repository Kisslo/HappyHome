import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { terapeutApi } from '../../api/terapeut';

const tomTerapeut = () => ({
  förnamn: '',
  efternamn: '',
  epost: '',
  roll: 'Psykolog',
  specialiseringar: [],
  aktivFromDatum: new Date().toISOString().slice(0, 10),
  aktiv: true,
});

const allaTerapiTyper = ['Individuell', 'Par', 'Familj', 'Grupp', 'Kris', 'Beroende'];

export default function Terapeuter() {
  const qc = useQueryClient();
  const { data: terapeuter = [], isLoading } = useQuery({
    queryKey: ['terapeuter'],
    queryFn: terapeutApi.alla,
  });

  const [öppen, setÖppen] = useState(false);
  const [form, setForm] = useState(tomTerapeut);

  const skapa = useMutation({
    mutationFn: terapeutApi.skapa,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['terapeuter'] });
      setForm(tomTerapeut());
      setÖppen(false);
    },
  });

  const inaktivera = useMutation({
    mutationFn: terapeutApi.inaktivera,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['terapeuter'] }),
  });

  const togglaSpec = (typ) =>
    setForm((f) => ({
      ...f,
      specialiseringar: f.specialiseringar.includes(typ)
        ? f.specialiseringar.filter((x) => x !== typ)
        : [...f.specialiseringar, typ],
    }));

  return (
    <div className="grid lg:grid-cols-[1fr,360px] gap-6">
      <section>
        <div className="flex items-center justify-between mb-4">
          <h1 className="text-2xl font-semibold text-happy-ink">Terapeuter</h1>
          <button
            onClick={() => setÖppen((o) => !o)}
            className="bg-happy text-white px-3 py-1.5 rounded-md hover:bg-happy-dark text-sm"
          >
            {öppen ? 'Stäng' : 'Lägg till'}
          </button>
        </div>

        {isLoading && <p className="text-happy-ink/60">Laddar…</p>}

        <table className="w-full bg-white border border-happy-light rounded-xl overflow-hidden">
          <thead className="bg-happy-light text-happy-ink text-sm">
            <tr>
              <th className="text-left px-3 py-2">Namn</th>
              <th className="text-left px-3 py-2">Roll</th>
              <th className="text-left px-3 py-2">Specialiseringar</th>
              <th className="px-3 py-2"></th>
            </tr>
          </thead>
          <tbody className="text-sm">
            {terapeuter.map((t) => (
              <tr key={t.id} className="border-t border-happy-light">
                <td className="px-3 py-2 text-happy-ink">
                  {t.förnamn} {t.efternamn}
                  <div className="text-xs text-happy-ink/50">{t.epost}</div>
                </td>
                <td className="px-3 py-2 text-happy-ink/80">{t.roll}</td>
                <td className="px-3 py-2 text-happy-ink/70">
                  {t.specialiseringar?.join(', ')}
                </td>
                <td className="px-3 py-2 text-right">
                  <button
                    onClick={() => {
                      if (window.confirm(`Inaktivera ${t.förnamn}?`))
                        inaktivera.mutate(t.id);
                    }}
                    className="text-sm text-red-600 hover:underline"
                  >
                    Inaktivera
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      {öppen && (
        <aside className="bg-white border border-happy-light rounded-xl p-5 h-fit">
          <h2 className="font-medium text-happy-ink mb-3">Ny terapeut</h2>
          <form
            onSubmit={(e) => {
              e.preventDefault();
              skapa.mutate(form);
            }}
            className="space-y-3"
          >
            <input
              required
              placeholder="Förnamn"
              value={form.förnamn}
              onChange={(e) => setForm({ ...form, förnamn: e.target.value })}
              className="w-full rounded-md border border-happy-light px-3 py-2 text-sm"
            />
            <input
              required
              placeholder="Efternamn"
              value={form.efternamn}
              onChange={(e) => setForm({ ...form, efternamn: e.target.value })}
              className="w-full rounded-md border border-happy-light px-3 py-2 text-sm"
            />
            <input
              required
              type="email"
              placeholder="E-post"
              value={form.epost}
              onChange={(e) => setForm({ ...form, epost: e.target.value })}
              className="w-full rounded-md border border-happy-light px-3 py-2 text-sm"
            />
            <select
              value={form.roll}
              onChange={(e) => setForm({ ...form, roll: e.target.value })}
              className="w-full rounded-md border border-happy-light px-3 py-2 text-sm"
            >
              <option>Psykolog</option>
              <option>Psykiatriker</option>
              <option>Familjeterapeut</option>
              <option>Beroendeterapeut</option>
              <option>Krisspecialist</option>
            </select>
            <div>
              <div className="text-xs text-happy-ink/70 mb-1">Specialiseringar</div>
              <div className="flex flex-wrap gap-1.5">
                {allaTerapiTyper.map((typ) => (
                  <button
                    type="button"
                    key={typ}
                    onClick={() => togglaSpec(typ)}
                    className={`text-xs px-2 py-1 rounded-md border ${
                      form.specialiseringar.includes(typ)
                        ? 'bg-happy text-white border-happy'
                        : 'border-happy-light text-happy-ink/70'
                    }`}
                  >
                    {typ}
                  </button>
                ))}
              </div>
            </div>
            {skapa.isError && (
              <div className="text-sm text-red-700 bg-red-50 rounded-md p-2">
                Kunde inte spara terapeuten.
              </div>
            )}
            <button
              type="submit"
              disabled={skapa.isPending}
              className="w-full bg-happy text-white px-3 py-2 rounded-md hover:bg-happy-dark disabled:opacity-50"
            >
              {skapa.isPending ? 'Sparar…' : 'Spara'}
            </button>
          </form>
        </aside>
      )}
    </div>
  );
}
