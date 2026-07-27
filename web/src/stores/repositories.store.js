import { defineStore } from "pinia";
import { repositoriesApi } from "@/api";

export const useRepositoriesStore = defineStore("repositories", {
  state: () => ({
    items: [],
    current: null,
    commits: [],
    pullRequests: [],
    contributors: [],
    healthScores: [],
    metrics: null,
    loading: false,
    error: null,
  }),
  actions: {
    async fetchList(params) {
      this.loading = true;
      this.error = null;
      try {
        const res = await repositoriesApi.list(params);
        this.items = res.items ?? res;
      } catch (err) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },
    async fetchById(id) {
      this.loading = true;
      this.error = null;
      try {
        this.current = await repositoriesApi.getById(id);
      } catch (err) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },
    async fetchCommits(id, params) {
      const res = await repositoriesApi.getCommits(id, params);
      this.commits = res.items ?? res;
    },
    async fetchPullRequests(id, params) {
      const res = await repositoriesApi.getPullRequests(id, params);
      this.pullRequests = res.items ?? res;
    },
    async fetchContributors(id, params) {
      const res = await repositoriesApi.getContributors(id, params);
      this.contributors = res.items ?? res;
    },
    async fetchHealthScores(id, params) {
      const res = await repositoriesApi.getHealthScores(id, params);
      this.healthScores = res.items ?? res;
    },
    async fetchMetrics(id, params) {
      this.metrics = await repositoriesApi.getMetrics(id, params);
    },
  },
});
