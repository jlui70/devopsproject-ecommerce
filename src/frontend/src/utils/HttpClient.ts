import axios from 'axios';

function createInstance(baseUrl: string) {
  const instance = axios.create({ baseURL: baseUrl });

  instance.interceptors.request.use(
    (request) => {
      request.headers = request.headers || {};
      request.headers['Content-type'] = 'application/json';
      const token = localStorage.getItem('jwtToken');
      if (token) {
        request.headers['Authorization'] = `Bearer ${token}`;
      }
      return request;
    },
    (error) => Promise.reject(error)
  );

  instance.interceptors.response.use(
    (response) => response,
    (error) => Promise.reject(error)
  );

  return instance;
}

export const http = createInstance(import.meta.env.VITE_APP_BASE_URL ?? '');
