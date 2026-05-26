import { useState } from 'react';

// Helt fristående AI-formulär. AI-svaret är stub:at i demon — vi vill att
// studenterna ska kunna diskutera etik och flöde utan att riktig data lämnar
// deras dator. När en riktig modell kopplas på sker det enbart server-side
// (via API:t), aldrig direkt från React.
const stubbadeFörslag = (terapiTyp) => [
  {
    diagnos: terapiTyp === 'Kris' ? 'Akut stressreaktion' : 'Generaliserat ångestsyndrom',
    sannolikhet: 64,
    motivering: 'Anamnesen pekar mot långvariga oroskänslor och autonom aktivering.',
    rekommendation: 'Strukturerad uppföljning, ev. KBT-paket.',
  },
  {
    diagnos: 'Lindrig depression',
    sannolikhet: 21,
    motivering: 'Nedstämdhet och energilöshet finns, men kärnkriterier ej fullt uppfyllda.',
    rekommendation: 'Återbesök inom 4 veckor för förnyad bedömning.',
  },
  {
    diagnos: 'Anpassningsstörning',
    sannolikhet: 15,
    motivering: 'Tydlig utlösande livshändelse — symptomen kan vara tidsbegränsade.',
    rekommendation: 'Stödsamtal samt psykoedukation.',
  },
];

export default function AiDiagnos() {
  const [ålder, setÅlder] = useState('');
  const [terapiTyp, setTerapiTyp] = useState('Individuell');
  const [symptom, setSymptom] = useState('');
  const [bakgrund, setBakgrund] = useState('');
  const [förslag, setFörslag] = useState(null);
  const [sparade, setSparade] = useState([]);

  const generera = (e) => {
    e.preventDefault();
    setFörslag(stubbadeFörslag(terapiTyp));
  };

  const spara = (f) => {
    setSparade((s) => [...s, f]);
  };

  return (
    <div className="space-y-6 max-w-4xl">
      <header>
        <h1 className="text-2xl font-semibold text-happy-ink">AI-diagnos</h1>
        <p className="text-sm text-happy-ink/60 mt-1">
          AI:n är ett beslutsstöd — du som terapeut beslutar alltid. All input loggas
          för spårbarhet enligt klinikens rutiner.
        </p>
      </header>

      <form onSubmit={generera} className="grid md:grid-cols-2 gap-4 bg-white border border-happy-light rounded-xl p-5">
        <label className="block">
          <span className="text-sm font-medium text-happy-ink">Klientens ålder</span>
          <input
            type="number"
            min={0}
            max={120}
            value={ålder}
            onChange={(e) => setÅlder(e.target.value)}
            className="mt-1 w-full rounded-md border border-happy-light px-3 py-2"
          />
        </label>
        <label className="block">
          <span className="text-sm font-medium text-happy-ink">Terapityp</span>
          <select
            value={terapiTyp}
            onChange={(e) => setTerapiTyp(e.target.value)}
            className="mt-1 w-full rounded-md border border-happy-light px-3 py-2"
          >
            <option>Individuell</option>
            <option>Par</option>
            <option>Familj</option>
            <option>Grupp</option>
            <option>Kris</option>
            <option>Beroende</option>
          </select>
        </label>
        <label className="md:col-span-2 block">
          <span className="text-sm font-medium text-happy-ink">Symptom</span>
          <textarea
            rows={3}
            required
            value={symptom}
            onChange={(e) => setSymptom(e.target.value)}
            className="mt-1 w-full rounded-md border border-happy-light px-3 py-2"
          />
        </label>
        <label className="md:col-span-2 block">
          <span className="text-sm font-medium text-happy-ink">Bakgrund</span>
          <textarea
            rows={3}
            value={bakgrund}
            onChange={(e) => setBakgrund(e.target.value)}
            className="mt-1 w-full rounded-md border border-happy-light px-3 py-2"
          />
        </label>
        <div className="md:col-span-2">
          <button
            type="submit"
            className="bg-happy text-white px-4 py-2 rounded-md hover:bg-happy-dark"
          >
            Generera förslag
          </button>
        </div>
      </form>

      {förslag && (
        <section className="space-y-3">
          <h2 className="text-lg font-medium text-happy-ink">Förslag från AI</h2>
          {förslag.map((f) => (
            <article
              key={f.diagnos}
              className="bg-white border border-happy-light rounded-xl p-4 flex items-start justify-between gap-4"
            >
              <div>
                <div className="flex items-baseline gap-3">
                  <span className="font-medium text-happy-ink">{f.diagnos}</span>
                  <span className="text-sm text-happy">{f.sannolikhet}% sannolikhet</span>
                </div>
                <p className="text-sm text-happy-ink/70 mt-1">{f.motivering}</p>
                <p className="text-sm text-happy-ink/70 italic mt-1">{f.rekommendation}</p>
              </div>
              <div className="flex flex-col gap-2 shrink-0">
                <button
                  onClick={() => spara(f)}
                  className="text-sm px-3 py-1.5 rounded-md bg-happy text-white hover:bg-happy-dark"
                >
                  Spara som beslutsstöd
                </button>
                <button
                  onClick={() => setFörslag((p) => p.filter((x) => x !== f))}
                  className="text-sm px-3 py-1.5 rounded-md border border-happy-light text-happy-ink/70 hover:bg-happy-light"
                >
                  Avfärda
                </button>
              </div>
            </article>
          ))}
        </section>
      )}

      {sparade.length > 0 && (
        <section>
          <h3 className="text-sm font-medium text-happy-ink mb-2">Sparat som beslutsstöd</h3>
          <ul className="text-sm text-happy-ink/70 list-disc pl-5 space-y-1">
            {sparade.map((s, i) => (
              <li key={i}>{s.diagnos} — {s.sannolikhet}%</li>
            ))}
          </ul>
        </section>
      )}
    </div>
  );
}
