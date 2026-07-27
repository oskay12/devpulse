<script setup>
defineProps({
  provider: { type: String, required: true },
});
</script>

<template>
  <span class="provider-badge mono" :class="`provider-badge--${provider.toLowerCase()}`">
    <span class="provider-badge__dot" />
    {{ provider }}
  </span>
</template>

<style scoped>
.provider-badge {
  display: inline-flex;
  align-items: center;
  gap: var(--space-2);
  font-size: var(--text-xs);
  color: var(--text-secondary);
}
.provider-badge__dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: var(--text-muted);
}
.provider-badge--github .provider-badge__dot {
  background: var(--provider-github);
}
.provider-badge--gitlab .provider-badge__dot {
  background: var(--provider-gitlab);
}
</style>
