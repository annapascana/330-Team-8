// Sell page functionality
async function loadSubmissions() {
    const container = document.getElementById('submissionsContent');
    container.innerHTML = '<div class="loading">Loading submissions...</div>';
    
    const user = getCurrentUser();
    if (!user) {
        container.innerHTML = '<div class="error">Please log in to view your submissions</div>';
        return;
    }
    
    try {
        const submissions = await submissionsAPI.getByUserId(user.userID);
        displaySubmissions(submissions);
    } catch (error) {
        container.innerHTML = `<div class="error">Error loading submissions: ${error.message}</div>`;
    }
}

async function loadPurchasedBooks() {
    const container = document.getElementById('purchasedBooksContent');
    if (!container) return;
    
    container.innerHTML = '<div class="loading">Loading your purchased books...</div>';
    
    const user = getCurrentUser();
    if (!user) {
        container.innerHTML = '<div class="error">Please log in to view your purchased books</div>';
        return;
    }
    
    try {
        const books = await ordersAPI.getPurchasedBooks(user.userID);
        console.log('Purchased books received:', books);
        displayPurchasedBooks(books);
    } catch (error) {
        console.error('Error loading purchased books:', error);
        container.innerHTML = `<div class="error">Error loading purchased books: ${error.message}</div>`;
    }
}

function displayPurchasedBooks(books) {
    const container = document.getElementById('purchasedBooksContent');
    if (!container) {
        console.error('purchasedBooksContent container not found');
        return;
    }
    
    console.log('Displaying purchased books:', books);
    
    if (!books || books.length === 0) {
        container.innerHTML = '<div class="error">You haven\'t purchased any books yet, or all your books have been sold.</div>';
        return;
    }
    
    container.innerHTML = books.map((book, index) => `
        <div class="book-card" style="margin-bottom: 1rem;">
            <h4>${escapeHtml(book.title)}</h4>
            ${book.author ? `<p><strong>Author:</strong> ${escapeHtml(book.author)}</p>` : ''}
            ${book.isbn ? `<p><strong>ISBN:</strong> ${escapeHtml(book.isbn)}</p>` : ''}
            <p><strong>Quantity Available:</strong> ${book.quantity}</p>
            <p><strong>Purchased:</strong> ${new Date(book.purchaseDate).toLocaleDateString()}</p>
            <button class="btn btn-primary btn-small sell-book-btn" 
                    data-book-index="${index}"
                    style="margin-top: 0.5rem;">
                Sell This Book
            </button>
        </div>
    `).join('');
    
    // Store books data for button handlers
    window.purchasedBooksData = books;
    
    // Add event listeners to all sell buttons
    container.querySelectorAll('.sell-book-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            const index = parseInt(this.getAttribute('data-book-index'));
            createSubmissionFromBook(window.purchasedBooksData[index]);
        });
    });
}

function createSubmissionFromBook(book) {
    // Pre-fill the form with book info
    if (document.getElementById('sellTitle')) document.getElementById('sellTitle').value = book.title || '';
    if (document.getElementById('sellAuthor')) document.getElementById('sellAuthor').value = book.author || '';
    if (document.getElementById('sellISBN')) document.getElementById('sellISBN').value = book.isbn || '';
    if (document.getElementById('sellEdition')) document.getElementById('sellEdition').value = book.edition || '';
    if (document.getElementById('sellCondition')) document.getElementById('sellCondition').value = book.condition || 'Good';
    
    // Scroll to form
    document.getElementById('sellForm').scrollIntoView({ behavior: 'smooth' });
}

function displaySubmissions(submissions) {
    const container = document.getElementById('submissionsContent');
    
    if (submissions.length === 0) {
        container.innerHTML = '<div class="error">No submissions found</div>';
        return;
    }
    
    container.innerHTML = submissions.map(sub => {
        const statusClass = `status-${sub.submissionStatus.toLowerCase()}`;
        return `
            <div class="submission-card">
                <h4>${escapeHtml(sub.title)}</h4>
                <p><strong>Author:</strong> ${escapeHtml(sub.author)}</p>
                <p><strong>ISBN:</strong> ${escapeHtml(sub.isbn)}</p>
                <p><strong>Condition:</strong> ${escapeHtml(sub.condition)}</p>
                <p><strong>Asking Price:</strong> $${sub.askingPrice.toFixed(2)}</p>
                <p><strong>Submitted:</strong> ${new Date(sub.createdAt).toLocaleDateString()}</p>
                <span class="submission-status ${statusClass}">${sub.submissionStatus}</span>
                ${sub.reviewedAt ? `<p><strong>Reviewed:</strong> ${new Date(sub.reviewedAt).toLocaleDateString()}</p>` : ''}
            </div>
        `;
    }).join('');
}

document.getElementById('sellForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const submissionData = {
        title: document.getElementById('sellTitle').value,
        author: document.getElementById('sellAuthor').value,
        isbn: document.getElementById('sellISBN').value.trim(),
        edition: document.getElementById('sellEdition').value.trim(),
        condition: document.getElementById('sellCondition').value,
        askingPrice: parseFloat(document.getElementById('sellPrice').value)
    };
    
    const user = getCurrentUser();
    if (!user) {
        alert('Please log in to submit a book');
        return;
    }
    
    try {
        await submissionsAPI.create(submissionData, user.userID);
        if (window.toast) {
            toast.success('Book submission created successfully! It will be reviewed by an admin.', 3000, 'Submitted');
        } else {
            alert('Book submission created successfully! It will be reviewed by an admin.');
        }
        document.getElementById('sellForm').reset();
        loadSubmissions();
        // Reload purchased books to show updated quantity (even though it won't change until approved)
        loadPurchasedBooks();
    } catch (error) {
        if (window.toast) {
            toast.error('Failed to submit book: ' + error.message, 5000, 'Error');
        } else {
            alert('Failed to submit book: ' + error.message);
        }
    }
});

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

document.addEventListener('DOMContentLoaded', () => {
    loadSubmissions();
    loadPurchasedBooks();
});

