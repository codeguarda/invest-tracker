import axios from "axios";
import { getToken, clearToken } from "./auth";

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || "http://localhost:5187",
});

// Anexa Bearer token em cada requisição
api.interceptors.request.use((cfg) => {
  const token = getToken();
  if (token) {
    cfg.headers = cfg.headers || {};
    cfg.headers.authorization = `Bearer ${token}`;
  }
  return cfg;
});

// Se expirar, desloga
api.interceptors.response.use(
  (r) => r,
  (err) => {
    if (err?.response?.status === 401) {
      clearToken();
      window.location.href = "/";
    }
    return Promise.reject(err);
  }
);
