import { defineStore } from "pinia";
import { usersApi } from "@/api";

export const useUsersStore = defineStore("users", {
  state: () => ({
    items: [],
    profile: null,
    metrics: null,
    loading: false,
    error: null,
  }),
  actions: {
    async fetchList(params) {
      this.loading = true;
      this.error = null;
      try {
        const res = await usersApi.list(params);
        this.items = res.items ?? res;
      } catch (err) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },
    async fetchProfile(id) {
      this.loading = true;
      this.error = null;
      try {
        this.profile = await usersApi.getProfile(id);
      } catch (err) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },
    async fetchMetrics(id, params) {
      this.metrics = await usersApi.getMetrics(id, params);
    },
  },
});
