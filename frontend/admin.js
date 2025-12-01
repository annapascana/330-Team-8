// Admin page functionality
function showTab(tabName) {
    // Hide all tabs
    document.querySelectorAll('.tab-content').forEach(tab => {
        tab.classList.remove('active');
    });
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.classList.remove('active');
    });
    
    // Show selected tab
    document.getElementById(tabName + 'Tab').classList.add('active');
    event.target.classList.add('active');
    
    // Load tab content
    if (tabName === 'inventory') loadInventory();
    else if (tabName === 'submissions') loadSubmissions();
    else if (tabName === 'orders') loadAllOrders();
    else if (tabName === 'users') loadUsers();
}

async function loadInventory() {
    const container = document.getElementById('inventoryContent');
    container.innerHTML = '<div class="loading">Loading inventory...</div>';
    
    try {
        const books = await booksAPI.getAll();
        displayInventory(books);
    } catch (error) {
        container.innerHTML = `<div class="error">Error loading inventory: ${error.message}</div>`;
    }
}

function displayInventory(books) {
    const container = document.getElementById('inventoryContent');
    
    let html = `
        <table class="inventory-table">
            <thead>
                <tr>
                    <th>ID</th>
                    <th>Title</th>
                    <th>Author</th>
                    <th>ISBN</th>
                    <th>Price</th>
                    <th>Stock</th>
                    <th>Status</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody>
    `;
    
    books.forEach(book => {
        html += `
            <tr>
                <td>${book.bookID}</td>
                <td>${escapeHtml(book.title)}</td>
                <td>${escapeHtml(book.author)}</td>
                <td>${escapeHtml(book.isbn)}</td>
                <td>$${book.sellingPrice.toFixed(2)}</td>
                <td>${book.stockQuantity}</td>
                <td>${escapeHtml(book.status)}</td>
                <td class="action-buttons">
                    <button onclick="editBook(${book.bookID})" class="btn btn-primary btn-small">Edit</button>
                    <button onclick="deleteBook(${book.bookID})" class="btn btn-danger btn-small">Delete</button>
                </td>
            </tr>
        `;
    });
    
    html += '</tbody></table>';
    container.innerHTML = html;
}

async function loadSubmissions() {
    const container = document.getElementById('submissionsContent');
    container.innerHTML = '<div class="loading">Loading submissions...</div>';
    
    try {
        const submissions = await submissionsAPI.getAll();
        displayAdminSubmissions(submissions);
    } catch (error) {
        container.innerHTML = `<div class="error">Error loading submissions: ${error.message}</div>`;
    }
}

function displayAdminSubmissions(submissions) {
    const container = document.getElementById('submissionsContent');
    
    container.innerHTML = submissions.map(sub => {
        const statusClass = `status-${sub.submissionStatus.toLowerCase()}`;
        return `
            <div class="submission-card">
                <h4>${escapeHtml(sub.title)}</h4>
                <p><strong>Author:</strong> ${escapeHtml(sub.author)} | <strong>ISBN:</strong> ${escapeHtml(sub.isbn)}</p>
                <p><strong>Asking Price:</strong> $${sub.askingPrice.toFixed(2)} | <strong>Condition:</strong> ${escapeHtml(sub.condition)}</p>
                <span class="submission-status ${statusClass}">${sub.submissionStatus}</span>
                ${sub.submissionStatus === 'Pending' ? `
                    <div class="action-buttons" style="margin-top: 1rem;">
                        <button onclick="approveSubmission(${sub.submissionID})" class="btn btn-success btn-small">Approve</button>
                        <button onclick="rejectSubmission(${sub.submissionID})" class="btn btn-danger btn-small">Reject</button>
                    </div>
                ` : ''}
            </div>
        `;
    }).join('');
}

