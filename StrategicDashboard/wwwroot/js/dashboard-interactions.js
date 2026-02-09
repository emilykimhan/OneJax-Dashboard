// OneJax Dashboard Interactions
// Tab switching, filters, notifications, and UI interactions

// Apply Filters Function (No Page Refresh)
function applyFilters() {
    const statusFilter = document.getElementById('statusFilter').value;
    const timeFilter = document.getElementById('timeFilter').value;
    const fiscalYearFilter = document.getElementById('fiscalYearFilter').value;
    const quarterFilter = document.getElementById('quarterFilter').value;
    
    // Build query parameters for URL
    const params = new URLSearchParams();
    if (statusFilter) params.set('status', statusFilter);
    if (timeFilter) params.set('time', timeFilter);
    if (fiscalYearFilter) params.set('fiscalYear', fiscalYearFilter);
    if (quarterFilter) params.set('quarter', quarterFilter);
    
    // Redirect to apply server-side filtering
    const newUrl = params.toString() ? 
        `${window.location.pathname}?${params.toString()}` : 
        window.location.pathname;
    
    // Show loading notification
    let filterText = [];
    if (statusFilter) filterText.push(`Status: ${statusFilter}`);
    if (fiscalYearFilter) filterText.push(`Fiscal Year: ${fiscalYearFilter}`);
    if (quarterFilter) filterText.push(`Quarter: ${quarterFilter}`);
    if (timeFilter) filterText.push(`Legacy Time: ${timeFilter}`);
    
    const filterDescription = filterText.length > 0 ? filterText.join(', ') : 'All items';
    showNotification('Applying Filters', `Filtering by: ${filterDescription}`, 'info');
    
    // Navigate to the new URL with filters
    window.location.href = newUrl;
}

// Reset Filters Function
function resetFilters() {
    // Reset the select elements to their default values
    const statusFilter = document.getElementById('statusFilter');
    const timeFilter = document.getElementById('timeFilter');
    const fiscalYearFilter = document.getElementById('fiscalYearFilter');
    const quarterFilter = document.getElementById('quarterFilter');
    
    if (statusFilter) statusFilter.value = '';
    if (timeFilter) timeFilter.value = '';
    if (fiscalYearFilter) fiscalYearFilter.value = '';
    if (quarterFilter) quarterFilter.value = '';
    
    // Show notification and redirect to clear URL
    showNotification('Filters Reset', 'All filters have been cleared. Showing all projects.', 'info');
    
    // Navigate to the base URL without any filters
    window.location.href = window.location.pathname;
}

// Enhanced Notification System
function showNotification(title, message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} floating-notification`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 350px;
        border-radius: 15px;
        box-shadow: var(--shadow-strong);
        backdrop-filter: blur(10px);
        border: none;
        animation: slideInRight 0.5s ease-out;
    `;
    notification.innerHTML = `
        <div style="display: flex; align-items: center; gap: 12px;">
            <div style="font-weight: 700; color: var(--onejax-navy);">${title}</div>
            <button type="button" class="btn-close" onclick="this.parentElement.parentElement.remove()"></button>
        </div>
        <div style="margin-top: 8px; color: var(--onejax-text-muted);">${message}</div>
    `;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        if (notification.parentNode) {
            notification.style.animation = 'slideOutRight 0.5s ease-in';
            setTimeout(() => notification.remove(), 500);
        }
    }, 4000);
}

// Tab Switching Function (No Page Refresh)
function switchTab(goalName, clickedTab) {
    // Prevent any default behavior
    if (event) {
        event.preventDefault();
        event.stopPropagation();
    }
    
    // Update active tab
    const allTabs = document.querySelectorAll('.nav-tabs-modern .nav-link');
    allTabs.forEach(tab => tab.classList.remove('active'));
    
    if (clickedTab) {
        clickedTab.classList.add('active');
    } else {
        // Find the tab by data attribute
        const targetTab = document.querySelector(`.nav-link[data-tab="${goalName}"]`);
        if (targetTab) {
            targetTab.classList.add('active');
        }
    }
    
    // Hide all goal sections
    const allSections = document.querySelectorAll('.goal-section');
    allSections.forEach(section => {
        section.style.display = 'none';
        section.style.opacity = '0';
    });
    
    // Show the selected section with animation
    const targetSection = document.querySelector(`.goal-section[data-goal="${goalName}"]`);
    if (targetSection) {
        targetSection.style.display = 'block';
        
        // Trigger fade-in animation
        setTimeout(() => {
            targetSection.style.opacity = '1';
            targetSection.style.transition = 'opacity 0.3s ease-in-out';
        }, 50);
        
        // Re-initialize progress bar animations for the new content
        if (typeof initializeProgressBars === 'function') {
            initializeProgressBars(targetSection);
        }
    }
    
    // Update URL without refresh (optional - for bookmarking)
    const newUrl = goalName === 'summary' ? 
        window.location.pathname : 
        `${window.location.pathname}?goal=${goalName.replace('_', ' ')}`;
    window.history.replaceState(null, '', newUrl);
}

