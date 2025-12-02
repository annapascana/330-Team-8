// Cart page functionality
function getSkeletonLoader(count = 3) {
    return `
        <div style="display: flex; flex-direction: column; gap: 1rem;">
            ${Array(count).fill(0).map(() => `
                <div class="skeleton-card">
                    <div class="skeleton-title"></div>
                    <div class="skeleton-text"></div>
                    <div class="skeleton-text short"></div>
                    <div class="skeleton-button" style="width: 100px;"></div>
                </div>
            `).join('')}
        </div>
    `;
}

async function loadCart() {
    const container = document.getElementById('cartContent');
    container.innerHTML = getSkeletonLoader(3);
    
    try {
        const cart = await cartAPI.get();
        if (!cart) {
            throw new Error('Cart data is null or undefined');
        }
        displayCart(cart);
    } catch (error) {
        console.error('Cart loading error:', error);
        let errorMessage = error.message;
        
        // Provide more helpful error messages
        if (error.message === 'Failed to fetch') {
            errorMessage = 'Unable to connect to server. Please ensure the backend server is running on http://localhost:5000';
        }
        
        container.innerHTML = `
            <div class="error">
                <strong>Error loading cart:</strong> ${errorMessage}
                <br><br>
                <small>Make sure you're accessing the site through http://localhost:5000 (not as a file)</small>
                <br>
                <button onclick="loadCart()" class="btn btn-primary btn-small" style="margin-top: 1rem;">Retry</button>
            </div>
        `;
    }
}

function displayCart(cart) {
    const container = document.getElementById('cartContent');
    
    if (cart.items.length === 0) {
        container.innerHTML = '<div class="error">Your cart is empty</div>';
        return;
    }
    
    let html = '<div class="cart-items">';
    
    cart.items.forEach(item => {
        html += `
            <div class="cart-item">
                <div>
                    <h3>${escapeHtml(item.title)}</h3>
                    <p>Author: ${escapeHtml(item.author)}</p>
                    <p>Price: $${item.unitPrice.toFixed(2)}</p>
                    <p>Available: ${item.availableStock}</p>
                </div>
                <div>
                    <label>Quantity:</label>
                    <input type="number" id="qty_${item.bookID}" 
                           value="${item.quantity}" 
                           min="1" 
                           max="${item.availableStock}"
                           class="quantity-input"
                           onchange="updateQuantity(${item.bookID}, this.value)">
                    <p><strong>Total: $${item.lineTotal.toFixed(2)}</strong></p>
                    <button onclick="removeItem(${item.bookID})" class="btn btn-danger btn-small">Remove</button>
                </div>
            </div>
        `;
    });
    
    html += '</div>';
    
    html += `
        <div class="cart-summary">
            <h3>Order Summary</h3>
            <p>Subtotal: $${cart.subTotal.toFixed(2)}</p>
            <p>Tax: $${cart.tax.toFixed(2)}</p>
            <p style="font-size: 1.5rem; font-weight: bold;">Total: $${cart.total.toFixed(2)}</p>
            <button onclick="checkout()" class="btn btn-primary btn-large" style="margin-top: 1rem;">Proceed to Checkout</button>
        </div>
    `;
    
    container.innerHTML = html;
}

async function updateQuantity(bookId, quantity) {
    const qty = parseInt(quantity);
    if (qty <= 0) {
        await removeItem(bookId);
        return;
    }
    
    try {
        await cartAPI.update(bookId, qty);
        loadCart();
    } catch (error) {
        alert('Failed to update quantity: ' + error.message);
        loadCart();
    }
}

async function removeItem(bookId) {
    if (!confirm('Remove this item from cart?')) return;
    
    try {
        await cartAPI.remove(bookId);
        loadCart();
    } catch (error) {
        alert('Failed to remove item: ' + error.message);
    }
}

function showPaymentModal() {
    document.getElementById('paymentModal').style.display = 'block';
    document.getElementById('paymentForm').reset();
    document.getElementById('paymentError').style.display = 'none';
    // Focus first input
    setTimeout(() => document.getElementById('cardholderName').focus(), 100);
}

function closePaymentModal() {
    document.getElementById('paymentModal').style.display = 'none';
    document.getElementById('paymentForm').reset();
    document.getElementById('paymentError').style.display = 'none';
}

function formatCardNumber(input) {
    let value = input.value.replace(/\s/g, '').replace(/[^0-9]/g, '');
    let formattedValue = value.match(/.{1,4}/g)?.join(' ') || value;
    input.value = formattedValue;
}

function formatExpiryDate(input) {
    let value = input.value.replace(/\D/g, '');
    if (value.length >= 2) {
        value = value.substring(0, 2) + '/' + value.substring(2, 4);
    }
    input.value = value;
}

function validateCardNumber(cardNumber) {
    // Remove spaces
    const cleaned = cardNumber.replace(/\s/g, '');
    // Check if it's all digits and has valid length
    if (!/^\d{13,19}$/.test(cleaned)) {
        return false;
    }
    // Luhn algorithm validation
    let sum = 0;
    let isEven = false;
    for (let i = cleaned.length - 1; i >= 0; i--) {
        let digit = parseInt(cleaned[i]);
        if (isEven) {
            digit *= 2;
            if (digit > 9) digit -= 9;
        }
        sum += digit;
        isEven = !isEven;
    }
    return sum % 10 === 0;
}

