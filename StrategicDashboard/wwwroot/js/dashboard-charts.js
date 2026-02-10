// OneJax Dashboard Charts
// Chart.js initialization and configuration

// OneJax Brand Colors
const OneJaxColors = {
    orange: '#f5a045',
    blue: '#4bb4c8', 
    gold: '#bfa871',
    darkBlue: '#278aa2',
    green: '#67ab8c',
    navy: '#096682',
    darkGreen: '#005f47',
    white: '#ffffff'
};

// OneJax Color Palettes for Charts
const OneJaxPalettes = {
    primary: [OneJaxColors.navy, OneJaxColors.orange, OneJaxColors.blue, OneJaxColors.green],
    secondary: [OneJaxColors.darkBlue, OneJaxColors.gold, OneJaxColors.darkGreen, OneJaxColors.orange, OneJaxColors.blue],
    warm: [OneJaxColors.orange, OneJaxColors.gold, OneJaxColors.navy, OneJaxColors.darkBlue],
    cool: [OneJaxColors.blue, OneJaxColors.darkBlue, OneJaxColors.green, OneJaxColors.darkGreen],
    earth: [OneJaxColors.green, OneJaxColors.darkGreen, OneJaxColors.gold, OneJaxColors.orange]
};

// Dashboard Charts Setup
function initializeDashboardCharts() {
    // Goal Progress Chart (Doughnut)
    const progressCtx = document.getElementById('progressChart');
    if (progressCtx) {
        const progressChart = new Chart(progressCtx, {
            type: 'doughnut',
            data: {
                labels: ['Organizational Building', 'Financial Sustainability', 'Identity/Value Prop', 'Community Engagement'],
                datasets: [{
                    data: window.chartData.goalProgress || [0, 0, 0, 0],
                    backgroundColor: OneJaxPalettes.primary,
                    borderWidth: 3,
                    borderColor: OneJaxColors.white,
                    hoverBackgroundColor: [
                        OneJaxColors.darkBlue,
                        OneJaxColors.gold, 
                        OneJaxColors.darkBlue,
                        OneJaxColors.darkGreen
                    ]
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 15,
                            usePointStyle: true
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return context.label + ': ' + Math.round(context.raw) + '% complete';
                            }
                        }
                    }
                }
            }
        });
    }

    // Activity Summary Chart (Bar)
    const activityCtx = document.getElementById('activityChart');
    if (activityCtx) {
        const activityChart = new Chart(activityCtx, {
            type: 'bar',
            data: {
                labels: ['Staff Surveys', 'Prof. Development', 'Media Placements', 'Website Traffic', 'Events'],
                datasets: [{
                    label: 'Count',
                    data: window.chartData.activitySummary || [0, 0, 0, 0, 0],
                    backgroundColor: OneJaxPalettes.secondary,
                    borderColor: OneJaxPalettes.secondary.map(color => color),
                    borderWidth: 2,
                    borderRadius: 8,
                    borderSkipped: false,
                    hoverBackgroundColor: [
                        OneJaxColors.orange,
                        OneJaxColors.navy,
                        OneJaxColors.green,
                        OneJaxColors.gold,
                        OneJaxColors.darkBlue
                    ]
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    }
                }
            }
        });
    }
    
    // Monthly Trends Chart (Line)
    const trendsCtx = document.getElementById('trendsChart');
    if (trendsCtx) {
        const trendsChart = new Chart(trendsCtx, {
            type: 'line',
            data: {
                labels: window.chartData.monthlyTrends?.labels || [],
                datasets: [
                    {
                        label: 'Organizational Building',
                        data: window.chartData.monthlyTrends?.organizational || [],
                        borderColor: OneJaxColors.navy,
                        backgroundColor: OneJaxColors.navy + '20',
                        tension: 0.4,
                        fill: false
                    },
                    {
                        label: 'Community Engagement',
                        data: window.chartData.monthlyTrends?.community || [],
                        borderColor: OneJaxColors.green,
                        backgroundColor: OneJaxColors.green + '20',
                        tension: 0.4,
                        fill: false
                    },
                    {
                        label: 'Financial Sustainability',
                        data: window.chartData.monthlyTrends?.financial || [],
                        borderColor: OneJaxColors.orange,
                        backgroundColor: OneJaxColors.orange + '20',
                        tension: 0.4,
                        fill: false
                    },
                    {
                        label: 'Identity/Value Prop',
                        data: window.chartData.monthlyTrends?.identity || [],
                        borderColor: OneJaxColors.blue,
                        backgroundColor: OneJaxColors.blue + '20',
                        tension: 0.4,
                        fill: false
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top'
                    },
                    tooltip: {
                        mode: 'index',
                        intersect: false,
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true
                    }
                },
                interaction: {
                    mode: 'nearest',
                    axis: 'x',
                    intersect: false
                }
            }
        });
    }
    
    // Metrics Type Distribution Chart (Pie)
    const metricsTypeCtx = document.getElementById('metricsTypeChart');
    if (metricsTypeCtx) {
        const metricsTypeChart = new Chart(metricsTypeCtx, {
            type: 'pie',
            data: {
                labels: window.chartData.metricTypes?.labels || [],
                datasets: [{
                    data: window.chartData.metricTypes?.values || [],
                    backgroundColor: [OneJaxColors.navy, OneJaxColors.orange, OneJaxColors.green, OneJaxColors.gold, OneJaxColors.blue, OneJaxColors.darkBlue],
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 10,
                            usePointStyle: true
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return context.label + ': ' + context.raw + ' metrics';
                            }
                        }
                    }
                }
            }
        });
    }
    
    // Goal Performance Radar Chart
    const radarCtx = document.getElementById('radarChart');
    if (radarCtx) {
        const radarChart = new Chart(radarCtx, {
            type: 'radar',
            data: {
                labels: ['Organizational', 'Financial', 'Identity/Value', 'Community', 'Staff Satisfaction', 'Public Engagement'],
                datasets: [{
                    label: 'Current Performance',
                    data: window.chartData.radarCurrent || [0, 0, 0, 0, 0, 0],
                    borderColor: OneJaxColors.blue,
                    backgroundColor: OneJaxColors.blue + '33',
                    borderWidth: 2,
                    pointBackgroundColor: OneJaxColors.blue,
                    pointBorderColor: OneJaxColors.white,
                    pointHoverBackgroundColor: OneJaxColors.white,
                    pointHoverBorderColor: OneJaxColors.blue
                }, {
                    label: 'Target Performance',
                    data: [85, 80, 75, 90, 85, 70],
                    borderColor: OneJaxColors.green,
                    backgroundColor: OneJaxColors.green + '1A',
                    borderWidth: 2,
                    borderDash: [5, 5],
                    pointBackgroundColor: OneJaxColors.green,
                    pointBorderColor: OneJaxColors.white
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top'
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return context.dataset.label + ': ' + Math.round(context.raw) + '%';
                            }
                        }
                    }
                },
                scales: {
                    r: {
                        beginAtZero: true,
                        max: 100
                    }
                }
            }
        });
    }
    
    // Financial Metrics Area Chart
    const financialCtx = document.getElementById('financialChart');
    if (financialCtx) {
        const financialChart = new Chart(financialCtx, {
            type: 'line',
            data: {
                labels: ['Q1', 'Q2', 'Q3', 'Q4'],
                datasets: [{
                    label: 'Revenue ($)',
                    data: [25000, 35000, 42000, 38000],
                    borderColor: OneJaxColors.green,
                    backgroundColor: OneJaxColors.green + '4D',
                    fill: true,
                    tension: 0.4
                }, {
                    label: 'Expenses ($)',
                    data: [20000, 28000, 32000, 30000],
                    borderColor: OneJaxColors.orange,
                    backgroundColor: OneJaxColors.orange + '4D',
                    fill: true,
                    tension: 0.4
                }, {
                    label: 'Net Income ($)',
                    data: [5000, 7000, 10000, 8000],
                    borderColor: OneJaxColors.gold,
                    backgroundColor: OneJaxColors.gold + '4D',
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function(value) {
                                return '$' + value.toLocaleString();
                            }
                        }
                    }
                }
            }
        });
    }
    
    // Quarterly Performance Chart (Horizontal Bar)
    const quarterlyCtx = document.getElementById('quarterlyChart');
    if (quarterlyCtx) {
        const quarterlyChart = new Chart(quarterlyCtx, {
            type: 'bar',
            data: {
                labels: window.chartData.quarterly?.labels || [],
                datasets: [
                    {
                        label: 'Website Traffic (Primary Metric)',
                        data: window.chartData.quarterly?.values || [],
                        backgroundColor: OneJaxPalettes.primary,
                        borderWidth: 1,
                        borderColor: '#fff'
                    }
                ]
            },
            plugins: [ChartDataLabels],
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: 'top'
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return context.dataset.label + ': ' + context.raw.toLocaleString() + ' clicks';
                            }
                        }
                    },
                    datalabels: {
                        anchor: 'end',
                        align: 'top',
                        offset: 5,
                        formatter: function(value, context) {
                            return value.toLocaleString() + ' clicks';
                        },
                        color: '#1f2937',
                        font: {
                            weight: 'bold',
                            size: 12
                        },
                        backgroundColor: 'rgba(255, 255, 255, 0.9)',
                        borderColor: '#d1d5db',
                        borderWidth: 1,
                        borderRadius: 4,
                        padding: 4
                    }
                },
                scales: {
                    x: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Fiscal Year Quarters'
                        }
                    },
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Website Traffic (Clicks)'
                        }
                    }
                }
            }
        });
    }
}

