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
        displayPurchasedBooks(books);
    } catch (error) {
        container.innerHTML = `<div class="error">Error loading purchased books: ${error.message}</div>`;
    }
}

function displayPurchasedBooks(books) {
    const container = document.getElementById('purchasedBooksContent');
    if (!container) return;
    
    if (books.length === 0) {
        container.innerHTML = '<div class="error">You haven\'t purchased any books yet</div>';
        return;
    }
    
    container.innerHTML = books.map(book => `
        <div class="book-card" style="margin-bottom: 1rem;">
            <h4>${escapeHtml(book.title)}</h4>
            <p><strong>Quantity:</strong> ${book.quantity}</p>
            <p><strong>Purchased:</strong> ${new Date(book.purchaseDate).toLocaleDateString()}</p>
            <button onclick="createSubmissionFromBook(${book.bookID}, '${escapeHtml(book.title)}')" 
                    class="btn btn-primary btn-small" style="margin-top: 0.5rem;">
                Sell This Book
            </button>
        </div>
    `).join('');
}

function createSubmissionFromBook(bookId, title) {
    // Pre-fill the form with book info
    document.getElementById('sellTitle').value = title;
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
        isbn: document.getElementById('sellISBN').value,
        edition: document.getElementById('sellEdition').value,
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
        alert('Book submission created successfully! It will be reviewed by an admin.');
        document.getElementById('sellForm').reset();
        loadSubmissions();
    } catch (error) {
        alert('Failed to submit book: ' + error.message);
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

