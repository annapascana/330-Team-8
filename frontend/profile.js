// Profile page functionality
let isEditing = false;
let originalUserData = null;

async function loadProfile() {
    const container = document.getElementById('profileContent');
    container.innerHTML = '<div class="loading">Loading profile...</div>';
    
    const user = getCurrentUser();
    if (!user) {
        container.innerHTML = '<div class="error">Please log in to view your profile</div>';
        return;
    }
    
    try {
        const userData = await usersAPI.getById(user.userID);
        originalUserData = userData;
        displayProfile(userData);
    } catch (error) {
        container.innerHTML = `<div class="error">Error loading profile: ${error.message}</div>`;
    }
}

function displayProfile(userData) {
    const container = document.getElementById('profileContent');
    
    if (isEditing) {
        container.innerHTML = `
            <div class="profile-card">
                <h3>Edit Profile</h3>
                <form id="profileForm">
                    <div class="form-group">
                        <label>User ID:</label>
                        <input type="text" value="${userData.userID}" disabled class="form-control">
                        <small style="color: #666;">User ID cannot be changed</small>
                    </div>
                    <div class="form-group">
                        <label>First Name:</label>
                        <input type="text" id="fName" value="${escapeHtml(userData.fName || '')}" required class="form-control">
                    </div>
                    <div class="form-group">
                        <label>Last Name:</label>
                        <input type="text" id="lName" value="${escapeHtml(userData.lName || '')}" required class="form-control">
                    </div>
                    <div class="form-group">
                        <label>Email:</label>
                        <input type="email" id="email" value="${escapeHtml(userData.email || '')}" required class="form-control">
                    </div>
                    <div class="form-group">
                        <label>User Type:</label>
                        <input type="text" value="${escapeHtml(userData.userType || '')}" disabled class="form-control">
                        <small style="color: #666;">User type cannot be changed from profile page</small>
                    </div>
                    <div class="form-group">
                        <label>Account Created:</label>
                        <input type="text" value="${new Date(userData.createdAt).toLocaleDateString()}" disabled class="form-control">
                    </div>
                    <div style="display: flex; gap: 1rem; margin-top: 1.5rem;">
                        <button type="submit" class="btn btn-primary">Save Changes</button>
                        <button type="button" onclick="cancelEdit()" class="btn btn-outline">Cancel</button>
                    </div>
                </form>
            </div>
        `;
        
        document.getElementById('profileForm').addEventListener('submit', handleUpdate);
    } else {
        container.innerHTML = `
            <div class="profile-card">
                <h3>Profile Information</h3>
                <div class="profile-info">
                    <div class="info-row">
                        <strong>User ID:</strong>
                        <span>${userData.userID}</span>
                    </div>
                    <div class="info-row">
                        <strong>First Name:</strong>
                        <span>${escapeHtml(userData.fName || '')}</span>
                    </div>
                    <div class="info-row">
                        <strong>Last Name:</strong>
                        <span>${escapeHtml(userData.lName || '')}</span>
                    </div>
                    <div class="info-row">
                        <strong>Email:</strong>
                        <span>${escapeHtml(userData.email || '')}</span>
                    </div>
                    <div class="info-row">
                        <strong>User Type:</strong>
                        <span>${escapeHtml(userData.userType || '')}</span>
                    </div>
                    <div class="info-row">
                        <strong>Account Created:</strong>
                        <span>${new Date(userData.createdAt).toLocaleDateString()}</span>
                    </div>
                </div>
                <button onclick="startEdit()" class="btn btn-primary" style="margin-top: 1.5rem;">Edit Profile</button>
            </div>
        `;
    }
}

function startEdit() {
    isEditing = true;
    loadProfile();
}

function cancelEdit() {
    isEditing = false;
    loadProfile();
}

async function handleUpdate(e) {
    e.preventDefault();
    
    const user = getCurrentUser();
    if (!user) {
        alert('Please log in to update your profile');
        return;
    }
    
    const fName = document.getElementById('fName').value.trim();
    const lName = document.getElementById('lName').value.trim();
    const email = document.getElementById('email').value.trim();
    
    if (!fName || !lName || !email) {
        alert('Please fill in all required fields');
        return;
    }
    
    try {
        await usersAPI.update(user.userID, {
            fName: fName,
            lName: lName,
            email: email
        });
        
        // Update the current user in session
        const updatedUser = await usersAPI.getById(user.userID);
        saveUser({
            ...user,
            username: `${updatedUser.fName} ${updatedUser.lName}`,
            email: updatedUser.email
        });
        
        alert('Profile updated successfully!');
        isEditing = false;
        loadProfile();
    } catch (error) {
        alert('Failed to update profile: ' + error.message);
    }
}

function escapeHtml(text) {
    if (text == null) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

document.addEventListener('DOMContentLoaded', loadProfile);

