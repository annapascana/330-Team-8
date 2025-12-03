// Toast Notification System
class ToastManager {
    constructor() {
        this.container = null;
        this.init();
    }

    init() {
        // Create toast container if it doesn't exist
        if (!document.getElementById('toastContainer')) {
            this.container = document.createElement('div');
            this.container.id = 'toastContainer';
            this.container.className = 'toast-container';
            document.body.appendChild(this.container);
        } else {
            this.container = document.getElementById('toastContainer');
        }
    }

    show(message, type = 'info', duration = 5000, title = null) {
        const toast = document.createElement('div');
        toast.className = `toast ${type}`;
        
        const iconMap = {
            success: '✓',
            error: '✕',
            info: 'ℹ',
            warning: '⚠'
        };

        const titleMap = {
            success: 'Success',
            error: 'Error',
            info: 'Info',
            warning: 'Warning'
        };

        toast.innerHTML = `
            <div class="toast-icon">${iconMap[type] || iconMap.info}</div>
            <div class="toast-content">
                ${title ? `<div class="toast-title">${title}</div>` : ''}
                <div class="toast-message">${message}</div>
            </div>
            <button class="toast-close" onclick="this.closest('.toast').remove()">×</button>
        `;

        this.container.appendChild(toast);

        // Auto remove after duration
        if (duration > 0) {
            setTimeout(() => {
                this.remove(toast);
            }, duration);
        }

        return toast;
    }

    remove(toast) {
        toast.classList.add('fade-out');
        setTimeout(() => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast);
            }
        }, 300);
    }

    success(message, duration = 5000, title = null) {
        return this.show(message, 'success', duration, title);
    }

    error(message, duration = 7000, title = null) {
        return this.show(message, 'error', duration, title);
    }

    info(message, duration = 5000, title = null) {
        return this.show(message, 'info', duration, title);
    }

    warning(message, duration = 6000, title = null) {
        return this.show(message, 'warning', duration, title);
    }
}

// Create global toast instance
const toast = new ToastManager();

// Export for use in other scripts
window.toast = toast;

