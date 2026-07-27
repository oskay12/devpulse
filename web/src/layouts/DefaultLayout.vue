<script setup>
import AppSidebar from "./AppSidebar.vue";
import AppTopbar from "./AppTopbar.vue";
</script>

<template>
  <div class="default-layout">
    <AppSidebar />
    <div class="default-layout__main">
      <AppTopbar />
      <main class="default-layout__content">
        <RouterView />
      </main>
    </div>
  </div>
</template>

<style scoped>
.default-layout {
  display: flex;
  height: 100vh;
}
.default-layout__main {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}
.default-layout__content {
  flex: 1;
  overflow-y: auto;
  padding: var(--space-6);
}
</style>
