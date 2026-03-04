// ============================================================
// SignalR Real-Time Notifications
// ============================================================

(function () {
    if (typeof escaleConfig === 'undefined' || !escaleConfig.isAuthenticated || !escaleConfig.accessToken) return;
    if (typeof signalR === 'undefined') return;

    var connection = new signalR.HubConnectionBuilder()
        .withUrl(escaleConfig.hubUrl, { accessTokenFactory: function () { return escaleConfig.accessToken; } })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    connection.on('DataChanged', function (changeType) {
        console.log('[SignalR] DataChanged:', changeType);

        // Auto-refresh dashboard when a sale is completed or data changes
        var isDashboard = window.location.pathname === '/' ||
            window.location.pathname.toLowerCase().indexOf('/dashboard') === 0;

        if (isDashboard && (changeType === 'sale_completed' || changeType === 'inventory_changed')) {
            // Show a brief toast then reload
            showUpdateToast('Dashboard updated — new sale recorded.');
            setTimeout(function () { location.reload(); }, 1500);
        }

        // Refresh other pages on relevant changes
        var isTransactions = window.location.pathname.toLowerCase().indexOf('/transactions') === 0;
        if (isTransactions && changeType === 'sale_completed') {
            showUpdateToast('New transaction recorded.');
            setTimeout(function () { location.reload(); }, 1500);
        }

        var isInventory = window.location.pathname.toLowerCase().indexOf('/inventory') === 0;
        if (isInventory && changeType === 'inventory_changed') {
            showUpdateToast('Inventory updated.');
            setTimeout(function () { location.reload(); }, 1500);
        }
    });

    connection.start().catch(function (err) {
        console.warn('[SignalR] Connection failed:', err);
    });

    function showUpdateToast(message) {
        // Create a simple Bootstrap toast for notifications
        var toastHtml = '<div class="position-fixed bottom-0 end-0 p-3" style="z-index: 9999;">' +
            '<div class="toast show align-items-center text-bg-primary border-0" role="alert">' +
            '<div class="d-flex"><div class="toast-body">' +
            '<i class="fas fa-sync-alt me-2"></i>' + message +
            '</div><button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>' +
            '</div></div></div>';
        var container = document.createElement('div');
        container.innerHTML = toastHtml;
        document.body.appendChild(container);
        setTimeout(function () { container.remove(); }, 3000);
    }
})();

// ============================================================
// Global Loading State Utilities
// ============================================================

/**
 * Show loading spinner on a button and disable it.
 * Saves original content for later restoration.
 */
function startLoading(btn, text) {
    if (!btn || btn.dataset.loading === 'true') return;
    btn.dataset.loading = 'true';
    btn.dataset.originalHtml = btn.innerHTML;
    btn.disabled = true;
    var loadingText = text || btn.dataset.loadingText || 'Processing...';
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>' + loadingText;
}

/**
 * Restore a button to its original state after loading completes.
 */
function stopLoading(btn) {
    if (!btn) return;
    btn.dataset.loading = 'false';
    btn.disabled = false;
    if (btn.dataset.originalHtml) {
        btn.innerHTML = btn.dataset.originalHtml;
    }
}

/**
 * Auto-attach loading state to all forms with [data-loading-form].
 * The submit button inside the form will show a spinner on submit.
 */
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('form[data-loading-form]').forEach(function (form) {
        form.addEventListener('submit', function () {
            var btn = form.querySelector('button[type="submit"], input[type="submit"]');
            if (btn) startLoading(btn);
        });
    });
});

/**
 * Helper: wrap a $.post or $.get AJAX call with loading state on a button.
 * Usage: ajaxWithLoading(btnElement, function() { return $.post(...); });
 */
function ajaxWithLoading(btn, ajaxFn, loadingText) {
    startLoading(btn, loadingText);
    var result = ajaxFn();
    if (result && typeof result.always === 'function') {
        result.always(function () { stopLoading(btn); });
    } else if (result && typeof result.finally === 'function') {
        result.finally(function () { stopLoading(btn); });
    }
    return result;
}
