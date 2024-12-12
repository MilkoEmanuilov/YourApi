<script setup>
const { isAuthenticated, loading, login } = useKeycloak()

onMounted(async () => {
    // Only trigger on client side
    if (process.client) {
        // Add a small delay to ensure Keycloak is properly initialized
        setTimeout(async () => {
            if (!loading.value && !isAuthenticated.value) {
                console.log('Triggering login redirect')
                await login()
            }
        }, 100)
    }
})

// Watch for changes in both loading and authentication state
watch([loading, isAuthenticated], async ([isLoading, isAuth]) => {
    if (process.client && !isLoading && !isAuth) {
        console.log('Auth state changed, triggering login')
        await login()
    }
})
</script>

<template>
    <div>
        <slot v-if="isAuthenticated" />
        <div v-else-if="loading">Loading...</div>
    </div>
</template>