async function loadAllOrders() {
    const container = document.getElementById('ordersContent');
    container.innerHTML = '<div class="loading">Loading orders...</div>';
    
    try {
        const orders = await ordersAPI.getAll();
        displayAdminOrders(orders);
    } catch (error) {
        container.innerHTML = `<div class="error">Error loading orders: ${error.message}</div>`;
    }
}

function displayAdminOrders(orders) {
    const container = document.getElementById('ordersContent');
    
    container.innerHTML = orders.map(order => {
        const statusClass = `status-${order.status.toLowerCase()}`;
        return `
            <div class="order-card">
                <div style="display: flex; justify-content: space-between; align-items: center;">
                    <h3>Order #${order.poid} - User ${order.userID}</h3>
                    <span class="order-status ${statusClass}">${order.status}</span>
                </div>
                <p><strong>Date:</strong> ${new Date(order.orderDate).toLocaleDateString()} | <strong>Total:</strong> $${order.total.toFixed(2)}</p>
                <div class="action-buttons" style="margin-top: 1rem;">
                    <select id="status_${order.poid}" onchange="updateOrderStatus(${order.poid})">
                        <option value="New" ${order.status === 'New' ? 'selected' : ''}>New</option>
                        <option value="Processing" ${order.status === 'Processing' ? 'selected' : ''}>Processing</option>
                        <option value="Shipped" ${order.status === 'Shipped' ? 'selected' : ''}>Shipped</option>
                        <option value="Completed" ${order.status === 'Completed' ? 'selected' : ''}>Completed</option>
                        <option value="Cancelled" ${order.status === 'Cancelled' ? 'selected' : ''}>Cancelled</option>
                    </select>
                </div>
            </div>
        `;
    }).join('');
}

async function approveSubmission(id) {
    if (!confirm('Approve this submission? It will be added to inventory.')) return;
    
    try {
        await submissionsAPI.approve(id);
        alert('Submission approved!');
        loadSubmissions();
    } catch (error) {
        alert('Failed to approve: ' + error.message);
    }
}

async function rejectSubmission(id) {
    if (!confirm('Reject this submission?')) return;
    
    try {
        await submissionsAPI.reject(id);
        alert('Submission rejected');
        loadSubmissions();
    } catch (error) {
        alert('Failed to reject: ' + error.message);
    }
}

async function updateOrderStatus(orderId) {
    const status = document.getElementById(`status_${orderId}`).value;
    
    try {
        await ordersAPI.updateStatus(orderId, status);
        alert('Order status updated');
        loadAllOrders();
    } catch (error) {
        alert('Failed to update status: ' + error.message);
    }
}

function showAddBookForm() {
    document.getElementById('addBookModal').style.display = 'block';
}

function closeAddBookForm() {
    document.getElementById('addBookModal').style.display = 'none';
}

document.getElementById('addBookForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const bookData = {
        isbn: document.getElementById('addISBN').value,
        title: document.getElementById('addTitle').value,
        author: document.getElementById('addAuthor').value,
        edition: document.getElementById('addEdition').value,
        condition: document.getElementById('addCondition').value,
        acquisitionCost: parseFloat(document.getElementById('addAcquisitionCost').value),
        sellingPrice: parseFloat(document.getElementById('addSellingPrice').value),
        stockQuantity: parseInt(document.getElementById('addStockQuantity').value)
    };
    
    try {
        await booksAPI.create(bookData);
        alert('Book added successfully!');
        closeAddBookForm();
        document.getElementById('addBookForm').reset();
        loadInventory();
    } catch (error) {
        alert('Failed to add book: ' + error.message);
    }
});

async function deleteBook(id) {
    if (!confirm('Delete this book?')) return;
    
    try {
        await booksAPI.delete(id);
        alert('Book deleted');
        loadInventory();
    } catch (error) {
        alert('Failed to delete: ' + error.message);
    }
}

