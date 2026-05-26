import { Link } from 'react-router-dom';

export default function NoAccess() {
  return (
    <div className="max-w-md mx-auto mt-16 text-center">
      <h1 className="text-2xl font-semibold text-happy-ink">Saknar behörighet</h1>
      <p className="mt-2 text-happy-ink/60">
        Du är inloggad men har inte rätt roll för att se den här sidan.
      </p>
      <Link to="/" className="inline-block mt-6 text-happy hover:underline">
        Tillbaka till startsidan
      </Link>
    </div>
  );
}
