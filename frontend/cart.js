// Cart page functionality
async function loadCart() {
    const container = document.getElementById('cartContent');
    container.innerHTML = '<div class="loading">Loading cart...</div>';
    
    try {
        const cart = await cartAPI.get();
        displayCart(cart);
    } catch (error) {
        container.innerHTML = `<div class="error">Error loading cart: ${error.message}</div>`;
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
            <button onclick="checkout()" class="btn btn-primary btn-large" style="margin-top: 1rem;">Checkout</button>
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

async function checkout() {
    if (!confirm('Proceed to checkout?')) return;
    
    const user = getCurrentUser();
    if (!user) {
        alert('Please log in to checkout');
        return;
    }
    
    try {
        const order = await ordersAPI.checkout(user.userID);
        alert('Order placed successfully! Order ID: ' + order.poid);
        window.location.href = 'orders.html';
    } catch (error) {
        alert('Checkout failed: ' + error.message);
    }
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

document.addEventListener('DOMContentLoaded', loadCart);

