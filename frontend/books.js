// Books page functionality
let allBooks = [];

async function loadBooks() {
    const grid = document.getElementById('booksGrid');
    grid.innerHTML = '<div class="loading">Loading books...</div>';
    
    try {
        allBooks = await booksAPI.getAll();
        displayBooks(allBooks);
    } catch (error) {
        grid.innerHTML = `<div class="error">Error loading books: ${error.message}</div>`;
    }
}

function displayBooks(books) {
    const grid = document.getElementById('booksGrid');
    
    if (books.length === 0) {
        grid.innerHTML = '<div class="error">No books found</div>';
        return;
    }
    
    grid.innerHTML = books.map(book => `
        <div class="book-card">
            <h3>${escapeHtml(book.title)}</h3>
            <p><strong>Author:</strong> ${escapeHtml(book.author)}</p>
            <p><strong>ISBN:</strong> ${escapeHtml(book.isbn)}</p>
            ${book.edition ? `<p><strong>Edition:</strong> ${escapeHtml(book.edition)}</p>` : ''}
            <p><strong>Condition:</strong> ${escapeHtml(book.condition)}</p>
            <p class="price">$${book.sellingPrice.toFixed(2)}</p>
            <p class="stock">Stock: ${book.stockQuantity} available</p>
            <button onclick="viewBookDetails(${book.bookID})" class="btn btn-primary">View Details</button>
            <button onclick="addToCart(${book.bookID})" class="btn btn-outline" style="margin-top: 0.5rem;">Add to Cart</button>
        </div>
    `).join('');
}

async function searchBooks() {
    const title = document.getElementById('searchTitle').value.trim();
    const author = document.getElementById('searchAuthor').value.trim();
    const isbn = document.getElementById('searchISBN').value.trim();
    
    const grid = document.getElementById('booksGrid');
    grid.innerHTML = '<div class="loading">Searching...</div>';
    
    try {
        const params = {};
        if (title) params.title = title;
        if (author) params.author = author;
        if (isbn) params.isbn = isbn;
        
        const books = Object.keys(params).length > 0 
            ? await booksAPI.search(params)
            : await booksAPI.getAll();
        
        displayBooks(books);
    } catch (error) {
        grid.innerHTML = `<div class="error">Search failed: ${error.message}</div>`;
    }
}

function clearSearch() {
    document.getElementById('searchTitle').value = '';
    document.getElementById('searchAuthor').value = '';
    document.getElementById('searchISBN').value = '';
    loadBooks();
}

function viewBookDetails(bookId) {
    window.location.href = `book-details.html?id=${bookId}`;
}

async function addToCart(bookId) {
    const quantity = prompt('Enter quantity:', '1');
    if (!quantity || parseInt(quantity) <= 0) return;
    
    try {
        await cartAPI.add(bookId, parseInt(quantity));
        alert('Book added to cart!');
    } catch (error) {
        alert('Failed to add to cart: ' + error.message);
    }
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Load books on page load
document.addEventListener('DOMContentLoaded', loadBooks);