async function editBook(id) {
    try {
        const book = await booksAPI.getById(id);
        if (!book) {
            alert('Book not found');
            return;
        }
        
        // Populate edit form
        document.getElementById('editBookID').value = book.bookID;
        document.getElementById('editTitle').value = book.title;
        document.getElementById('editAuthor').value = book.author;
        document.getElementById('editEdition').value = book.edition || '';
        document.getElementById('editCondition').value = book.condition;
        document.getElementById('editAcquisitionCost').value = book.acquisitionCost || '';
        document.getElementById('editSellingPrice').value = book.sellingPrice;
        document.getElementById('editStockQuantity').value = book.stockQuantity;
        document.getElementById('editStatus').value = book.status;
        
        // Show modal
        document.getElementById('editBookModal').style.display = 'block';
    } catch (error) {
        alert('Failed to load book: ' + error.message);
    }
}

function closeEditBookForm() {
    document.getElementById('editBookModal').style.display = 'none';
}

document.getElementById('editBookForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const bookId = parseInt(document.getElementById('editBookID').value);
    const bookData = {
        title: document.getElementById('editTitle').value,
        author: document.getElementById('editAuthor').value,
        edition: document.getElementById('editEdition').value,
        condition: document.getElementById('editCondition').value,
        acquisitionCost: parseFloat(document.getElementById('editAcquisitionCost').value),
        sellingPrice: parseFloat(document.getElementById('editSellingPrice').value),
        stockQuantity: parseInt(document.getElementById('editStockQuantity').value),
        status: document.getElementById('editStatus').value
    };
    
    try {
        await booksAPI.update(bookId, bookData);
        alert('Book updated successfully!');
        closeEditBookForm();
        loadInventory();
    } catch (error) {
        alert('Failed to update book: ' + error.message);
    }
});

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

async function loadUsers() {
    const container = document.getElementById('usersContent');
    container.innerHTML = '<div class="loading">Loading users...</div>';
    
    try {
        const users = await usersAPI.getAll();
        displayUsers(users);
    } catch (error) {
        container.innerHTML = `<div class="error">Error loading users: ${error.message}</div>`;
    }
}

function displayUsers(users) {
    const container = document.getElementById('usersContent');
    
    let html = `
        <table class="inventory-table">
            <thead>
                <tr>
                    <th>ID</th>
                    <th>Username</th>
                    <th>Email</th>
                    <th>User Type</th>
                    <th>Created</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody>
    `;
    
    users.forEach(user => {
        html += `
            <tr>
                <td>${user.userID}</td>
                <td>${escapeHtml(user.username)}</td>
                <td>${escapeHtml(user.email)}</td>
                <td>${escapeHtml(user.userType)}</td>
                <td>${new Date(user.createdAt).toLocaleDateString()}</td>
                <td class="action-buttons">
                    <button onclick="editUser(${user.userID})" class="btn btn-primary btn-small">Edit</button>
                </td>
            </tr>
        `;
    });
    
    html += '</tbody></table>';
    container.innerHTML = html;
}

async function editUser(id) {
    try {
        const user = await usersAPI.getById(id);
        if (!user) {
            alert('User not found');
            return;
        }
        
        const newUsername = prompt('Enter new username:', user.username);
        if (newUsername === null) return;
        
        const newEmail = prompt('Enter new email:', user.email);
        if (newEmail === null) return;
        
        const newUserType = prompt('Enter user type (Customer/Admin):', user.userType);
        if (newUserType === null) return;
        
        if (!['Customer', 'Admin'].includes(newUserType)) {
            alert('User type must be Customer or Admin');
            return;
        }
        
        const userData = {
            username: newUsername,
            email: newEmail,
            userType: newUserType
        };
        
        await usersAPI.update(id, userData);
        alert('User updated successfully!');
        loadUsers();
    } catch (error) {
        alert('Failed to update user: ' + error.message);
    }
}

// Load inventory by default
document.addEventListener('DOMContentLoaded', () => {
    loadInventory();
});

