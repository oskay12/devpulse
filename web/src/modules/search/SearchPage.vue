<script setup>
import { ref, watchEffect } from "vue";
import { useRoute } from "vue-router";
import DOMPurify from "dompurify";
import PanelCard from "@/components/common/PanelCard.vue";
import { useSearchStore } from "@/stores/search.store";

const route = useRoute();
const searchStore = useSearchStore();
const query = ref(route.query.q ?? "");

const scopes = [
  { key: "commits", label: "Commits" },
  { key: "pull-requests", label: "Pull Requests" },
  { key: "reviews", label: "Reviews" },
];

function runSearch(scope = searchStore.scope) {
  if (!query.value.trim()) return;
  searchStore.search(query.value, scope);
}

watchEffect(() => {
  if (route.query.q) runSearch();
});

function resultKey(r, index) {
  return r.sha ?? r.prNumber ?? index;
}

function highlight(snippet, fallback) {
  return DOMPurify.sanitize(snippet ?? fallback, { ALLOWED_TAGS: ["em"] });
}
</script>

<template>
  <div class="search-page">
    <h1 class="page-title">Search</h1>

    <div class="search-page__bar">
      <input
        v-model="query"
        type="text"
        class="search-page__input mono"
        placeholder="e.g. fix, feat, retry..."
        @keyup.enter="runSearch()"
      />
      <div class="search-page__scopes">
        <button
          v-for="s in scopes"
          :key="s.key"
          class="scope-btn"
          :class="{ 'scope-btn--active': searchStore.scope === s.key }"
          @click="runSearch(s.key)"
        >
          {{ s.label }}
        </button>
      </div>
    </div>

    <PanelCard :title="`Results — ${searchStore.scope} (${searchStore.totalHits})`">
      <p v-if="searchStore.loading" class="search-page__empty">Searching…</p>
      <p v-else-if="searchStore.error" class="search-page__empty search-page__empty--error">
        {{ searchStore.error }}
      </p>
      <ul v-else-if="searchStore.results.length" class="result-list">
        <li v-for="(r, i) in searchStore.results" :key="resultKey(r, i)" class="result-list__item">
          <template v-if="searchStore.scope === 'commits'">
            <span class="mono result-list__sha">{{ r.sha }}</span>
            <span v-html="highlight(r.highlightSnippets?.[0], r.message)" />
            <span class="result-list__meta mono">{{ r.repositoryName }} · {{ r.authorName }}</span>
          </template>
          <template v-else-if="searchStore.scope === 'pull-requests'">
            <span class="mono">#{{ r.prNumber }}</span>
            <span v-html="highlight(r.highlightSnippets?.[0], r.title)" />
            <span class="result-list__meta mono">{{ r.repositoryName }} · {{ r.authorName }}</span>
          </template>
          <template v-else>
            <span v-html="highlight(r.highlightSnippets?.[0], r.body)" />
            <span class="result-list__meta mono">{{ r.pullRequestTitle }} · {{ r.authorName }}</span>
          </template>
        </li>
      </ul>
      <p v-else class="search-page__empty">
        {{ query ? "No results for this query/scope." : "Type a query and press Enter to search." }}
      </p>
    </PanelCard>
  </div>
</template>

<style scoped>
.page-title {
  font-size: var(--text-lg);
  margin: 0 0 var(--space-4);
}

.search-page__bar {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
  margin-bottom: var(--space-5);
}
.search-page__input {
  height: 40px;
  padding: 0 var(--space-3);
  background: var(--surface-2);
  border: 1px solid var(--border-default);
  border-radius: var(--radius-md);
  color: var(--text-primary);
  font-size: var(--text-md);
}
.search-page__input:focus {
  outline: none;
  border-color: var(--accent-dim);
}

.search-page__scopes {
  display: flex;
  gap: var(--space-2);
}
.scope-btn {
  padding: var(--space-1) var(--space-3);
  background: var(--surface-2);
  border: 1px solid var(--border-default);
  border-radius: var(--radius-sm);
  color: var(--text-secondary);
  font-size: var(--text-xs);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
.scope-btn--active {
  border-color: var(--accent-dim);
  color: var(--accent);
}

.result-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}
.result-list__item {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
  font-size: var(--text-sm);
  padding-bottom: var(--space-3);
  border-bottom: 1px solid var(--border-subtle);
}
.result-list__item:last-child {
  border-bottom: none;
  padding-bottom: 0;
}
.result-list__sha {
  color: var(--text-muted);
}
.result-list__meta {
  color: var(--text-muted);
  font-size: var(--text-xs);
}

.search-page__empty {
  color: var(--text-muted);
  font-size: var(--text-sm);
}
.search-page__empty--error {
  color: var(--status-danger);
}
</style>
