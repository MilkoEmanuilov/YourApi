// composables/useKeycloak.ts
import Keycloak from 'keycloak-js'
import { ref } from 'vue'

// Move these to module scope so they're shared across all uses of the composable
const isInitialized = ref(false)
const isAuthenticated = ref(false)
const user = ref(null)
const error = ref(null)
const loading = ref(true)
const token = ref(null)
let keycloakInstance = null
let isInitializing = false

export const useKeycloak = () => {
    const config = useRuntimeConfig()

    const initKeycloak = () => {
        if (!keycloakInstance) {
            keycloakInstance = new Keycloak({
                url: config.public.KEYCLOAK_URL,
                realm: config.public.KEYCLOAK_REALM,
                clientId: config.public.KEYCLOAK_CLIENT_ID
            })

            // Add event listeners for token state
            keycloakInstance.onAuthSuccess = () => {
                isAuthenticated.value = true
                token.value = keycloakInstance?.token
                updateUserInfo()
            }

            keycloakInstance.onAuthError = () => {
                isAuthenticated.value = false
                token.value = null
                user.value = null
            }

            keycloakInstance.onTokenExpired = () => {
                refreshToken()
            }
        }
        return keycloakInstance
    }

    const updateUserInfo = () => {
        if (keycloakInstance?.tokenParsed) {
            user.value = {
                name: keycloakInstance.tokenParsed?.preferred_username,
                email: keycloakInstance.tokenParsed?.email,
                roles: keycloakInstance.realmAccess?.roles || []
            }
        }
    }

    const refreshToken = async () => {
        try {
            const refreshed = await keycloakInstance?.updateToken(70)
            if (refreshed) {
                token.value = keycloakInstance?.token
                isAuthenticated.value = true
                updateUserInfo()
            }
        } catch (err) {
            console.error('Token refresh failed:', err)
            isAuthenticated.value = false
            token.value = null
            user.value = null
        }
    }

    const init = async () => {
        // Return if already initialized or initializing
        if (isInitialized.value || isInitializing) {
            return isInitialized.value
        }

        isInitializing = true

        try {
            loading.value = true
            const kc = initKeycloak()
            const auth = await kc.init({
                onLoad: 'check-sso',
                checkLoginIframe: false,
                enableLogging: true,
                pkceMethod: 'S256'
            })

            isInitialized.value = true
            isAuthenticated.value = auth
            token.value = keycloakInstance?.token

            if (auth) {
                updateUserInfo()
                // Set up token refresh
                setInterval(() => refreshToken(), 60000)
            }
            return auth
        } catch (err) {
            error.value = err
            console.error('Keycloak init error:', err)
            return false
        } finally {
            loading.value = false
            isInitializing = false
        }
    }

    const login = async () => {
        if (!isInitialized.value && !isInitializing) {
            await init()
        }
        return keycloakInstance.login()
    }

    const logout = async () => {
        if (!isInitialized.value && !isInitializing) {
            await init()
        }
        isAuthenticated.value = false
        token.value = null
        user.value = null
        return keycloakInstance.logout()
    }

    const getToken = () => token.value || keycloakInstance?.token
    const hasRole = (role) => keycloakInstance?.hasRealmRole(role)

    // Initialize on client side only once
    if (process.client && !isInitialized.value && !isInitializing) {
        init()
    }

    return {
        keycloak: keycloakInstance,
        isInitialized,
        isAuthenticated,
        user,
        error,
        loading,
        token,
        login,
        logout,
        getToken,
        hasRole,
        refreshToken
    }
}