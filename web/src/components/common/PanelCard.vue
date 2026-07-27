<script setup>
defineProps({
  title: { type: String, default: "" },
});
</script>

<template>
  <section class="panel-card">
    <header v-if="title || $slots.actions" class="panel-card__header">
      <h3 v-if="title" class="panel-card__title">{{ title }}</h3>
      <div v-if="$slots.actions" class="panel-card__actions">
        <slot name="actions" />
      </div>
    </header>
    <div class="panel-card__body">
      <slot />
    </div>
  </section>
</template>

<style scoped>
.panel-card {
  background: var(--surface-2);
  border: 1px solid var(--border-subtle);
  border-radius: var(--radius-md);
}
.panel-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-3) var(--space-4);
  border-bottom: 1px solid var(--border-subtle);
}
.panel-card__title {
  margin: 0;
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--text-primary);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
.panel-card__body {
  padding: var(--space-4);
}
</style>
