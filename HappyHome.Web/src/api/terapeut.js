import { api } from './client';

export const terapeutApi = {
  alla: () => api.get('/Terapeut').then((r) => r.data),
  en: (id) => api.get(`/Terapeut/${id}`).then((r) => r.data),
  skapa: (data) => api.post('/Terapeut', data).then((r) => r.data),
  inaktivera: (id) => api.delete(`/Terapeut/${id}`),
};
