<!-- pages/admin/dashboard.vue -->
<script setup>
definePageMeta({
    layout: 'auth'
})
</script>

<template>
    <div>
        Protected page you can now go to test page <nuxt-link to="/test">here</nuxt-link> and try calling the backend
        api.
    </div>
</template>