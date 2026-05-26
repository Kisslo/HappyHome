import { api } from './client';

export const klientApi = {
  alla: () => api.get('/Klient').then((r) => r.data),
  en: (id) => api.get(`/Klient/${id}`).then((r) => r.data),
};
