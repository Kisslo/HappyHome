import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { klientApi } from '../../api/klient';

// AI-anropen är stub:ade i demon — vi har ingen Claude API-nyckel hookad in
// ännu. Det gör att studenterna kan resonera kring etiken utan att riktig
// patientdata skickas iväg. Stubbarna returnerar trovärdig men fast text.
const aiDiagnosStub = (symptom) => [
  {
    diagnos: 'Generaliserat ångestsyndrom (GAD)',
    sannolikhet: 67,
    motivering: 'Långvarig oro, sömnstörning och kroppsliga symptom återkommer i beskrivningen.',
    rekommendation: 'KBT med ångestfokus, 8–12 sessioner.',
  },
  {
    diagnos: 'Anpassningsstörning',
    sannolikhet: 22,
    motivering: 'Symptomdebut sammanfaller med livsförändring; reaktionen kan vara tidsbegränsad.',
    rekommendation: 'Stödsamtal och uppföljning om 4 veckor.',
  },
  {
    diagnos: 'Depressiv episod (lindrig)',
    sannolikhet: 11,
    motivering: 'Nedstämdhet nämns men kärnsymptomen är begränsade enligt anamnesen.',
    rekommendation: 'Återbesök för fördjupad bedömning.',
  },
];

const aiJournalUtkast = (symptom, bakgrund) =>
  `Klienten beskriver ${symptom?.slice(0, 80) || '...'}. Bakgrunden präglas av ${
    bakgrund?.slice(0, 80) || '...'
  }. Bedömning: behov av strukturerad uppföljning; psykoedukation om sömn och andningsövningar inleds vid nästa möte. Plan: 6 sessioner KBT med fokus på exponering och beteendeaktivering.`;

export default function Konsultation() {
  const { bokningId: klientId } = useParams();
  const { data: klient } = useQuery({
    queryKey: ['klient', klientId],
    queryFn: () => klientApi.en(klientId),
    enabled: !!klientId,
  });

  const [symptom, setSymptom] = useState('');
  const [bakgrund, setBakgrund] = useState('');
  const [anteckningsTyp, setAnteckningsTyp] = useState('Bedömning');
  const [diagnoser, setDiagnoser] = useState(null);
  const [utkast, setUtkast] = useState('');
  const [signerad, setSignerad] = useState(false);

  return (
    <div className="space-y-5 max-w-4xl">
      <h1 className="text-2xl font-semibold text-happy-ink">Konsultation</h1>

      {klient && (
        <div className="bg-happy-light/60 border border-happy-light rounded-xl p-4">
          <div className="font-medium text-happy-ink">
            {klient.förnamn} {klient.efternamn}
          </div>
          <div className="text-sm text-happy-ink/70">
            Personnr {klient.personnummer} · Tel {klient.telefon}
          </div>
        </div>
      )}

      <div className="grid md:grid-cols-2 gap-4">
        <label className="block">
          <span className="text-sm font-medium text-happy-ink">Symptom</span>
          <textarea
            rows={5}
            value={symptom}
            onChange={(e) => setSymptom(e.target.value)}
            className="mt-1 w-full rounded-md border border-happy-light px-3 py-2"
            disabled={signerad}
          />
        </label>
        <label className="block">
          <span className="text-sm font-medium text-happy-ink">Bakgrund</span>
          <textarea
            rows={5}
            value={bakgrund}
            onChange={(e) => setBakgrund(e.target.value)}
            className="mt-1 w-full rounded-md border border-happy-light px-3 py-2"
            disabled={signerad}
          />
        </label>
      </div>

      <label className="block max-w-xs">
        <span className="text-sm font-medium text-happy-ink">Anteckningstyp</span>
        <select
          value={anteckningsTyp}
          onChange={(e) => setAnteckningsTyp(e.target.value)}
          className="mt-1 w-full rounded-md border border-happy-light px-3 py-2"
          disabled={signerad}
        >
          <option>Bedömning</option>
          <option>Behandlingsplan</option>
          <option>Förlopp</option>
          <option>Sammanfattning</option>
          <option>Övrigt</option>
        </select>
      </label>

      <div className="flex flex-wrap gap-3">
        <button
          onClick={() => setDiagnoser(aiDiagnosStub(symptom))}
          className="px-4 py-2 rounded-md border border-happy text-happy hover:bg-happy-light"
          disabled={signerad || !symptom}
        >
          Generera AI-diagnosförslag
        </button>
        <button
          onClick={() => setUtkast(aiJournalUtkast(symptom, bakgrund))}
          className="px-4 py-2 rounded-md border border-happy text-happy hover:bg-happy-light"
          disabled={signerad || !symptom}
        >
          Generera journalutkast med AI
        </button>
        <button
          onClick={() => setSignerad(true)}
          className="px-4 py-2 rounded-md bg-happy text-white hover:bg-happy-dark disabled:opacity-50 ml-auto"
          disabled={signerad || !utkast}
        >
          {signerad ? 'Anteckning signerad' : 'Signera anteckning'}
        </button>
      </div>

      {diagnoser && (
        <div className="rounded-xl border border-happy-light bg-white p-4">
          <div className="text-sm text-happy-ink/60 mb-2">
            AI-genererade förslag — terapeuten beslutar alltid. (Demo: svaret är stub:at, ingen data skickas externt.)
          </div>
          <ul className="space-y-3">
            {diagnoser.map((d) => (
              <li key={d.diagnos} className="border-l-4 border-happy pl-3">
                <div className="flex items-baseline justify-between">
                  <div className="font-medium text-happy-ink">{d.diagnos}</div>
                  <div className="text-sm text-happy">{d.sannolikhet}%</div>
                </div>
                <div className="text-sm text-happy-ink/70">{d.motivering}</div>
                <div className="text-sm text-happy-ink/70 italic">{d.rekommendation}</div>
              </li>
            ))}
          </ul>
        </div>
      )}

      {utkast && (
        <div>
          <label className="text-sm font-medium text-happy-ink">
            Journalutkast (redigerbart — AI föreslår, terapeuten väljer)
          </label>
          <textarea
            rows={6}
            value={utkast}
            onChange={(e) => setUtkast(e.target.value)}
            disabled={signerad}
            className="mt-1 w-full rounded-md border border-happy-light px-3 py-2"
          />
        </div>
      )}

      {signerad && (
        <div className="text-sm text-happy bg-happy-light/60 border border-happy-light rounded-md p-3">
          Anteckningen är låst och kan inte längre ändras. För komplettering, skapa en ny anteckning.
        </div>
      )}
    </div>
  );
}
