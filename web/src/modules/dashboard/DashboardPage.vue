<script setup>
import { onMounted, computed } from "vue";
import PanelCard from "@/components/common/PanelCard.vue";
import MetricStat from "@/components/common/MetricStat.vue";
import StatusPill from "@/components/common/StatusPill.vue";
import ProviderBadge from "@/components/common/ProviderBadge.vue";
import { useRepositoriesStore } from "@/stores/repositories.store";

const repositoriesStore = useRepositoriesStore();

onMounted(() => {
  repositoriesStore.fetchList();
});

const totals = computed(() => {
  const repos = repositoriesStore.items;
  return {
    repositories: repos.length,
    active: repos.filter((r) => r.isActive).length,
  };
});

function formatTime(iso) {
  if (!iso) return "—";
  return new Date(iso).toLocaleString("tr-TR", { dateStyle: "medium", timeStyle: "short" });
}
</script>

<template>
  <div class="dashboard-page">
    <div class="dashboard-page__stats">
      <MetricStat label="Monitored Repositories" :value="totals.repositories" />
      <MetricStat label="Active Repositories" :value="totals.active" delta-tone="ok" />
      <MetricStat label="Ingest Queue" value="0 pending" delta-tone="neutral" />
    </div>

    <PanelCard title="Repositories">
      <p v-if="repositoriesStore.loading" class="state-msg">Loading repositories…</p>
      <p v-else-if="repositoriesStore.error" class="state-msg state-msg--error">
        {{ repositoriesStore.error }}
      </p>
      <p v-else-if="!repositoriesStore.items.length" class="state-msg">No repositories yet.</p>
      <table v-else class="data-table">
        <thead>
          <tr>
            <th>Repository</th>
            <th>Provider</th>
            <th>Status</th>
            <th>Stars</th>
            <th>Last Synced</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="repo in repositoriesStore.items" :key="repo.id">
            <td>
              <RouterLink class="link" :to="`/repositories/${repo.id}`">{{ repo.fullName }}</RouterLink>
            </td>
            <td>
              <ProviderBadge :provider="repo.provider" />
            </td>
            <td>
              <StatusPill :label="repo.isActive ? 'active' : 'inactive'" :tone="repo.isActive ? 'ok' : 'neutral'" />
            </td>
            <td class="mono">{{ repo.starCount }}</td>
            <td class="mono">{{ formatTime(repo.lastSyncedAt) }}</td>
          </tr>
        </tbody>
      </table>
    </PanelCard>
  </div>
</template>

<style scoped>
.dashboard-page__stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--space-4);
  margin-bottom: var(--space-6);
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: var(--text-sm);
}
.data-table th {
  text-align: left;
  color: var(--text-muted);
  font-size: var(--text-xs);
  text-transform: uppercase;
  letter-spacing: 0.04em;
  padding: var(--space-2);
  border-bottom: 1px solid var(--border-subtle);
}
.data-table td {
  padding: var(--space-2);
  border-bottom: 1px solid var(--border-subtle);
  color: var(--text-secondary);
}
.data-table tr:last-child td {
  border-bottom: none;
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
