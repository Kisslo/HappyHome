import { api } from './client';

export const authApi = {
  login: (epost, lösenord) =>
    api.post('/Auth/login', { epost, lösenord }).then((r) => r.data),
  me: () => api.get('/Auth/me').then((r) => r.data),
};