// Initialize progress bars for a specific section
function initializeProgressBars(container) {
    const progressBars = container.querySelectorAll('.progress-bar');
    progressBars.forEach(bar => {
        const targetWidth = bar.style.width;
        bar.style.width = '0%';
        bar.style.transition = 'none';
        
        setTimeout(() => {
            bar.style.transition = 'width 1s cubic-bezier(0.4, 0, 0.2, 1)';
            bar.style.width = targetWidth;
        }, 100);
    });
}

// Quick Edit Metric Function - Staff/Admin Only
function editMetric(metricId) {
    // Check if user is authenticated (simple client-side check)
    var isAuthenticated = window.userAuthenticated || false;
    
    if (!isAuthenticated) {
        showNotification('Access Denied', 'Please log in as staff/admin to edit metrics', 'error');
        return;
    }
    
    // Create a modal or inline edit interface
    const currentValue = prompt('Enter new value for this metric (Staff Only):');
    
    if (currentValue !== null && currentValue !== '') {
        // Send AJAX request to update the metric
        fetch('/DashboardMetrics/QuickUpdate', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify({
                metricId: metricId,
                currentValue: parseFloat(currentValue) || 0
            })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Refresh the current tab to show updated values
                location.reload();
                showNotification('Metric Updated', 'The metric value has been successfully updated', 'success');
            } else {
                if (data.message.includes('Authentication')) {
                    showNotification('Access Denied', 'Please log in as staff/admin to edit metrics', 'error');
                } else {
                    showNotification('Error', 'Failed to update metric: ' + data.message, 'error');
                }
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showNotification('Error', 'Failed to update metric. Please check your connection and authentication.', 'error');
        });
    }
}

// Show Coming Soon Feature
function showComingSoon(feature) {
    showNotification(feature + ' Coming Soon!', 'This exciting feature is currently in development and will be available in the next update.', 'info');
}

// Enhanced Interactive Features
function initializeDashboardInteractions() {
    // Initialize charts first
    if (typeof initializeDashboardCharts === 'function') {
        initializeDashboardCharts();
    }
    
    // Initialize animations and interactions
    initializeAnimations();
    
    // Add enhanced hover effects
    const interactiveElements = document.querySelectorAll('.floating-card, .stat-card-modern, .metric-card');
    interactiveElements.forEach(element => {
        element.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-8px) scale(1.02)';
            this.style.transition = 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)';
        });
        
        element.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0) scale(1)';
        });
    });
    
    // Add click animations to buttons (excluding nav-links with onclick handlers)
    const buttons = document.querySelectorAll('.btn-modern');
    buttons.forEach(button => {
        button.addEventListener('click', function(e) {
            const ripple = document.createElement('span');
            const rect = this.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const x = e.clientX - rect.left - size / 2;
            const y = e.clientY - rect.top - size / 2;
            
            ripple.style.cssText = `
                position: absolute;
                width: ${size}px;
                height: ${size}px;
                left: ${x}px;
                top: ${y}px;
                background: rgba(255, 255, 255, 0.4);
                border-radius: 50%;
                transform: scale(0);
                animation: ripple 0.6s ease-out;
                pointer-events: none;
            `;
            
            this.style.position = 'relative';
            this.style.overflow = 'hidden';
            this.appendChild(ripple);
            
            setTimeout(() => ripple.remove(), 600);
        });
    });
    
    // Auto-refresh with visual indicator
    setInterval(() => {
        showDataRefresh();
    }, 60000); // Every minute
}

// Enhanced Progress Bar Animation
function initializeAnimations() {
    const progressBars = document.querySelectorAll('.progress-bar');
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const progressBar = entry.target;
                const targetWidth = progressBar.style.width;
                progressBar.style.width = '0%';
                
                setTimeout(() => {
                    progressBar.style.width = targetWidth;
                    progressBar.style.transition = 'width 2s cubic-bezier(0.4, 0, 0.2, 1)';
                }, 200);
                
                observer.unobserve(progressBar);
            }
        });
    }, observerOptions);
    
    progressBars.forEach(bar => observer.observe(bar));
}

function showDataRefresh() {
    const refreshIndicator = document.createElement('div');
    refreshIndicator.style.cssText = `
        position: fixed;
        bottom: 20px;
        right: 20px;
        background: var(--gradient-secondary);
        color: white;
        padding: 12px 20px;
        border-radius: 25px;
        font-size: 0.9rem;
        font-weight: 600;
        box-shadow: var(--shadow-medium);
        z-index: 9999;
        animation: fadeIn 0.5s ease-out;
    `;
    refreshIndicator.innerHTML = 'Data refreshed • ' + new Date().toLocaleTimeString();
    
    document.body.appendChild(refreshIndicator);
    
    setTimeout(() => {
        refreshIndicator.style.animation = 'fadeOut 0.5s ease-in';
        setTimeout(() => refreshIndicator.remove(), 500);
    }, 3000);
}

// Initialize everything when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initializeDashboardInteractions();
    
    // Welcome message
    setTimeout(() => {
        showNotification('Welcome to OneJax Dashboard', 'Explore your strategic initiatives with enhanced visuals and interactive features!', 'info');
    }, 1000);
});
