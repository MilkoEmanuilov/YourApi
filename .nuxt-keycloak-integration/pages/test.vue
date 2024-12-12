<template>
    <div>
        <div v-if="loading">Loading...</div>
        <div v-else>
            <button @click="fetchData">Fetch Data</button>
            <button @click="logout">Logout</button>
        </div>
    </div>
</template>

<script setup>
definePageMeta({
    layout: 'auth'
})

import { useApi } from '~/composables/useApi'

const {
    loading,
    logout
} = useKeycloak()

const api = useApi()

const fetchData = async () => {
    try {
        const response = await api.get('/posts')
        posts.value = response.data
    } catch (error) {
        console.error('Error fetching posts:', error)
    }
}
</script>