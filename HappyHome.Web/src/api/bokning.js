import { api } from './client';

export const bokningApi = {
  förKlient: (klientId) =>
    api.get(`/Bokning/klient/${klientId}`).then((r) => r.data),
  förTerapeut: (terapeutId) =>
    api.get(`/Bokning/terapeut/${terapeutId}`).then((r) => r.data),
  en: (id) => api.get(`/Bokning/${id}`).then((r) => r.data),
  skapa: ({ klientId, tidsluckaId, terapiTyp, anledningTillBesok }) =>
    api
      .post('/Bokning', {
        klientId,
        tidsluckaId,
        terapiTyp,
        anledningTillBesok,
      })
      .then((r) => r.data),
  ändraStatus: (id, status) =>
    api.put(`/Bokning/${id}/status`, { status }),
};