// Export Dashboard Function
function exportDashboard() {
    const dashboardData = {
        summary: window.summaryData || {},
        exportDate: new Date().toISOString(),
        source: 'OneJax Dashboard'
    };
    
    const dataStr = JSON.stringify(dashboardData, null, 2);
    const dataUri = 'data:application/json;charset=utf-8,'+ encodeURIComponent(dataStr);
    
    const exportFileName = `OneJax_Dashboard_${new Date().toISOString().split('T')[0]}.json`;
    
    const linkElement = document.createElement('a');
    linkElement.setAttribute('href', dataUri);
    linkElement.setAttribute('download', exportFileName);
    linkElement.click();
}

// OneJax Chart Theme Helpers
function getOneJaxChartOptions(type = 'default') {
    const baseOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                labels: {
                    color: OneJaxColors.navy,
                    font: {
                        family: 'Poppins, sans-serif',
                        size: 12,
                        weight: '600'
                    }
                }
            },
            tooltip: {
                backgroundColor: OneJaxColors.white,
                titleColor: OneJaxColors.navy,
                bodyColor: OneJaxColors.navy,
                borderColor: OneJaxColors.orange,
                borderWidth: 2,
                cornerRadius: 8
            }
        }
    };
    
    return baseOptions;
}

// OneJax Color Generator for Dynamic Charts
function generateOneJaxColors(count) {
    const colors = [
        OneJaxColors.navy,
        OneJaxColors.orange,
        OneJaxColors.blue,
        OneJaxColors.green,
        OneJaxColors.gold,
        OneJaxColors.darkBlue,
        OneJaxColors.darkGreen
    ];
    
    const result = [];
    for (let i = 0; i < count; i++) {
        result.push(colors[i % colors.length]);
    }
    return result;
}
