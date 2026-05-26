import axios from 'axios';

// En enda axios-instans för hela appen. Här bestäms basURL och här hänger
// vi också på interceptors. Om backend flyttas är detta enda stället vi
// behöver röra.
export const api = axios.create({
  baseURL: 'http://localhost:5189/api',
  headers: { 'Content-Type': 'application/json' },
});

// Request-interceptor: lägg automatiskt på JWT-token om vi har en. Då slipper
// varje komponent komma ihåg att skicka med den.
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('happyhome.token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Response-interceptor: om vi får 401 (token utgången/ogiltig) — rensa
// inloggningen och tvinga om-login. Annars riskerar React att stanna i
// ett trasigt halvinloggat tillstånd.
api.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      localStorage.removeItem('happyhome.token');
      localStorage.removeItem('happyhome.user');
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
    }
    return Promise.reject(err);
  },
);
