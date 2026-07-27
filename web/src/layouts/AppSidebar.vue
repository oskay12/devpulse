<script setup>
const navGroups = [
  {
    label: "Overview",
    items: [{ to: "/", label: "Dashboard", icon: "grid" }],
  },
  {
    label: "Domain",
    items: [
      { to: "/repositories", label: "Repositories", icon: "repo" },
      { to: "/developers", label: "Developers", icon: "user" },
      { to: "/search", label: "Search", icon: "search" },
    ],
  },
];

// Reflects the services this SPA talks to — kept static here;
// wired to real health checks once /api/health exists.
const services = [
  { name: "api", tone: "ok" },
  { name: "worker", tone: "ok" },
  { name: "rabbitmq", tone: "ok" },
  { name: "opensearch", tone: "warning" },
];
</script>

<template>
  <aside class="app-sidebar">
    <div class="app-sidebar__brand">
      <span class="app-sidebar__brand-mark">&gt;_</span>
      <span class="app-sidebar__brand-name">DevPulse</span>
    </div>

    <nav class="app-sidebar__nav">
      <div v-for="group in navGroups" :key="group.label" class="app-sidebar__group">
        <span class="app-sidebar__group-label">{{ group.label }}</span>
        <RouterLink
          v-for="item in group.items"
          :key="item.to"
          :to="item.to"
          class="app-sidebar__link"
          active-class="app-sidebar__link--active"
          :exact-active-class="item.to === '/' ? 'app-sidebar__link--active' : ''"
        >
          {{ item.label }}
        </RouterLink>
      </div>
    </nav>

    <div class="app-sidebar__services">
      <span class="app-sidebar__group-label">Services</span>
      <div v-for="svc in services" :key="svc.name" class="app-sidebar__service">
        <span class="status-dot" :class="`status-dot--${svc.tone}`" />
        <span class="mono">{{ svc.name }}</span>
      </div>
    </div>
  </aside>
</template>

<style scoped>
.app-sidebar {
  width: var(--sidebar-width);
  flex-shrink: 0;
  height: 100%;
  background: var(--surface-1);
  border-right: 1px solid var(--border-subtle);
  display: flex;
  flex-direction: column;
}

.app-sidebar__brand {
  height: var(--topbar-height);
  display: flex;
  align-items: center;
  gap: var(--space-2);
  padding: 0 var(--space-4);
  border-bottom: 1px solid var(--border-subtle);
}
.app-sidebar__brand-mark {
  font-family: var(--font-mono);
  color: var(--accent);
  font-weight: 700;
}
.app-sidebar__brand-name {
  font-weight: 600;
  letter-spacing: 0.02em;
}

.app-sidebar__nav {
  flex: 1;
  padding: var(--space-4) var(--space-2);
  overflow-y: auto;
}
.app-sidebar__group {
  display: flex;
  flex-direction: column;
  margin-bottom: var(--space-5);
}
.app-sidebar__group-label {
  font-size: var(--text-xs);
  color: var(--text-disabled);
  text-transform: uppercase;
  letter-spacing: 0.08em;
  padding: 0 var(--space-2);
  margin-bottom: var(--space-2);
}
.app-sidebar__link {
  padding: var(--space-2) var(--space-2);
  border-radius: var(--radius-sm);
  color: var(--text-secondary);
  font-size: var(--text-sm);
  transition: background var(--transition-fast), color var(--transition-fast);
}
.app-sidebar__link:hover {
  background: var(--surface-3);
  color: var(--text-primary);
}
.app-sidebar__link--active {
  background: var(--accent-bg);
  color: var(--accent);
  font-weight: 600;
}

.app-sidebar__services {
  padding: var(--space-4) var(--space-2);
  border-top: 1px solid var(--border-subtle);
}
.app-sidebar__service {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  padding: var(--space-1) var(--space-2);
  font-size: var(--text-xs);
  color: var(--text-secondary);
}
</style>
