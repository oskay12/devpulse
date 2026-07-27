<script setup>
import { ref, watchEffect } from "vue";
import PanelCard from "@/components/common/PanelCard.vue";
import StatusPill from "@/components/common/StatusPill.vue";
import MetricStat from "@/components/common/MetricStat.vue";
import { useRepositoriesStore } from "@/stores/repositories.store";

const props = defineProps({ id: { type: [String, Number], required: true } });

const repositoriesStore = useRepositoriesStore();
const tabs = ["Commits", "Pull Requests", "Contributors", "Health"];
const activeTab = ref("Commits");

watchEffect(() => {
  const id = props.id;
  repositoriesStore.fetchById(id);
  repositoriesStore.fetchMetrics(id);
  repositoriesStore.fetchCommits(id);
  repositoriesStore.fetchPullRequests(id);
  repositoriesStore.fetchContributors(id);
  repositoriesStore.fetchHealthScores(id);
});

function prTone(state) {
  if (state === "Open") return "info";
  if (state === "Merged") return "ok";
  return "neutral";
}

function formatTime(iso) {
  if (!iso) return "—";
  return new Date(iso).toLocaleDateString("tr-TR");
}
</script>

<template>
  <div class="repo-detail-page" v-if="repositoriesStore.current">
    <div class="repo-detail-page__header">
      <div>
        <h1 class="page-title mono">{{ repositoriesStore.current.fullName }}</h1>
        <span class="repo-detail-page__provider mono">{{ repositoriesStore.current.provider }}</span>
      </div>
      <StatusPill
        :label="repositoriesStore.current.isActive ? 'monitoring active' : 'inactive'"
        :tone="repositoriesStore.current.isActive ? 'ok' : 'neutral'"
      />
    </div>

    <div class="repo-detail-page__stats">
      <MetricStat label="Health Score" :value="`${repositoriesStore.metrics?.codeHealthScore ?? 0}/100`" />
      <MetricStat label="Total Pull Requests" :value="repositoriesStore.metrics?.totalPullRequests ?? 0" />
      <MetricStat label="Active Contributors" :value="repositoriesStore.metrics?.activeContributors ?? 0" />
    </div>

    <nav class="tab-bar">
      <button
        v-for="tab in tabs"
        :key="tab"
        class="tab-bar__item"
        :class="{ 'tab-bar__item--active': activeTab === tab }"
        @click="activeTab = tab"
      >
        {{ tab }}
      </button>
    </nav>

    <PanelCard>
      <template v-if="activeTab === 'Commits'">
        <p v-if="!repositoriesStore.commits.length" class="state-msg">No commits ingested yet.</p>
        <table v-else class="data-table">
          <thead>
            <tr><th>SHA</th><th>Message</th><th>Author</th><th>+/-</th></tr>
          </thead>
          <tbody>
            <tr v-for="c in repositoriesStore.commits" :key="c.id">
              <td class="mono">{{ c.sha }}</td>
              <td>{{ c.message }}</td>
              <td class="mono">{{ c.authorName }}</td>
              <td class="mono">
                <span class="diff diff--add">+{{ c.additions }}</span>
                <span class="diff diff--del">-{{ c.deletions }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </template>

      <template v-else-if="activeTab === 'Pull Requests'">
        <p v-if="!repositoriesStore.pullRequests.length" class="state-msg">No pull requests ingested yet.</p>
        <table v-else class="data-table">
          <thead>
            <tr><th>#</th><th>Title</th><th>Author</th><th>State</th></tr>
          </thead>
          <tbody>
            <tr v-for="pr in repositoriesStore.pullRequests" :key="pr.id">
              <td class="mono">{{ pr.prNumber }}</td>
              <td>{{ pr.title }}</td>
              <td class="mono">{{ pr.authorName }}</td>
              <td><StatusPill :label="pr.state" :tone="prTone(pr.state)" /></td>
            </tr>
          </tbody>
        </table>
      </template>

      <template v-else-if="activeTab === 'Contributors'">
        <p v-if="!repositoriesStore.contributors.length" class="state-msg">No contributor data yet.</p>
        <table v-else class="data-table">
          <thead>
            <tr><th>Developer</th><th>Commits</th><th>Additions</th><th>Deletions</th></tr>
          </thead>
          <tbody>
            <tr v-for="c in repositoriesStore.contributors" :key="c.userId">
              <td>
                <RouterLink class="link" :to="`/developers/${c.userId}`">{{ c.username }}</RouterLink>
              </td>
              <td class="mono">{{ c.commitCount }}</td>
              <td class="mono diff--add">+{{ c.additions }}</td>
              <td class="mono diff--del">-{{ c.deletions }}</td>
            </tr>
          </tbody>
        </table>
      </template>

      <template v-else>
        <p v-if="!repositoriesStore.healthScores.length" class="state-msg">No health score history yet.</p>
        <table v-else class="data-table">
          <thead>
            <tr><th>Recorded</th><th>Score</th></tr>
          </thead>
          <tbody>
            <tr v-for="h in repositoriesStore.healthScores" :key="h.recordedAt">
              <td class="mono">{{ formatTime(h.recordedAt) }}</td>
              <td class="mono">{{ h.score }}</td>
            </tr>
          </tbody>
        </table>
      </template>
    </PanelCard>
  </div>
  <p v-else class="state-msg">Loading repository…</p>
</template>

<style scoped>
.repo-detail-page__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: var(--space-4);
}
.page-title {
  font-size: var(--text-lg);
  margin: 0;
}
.repo-detail-page__provider {
  font-size: var(--text-xs);
  color: var(--text-muted);
}

.repo-detail-page__stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--space-4);
  margin-bottom: var(--space-5);
}

.tab-bar {
  display: flex;
  gap: var(--space-1);
  border-bottom: 1px solid var(--border-subtle);
  margin-bottom: var(--space-4);
}
.tab-bar__item {
  background: none;
  border: none;
  padding: var(--space-2) var(--space-3);
  color: var(--text-muted);
  font-size: var(--text-sm);
  border-bottom: 2px solid transparent;
  transform: translateY(1px);
}
.tab-bar__item:hover {
  color: var(--text-primary);
}
.tab-bar__item--active {
  color: var(--text-primary);
  border-bottom-color: var(--accent);
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
.diff {
  margin-right: var(--space-2);
}
.diff--add {
  color: var(--accent);
}
.diff--del {
  color: var(--status-danger);
}
.state-msg {
  color: var(--text-muted);
  font-size: var(--text-sm);
  margin: 0;
}
</style>
