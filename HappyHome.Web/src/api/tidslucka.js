import { api } from './client';

export const tidsluckaApi = {
  lediga: (terapeutId, datumISO) =>
    api
      .get(`/Tidslucka/lediga`, { params: { terapeutId, datum: datumISO } })
      .then((r) => r.data),
  skapa: ({ terapeutId, start, slut }) =>
    api
      .post('/Tidslucka', { terapeutId, start, slut })
      .then((r) => r.data),
  taBort: (id) => api.delete(`/Tidslucka/${id}`),
};
