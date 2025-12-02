// API Base URL - Use the same origin as the current page
// Check if running from file:// protocol and show helpful error
if (window.location.protocol === 'file:') {
    document.addEventListener('DOMContentLoaded', () => {
        const body = document.body;
        if (body) {
            body.innerHTML = `
                <div style="padding: 2rem; font-family: Arial, sans-serif; max-width: 600px; margin: 2rem auto;">
                    <h1 style="color: #8B0000;">⚠️ Incorrect Access Method</h1>
                    <p style="font-size: 1.1rem; margin: 1rem 0;">
                        You're opening this file directly from your file system (file://), which doesn't work with this application.
                    </p>
                    <h2 style="color: #333; margin-top: 2rem;">How to Fix:</h2>
                    <ol style="line-height: 1.8;">
                        <li><strong>Start the backend server:</strong>
                            <pre style="background: #f5f5f5; padding: 1rem; border-radius: 4px; margin: 0.5rem 0;">
cd 330-Team-8-main/backend/CrimsonBookStore.Api
dotnet run</pre>
                        </li>
                        <li><strong>Open your browser</strong> and navigate to:
                            <pre style="background: #f5f5f5; padding: 1rem; border-radius: 4px; margin: 0.5rem 0;">
<a href="http://localhost:5000" style="color: #8B0000; font-weight: bold;">http://localhost:5000</a></pre>
                        </li>
                        <li><strong>Do NOT</strong> open the HTML files directly from your file explorer</li>
                    </ol>
                    <p style="margin-top: 2rem; padding: 1rem; background: #e3f2fd; border-radius: 4px;">
                        <strong>Note:</strong> The backend server automatically serves all frontend files. 
                        You don't need a separate web server - just run <code>dotnet run</code> and access via <code>http://localhost:5000</code>
                    </p>
                </div>
            `;
        }
    });
}

const API_BASE_URL = `${window.location.origin}/api`;

// Helper function for API calls with retry mechanism
async function apiCall(endpoint, options = {}, retries = 3, retryDelay = 1000) {
    // Check if we're running from file:// protocol (not supported)
    if (window.location.protocol === 'file:') {
        throw new Error('Please access this site through http://localhost:5000. File:// protocol is not supported.');
    }
    
    const url = `${API_BASE_URL}${endpoint}`;
    
    for (let attempt = 1; attempt <= retries; attempt++) {
        try {
            console.log(`API Call (attempt ${attempt}/${retries}):`, url, options.method || 'GET');
            
            const response = await fetch(url, {
                ...options,
                credentials: 'include', // Include cookies for session
                headers: {
                    'Content-Type': 'application/json',
                    ...options.headers
                }
            });

        // Handle 204 No Content responses (no body) - check BEFORE reading body
        if (response.status === 204) {
            return null;
        }

        if (!response.ok) {
            // Try to get error message, but handle empty responses
            let errorMessage = 'Request failed';
            try {
                const text = await response.text();
                if (text && text.trim().length > 0) {
                    const error = JSON.parse(text);
                    errorMessage = error.message || error.error || errorMessage;
                    console.error('API Error Response:', error);
                } else {
                    errorMessage = `HTTP error! status: ${response.status}`;
                }
            } catch (e) {
                // If we can't parse, use status code
                console.error('Failed to parse error response:', e);
                errorMessage = `HTTP error! status: ${response.status}`;
            }
            throw new Error(errorMessage);
        }

        // Check if response has content to parse
        const contentType = response.headers.get('content-type');
        const text = await response.text();
        
        // If no content, return null
        if (!text || text.trim().length === 0) {
            return null;
        }
        
        // Try to parse as JSON if content-type suggests JSON
        if (contentType && contentType.includes('application/json')) {
            try {
                return JSON.parse(text);
            } catch (e) {
                // If parsing fails, return null
                console.warn('Failed to parse JSON response:', e);
                return null;
            }
        }
        
            return text;
        } catch (error) {
            // If it's the last attempt or a non-retryable error, throw
            if (attempt === retries || (error.name !== 'TypeError' && error.name !== 'NetworkError')) {
                console.error('API Error (final attempt):', error);
                throw error;
            }
            
            // Wait before retrying
            console.warn(`API call failed, retrying in ${retryDelay}ms... (attempt ${attempt}/${retries})`);
            await new Promise(resolve => setTimeout(resolve, retryDelay));
            retryDelay *= 1.5; // Exponential backoff
        }
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
        return apiCall(`/books?${query}`);
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
        }),
    
    clear: () => 
        apiCall('/cart/clear', {
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

