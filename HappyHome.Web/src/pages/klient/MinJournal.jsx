export default function MinJournal() {
  return (
    <div className="max-w-2xl">
      <h1 className="text-2xl font-semibold text-happy-ink">Min journal</h1>
      <p className="text-happy-ink/60 mt-2">
        Här kommer du som klient se signerade journalanteckningar från
        dina konsultationer.
      </p>
      <div className="mt-6 p-5 rounded-xl border border-dashed border-happy-light text-sm text-happy-ink/60 bg-white">
        Journalmodulen byggs ut i steg 2C. Just nu finns endast Konsultation
        i datalagret — anteckningar, signering och loggning saknas fortfarande.
      </div>
    </div>
  );
}
