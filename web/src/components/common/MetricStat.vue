<script setup>
defineProps({
  label: { type: String, required: true },
  value: { type: [String, Number], required: true },
  delta: { type: String, default: "" },
  deltaTone: { type: String, default: "neutral" },
});
</script>

<template>
  <div class="metric-stat">
    <span class="metric-stat__label">{{ label }}</span>
    <span class="metric-stat__value mono">{{ value }}</span>
    <span v-if="delta" class="metric-stat__delta" :class="`metric-stat__delta--${deltaTone}`">
      {{ delta }}
    </span>
  </div>
</template>

<style scoped>
.metric-stat {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
  padding: var(--space-3) var(--space-4);
  border: 1px solid var(--border-subtle);
  border-radius: var(--radius-md);
  background: var(--surface-2);
}
.metric-stat__label {
  font-size: var(--text-xs);
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}
.metric-stat__value {
  font-size: var(--text-xl);
  font-weight: 600;
  color: var(--text-primary);
}
.metric-stat__delta {
  font-size: var(--text-xs);
  font-family: var(--font-mono);
}
.metric-stat__delta--ok { color: var(--accent); }
.metric-stat__delta--danger { color: var(--status-danger); }
.metric-stat__delta--neutral { color: var(--text-muted); }
</style>
