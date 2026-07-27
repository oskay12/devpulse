import { http } from "./http";

export const searchApi = {
  searchCommits: (params) => http.get("/search/commits", { params }),
  searchPullRequests: (params) => http.get("/search/pull-requests", { params }),
  searchReviews: (params) => http.get("/search/reviews", { params }),
  reindex: (payload) => http.post("/search/reindex", payload),
};
