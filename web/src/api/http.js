import axios from "axios";

export const http = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "/api",
  timeout: 10_000,
});

function toCamel(str) {
  return str.replace(/_([a-z0-9])/g, (_, c) => c.toUpperCase());
}

// Backend returns snake_case JSON; components use camelCase throughout.
function camelizeKeys(value) {
  if (Array.isArray(value)) return value.map(camelizeKeys);
  if (value !== null && typeof value === "object" && !(value instanceof Date)) {
    return Object.fromEntries(
      Object.entries(value).map(([key, val]) => [toCamel(key), camelizeKeys(val)])
    );
  }
  return value;
}

http.interceptors.response.use(
  (response) => camelizeKeys(response.data),
  (error) => {
    const message = error.response?.data?.message ?? error.message ?? "Request failed";
    return Promise.reject(new Error(message));
  }
);
