// Orders page functionality
async function loadOrders() {
    const container = document.getElementById('ordersContent');
    container.innerHTML = '<div class="loading">Loading orders...</div>';
    
    const user = getCurrentUser();
    if (!user) {
        container.innerHTML = '<div class="error">Please log in to view your orders</div>';
        return;
    }
    
    try {
        const orders = await ordersAPI.getByUserId(user.userID);
        displayOrders(orders);
    } catch (error) {
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
        alert('Order cancelled successfully');
        loadOrders();
    } catch (error) {
        alert('Failed to cancel order: ' + error.message);
    }
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

document.addEventListener('DOMContentLoaded', loadOrders);

