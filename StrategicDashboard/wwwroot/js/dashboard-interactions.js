// OneJax Dashboard Interactions
// Tab switching, filters, modal loading, and UI interactions.

const DASHBOARD_SCROLL_KEY = 'onejax-dashboard-scroll-y';
const DASHBOARD_GOAL_KEY = 'onejax-dashboard-goal';

function getActiveDashboardGoal() {
    const activeTab = document.querySelector('.nav-tabs-modern .nav-link.active');
    if (activeTab) {
        return activeTab.getAttribute('data-tab') || 'summary';
    }

    const visibleSection = getVisibleGoalSection();
    return visibleSection?.getAttribute('data-goal') || 'summary';
}

function persistDashboardViewState() {
    try {
        sessionStorage.setItem(DASHBOARD_SCROLL_KEY, String(window.scrollY || 0));
        sessionStorage.setItem(DASHBOARD_GOAL_KEY, getActiveDashboardGoal());
    } catch (_error) {
        // Ignore storage issues and allow normal navigation.
    }
}

function buildDashboardFilterUrl(fiscalYearValue) {
    const params = new URLSearchParams(window.location.search);
    const activeGoal = getActiveDashboardGoal();

    // Keep the query key even when blank so the server can distinguish
    // "All Years" from "no filter param provided" (which defaults to current FY).
    params.set('fiscalYear', fiscalYearValue || '');

    if (activeGoal && activeGoal !== 'summary') {
        params.set('goal', activeGoal.replaceAll('_', ' '));
    } else {
        params.delete('goal');
    }

    const query = params.toString();
    return query ? `${window.location.pathname}?${query}` : window.location.pathname;
}

function applyFilters() {
    const fiscalYearFilter = document.getElementById('fiscalYearFilter');
    const fiscalYearValue = fiscalYearFilter ? (fiscalYearFilter.value || '') : '';

    persistDashboardViewState();
    showNotification(
        'Applying Filters',
        fiscalYearValue ? `Filtering by Fiscal Year: ${fiscalYearValue}` : 'Showing all fiscal years',
        'info'
    );
    window.location.href = buildDashboardFilterUrl(fiscalYearValue);
}

function resetFilters() {
    const fiscalYearFilter = document.getElementById('fiscalYearFilter');
    const defaultFiscalYear = fiscalYearFilter?.dataset.defaultFiscalYear || '';
    if (fiscalYearFilter) {
        fiscalYearFilter.value = defaultFiscalYear;
    }

    persistDashboardViewState();
    showNotification(
        'Filters Reset',
        defaultFiscalYear
            ? `Fiscal year filter has been reset to FY ${defaultFiscalYear}.`
            : 'Fiscal year filter has been reset.',
        'info'
    );
    window.location.href = buildDashboardFilterUrl(defaultFiscalYear);
}

function showComingSoon(feature) {
    showNotification(feature + ' Coming Soon!', 'This feature is currently in development.', 'info');
}

function showNotification(_title, _message, _type) {
    // Notifications are intentionally disabled for this dashboard.
}

function initializeTabs() {
    const navContainer = document.querySelector('.nav-tabs-modern');
    const allTabs = document.querySelectorAll('.nav-tabs-modern .nav-link');
    const allTabItems = document.querySelectorAll('.nav-tabs-modern .nav-item');

    if (navContainer) {
        navContainer.style.display = 'flex';
        navContainer.style.visibility = 'visible';
        navContainer.style.opacity = '1';
        navContainer.style.flexWrap = 'wrap';
        navContainer.style.animation = 'none';
        navContainer.classList.add('animation-complete');
    }

    allTabItems.forEach((item) => {
        item.style.display = 'block';
        item.style.visibility = 'visible';
        item.style.opacity = '1';
        item.style.flexShrink = '0';
        item.style.animation = 'none';
    });

    allTabs.forEach((tab) => {
        tab.style.display = 'block';
        tab.style.visibility = 'visible';
        tab.style.opacity = '1';
        tab.style.animation = 'none';
    });

    const activeTab = document.querySelector('.nav-tabs-modern .nav-link.active');
    if (activeTab) {
        const goalName = activeTab.getAttribute('data-tab');
        const targetSection = document.querySelector(`.goal-section[data-goal="${goalName}"]`);
        if (targetSection) {
            targetSection.style.display = 'block';
            targetSection.style.opacity = '1';
        }
        return;
    }

    const summaryTab = document.querySelector('.nav-link[data-tab="summary"]');
    const summarySection = document.querySelector('.goal-section[data-goal="summary"]');
    if (summaryTab) {
        summaryTab.classList.add('active');
    }
    if (summarySection) {
        summarySection.style.display = 'block';
        summarySection.style.opacity = '1';
    }
}

