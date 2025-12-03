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

function getSkeletonLoader(count = 5) {
    return `
        <div style="display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 1rem;">
            ${Array(count).fill(0).map(() => `
                <div class="skeleton-card">
                    <div class="skeleton-title"></div>
                    <div class="skeleton-text"></div>
                    <div class="skeleton-text short"></div>
                </div>
            `).join('')}
        </div>
    `;
}

async function loadInventory() {
    const container = document.getElementById('inventoryContent');
    container.innerHTML = getSkeletonLoader(8);
    
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

let submissionsSortBy = 'date';
let submissionsSortOrder = 'desc';

async function loadSubmissions() {
    const container = document.getElementById('submissionsContent');
    container.innerHTML = getSkeletonLoader(5);
    
    try {
        const submissions = await submissionsAPI.getAll();
        displayAdminSubmissions(submissions);
    } catch (error) {
        container.innerHTML = `<div class="error">Error loading submissions: ${error.message}</div>`;
    }
}

function sortSubmissions(submissions, sortBy, sortOrder) {
    const sorted = [...submissions];
    
    sorted.sort((a, b) => {
        let aVal, bVal;
        
        switch (sortBy) {
            case 'date':
                aVal = new Date(a.submittedAt || a.createdAt || 0);
                bVal = new Date(b.submittedAt || b.createdAt || 0);
                break;
            case 'status':
                aVal = a.submissionStatus || '';
                bVal = b.submissionStatus || '';
                break;
            case 'price':
                aVal = a.askingPrice || 0;
                bVal = b.askingPrice || 0;
                break;
            case 'title':
                aVal = (a.title || '').toLowerCase();
                bVal = (b.title || '').toLowerCase();
                break;
            default:
                return 0;
        }
        
        if (aVal < bVal) return sortOrder === 'asc' ? -1 : 1;
        if (aVal > bVal) return sortOrder === 'asc' ? 1 : -1;
        return 0;
    });
    
    return sorted;
}

function displayAdminSubmissions(submissions) {
    const container = document.getElementById('submissionsContent');
    
    // Add sort controls
    let html = `
        <div style="margin-bottom: 1rem; display: flex; gap: 1rem; align-items: center;">
            <label><strong>Sort by:</strong></label>
            <select id="submissionsSortBy" onchange="updateSubmissionsSort()" style="padding: 0.5rem;">
                <option value="date" ${submissionsSortBy === 'date' ? 'selected' : ''}>Date</option>
                <option value="status" ${submissionsSortBy === 'status' ? 'selected' : ''}>Status</option>
                <option value="price" ${submissionsSortBy === 'price' ? 'selected' : ''}>Price</option>
                <option value="title" ${submissionsSortBy === 'title' ? 'selected' : ''}>Title</option>
            </select>
            <select id="submissionsSortOrder" onchange="updateSubmissionsSort()" style="padding: 0.5rem;">
                <option value="desc" ${submissionsSortOrder === 'desc' ? 'selected' : ''}>Descending</option>
                <option value="asc" ${submissionsSortOrder === 'asc' ? 'selected' : ''}>Ascending</option>
            </select>
        </div>
    `;
    
    // Sort submissions
    const sortedSubmissions = sortSubmissions(submissions, submissionsSortBy, submissionsSortOrder);
    
    html += sortedSubmissions.map(sub => {
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
    
    container.innerHTML = html;
}

function updateSubmissionsSort() {
    submissionsSortBy = document.getElementById('submissionsSortBy').value;
    submissionsSortOrder = document.getElementById('submissionsSortOrder').value;
    loadSubmissions();
}

let ordersSortBy = 'date';
let ordersSortOrder = 'desc';

async function loadAllOrders() {
    const container = document.getElementById('ordersContent');
    container.innerHTML = getSkeletonLoader(5);
    
    try {
        const orders = await ordersAPI.getAll();
        displayAdminOrders(orders);
    } catch (error) {
        container.innerHTML = `<div class="error">Error loading orders: ${error.message}</div>`;
    }
}

function sortOrders(orders, sortBy, sortOrder) {
    const sorted = [...orders];
    
    sorted.sort((a, b) => {
        let aVal, bVal;
        
        switch (sortBy) {
            case 'date':
                aVal = new Date(a.orderDate || 0);
                bVal = new Date(b.orderDate || 0);
                break;
            case 'status':
                aVal = a.status || '';
                bVal = b.status || '';
                break;
            case 'total':
                aVal = a.total || 0;
                bVal = b.total || 0;
                break;
            case 'orderId':
                aVal = a.poid || 0;
                bVal = b.poid || 0;
                break;
            case 'userId':
                aVal = a.userID || 0;
                bVal = b.userID || 0;
                break;
            default:
                return 0;
        }
        
        if (aVal < bVal) return sortOrder === 'asc' ? -1 : 1;
        if (aVal > bVal) return sortOrder === 'asc' ? 1 : -1;
        return 0;
    });
    
    return sorted;
}

function displayAdminOrders(orders) {
    const container = document.getElementById('ordersContent');
    
    // Add sort controls
    let html = `
        <div style="margin-bottom: 1rem; display: flex; gap: 1rem; align-items: center;">
            <label><strong>Sort by:</strong></label>
            <select id="ordersSortBy" onchange="updateOrdersSort()" style="padding: 0.5rem;">
                <option value="date" ${ordersSortBy === 'date' ? 'selected' : ''}>Date</option>
                <option value="status" ${ordersSortBy === 'status' ? 'selected' : ''}>Status</option>
                <option value="total" ${ordersSortBy === 'total' ? 'selected' : ''}>Total</option>
                <option value="orderId" ${ordersSortBy === 'orderId' ? 'selected' : ''}>Order ID</option>
                <option value="userId" ${ordersSortBy === 'userId' ? 'selected' : ''}>User ID</option>
            </select>
            <select id="ordersSortOrder" onchange="updateOrdersSort()" style="padding: 0.5rem;">
                <option value="desc" ${ordersSortOrder === 'desc' ? 'selected' : ''}>Descending</option>
                <option value="asc" ${ordersSortOrder === 'asc' ? 'selected' : ''}>Ascending</option>
            </select>
            <button onclick="loadAllOrders()" class="btn btn-outline btn-small">Refresh</button>
        </div>
    `;
    
    // Sort orders
    const sortedOrders = sortOrders(orders, ordersSortBy, ordersSortOrder);
    
    if (sortedOrders.length === 0) {
        html += '<div class="error">No orders found</div>';
    } else {
        html += sortedOrders.map(order => {
            const statusClass = `status-${order.status.toLowerCase()}`;
            return `
                <div class="order-card">
                    <div style="display: flex; justify-content: space-between; align-items: center;">
                        <h3>Order #${order.poid} - User ${order.userID}</h3>
                        <span class="order-status ${statusClass}">${order.status}</span>
                    </div>
                    <p><strong>Date:</strong> ${new Date(order.orderDate).toLocaleDateString()} | <strong>Total:</strong> $${order.total.toFixed(2)}</p>
                    ${order.lineItems && order.lineItems.length > 0 ? `
                        <details style="margin-top: 1rem;">
                            <summary>View Items (${order.lineItems.length})</summary>
                            <table style="width: 100%; margin-top: 1rem;">
                                <thead>
                                    <tr>
                                        <th>Book</th>
                                        <th>Quantity</th>
                                        <th>Unit Price</th>
                                        <th>Total</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    ${order.lineItems.map(item => `
                                        <tr>
                                            <td>${escapeHtml(item.bookTitle)}</td>
                                            <td>${item.quantity}</td>
                                            <td>$${item.unitPrice.toFixed(2)}</td>
                                            <td>$${item.lineTotal.toFixed(2)}</td>
                                        </tr>
                                    `).join('')}
                                </tbody>
                            </table>
                        </details>
                    ` : ''}
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
    
    container.innerHTML = html;
}

function updateOrdersSort() {
    ordersSortBy = document.getElementById('ordersSortBy').value;
    ordersSortOrder = document.getElementById('ordersSortOrder').value;
    loadAllOrders();
}

async function approveSubmission(id) {
    if (!confirm('Approve this submission? It will be added to inventory.')) return;
    
    try {
        await submissionsAPI.approve(id);
        if (window.toast) {
            toast.success('Submission approved and added to inventory!', 3000, 'Approved');
        } else {
            alert('Submission approved!');
        }
        loadSubmissions();
    } catch (error) {
        if (window.toast) {
            toast.error('Failed to approve: ' + error.message, 5000, 'Error');
        } else {
            alert('Failed to approve: ' + error.message);
        }
    }
}

async function rejectSubmission(id) {
    if (!confirm('Reject this submission?')) return;
    
    try {
        await submissionsAPI.reject(id);
        if (window.toast) {
            toast.info('Submission rejected', 3000, 'Rejected');
        } else {
            alert('Submission rejected');
        }
        loadSubmissions();
    } catch (error) {
        if (window.toast) {
            toast.error('Failed to reject: ' + error.message, 5000, 'Error');
        } else {
            alert('Failed to reject: ' + error.message);
        }
    }
}

async function updateOrderStatus(orderId) {
    const status = document.getElementById(`status_${orderId}`).value;
    
    try {
        await ordersAPI.updateStatus(orderId, status);
        if (window.toast) {
            toast.success(`Order status updated to ${status}`, 3000, 'Status Updated');
        } else {
            alert('Order status updated');
        }
        loadAllOrders();
    } catch (error) {
        if (window.toast) {
            toast.error('Failed to update status: ' + error.message, 5000, 'Error');
        } else {
            alert('Failed to update status: ' + error.message);
        }
    }
}

function showAddBookForm() {
    const modal = document.getElementById('addBookModal');
    modal.style.display = 'block';
    document.getElementById('addBookForm').reset();
    // Focus first input
    setTimeout(() => document.getElementById('addISBN').focus(), 100);
}

function closeAddBookForm() {
    document.getElementById('addBookModal').style.display = 'none';
    document.getElementById('addBookForm').reset();
}

document.getElementById('addBookForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const submitBtn = e.target.querySelector('button[type="submit"]');
    const originalText = submitBtn.textContent;
    submitBtn.disabled = true;
    submitBtn.textContent = 'Adding...';
    
    const bookData = {
        isbn: document.getElementById('addISBN').value.trim(),
        title: document.getElementById('addTitle').value.trim(),
        author: document.getElementById('addAuthor').value.trim(),
        edition: document.getElementById('addEdition').value.trim(),
        condition: document.getElementById('addCondition').value,
        acquisitionCost: parseFloat(document.getElementById('addAcquisitionCost').value),
        sellingPrice: parseFloat(document.getElementById('addSellingPrice').value),
        stockQuantity: parseInt(document.getElementById('addStockQuantity').value)
    };
    
    // Validation
    if (bookData.sellingPrice <= bookData.acquisitionCost) {
        alert('Selling price must be greater than acquisition cost');
        submitBtn.disabled = false;
        submitBtn.textContent = originalText;
        return;
    }
    
    try {
        await booksAPI.create(bookData);
        if (window.toast) {
            toast.success('Book added successfully!', 3000, 'Success');
        } else {
            alert('Book added successfully!');
        }
        closeAddBookForm();
        loadInventory();
    } catch (error) {
        if (window.toast) {
            toast.error('Failed to add book: ' + error.message, 5000, 'Error');
        } else {
            alert('Failed to add book: ' + error.message);
        }
        submitBtn.disabled = false;
        submitBtn.textContent = originalText;
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
        const modal = document.getElementById('editBookModal');
        modal.style.display = 'block';
        // Focus first input
        setTimeout(() => document.getElementById('editTitle').focus(), 100);
    } catch (error) {
        alert('Failed to load book: ' + error.message);
    }
}

function closeEditBookForm() {
    document.getElementById('editBookModal').style.display = 'none';
    document.getElementById('editBookForm').reset();
}

document.getElementById('editBookForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const submitBtn = e.target.querySelector('button[type="submit"]');
    const originalText = submitBtn.textContent;
    submitBtn.disabled = true;
    submitBtn.textContent = 'Updating...';
    
    const bookId = parseInt(document.getElementById('editBookID').value);
    const bookData = {
        title: document.getElementById('editTitle').value.trim(),
        author: document.getElementById('editAuthor').value.trim(),
        edition: document.getElementById('editEdition').value.trim(),
        condition: document.getElementById('editCondition').value,
        acquisitionCost: parseFloat(document.getElementById('editAcquisitionCost').value),
        sellingPrice: parseFloat(document.getElementById('editSellingPrice').value),
        stockQuantity: parseInt(document.getElementById('editStockQuantity').value),
        status: document.getElementById('editStatus').value
    };
    
    // Validation
    if (bookData.sellingPrice <= bookData.acquisitionCost) {
        alert('Selling price must be greater than acquisition cost');
        submitBtn.disabled = false;
        submitBtn.textContent = originalText;
        return;
    }
    
    try {
        await booksAPI.update(bookId, bookData);
        if (window.toast) {
            toast.success('Book updated successfully!', 3000, 'Success');
        } else {
            alert('Book updated successfully!');
        }
        closeEditBookForm();
        loadInventory();
    } catch (error) {
        if (window.toast) {
            toast.error('Failed to update book: ' + error.message, 5000, 'Error');
        } else {
            alert('Failed to update book: ' + error.message);
        }
        submitBtn.disabled = false;
        submitBtn.textContent = originalText;
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

// Close modals on ESC key
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        const editModal = document.getElementById('editBookModal');
        const addModal = document.getElementById('addBookModal');
        if (editModal && editModal.style.display === 'block') {
            closeEditBookForm();
        }
        if (addModal && addModal.style.display === 'block') {
            closeAddBookForm();
        }
    }
});

// Close modals on outside click
document.addEventListener('click', (e) => {
    const editModal = document.getElementById('editBookModal');
    const addModal = document.getElementById('addBookModal');
    if (e.target === editModal) {
        closeEditBookForm();
    }
    if (e.target === addModal) {
        closeAddBookForm();
    }
});

// Load inventory by default
document.addEventListener('DOMContentLoaded', () => {
    loadInventory();
});

