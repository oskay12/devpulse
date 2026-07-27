import { http } from "./http";

export const commitsApi = {
  getById: (id) => http.get(`/commits/${id}`),
  getFiles: (id) => http.get(`/commits/${id}/files`),
};
