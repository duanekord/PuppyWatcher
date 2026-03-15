window.weightChart = {
    _chart: null,

    render: function (canvasId, labels, data, unit) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        if (this._chart) {
            this._chart.destroy();
        }

        this._chart = new Chart(canvas, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Weight (' + unit + ')',
                    data: data,
                    borderColor: '#198754',
                    backgroundColor: 'rgba(25, 135, 84, 0.1)',
                    fill: true,
                    tension: 0.3,
                    pointRadius: 4,
                    pointBackgroundColor: '#198754',
                    pointBorderColor: '#fff',
                    pointBorderWidth: 2,
                    pointHoverRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                return context.parsed.y.toFixed(2) + ' ' + unit;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        title: { display: true, text: 'Date' },
                        ticks: { maxRotation: 45, autoSkipPadding: 10 }
                    },
                    y: {
                        title: { display: true, text: 'Weight (' + unit + ')' },
                        beginAtZero: false
                    }
                }
            }
        });
    },

    destroy: function () {
        if (this._chart) {
            this._chart.destroy();
            this._chart = null;
        }
    },

    _multiChart: null,

    renderMulti: function (canvasId, labels, datasets) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        if (this._multiChart) {
            this._multiChart.destroy();
        }

        var unit = datasets.length > 0 ? datasets[0].unit : '';

        var chartDatasets = datasets.map(function (ds) {
            return {
                label: ds.name,
                data: ds.data,
                borderColor: ds.color,
                backgroundColor: 'transparent',
                fill: false,
                tension: 0.3,
                pointRadius: 4,
                pointBackgroundColor: ds.color,
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                pointHoverRadius: 6
            };
        });

        this._multiChart = new Chart(canvas, {
            type: 'line',
            data: {
                labels: labels,
                datasets: chartDatasets
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: true, position: 'top' },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                return context.dataset.label + ': ' + context.parsed.y.toFixed(2) + ' ' + unit;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        title: { display: true, text: 'Date' },
                        ticks: { maxRotation: 45, autoSkipPadding: 10 }
                    },
                    y: {
                        title: { display: true, text: 'Weight (' + unit + ')' },
                        beginAtZero: false
                    }
                }
            }
        });
    },

    destroyMulti: function () {
        if (this._multiChart) {
            this._multiChart.destroy();
            this._multiChart = null;
        }
    }
};
