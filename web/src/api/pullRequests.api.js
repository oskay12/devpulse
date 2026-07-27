import { http } from "./http";

export const pullRequestsApi = {
  getById: (id) => http.get(`/pull-requests/${id}`),
  getReviews: (id) => http.get(`/pull-requests/${id}/reviews`),
  submitReview: (id, payload) => http.post(`/pull-requests/${id}/reviews`, payload),
  getComments: (id) => http.get(`/pull-requests/${id}/comments`),
  postComment: (id, payload) => http.post(`/pull-requests/${id}/comments`, payload),
};