function validateExpiryDate(expiry) {
    const parts = expiry.split('/');
    if (parts.length !== 2) return false;
    const month = parseInt(parts[0]);
    const year = parseInt('20' + parts[1]);
    if (month < 1 || month > 12) return false;
    const now = new Date();
    const expiryDate = new Date(year, month - 1);
    return expiryDate >= now;
}

async function checkout() {
    const user = getCurrentUser();
    if (!user) {
        alert('Please log in to checkout');
        window.location.href = 'index.html';
        return;
    }
    
    // Show payment modal instead of direct checkout
    showPaymentModal();
}

async function processPayment() {
    const user = getCurrentUser();
    if (!user) {
        alert('Please log in to checkout');
        return;
    }
    
    // Get payment form values
    const cardholderName = document.getElementById('cardholderName').value.trim();
    const cardNumber = document.getElementById('cardNumber').value.trim();
    const expiryDate = document.getElementById('expiryDate').value.trim();
    const cvv = document.getElementById('cvv').value.trim();
    const billingAddress = document.getElementById('billingAddress').value.trim();
    const billingCity = document.getElementById('billingCity').value.trim();
    const billingState = document.getElementById('billingState').value.trim().toUpperCase();
    const billingZip = document.getElementById('billingZip').value.trim();
    
    const errorDiv = document.getElementById('paymentError');
    errorDiv.style.display = 'none';
    
    // Validate payment information
    if (!cardholderName) {
        errorDiv.textContent = 'Please enter cardholder name';
        errorDiv.style.display = 'block';
        return;
    }
    
    if (!validateCardNumber(cardNumber)) {
        errorDiv.textContent = 'Please enter a valid card number';
        errorDiv.style.display = 'block';
        return;
    }
    
    if (!validateExpiryDate(expiryDate)) {
        errorDiv.textContent = 'Please enter a valid expiration date (MM/YY)';
        errorDiv.style.display = 'block';
        return;
    }
    
    if (!cvv || cvv.length < 3) {
        errorDiv.textContent = 'Please enter a valid CVV';
        errorDiv.style.display = 'block';
        return;
    }
    
    if (!billingAddress || !billingCity || !billingState || !billingZip) {
        errorDiv.textContent = 'Please complete all billing address fields';
        errorDiv.style.display = 'block';
        return;
    }
    
    // Disable form during processing
    const submitBtn = document.querySelector('#paymentForm button[type="submit"]');
    const originalText = submitBtn.textContent;
    submitBtn.disabled = true;
    submitBtn.textContent = 'Processing...';
    
    try {
        // In a real application, you would send payment info to a payment processor here
        // For now, we'll just validate and proceed with order creation
        // Note: Never store full card numbers - use tokenization in production
        
        const order = await ordersAPI.checkout(user.userID);
        if (!order || !order.poid) {
            throw new Error('Order was created but no order ID was returned');
        }
        
        // Store order ID in sessionStorage for orders page
        sessionStorage.setItem('lastOrderId', order.poid.toString());
        
        closePaymentModal();
        if (window.toast) {
            toast.success(`Order placed successfully! Order ID: ${order.poid}`, 5000, 'Order Confirmed');
        } else {
            alert('Payment processed and order placed successfully! Order ID: ' + order.poid);
        }
        
        // Small delay to ensure order is fully committed, then redirect
        setTimeout(() => {
            window.location.href = 'orders.html';
        }, 500);
    } catch (error) {
        console.error('Checkout error:', error);
        errorDiv.textContent = 'Payment processing failed: ' + error.message;
        errorDiv.style.display = 'block';
        submitBtn.disabled = false;
        submitBtn.textContent = originalText;
    }
}

// Setup payment form submission
document.addEventListener('DOMContentLoaded', () => {
    const paymentForm = document.getElementById('paymentForm');
    if (paymentForm) {
        paymentForm.addEventListener('submit', (e) => {
            e.preventDefault();
            processPayment();
        });
    }
    
    // Close payment modal on outside click
    const paymentModal = document.getElementById('paymentModal');
    if (paymentModal) {
        window.addEventListener('click', (e) => {
            if (e.target === paymentModal) {
                closePaymentModal();
            }
        });
    }
});

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Listen for storage changes (when user logs out in another tab/window)
window.addEventListener('storage', (e) => {
    if (e.key === 'currentUser' && e.newValue === null) {
        // User logged out, reload cart
        loadCart();
    }
});

// Check if user is logged in when page loads
document.addEventListener('DOMContentLoaded', () => {
    const user = getCurrentUser();
    if (!user) {
        // No user logged in, ensure cart is empty
        const container = document.getElementById('cartContent');
        container.innerHTML = '<div class="error">Please log in to view your cart</div>';
    } else {
        loadCart();
    }
});

