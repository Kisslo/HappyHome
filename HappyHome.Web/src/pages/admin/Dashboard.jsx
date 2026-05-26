import { useQueries } from '@tanstack/react-query';
import { klientApi } from '../../api/klient';
import { terapeutApi } from '../../api/terapeut';
import { bokningApi } from '../../api/bokning';

const Kort = ({ titel, värde, hint }) => (
  <div className="bg-white border border-happy-light rounded-2xl p-5">
    <div className="text-xs uppercase tracking-wider text-happy-ink/50">{titel}</div>
    <div className="mt-2 text-3xl font-semibold text-happy">{värde}</div>
    {hint && <div className="text-xs text-happy-ink/50 mt-1">{hint}</div>}
  </div>
);

export default function Dashboard() {
  const [klienterQ, terapeuterQ] = useQueries({
    queries: [
      { queryKey: ['klienter'], queryFn: klientApi.alla },
      { queryKey: ['terapeuter'], queryFn: terapeutApi.alla },
    ],
  });

  const klienter = klienterQ.data ?? [];
  const terapeuter = terapeuterQ.data ?? [];

  // Bokningar idag: hämta per klient. I ett riktigt system skulle vi haft
  // en endpoint /api/Bokning/idag — men för demon räcker det att aggregera.
  // Detta är ett N+1-anrop och inte hållbart i produktion.
  const bokningarQs = useQueries({
    queries: klienter.map((k) => ({
      queryKey: ['bokningar', k.id],
      queryFn: () => bokningApi.förKlient(k.id),
      enabled: klienter.length > 0,
    })),
  });

  const allaBokningar = bokningarQs.flatMap((q) => q.data ?? []);
  const idagISO = new Date().toISOString().slice(0, 10);
  const bokningarIdag = allaBokningar.filter((b) =>
    b.tidslucka?.start?.startsWith(idagISO) && b.status === 'Bokad',
  ).length;

  const noShowsSenaste30Dagar = allaBokningar.filter((b) => b.status === 'Uteblev').length;

  return (
    <div>
      <h1 className="text-2xl font-semibold text-happy-ink mb-6">Dashboard</h1>

      <div className="grid md:grid-cols-4 gap-4">
        <Kort titel="Aktiva klienter" värde={klienter.length} hint="totalt i registret" />
        <Kort titel="Bokningar idag" värde={bokningarIdag} hint="status Bokad, dagens datum" />
        <Kort titel="No-shows (30 dagar)" värde={noShowsSenaste30Dagar} hint="status Uteblev" />
        <Kort titel="Aktiva terapeuter" värde={terapeuter.length} hint="terapeuter med Aktiv = true" />
      </div>

      <section className="mt-8 bg-white border border-happy-light rounded-2xl p-5">
        <h2 className="text-sm font-medium text-happy-ink mb-3">Bokningar senaste 7 dagarna</h2>
        <SimpelLinje data={byggSerie(allaBokningar, 7)} />
      </section>
    </div>
  );
}

function byggSerie(bokningar, dagar) {
  const idag = new Date();
  const serie = [];
  for (let i = dagar - 1; i >= 0; i--) {
    const d = new Date(idag);
    d.setDate(idag.getDate() - i);
    const iso = d.toISOString().slice(0, 10);
    const antal = bokningar.filter((b) => b.tidslucka?.start?.startsWith(iso)).length;
    serie.push({ datum: iso.slice(5), antal });
  }
  return serie;
}

function SimpelLinje({ data }) {
  if (!data?.length) return null;
  const maxAntal = Math.max(1, ...data.map((d) => d.antal));
  const W = 600, H = 120, padding = 24;
  const stegX = (W - 2 * padding) / Math.max(1, data.length - 1);
  const punkter = data.map((d, i) => ({
    x: padding + i * stegX,
    y: H - padding - (d.antal / maxAntal) * (H - 2 * padding),
    ...d,
  }));
  const path = punkter
    .map((p, i) => (i === 0 ? `M${p.x},${p.y}` : `L${p.x},${p.y}`))
    .join(' ');

  return (
    <svg viewBox={`0 0 ${W} ${H}`} className="w-full h-32">
      <path d={path} fill="none" stroke="#4CAF8A" strokeWidth="2" />
      {punkter.map((p) => (
        <g key={p.datum}>
          <circle cx={p.x} cy={p.y} r={3} fill="#4CAF8A" />
          <text x={p.x} y={H - 4} textAnchor="middle" fontSize="9" fill="#1f2d28aa">
            {p.datum}
          </text>
        </g>
      ))}
    </svg>
  );
}
