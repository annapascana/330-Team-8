// Orders page functionality
function getSkeletonLoader(count = 3) {
    return `
        <div style="display: flex; flex-direction: column; gap: 1rem;">
            ${Array(count).fill(0).map(() => `
                <div class="skeleton-card">
                    <div class="skeleton-title"></div>
                    <div class="skeleton-text"></div>
                    <div class="skeleton-text short"></div>
                    <div class="skeleton-text"></div>
                </div>
            `).join('')}
        </div>
    `;
}

async function loadOrders() {
    const container = document.getElementById('ordersContent');
    container.innerHTML = getSkeletonLoader(3);
    
    const user = getCurrentUser();
    if (!user) {
        container.innerHTML = '<div class="error">Please log in to view your orders</div>';
        return;
    }
    
    try {
        const orders = await ordersAPI.getByUserId(user.userID);
        
        // Check if we just placed an order
        const lastOrderId = sessionStorage.getItem('lastOrderId');
        if (lastOrderId) {
            sessionStorage.removeItem('lastOrderId');
            // Verify the order is in the list
            const orderFound = orders.some(o => o.poid.toString() === lastOrderId);
            if (!orderFound) {
                console.warn('Order ' + lastOrderId + ' not found in orders list, retrying...');
                // Retry after a short delay
                setTimeout(loadOrders, 1000);
                return;
            }
        }
        
        displayOrders(orders);
    } catch (error) {
        console.error('Error loading orders:', error);
        container.innerHTML = `<div class="error">Error loading orders: ${error.message}</div>`;
    }
}

function displayOrders(orders) {
    const container = document.getElementById('ordersContent');
    
    if (orders.length === 0) {
        container.innerHTML = '<div class="error">No orders found</div>';
        return;
    }
    
    let html = '';
    
    orders.forEach(order => {
        const statusClass = `status-${order.status.toLowerCase()}`;
        html += `
            <div class="order-card">
                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem;">
                    <h3>Order #${order.poid}</h3>
                    <span class="order-status ${statusClass}">${order.status}</span>
                </div>
                <p><strong>Order Date:</strong> ${new Date(order.orderDate).toLocaleDateString()}</p>
                <p><strong>Total:</strong> $${order.total.toFixed(2)}</p>
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
                ${order.status !== 'Cancelled' && order.status !== 'Completed' ? 
                    `<button onclick="cancelOrder(${order.poid})" class="btn btn-danger btn-small" style="margin-top: 1rem;">Cancel Order</button>` 
                    : ''}
            </div>
        `;
    });
    
    container.innerHTML = html;
}

async function cancelOrder(orderId) {
    if (!confirm('Cancel this order? Stock will be restored.')) return;
    
    try {
        await ordersAPI.cancel(orderId);
        if (window.toast) {
            toast.success('Order cancelled successfully. Stock has been restored.', 5000, 'Order Cancelled');
        } else {
            alert('Order cancelled successfully');
        }
        loadOrders();
    } catch (error) {
        if (window.toast) {
            toast.error('Failed to cancel order: ' + error.message, 5000, 'Error');
        } else {
            alert('Failed to cancel order: ' + error.message);
        }
    }
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Listen for storage changes (when user logs out)
window.addEventListener('storage', (e) => {
    if (e.key === 'currentUser' && e.newValue === null) {
        // User logged out, clear orders display
        const container = document.getElementById('ordersContent');
        container.innerHTML = '<div class="error">Please log in to view your orders</div>';
    }
});

document.addEventListener('DOMContentLoaded', loadOrders);

