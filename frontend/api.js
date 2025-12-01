// API Base URL - Use the same origin as the current page
const API_BASE_URL = `${window.location.origin}/api`;

// Helper function for API calls
async function apiCall(endpoint, options = {}) {
    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            ...options,
            credentials: 'include', // Include cookies for session
            headers: {
                'Content-Type': 'application/json',
                ...options.headers
            }
        });

        if (!response.ok) {
            const error = await response.json().catch(() => ({ message: 'Request failed' }));
            throw new Error(error.message || `HTTP error! status: ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('API Error:', error);
        throw error;
    }
}

// Auth API
const authAPI = {
    register: (username, email, password) => 
        apiCall('/auth/register', {
            method: 'POST',
            body: JSON.stringify({ username, email, password })
        }),
    
    login: (email, password) => 
        apiCall('/auth/login', {
            method: 'POST',
            body: JSON.stringify({ email, password })
        })
};

// Books API
const booksAPI = {
    getAll: () => apiCall('/books'),
    
    getById: (id) => apiCall(`/books/${id}`),
    
    search: (params) => {
        const query = new URLSearchParams(params).toString();
        return apiCall(`/books/search?${query}`);
    },
    
    create: (bookData) => 
        apiCall('/books', {
            method: 'POST',
            body: JSON.stringify(bookData)
        }),
    
    update: (id, bookData) => 
        apiCall(`/books/${id}`, {
            method: 'PUT',
            body: JSON.stringify(bookData)
        }),
    
    delete: (id) => 
        apiCall(`/books/${id}`, {
            method: 'DELETE'
        })
};

// Cart API
const cartAPI = {
    get: () => apiCall('/cart'),
    
    add: (bookId, quantity) => 
        apiCall('/cart/add', {
            method: 'POST',
            body: JSON.stringify({ bookID: bookId, quantity })
        }),
    
    update: (bookId, quantity) => 
        apiCall('/cart/update', {
            method: 'PUT',
            body: JSON.stringify({ bookID: bookId, quantity })
        }),
    
    remove: (bookId) => 
        apiCall(`/cart/remove/${bookId}`, {
            method: 'DELETE'
        })
};

// Orders API
const ordersAPI = {
    checkout: (userId) => 
        apiCall('/orders/checkout', {
            method: 'POST',
            body: JSON.stringify({ userID: userId })
        }),
    
    getByUserId: (userId) => apiCall(`/orders/customer/${userId}`),
    
    getAll: () => apiCall('/orders'),
    
    updateStatus: (orderId, status) => 
        apiCall(`/orders/${orderId}/status`, {
            method: 'PUT',
            body: JSON.stringify({ status })
        }),
    
    cancel: (orderId) => 
        apiCall(`/orders/${orderId}/cancel`, {
            method: 'PUT'
        }),
    
    getPurchasedBooks: (userId) => apiCall(`/orders/customer/${userId}/purchased-books`)
};

// Sell Submissions API
const submissionsAPI = {
    create: (submissionData, userId) => 
        apiCall('/sell-submissions', {
            method: 'POST',
            body: JSON.stringify({ ...submissionData, userID: userId })
        }),
    
    getByUserId: (userId) => apiCall(`/sell-submissions/customer/${userId}`),
    
    getAll: () => apiCall('/sell-submissions'),
    
    approve: (id) => 
        apiCall(`/sell-submissions/${id}/approve`, {
            method: 'PUT'
        }),
    
    reject: (id) => 
        apiCall(`/sell-submissions/${id}/reject`, {
            method: 'PUT'
        })
};

// Users API
const usersAPI = {
    getAll: () => apiCall('/users'),
    
    getById: (id) => apiCall(`/users/${id}`),
    
    update: (id, userData) => 
        apiCall(`/users/${id}`, {
            method: 'PUT',
            body: JSON.stringify(userData)
        })
};

// Health API
const healthAPI = {
    checkDatabase: () => apiCall('/health/database')
};

