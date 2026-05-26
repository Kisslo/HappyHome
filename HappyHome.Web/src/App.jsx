import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import Layout from './components/Layout';
import { ProtectedRoute } from './components/ProtectedRoute';
import { AuthProvider } from './context/AuthContext';
import Hem from './pages/Hem';
import NoAccess from './pages/NoAccess';
import Login from './pages/auth/Login';
import MinaBokningar from './pages/klient/MinaBokningar';
import Boka from './pages/klient/Boka';
import MinJournal from './pages/klient/MinJournal';
import MinaPatienter from './pages/terapeut/MinaPatienter';
import Konsultation from './pages/terapeut/Konsultation';
import AiDiagnos from './pages/terapeut/AiDiagnos';
import Dashboard from './pages/admin/Dashboard';
import Terapeuter from './pages/admin/Terapeuter';
import Tidsluckor from './pages/admin/Tidsluckor';

const KlientRoute = ({ children }) => (
  <ProtectedRoute roller={['Klient']}>{children}</ProtectedRoute>
);
const TerapeutRoute = ({ children }) => (
  <ProtectedRoute roller={['Terapeut']}>{children}</ProtectedRoute>
);
const AdminRoute = ({ children }) => (
  <ProtectedRoute roller={['Admin']}>{children}</ProtectedRoute>
);

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<Login />} />

          <Route element={<Layout />}>
            <Route index element={<Hem />} />
            <Route path="no-access" element={<NoAccess />} />

            {/* Klient */}
            <Route path="mina-bokningar" element={<KlientRoute><MinaBokningar /></KlientRoute>} />
            <Route path="boka" element={<KlientRoute><Boka /></KlientRoute>} />
            <Route path="min-journal" element={<KlientRoute><MinJournal /></KlientRoute>} />

            {/* Terapeut */}
            <Route path="mina-patienter" element={<TerapeutRoute><MinaPatienter /></TerapeutRoute>} />
            <Route path="konsultation/:bokningId" element={<TerapeutRoute><Konsultation /></TerapeutRoute>} />
            <Route path="ai-diagnos" element={<TerapeutRoute><AiDiagnos /></TerapeutRoute>} />

            {/* Admin */}
            <Route path="admin" element={<AdminRoute><Dashboard /></AdminRoute>} />
            <Route path="admin/terapeuter" element={<AdminRoute><Terapeuter /></AdminRoute>} />
            <Route path="admin/tidsluckor" element={<AdminRoute><Tidsluckor /></AdminRoute>} />
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
