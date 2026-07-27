import { createRouter, createWebHistory } from "vue-router";
import DefaultLayout from "@/layouts/DefaultLayout.vue";

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/",
      component: DefaultLayout,
      children: [
        {
          path: "",
          name: "dashboard",
          component: () => import("@/modules/dashboard/DashboardPage.vue"),
        },
        {
          path: "repositories",
          name: "repositories",
          component: () => import("@/modules/repositories/RepositoryListPage.vue"),
        },
        {
          path: "repositories/:id",
          name: "repository-detail",
          component: () => import("@/modules/repositories/RepositoryDetailPage.vue"),
          props: true,
        },
        {
          path: "developers/:id?",
          name: "developer-profile",
          component: () => import("@/modules/users/DeveloperProfilePage.vue"),
          props: true,
        },
        {
          path: "search",
          name: "search",
          component: () => import("@/modules/search/SearchPage.vue"),
        },
      ],
    },
  ],
});

export default router;
