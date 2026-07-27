<script setup>
import { watchEffect } from "vue";
import PanelCard from "@/components/common/PanelCard.vue";
import MetricStat from "@/components/common/MetricStat.vue";
import { useUsersStore } from "@/stores/users.store";

const props = defineProps({ id: { type: [String, Number], default: null } });

const usersStore = useUsersStore();

watchEffect(() => {
  if (props.id) usersStore.fetchProfile(props.id);
});
</script>

<template>
  <div class="developer-page" v-if="usersStore.profile">
    <div class="developer-page__header">
      <div class="developer-page__avatar mono">
        {{ usersStore.profile.username.slice(0, 2).toUpperCase() }}
      </div>
      <div>
        <h1 class="page-title mono">{{ usersStore.profile.username }}</h1>
        <span class="developer-page__role">{{ usersStore.profile.email }}</span>
      </div>
    </div>

    <div class="developer-page__stats">
      <MetricStat label="Total Commits" :value="usersStore.profile.metrics.totalCommits" />
      <MetricStat label="Pull Requests" :value="usersStore.profile.metrics.totalPullRequests" />
      <MetricStat label="Code Reviews" :value="usersStore.profile.metrics.codeReviews" />
      <MetricStat
        label="Productivity Score"
        :value="usersStore.profile.metrics.productivityScore.toFixed(1)"
      />
    </div>

    <PanelCard title="Repositories">
      <p v-if="!usersStore.profile.repositories.length" class="state-msg">
        No repository associations yet.
      </p>
      <ul v-else class="repo-mini-list">
        <li v-for="r in usersStore.profile.repositories" :key="r.id" class="repo-mini-list__item">
          <RouterLink class="link" :to="`/repositories/${r.id}`">{{ r.fullName }}</RouterLink>
          <span class="mono repo-mini-list__count">{{ r.commitCount }} commits</span>
        </li>
      </ul>
    </PanelCard>
  </div>
  <p v-else-if="usersStore.loading" class="state-msg">Loading developer profile…</p>
  <p v-else-if="usersStore.error" class="state-msg state-msg--error">{{ usersStore.error }}</p>
</template>

<style scoped>
.developer-page__header {
  display: flex;
  align-items: center;
  gap: var(--space-4);
  margin-bottom: var(--space-5);
}
.developer-page__avatar {
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--surface-2);
  border: 1px solid var(--border-default);
  border-radius: var(--radius-md);
  font-weight: 700;
  color: var(--accent);
}
.page-title {
  font-size: var(--text-lg);
  margin: 0;
}
.developer-page__role {
  font-size: var(--text-sm);
  color: var(--text-muted);
}

.developer-page__stats {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--space-4);
  margin-bottom: var(--space-5);
}

.repo-mini-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
}
.repo-mini-list__item {
  display: flex;
  justify-content: space-between;
  font-size: var(--text-sm);
}
.repo-mini-list__count {
  color: var(--text-muted);
  font-size: var(--text-xs);
}
.link {
  color: var(--status-info);
}
.link:hover {
  text-decoration: underline;
}
.state-msg {
  color: var(--text-muted);
  font-size: var(--text-sm);
  margin: 0;
}
.state-msg--error {
  color: var(--status-danger);
}
</style>