function getVisibleGoalSection() {
    const sections = document.querySelectorAll('.goal-section');
    for (const section of sections) {
        if (window.getComputedStyle(section).display !== 'none') {
            return section;
        }
    }
    return null;
}

function switchTab(goalName, clickedTab, evt) {
    const activeEvent = evt || (typeof window !== 'undefined' ? window.event : null);
    if (activeEvent) {
        activeEvent.preventDefault();
        activeEvent.stopPropagation();
    }

    const allTabs = document.querySelectorAll('.nav-tabs-modern .nav-link');
    allTabs.forEach((tab) => {
        tab.classList.remove('active');
        tab.style.display = 'block';
        tab.style.visibility = 'visible';
        tab.style.opacity = '1';
    });

    const navContainer = document.querySelector('.nav-tabs-modern');
    if (navContainer) {
        navContainer.style.display = 'flex';
        navContainer.style.visibility = 'visible';
        navContainer.style.opacity = '1';
    }

    if (clickedTab) {
        clickedTab.classList.add('active');
    } else {
        const targetTab = document.querySelector(`.nav-link[data-tab="${goalName}"]`);
        if (targetTab) {
            targetTab.classList.add('active');
        }
    }

    const allSections = document.querySelectorAll('.goal-section');
    allSections.forEach((section) => {
        section.style.display = 'none';
        section.style.opacity = '0';
    });

    const targetSection = document.querySelector(`.goal-section[data-goal="${goalName}"]`);
    if (targetSection) {
        targetSection.style.display = 'block';
        setTimeout(() => {
            targetSection.style.opacity = '1';
            targetSection.style.transition = 'opacity 0.3s ease-in-out';
        }, 50);
        scheduleProgressBarInitialization(targetSection);
    }

    const params = new URLSearchParams(window.location.search);
    const fiscalYearFilter = document.getElementById('fiscalYearFilter');
    if (fiscalYearFilter) {
        params.set('fiscalYear', fiscalYearFilter.value || '');
    }

    if (goalName === 'summary') {
        params.delete('goal');
    } else {
        params.set('goal', goalName.replaceAll('_', ' '));
    }

    const query = params.toString();
    window.history.replaceState(null, '', query ? `${window.location.pathname}?${query}` : window.location.pathname);
}

function restoreDashboardViewState() {
    try {
        const savedGoal = sessionStorage.getItem(DASHBOARD_GOAL_KEY);
        const savedScroll = sessionStorage.getItem(DASHBOARD_SCROLL_KEY);

        if (savedGoal) {
            const activeTab = document.querySelector('.nav-tabs-modern .nav-link.active');
            const currentGoal = activeTab?.getAttribute('data-tab') || 'summary';
            if (savedGoal !== currentGoal) {
                const targetTab = document.querySelector(`.nav-link[data-tab="${savedGoal}"]`);
                if (targetTab) {
                    switchTab(savedGoal, targetTab);
                }
            }
        }

        window.requestAnimationFrame(() => {
            window.requestAnimationFrame(() => {
                if (savedScroll) {
                    window.scrollTo({ top: Math.max(parseInt(savedScroll, 10) || 0, 0), behavior: 'auto' });
                } else if (savedGoal && savedGoal !== 'summary') {
                    const section = document.querySelector(`.goal-section[data-goal="${savedGoal}"]`);
                    if (section) {
                        const targetTop = window.scrollY + section.getBoundingClientRect().top - 24;
                        window.scrollTo({ top: Math.max(targetTop, 0), behavior: 'auto' });
                    }
                }

                sessionStorage.removeItem(DASHBOARD_GOAL_KEY);
                sessionStorage.removeItem(DASHBOARD_SCROLL_KEY);
            });
        });
    } catch (_error) {
        // Ignore storage issues and allow normal page load behavior.
    }
}

