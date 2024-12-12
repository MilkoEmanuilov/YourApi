// composables/useApi.ts
import axios from 'axios'

export const useApi = () => {
    const { getToken, refreshToken } = useKeycloak()
    const config = useRuntimeConfig()

    const api = axios.create({
        baseURL: config.public.WEBAPI_URL,
        timeout: 5000
    })

    // composables/useApi.ts
    api.interceptors.request.use(async (config) => {
        const token = getToken()
        if (token) {
            config.headers.Authorization = `Bearer ${token}`
            // Debug log
            console.log('Sending request with headers:', {
                Authorization: config.headers.Authorization,
                ContentType: config.headers['Content-Type'],
                AllHeaders: config.headers
            })
        }
        return config
    })

    api.interceptors.response.use(
        (response) => response,
        async (error) => {
            if (error.response?.status === 401) {
                try {
                    await refreshToken()
                    // Retry the original request with new token
                    const token = getToken()
                    error.config.headers.Authorization = `Bearer ${token}`
                    return axios(error.config)
                } catch (refreshError) {
                    return Promise.reject(refreshError)
                }
            }
            return Promise.reject(error)
        }
    )

    return api
}