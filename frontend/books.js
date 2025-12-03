// Books page functionality
let allBooks = [];
let filteredBooks = [];
let currentFilters = {
    minPrice: null,
    maxPrice: null,
    condition: null,
    sortBy: 'title',
    sortOrder: 'asc'
};

// Skeleton loader HTML
function getSkeletonLoader(count = 6) {
    return `
        <div class="skeleton-loader">
            ${Array(count).fill(0).map(() => `
                <div class="skeleton-card">
                    <div class="skeleton-title"></div>
                    <div class="skeleton-text"></div>
                    <div class="skeleton-text short"></div>
                    <div class="skeleton-text"></div>
                    <div class="skeleton-text short"></div>
                    <div class="skeleton-button"></div>
                </div>
            `).join('')}
        </div>
    `;
}

async function loadBooks() {
    const grid = document.getElementById('booksGrid');
    grid.innerHTML = getSkeletonLoader();
    
    try {
        allBooks = await booksAPI.getAll();
        applyFilters();
    } catch (error) {
        console.error('Error loading books:', error);
        grid.innerHTML = `
            <div class="error">
                <strong>Error loading books:</strong> ${error.message}
                <div class="retry-container">
                    <button onclick="loadBooks()" class="btn btn-primary retry-button">Retry</button>
                </div>
            </div>
        `;
        if (window.toast) {
            toast.error('Failed to load books. Please try again.', 7000, 'Error');
        }
    }
}