function getDisplayProgressWidth(targetWidth, minVisiblePercent = 4) {
    const parsed = parseFloat(targetWidth);
    if (!isFinite(parsed) || parsed <= 0) return '0%';
    if (parsed < minVisiblePercent) return `${minVisiblePercent}%`;
    return `${Math.min(parsed, 100)}%`;
}

function scheduleProgressBarInitialization(container) {
    if (!container || typeof initializeProgressBars !== 'function') {
        return;
    }

    window.requestAnimationFrame(() => {
        window.requestAnimationFrame(() => {
            initializeProgressBars(container);
        });
    });
}

function initializeProgressBars(container) {
    const progressBars = container.querySelectorAll('.progress-bar');
    progressBars.forEach((bar) => {
        if (!bar.dataset.targetWidth) {
            bar.dataset.targetWidth = bar.style.width || '0%';
        }

        const targetWidth = bar.dataset.targetWidth;
        const displayWidth = getDisplayProgressWidth(targetWidth);
        const numericTarget = parseFloat(targetWidth);
        if (isFinite(numericTarget) ? (numericTarget > 0 ? numericTarget < 4 : false) : false) {
            bar.title = `${numericTarget.toFixed(1)}% progress`;
        }

        if (bar.dataset.progressInitialized === 'true') {
            bar.style.transition = 'width 0.35s cubic-bezier(0.4, 0, 0.2, 1)';
            bar.style.width = displayWidth;
            return;
        }

        if (bar.dataset.progressScheduled === 'true') {
            return;
        }

        bar.dataset.progressScheduled = 'true';
        bar.style.width = '0%';
        bar.style.transition = 'none';
        setTimeout(() => {
            bar.style.transition = 'width 0.9s cubic-bezier(0.4, 0, 0.2, 1)';
            bar.style.width = displayWidth;
            bar.dataset.progressInitialized = 'true';
            bar.dataset.progressScheduled = 'false';
        }, 80);
    });
}

function editMetric(metricId) {
    const isAuthenticated = !!window.dashboardConfig?.userAuthenticated;
    if (!isAuthenticated) {
        showNotification('Access Denied', 'Please log in as staff/admin to edit metrics', 'error');
        return;
    }

    const currentValue = prompt('Enter new value for this metric (Staff Only):');
    if (!(currentValue !== null ? currentValue !== '' : false)) {
        return;
    }

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
        .then((response) => response.json())
        .then((data) => {
            if (data.success) {
                location.reload();
                showNotification('Metric Updated', 'The metric value has been successfully updated', 'success');
                return;
            }

            if (data.message.includes('Authentication')) {
                showNotification('Access Denied', 'Please log in as staff/admin to edit metrics', 'error');
                return;
            }

            showNotification('Error', 'Failed to update metric: ' + data.message, 'error');
        })
        .catch((error) => {
            console.error('Error:', error);
            showNotification('Error', 'Failed to update metric. Please check your connection and authentication.', 'error');
        });
}

