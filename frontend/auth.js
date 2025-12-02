// Auth state management
let currentUser = null;

// Load user from sessionStorage on page load
function loadUser() {
    const userStr = sessionStorage.getItem('currentUser');
    if (userStr) {
        currentUser = JSON.parse(userStr);
    }
    // Always update UI, even if no user (to hide admin links, etc.)
    updateUI();
}

// Save user to sessionStorage
function saveUser(user) {
    currentUser = user;
    sessionStorage.setItem('currentUser', JSON.stringify(user));
    updateUI();
}

// Clear user from sessionStorage
function clearUser() {
    currentUser = null;
    sessionStorage.removeItem('currentUser');
    updateUI();
}

// Update UI based on auth state
function updateUI() {
    const loginBtn = document.getElementById('loginBtn');
    const registerBtn = document.getElementById('registerBtn');
    const logoutBtn = document.getElementById('logoutBtn');
    const userInfo = document.getElementById('userInfo');
    
    // Hide/show admin link based on user role
    const adminLinks = document.querySelectorAll('a[href="admin.html"]');
    const isAdminUser = isAdmin();
    
    // Always hide admin links by default, only show if user is admin
    adminLinks.forEach(link => {
        if (isAdminUser) {
            link.style.display = 'inline-block';
        } else {
            link.style.display = 'none';
        }
    });

    if (currentUser) {
        if (loginBtn) loginBtn.style.display = 'none';
        if (registerBtn) registerBtn.style.display = 'none';
        if (logoutBtn) logoutBtn.style.display = 'block';
        if (userInfo) {
            userInfo.textContent = `Welcome, ${currentUser.username}`;
            userInfo.style.display = 'inline-block';
        }
    } else {
        if (loginBtn) loginBtn.style.display = 'inline-block';
        if (registerBtn) registerBtn.style.display = 'inline-block';
        if (logoutBtn) logoutBtn.style.display = 'none';
        if (userInfo) userInfo.style.display = 'none';
        // Hide admin link when logged out
        adminLinks.forEach(link => link.style.display = 'none');
    }
}

// Initialize auth on page load
document.addEventListener('DOMContentLoaded', () => {
    loadUser();
    
    // Setup login modal
    const loginModal = document.getElementById('loginModal');
    const loginBtn = document.getElementById('loginBtn');
    const loginForm = document.getElementById('loginForm');
    
    if (loginBtn) {
        loginBtn.addEventListener('click', () => {
            if (loginModal) loginModal.style.display = 'block';
        });
    }
    
    if (loginForm) {
        loginForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const email = document.getElementById('loginEmail').value;
            const password = document.getElementById('loginPassword').value;
            
            try {
                const user = await authAPI.login(email, password);
                saveUser(user);
                if (loginModal) loginModal.style.display = 'none';
                alert('Login successful!');
            } catch (error) {
                alert('Login failed: ' + error.message);
            }
        });
    }
    
    // Setup register modal
    const registerModal = document.getElementById('registerModal');
    const registerBtn = document.getElementById('registerBtn');
    const registerForm = document.getElementById('registerForm');
    
    if (registerBtn) {
        registerBtn.addEventListener('click', () => {
            if (registerModal) registerModal.style.display = 'block';
        });
    }
    
    if (registerForm) {
        registerForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const username = document.getElementById('registerUsername').value;
            const email = document.getElementById('registerEmail').value;
            const password = document.getElementById('registerPassword').value;
            
            try {
                const user = await authAPI.register(username, email, password);
                saveUser(user);
                if (registerModal) registerModal.style.display = 'none';
                alert('Registration successful!');
            } catch (error) {
                alert('Registration failed: ' + error.message);
            }
        });
    }
    
    // Setup logout
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', () => {
            clearUser();
            alert('Logged out successfully');
        });
    }
    
    // Close modals on X click
    document.querySelectorAll('.close').forEach(closeBtn => {
        closeBtn.addEventListener('click', (e) => {
            e.target.closest('.modal').style.display = 'none';
        });
    });
    
    // Close modals on outside click
    window.addEventListener('click', (e) => {
        if (e.target.classList.contains('modal')) {
            e.target.style.display = 'none';
        }
    });
});

// Export for use in other scripts
function getCurrentUser() {
    return currentUser;
}

function isAdmin() {
    return currentUser && currentUser.userType && currentUser.userType.toLowerCase() === 'admin';
}

