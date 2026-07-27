<script setup>
import { useRouter } from "vue-router";
import { ref } from "vue";

const router = useRouter();
const query = ref("");

function submitSearch() {
  if (!query.value.trim()) return;
  router.push({ path: "/search", query: { q: query.value } });
}
</script>

<template>
  <header class="app-topbar">
    <div class="app-topbar__search">
      <span class="app-topbar__search-icon mono">/</span>
      <input
        v-model="query"
        type="text"
        placeholder="Search commits, pull requests, reviews..."
        @keyup.enter="submitSearch"
      />
    </div>

    <div class="app-topbar__meta">
      <span class="mono app-topbar__env">env: production</span>
    </div>
  </header>
</template>

<style scoped>
.app-topbar {
  height: var(--topbar-height);
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 var(--space-4);
  background: var(--surface-1);
  border-bottom: 1px solid var(--border-subtle);
}

.app-topbar__search {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  width: 380px;
  max-width: 50%;
  padding: 0 var(--space-3);
  height: 30px;
  background: var(--surface-0);
  border: 1px solid var(--border-default);
  border-radius: var(--radius-sm);
}
.app-topbar__search-icon {
  color: var(--text-disabled);
}
.app-topbar__search input {
  flex: 1;
  border: none;
  outline: none;
  background: transparent;
  color: var(--text-primary);
  font-size: var(--text-sm);
}
.app-topbar__search input::placeholder {
  color: var(--text-disabled);
}

.app-topbar__env {
  font-size: var(--text-xs);
  color: var(--text-muted);
}
</style>
