<script setup>
defineProps({
  label: { type: String, required: true },
  tone: {
    type: String,
    default: "neutral",
    validator: (v) => ["ok", "info", "warning", "danger", "neutral"].includes(v),
  },
});
</script>

<template>
  <span class="status-pill" :class="`status-pill--${tone}`">
    <span class="status-dot" :class="`status-dot--${tone}`" />
    {{ label }}
  </span>
</template>

<style scoped>
.status-pill {
  display: inline-flex;
  align-items: center;
  gap: var(--space-2);
  padding: 2px var(--space-2);
  font-size: var(--text-xs);
  font-family: var(--font-mono);
  border: 1px solid var(--border-default);
  border-radius: var(--radius-sm);
  color: var(--text-secondary);
  white-space: nowrap;
}
.status-pill--ok { border-color: rgba(63, 185, 80, 0.3); color: var(--accent); }
.status-pill--info { border-color: rgba(88, 166, 255, 0.3); color: var(--status-info); }
.status-pill--warning { border-color: rgba(210, 153, 34, 0.3); color: var(--status-warning); }
.status-pill--danger { border-color: rgba(248, 81, 73, 0.3); color: var(--status-danger); }
</style>
