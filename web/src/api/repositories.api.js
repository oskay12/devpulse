import { http } from "./http";

export const repositoriesApi = {
  list: (params) => http.get("/repositories", { params }),
  getById: (id) => http.get(`/repositories/${id}`),
  create: (payload) => http.post("/repositories", payload),
  update: (id, payload) => http.put(`/repositories/${id}`, payload),
  deactivate: (id) => http.delete(`/repositories/${id}`),
  getCommits: (id, params) => http.get(`/repositories/${id}/commits`, { params }),
  getPullRequests: (id, params) => http.get(`/repositories/${id}/pull-requests`, { params }),
  getContributors: (id, params) => http.get(`/repositories/${id}/contributors`, { params }),
  getMetrics: (id, params) => http.get(`/repositories/${id}/metrics`, { params }),
  getHealthScores: (id, params) => http.get(`/repositories/${id}/health-scores`, { params }),

  listTokens: (id) => http.get(`/repositories/${id}/tokens`),
  createToken: (id, payload) => http.post(`/repositories/${id}/tokens`, payload),
  revokeToken: (id, tokenId) => http.delete(`/repositories/${id}/tokens/${tokenId}`),
};
