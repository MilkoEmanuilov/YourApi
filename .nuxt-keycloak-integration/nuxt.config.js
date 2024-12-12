// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  runtimeConfig: {
    public: {
      KEYCLOAK_URL: process.env.NUXT_PUBLIC_KEYCLOAK_URL,
      KEYCLOAK_REALM: process.env.NUXT_PUBLIC_KEYCLOAK_REALM,
      KEYCLOAK_CLIENT_ID: process.env.NUXT_PUBLIC_KEYCLOAK_CLIENT_ID,
      WEBAPI_URL: process.env.NUXT_PUBLIC_WEBAPI_URL
    }
  },
})
