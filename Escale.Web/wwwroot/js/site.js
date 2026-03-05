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

        // Auto-refresh dashboard seamlessly when a sale is completed or data changes
        var isDashboard = window.location.pathname === '/' ||
            window.location.pathname.toLowerCase().indexOf('/dashboard') === 0;

        if (isDashboard && (changeType === 'sale_completed' || changeType === 'inventory_changed')) {
            refreshDashboardData();
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

    function refreshDashboardData() {
        var period = window.escaleDashboardPeriod || 'today';
        fetch('/Dashboard/Data?period=' + encodeURIComponent(period))
            .then(function (res) { return res.json(); })
            .then(function (data) {
                // Update stat cards with animation (PascalCase from server)
                animateValue('stat-stations', data.TotalStations);
                animateValue('stat-transactions', data.TransactionCount);
                animateValue('stat-sales', formatNumber(data.TodaysSales) + ' RWF');
                updateAlerts('stat-alerts', data.LowStockAlerts);

                // Update recent transactions table
                updateRecentTransactions(data.RecentTransactions || []);

                // Update top stations
                updateTopStations(data.TopStations || []);

                // Update charts
                updateCharts(data);

                showUpdateToast('Dashboard updated — new sale recorded.');
            })
            .catch(function (err) {
                console.warn('[Dashboard] Seamless refresh failed, falling back to reload:', err);
                location.reload();
            });
    }

    function animateValue(elementId, newValue) {
        var el = document.getElementById(elementId);
        if (!el) return;
        el.style.transition = 'opacity 0.3s';
        el.style.opacity = '0.3';
        setTimeout(function () {
            el.textContent = newValue;
            el.style.opacity = '1';
        }, 300);
    }

    function updateAlerts(elementId, count) {
        var el = document.getElementById(elementId);
        if (!el) return;
        el.style.transition = 'opacity 0.3s';
        el.style.opacity = '0.3';
        setTimeout(function () {
            el.textContent = count;
            el.className = 'h5 mb-0 font-weight-bold ' + (count > 0 ? 'text-danger' : 'text-gray-800');
            el.style.opacity = '1';
        }, 300);
    }

    function formatNumber(num) {
        return Math.round(num).toLocaleString('en-US');
    }

    function updateRecentTransactions(transactions) {
        var tbody = document.getElementById('recent-transactions-body');
        if (!tbody) return;
        tbody.style.transition = 'opacity 0.3s';
        tbody.style.opacity = '0.3';
        setTimeout(function () {
            var html = '';
            transactions.forEach(function (txn) {
                var time = new Date(txn.Time);
                var hh = String(time.getHours()).padStart(2, '0');
                var mm = String(time.getMinutes()).padStart(2, '0');
                html += '<tr>' +
                    '<td>' + escapeHtml(txn.TransactionId) + '</td>' +
                    '<td>' + escapeHtml(txn.FuelType) + '</td>' +
                    '<td>' + txn.Quantity.toFixed(1) + 'L</td>' +
                    '<td>' + formatNumber(txn.Total) + ' RWF</td>' +
                    '<td>' + hh + ':' + mm + '</td>' +
                    '</tr>';
            });
            tbody.innerHTML = html;
            tbody.style.opacity = '1';
        }, 300);
    }

    function updateTopStations(stations) {
        var container = document.getElementById('top-stations-container');
        if (!container) return;
        container.style.transition = 'opacity 0.3s';
        container.style.opacity = '0.3';
        setTimeout(function () {
            if (stations.length === 0) {
                container.innerHTML = '<div class="text-center text-muted py-4">' +
                    '<i class="fas fa-chart-bar fa-2x mb-2 d-block"></i>' +
                    'No station performance data available for this period.</div>';
            } else {
                var html = '<div class="table-responsive"><table class="table table-hover align-middle mb-0">' +
                    '<thead class="table-light"><tr>' +
                    '<th style="width: 60px;" class="text-center">Rank</th>' +
                    '<th>Station</th><th class="text-end">Total Sales</th>' +
                    '<th class="text-center">Transactions</th><th class="text-end">Volume (L)</th>' +
                    '<th style="width: 50px;"></th></tr></thead><tbody>';
                stations.forEach(function (s) {
                    var badgeClass = s.Rank === 1 ? 'bg-warning text-dark' : s.Rank === 2 ? 'bg-secondary' : s.Rank === 3 ? 'bg-primary' : 'bg-light text-dark';
                    html += '<tr>' +
                        '<td class="text-center"><span class="badge ' + badgeClass + ' rounded-circle d-inline-flex align-items-center justify-content-center" style="width: 30px; height: 30px; font-size: 0.8rem;">' + s.Rank + '</span></td>' +
                        '<td class="fw-semibold">' + escapeHtml(s.StationName) + '</td>' +
                        '<td class="text-end fw-bold text-primary">' + formatNumber(s.TotalSales) + ' RWF</td>' +
                        '<td class="text-center">' + s.TransactionCount + '</td>' +
                        '<td class="text-end">' + formatNumber(s.TotalLiters) + '</td>' +
                        '<td class="text-center"><a href="/Stations/Details/' + s.StationId + '" class="text-muted"><i class="fas fa-chevron-right"></i></a></td>' +
                        '</tr>';
                });
                html += '</tbody></table></div>';
                container.innerHTML = html;
            }
            container.style.opacity = '1';
        }, 300);
    }

    function updateCharts(data) {
        // Update sales trend chart (PascalCase from server)
        if (window.escaleSalesChart && data.SalesChart) {
            window.escaleSalesChart.data.labels = data.SalesChart.map(function (s) { return s.Date; });
            window.escaleSalesChart.data.datasets[0].data = data.SalesChart.map(function (s) { return s.Sales; });
            window.escaleSalesChart.update('none');
        }
        // Update fuel type chart
        if (window.escaleFuelChart && data.FuelTypeChart) {
            window.escaleFuelChart.data.labels = data.FuelTypeChart.map(function (f) { return f.FuelType; });
            window.escaleFuelChart.data.datasets[0].data = data.FuelTypeChart.map(function (f) { return f.Amount; });
            window.escaleFuelChart.update('none');
        }
    }

    function escapeHtml(str) {
        if (!str) return '';
        var div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
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
