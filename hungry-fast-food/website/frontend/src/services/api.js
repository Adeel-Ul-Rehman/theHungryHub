// E:\hungryHub\hungry-fast-food\website\frontend\src\services\api.js

let API_URL = import.meta.env.VITE_API_URL || '/api';
if (API_URL.startsWith('http') && !API_URL.endsWith('/api') && !API_URL.endsWith('/api/')) {
    API_URL = API_URL.endsWith('/') ? `${API_URL}api` : `${API_URL}/api`;
}

class ApiService {
    constructor() {
        this.baseURL = API_URL;
        this.defaultHeaders = {
            'Content-Type': 'application/json',
        };
    }

    // Get auth token from storage
    getAuthToken() {
        const tokens = JSON.parse(localStorage.getItem('tokens') || '{}');
        return tokens.accessToken || null;
    }

    // Get admin API key
    getAdminKey() {
        return import.meta.env.VITE_ADMIN_API_KEY || null;
    }

    // Build headers with auth
    getHeaders(includeAuth = true, includeAdmin = false) {
        const headers = { ...this.defaultHeaders };

        if (includeAuth) {
            const token = this.getAuthToken();
            if (token) {
                headers['Authorization'] = `Bearer ${token}`;
            }
        }

        if (includeAdmin) {
            const adminKey = this.getAdminKey();
            if (adminKey) {
                headers['x-admin-api-key'] = adminKey;
            }
        }

        return headers;
    }

    // Handle response
    async handleResponse(response) {
        const data = await response.json();

        if (!response.ok) {
            // Handle token expiration
            if (response.status === 401 && data.code === 'TOKEN_EXPIRED') {
                // Try to refresh token
                const refreshed = await this.refreshToken();
                if (refreshed) {
                    // Retry original request
                    return this.handleResponse(response);
                }
            }

            throw new Error(data.message || 'Something went wrong');
        }

        return data;
    }

    // SWR fetcher helper
    async swrFetcher(url, options = {}) {
        const res = await this.get(url, options);
        if (!res.success) {
            throw new Error(res.message || 'Fetch failed');
        }
        return res.data;
    }

    // Refresh token
    async refreshToken() {
        try {
            const tokens = JSON.parse(localStorage.getItem('tokens') || '{}');
            const response = await fetch(`${this.baseURL}/auth/refresh-token`, {
                method: 'POST',
                headers: this.defaultHeaders,
                body: JSON.stringify({ refreshToken: tokens.refreshToken }),
            });

            const data = await response.json();

            if (data.success) {
                localStorage.setItem('tokens', JSON.stringify(data.data));
                return true;
            }

            return false;
        } catch (error) {
            return false;
        }
    }

    // HTTP methods
    async get(endpoint, options = {}) {
        const response = await fetch(`${this.baseURL}${endpoint}`, {
            method: 'GET',
            headers: this.getHeaders(options.includeAuth, options.includeAdmin),
            ...options,
        });
        return this.handleResponse(response);
    }

    async post(endpoint, data, options = {}) {
        const response = await fetch(`${this.baseURL}${endpoint}`, {
            method: 'POST',
            headers: this.getHeaders(options.includeAuth, options.includeAdmin),
            body: JSON.stringify(data),
            ...options,
        });
        return this.handleResponse(response);
    }

    async put(endpoint, data, options = {}) {
        const response = await fetch(`${this.baseURL}${endpoint}`, {
            method: 'PUT',
            headers: this.getHeaders(options.includeAuth, options.includeAdmin),
            body: JSON.stringify(data),
            ...options,
        });
        return this.handleResponse(response);
    }

    async patch(endpoint, data, options = {}) {
        const response = await fetch(`${this.baseURL}${endpoint}`, {
            method: 'PATCH',
            headers: this.getHeaders(options.includeAuth, options.includeAdmin),
            body: JSON.stringify(data),
            ...options,
        });
        return this.handleResponse(response);
    }

    async delete(endpoint, options = {}) {
        const response = await fetch(`${this.baseURL}${endpoint}`, {
            method: 'DELETE',
            headers: this.getHeaders(options.includeAuth, options.includeAdmin),
            ...options,
        });
        return this.handleResponse(response);
    }
}

export const api = new ApiService();