import { defineStore } from "pinia";
import { searchApi } from "@/api";

const SCOPES = {
  commits: searchApi.searchCommits,
  "pull-requests": searchApi.searchPullRequests,
  reviews: searchApi.searchReviews,
};

export const useSearchStore = defineStore("search", {
  state: () => ({
    scope: "commits",
    query: "",
    results: [],
    totalHits: 0,
    loading: false,
    error: null,
  }),
  actions: {
    async search(query, scope = this.scope) {
      this.query = query;
      this.scope = scope;
      this.loading = true;
      this.error = null;
      try {
        const res = await SCOPES[scope]({ q: query });
        this.results = res.results ?? res.items ?? res;
        this.totalHits = res.totalHits ?? this.results.length;
      } catch (err) {
        this.error = err.message;
        this.results = [];
        this.totalHits = 0;
      } finally {
        this.loading = false;
      }
    },
  },
});
