// Book details page
async function loadBookDetails() {
    const urlParams = new URLSearchParams(window.location.search);
    const bookId = urlParams.get('id');
    
    if (!bookId) {
        document.getElementById('bookDetails').innerHTML = '<div class="error">No book ID provided</div>';
        return;
    }
    
    const container = document.getElementById('bookDetails');
    container.innerHTML = '<div class="loading">Loading book details...</div>';
    
    try {
        const book = await booksAPI.getById(parseInt(bookId));
        displayBookDetails(book);
    } catch (error) {
        container.innerHTML = `<div class="error">Error loading book: ${error.message}</div>`;
    }
}

function displayBookDetails(book) {
    const container = document.getElementById('bookDetails');
    
    container.innerHTML = `
        <div class="book-details">
            <h2>${escapeHtml(book.title)}</h2>
            <div class="book-info">
                <p><strong>Author:</strong> ${escapeHtml(book.author)}</p>
                <p><strong>ISBN:</strong> ${escapeHtml(book.isbn)}</p>
                ${book.edition ? `<p><strong>Edition:</strong> ${escapeHtml(book.edition)}</p>` : ''}
                <p><strong>Condition:</strong> ${escapeHtml(book.condition)}</p>
                <p><strong>Status:</strong> ${escapeHtml(book.status)}</p>
                <p><strong>Stock Available:</strong> ${book.stockQuantity}</p>
                <p class="price" style="font-size: 2rem; margin: 1rem 0;">$${book.sellingPrice.toFixed(2)}</p>
            </div>
            ${book.stockQuantity > 0 ? `
                <div class="form-group">
                    <label>Quantity:</label>
                    <input type="number" id="quantity" min="1" max="${book.stockQuantity}" value="1" class="quantity-input">
                </div>
                <button onclick="addToCartFromDetails(${book.bookID})" class="btn btn-primary btn-large">Add to Cart</button>
            ` : '<p class="error">Out of Stock</p>'}
        </div>
    `;
}

async function addToCartFromDetails(bookId) {
    const quantity = parseInt(document.getElementById('quantity').value);
    if (quantity <= 0) {
        alert('Please enter a valid quantity');
        return;
    }
    
    try {
        await cartAPI.add(bookId, quantity);
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

document.addEventListener('DOMContentLoaded', loadBookDetails);

