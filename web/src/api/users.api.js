import { http } from "./http";

export const usersApi = {
  list: (params) => http.get("/users", { params }),
  getById: (id) => http.get(`/users/${id}`),
  create: (payload) => http.post("/users", payload),
  update: (id, payload) => http.put(`/users/${id}`, payload),
  deactivate: (id) => http.delete(`/users/${id}`),
  getProfile: (id) => http.get(`/users/${id}/profile`),
  getMetrics: (id, params) => http.get(`/users/${id}/metrics`, { params }),
};