function displayBooks(books) {
    const grid = document.getElementById('booksGrid');
    
    if (books.length === 0) {
        grid.innerHTML = `
            <div class="error" style="text-align: center; padding: 3rem;">
                <h3>No books found</h3>
                <p>Try adjusting your filters or search criteria.</p>
                <button onclick="clearAllFilters()" class="btn btn-primary" style="margin-top: 1rem;">Clear All Filters</button>
            </div>
        `;
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
    
    // Show result count
    const resultCount = document.getElementById('resultCount');
    if (resultCount) {
        resultCount.textContent = `Showing ${books.length} of ${allBooks.length} books`;
    }
}

async function searchBooks() {
    const title = document.getElementById('searchTitle').value.trim();
    const author = document.getElementById('searchAuthor').value.trim();
    const isbn = document.getElementById('searchISBN').value.trim();
    
    const grid = document.getElementById('booksGrid');
    grid.innerHTML = getSkeletonLoader(4);
    
    try {
        const params = {};
        if (title) params.title = title;
        if (author) params.author = author;
        if (isbn) params.isbn = isbn;
        
        allBooks = Object.keys(params).length > 0 
            ? await booksAPI.search(params)
            : await booksAPI.getAll();
        
        // Save search to sessionStorage
        if (Object.keys(params).length > 0) {
            sessionStorage.setItem('lastSearch', JSON.stringify(params));
        }
        
        applyFilters();
        
        if (window.toast && Object.keys(params).length > 0) {
            toast.success(`Found ${allBooks.length} book(s)`, 3000);
        }
    } catch (error) {
        console.error('Search error:', error);
        grid.innerHTML = `
            <div class="error">
                <strong>Search failed:</strong> ${error.message}
                <div class="retry-container">
                    <button onclick="searchBooks()" class="btn btn-primary retry-button">Retry</button>
                </div>
            </div>
        `;
        if (window.toast) {
            toast.error('Search failed. Please try again.', 7000, 'Search Error');
        }
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
    
    const book = allBooks.find(b => b.bookID === bookId);
    const bookTitle = book ? book.title : 'Book';
    
    try {
        await cartAPI.add(bookId, parseInt(quantity));
        if (window.toast) {
            toast.success(`${bookTitle} added to cart!`, 3000, 'Cart Updated');
        } else {
            alert('Book added to cart!');
        }
    } catch (error) {
        console.error('Add to cart error:', error);
        if (window.toast) {
            toast.error('Failed to add to cart: ' + error.message, 5000, 'Error');
        } else {
            alert('Failed to add to cart: ' + error.message);
        }
    }
}

// Filter and Sort Functions
function applyFilters() {
    filteredBooks = [...allBooks];
    
    // Apply price filter
    if (currentFilters.minPrice !== null && currentFilters.minPrice !== '') {
        filteredBooks = filteredBooks.filter(book => 
            book.sellingPrice >= parseFloat(currentFilters.minPrice)
        );
    }
    
    if (currentFilters.maxPrice !== null && currentFilters.maxPrice !== '') {
        filteredBooks = filteredBooks.filter(book => 
            book.sellingPrice <= parseFloat(currentFilters.maxPrice)
        );
    }
    
    // Apply condition filter
    if (currentFilters.condition && currentFilters.condition !== 'all') {
        filteredBooks = filteredBooks.filter(book => 
            book.condition.toLowerCase() === currentFilters.condition.toLowerCase()
        );
    }
    
    // Apply sorting
    sortBooks();
    
    displayBooks(filteredBooks);
}

function sortBooks() {
    filteredBooks.sort((a, b) => {
        let aVal, bVal;
        
        switch (currentFilters.sortBy) {
            case 'price':
                aVal = a.sellingPrice;
                bVal = b.sellingPrice;
                break;
            case 'title':
                aVal = (a.title || '').toLowerCase();
                bVal = (b.title || '').toLowerCase();
                break;
            case 'author':
                aVal = (a.author || '').toLowerCase();
                bVal = (b.author || '').toLowerCase();
                break;
            case 'stock':
                aVal = a.stockQuantity;
                bVal = b.stockQuantity;
                break;
            default:
                aVal = (a.title || '').toLowerCase();
                bVal = (b.title || '').toLowerCase();
        }
        
        if (aVal < bVal) return currentFilters.sortOrder === 'asc' ? -1 : 1;
        if (aVal > bVal) return currentFilters.sortOrder === 'asc' ? 1 : -1;
        return 0;
    });
}

function updateFilter(filterType, value) {
    currentFilters[filterType] = value;
    applyFilters();
    
    // Save filters to localStorage
    localStorage.setItem('bookFilters', JSON.stringify(currentFilters));
}

function clearAllFilters() {
    currentFilters = {
        minPrice: null,
        maxPrice: null,
        condition: null,
        sortBy: 'title',
        sortOrder: 'asc'
    };
    
    // Reset form inputs
    document.getElementById('filterMinPrice').value = '';
    document.getElementById('filterMaxPrice').value = '';
    document.getElementById('filterCondition').value = 'all';
    document.getElementById('sortBy').value = 'title';
    document.getElementById('sortOrder').value = 'asc';
    
    localStorage.removeItem('bookFilters');
    applyFilters();
    
    if (window.toast) {
        toast.info('All filters cleared', 3000);
    }
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Load books on page load
document.addEventListener('DOMContentLoaded', () => {
    // Load saved filters from localStorage
    const savedFilters = localStorage.getItem('bookFilters');
    if (savedFilters) {
        try {
            currentFilters = { ...currentFilters, ...JSON.parse(savedFilters) };
            // Apply saved filters to UI
            if (currentFilters.minPrice) document.getElementById('filterMinPrice').value = currentFilters.minPrice;
            if (currentFilters.maxPrice) document.getElementById('filterMaxPrice').value = currentFilters.maxPrice;
            if (currentFilters.condition) document.getElementById('filterCondition').value = currentFilters.condition;
            if (currentFilters.sortBy) document.getElementById('sortBy').value = currentFilters.sortBy;
            if (currentFilters.sortOrder) document.getElementById('sortOrder').value = currentFilters.sortOrder;
        } catch (e) {
            console.warn('Failed to load saved filters:', e);
        }
    }
    
    loadBooks();
});