function initializeAnimations() {
    const visibleSection = getVisibleGoalSection();
    scheduleProgressBarInitialization(visibleSection);
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

function loadEventDetails(eventId) {
    const isAuthenticated = !!window.dashboardConfig?.userAuthenticated;
    if (!isAuthenticated) {
        return;
    }

    const modalLabel = document.getElementById('eventDetailsModalLabel');
    const modalContent = document.getElementById('eventDetailsContent');

    if (!modalLabel || !modalContent) {
        return;
    }

    modalLabel.textContent = 'Loading...';
    modalContent.innerHTML = '<div class="text-center p-4"><div class="spinner-border text-primary"></div><p class="mt-2">Loading event details...</p></div>';

    const editButton = document.getElementById('editEventButton');
    if (editButton) {
        editButton.href = window.dashboardConfig?.editEventsUrl || '/Public/Events';
    }

    fetch('/Events/Api/Details/' + eventId)
        .then((response) => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then((data) => {
            if (data.error) {
                throw new Error(data.error);
            }

            modalLabel.textContent = data.Title || 'Event Details';
            modalContent.innerHTML = `
                <div class="row g-4">
                    <div class="col-lg-8">
                        <div class="card" style="border:none;box-shadow:0 4px 20px rgba(0,0,0,0.08)">
                            <div class="card-body p-4">
                                <h5 style="color:var(--onejax-navy);margin-bottom:1.5rem">Event Information</h5>
                                <div class="row">
                                    <div class="col-md-6 mb-3">
                                        <h6 style="color:var(--onejax-text);font-weight:600">Title</h6>
                                        <p style="color:var(--onejax-text-muted);margin:0">${data.Title || 'Not specified'}</p>
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <h6 style="color:var(--onejax-text);font-weight:600">Type</h6>
                                        <p style="color:var(--onejax-text-muted);margin:0">${data.Type || 'Not specified'}</p>
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <h6 style="color:var(--onejax-text);font-weight:600">Status</h6>
                                        <p style="color:var(--onejax-text-muted);margin:0">${data.Status || 'Not specified'}</p>
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <h6 style="color:var(--onejax-text);font-weight:600">Date</h6>
                                        <p style="color:var(--onejax-text-muted);margin:0">${data.DueDate || 'Not specified'}</p>
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <h6 style="color:var(--onejax-text);font-weight:600">Location</h6>
                                        <p style="color:var(--onejax-text-muted);margin:0">${data.Location || 'Not specified'}</p>
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <h6 style="color:var(--onejax-text);font-weight:600">Strategic Goal</h6>
                                        <p style="color:var(--onejax-text-muted);margin:0">${data.GoalName || 'Not specified'}</p>
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <h6 style="color:var(--onejax-text);font-weight:600">Expected Attendees</h6>
                                        <p style="color:var(--onejax-text-muted);margin:0">${data.Attendees || 'Not specified'}</p>
                                    </div>
                                </div>
                                ${data.Description ? `
                                    <div class="mt-4">
                                        <h6 style="color:var(--onejax-text);font-weight:600">Description</h6>
                                        <p style="color:var(--onejax-text-muted);line-height:1.6">${data.Description}</p>
                                    </div>
                                ` : ''}
                                ${data.Notes ? `
                                    <div class="mt-4">
                                        <h6 style="color:var(--onejax-text);font-weight:600">Notes</h6>
                                        <p style="color:var(--onejax-text-muted);line-height:1.6">${data.Notes}</p>
                                    </div>
                                ` : ''}
                            </div>
                        </div>
                    </div>
                    <div class="col-lg-4">
                        ${data.SatisfactionScore ? `
                            <div class="card mb-4" style="border:none;box-shadow:0 4px 20px rgba(0,0,0,0.08)">
                                <div class="card-body text-center p-4">
                                    <h5 style="color:var(--onejax-navy);margin-bottom:1.5rem">Satisfaction Score</h5>
                                    <div style="font-size:2.5rem;font-weight:700;color:var(--onejax-green);margin-bottom:0.5rem">
                                        ${data.SatisfactionScore.toFixed(1)}
                                    </div>
                                    <small style="color:var(--onejax-text-muted)">Out of 5.0</small>
                                </div>
                            </div>
                        ` : ''}
                        <div class="card" style="border:none;box-shadow:0 4px 20px rgba(0,0,0,0.08)">
                            <div class="card-body p-4">
                                <h5 style="color:var(--onejax-navy);margin-bottom:1.5rem">Event Source</h5>
                                <p style="color:var(--onejax-text-muted);margin:0">
                                    ${data.Source === 'Database'
                                        ? '<i class="fas fa-database text-primary"></i> Database Event'
                                        : '<i class="fas fa-lightbulb text-warning"></i> Core Strategy Event'}
                                </p>
                                <small style="color:var(--onejax-text-muted);font-size:0.85rem">
                                    ${data.Source === 'Database'
                                        ? 'This event was added through the Events management system.'
                                        : 'This event was added through the Core Strategies tab.'}
                                </small>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        })
        .catch((error) => {
            console.error('Error loading event details:', error);
            modalLabel.textContent = 'Error Loading Event';
            modalContent.innerHTML = `
                <div class="alert alert-danger">
                    <i class="fas fa-exclamation-circle"></i>
                    <strong>Error loading event details.</strong><br>
                    ${error.message || 'Please try again later.'}
                    <br><br>
                    <a href="/Events/Details/${eventId}" class="btn btn-primary">
                        <i class="fas fa-external-link-alt"></i> View Full Details Page
                    </a>
                </div>
            `;
        });
}

function initializeManageLinks() {
    const canManageData = !!window.dashboardConfig?.canManageData;
    if (!canManageData) {
        return;
    }

    const manageLinks = document.querySelectorAll('a[href*="/Strategy/ViewEvents"], a[href*="Strategy/ViewEvents"]');
    manageLinks.forEach((link) => {
        link.removeAttribute('target');
        link.removeAttribute('rel');
        link.target = '_self';
        link.onclick = function (e) {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();
            window.location.href = '/Strategy/ViewEvents';
            return false;
        };
        link.setAttribute('onclick', 'event.preventDefault(); window.location.href="/Strategy/ViewEvents"; return false;');
    });
}

function injectDashboardAnimationStyles() {
    if (document.getElementById('dashboard-interactions-style')) {
        return;
    }

    const style = document.createElement('style');
    style.id = 'dashboard-interactions-style';
    style.textContent = `
        .fade-in {
            animation: fadeIn 0.6s ease-out forwards;
            opacity: 0;
        }

        .slide-in-left {
            animation: slideInLeft 0.8s ease-out forwards;
            opacity: 0;
        }

        .slide-in-right {
            animation: slideInRight 0.8s ease-out forwards;
            opacity: 0;
        }

        .pulse-animation {
            animation: pulse 2s ease-in-out infinite;
        }

        .nav-tabs-modern {
            opacity: 1 !important;
            visibility: visible !important;
            display: flex !important;
            animation: none !important;
        }

        .nav-tabs-modern.fade-in {
            opacity: 1 !important;
            animation: none !important;
        }

        .nav-tabs-modern .nav-item {
            opacity: 1 !important;
            visibility: visible !important;
            display: block !important;
            animation: none !important;
        }

        .nav-tabs-modern .nav-link {
            opacity: 1 !important;
            visibility: visible !important;
            display: block !important;
            animation: none !important;
        }

        .nav-tabs-modern,
        .nav-tabs-modern.fade-in,
        .nav-tabs-modern .nav-item,
        .nav-tabs-modern .nav-link {
            opacity: 1 !important;
            visibility: visible !important;
            display: flex !important;
        }

        .nav-tabs-modern .nav-item,
        .nav-tabs-modern .nav-link {
            display: block !important;
        }

        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(20px); }
            to { opacity: 1; transform: translateY(0); }
        }

        @keyframes slideInLeft {
            from { opacity: 0; transform: translateX(-50px); }
            to { opacity: 1; transform: translateX(0); }
        }

        @keyframes slideInRight {
            from { transform: translateX(100%); opacity: 0; }
            to { transform: translateX(0); opacity: 1; }
        }

        @keyframes slideOutRight {
            from { transform: translateX(0); opacity: 1; }
            to { transform: translateX(100%); opacity: 0; }
        }

        @keyframes pulse {
            0%, 100% { transform: scale(1); }
            50% { transform: scale(1.05); }
        }

        @keyframes ripple {
            to { transform: scale(4); opacity: 0; }
        }

        @keyframes fadeOut {
            from { opacity: 1; }
            to { opacity: 0; }
        }
    `;
    document.head.appendChild(style);
}

document.addEventListener('DOMContentLoaded', function () {
    injectDashboardAnimationStyles();
    initializeTabs();
    scheduleProgressBarInitialization(getVisibleGoalSection());

    if (typeof initializeDashboardCharts === 'function') {
        initializeDashboardCharts();
    }

    initializeAnimations();
    initializeManageLinks();

    const interactiveElements = document.querySelectorAll('.floating-card, .stat-card-modern, .metric-card');
    interactiveElements.forEach((element) => {
        element.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-8px) scale(1.02)';
            this.style.transition = 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)';
        });

        element.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0) scale(1)';
        });
    });

    const buttons = document.querySelectorAll('.btn-modern');
    buttons.forEach((button) => {
        button.addEventListener('click', function (e) {
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

    setInterval(() => {
        showDataRefresh();
    }, 60000);

    setTimeout(() => {
        initializeTabs();
    }, 100);

    restoreDashboardViewState();
});

window.addEventListener('load', function () {
    setTimeout(() => {
        initializeTabs();
        scheduleProgressBarInitialization(getVisibleGoalSection());
    }, 200);
});
