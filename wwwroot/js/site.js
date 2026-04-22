window.site = {
    renderBarChart: function (canvasId, labels, data, colors) {
        const el = document.getElementById(canvasId);
        if (!el) return;

        const ctx = el.getContext('2d');

        // Se já existir um gráfico no canvas, destrói-o para poder criar um novo
        if (window.myChartInstance) {
            window.myChartInstance.destroy();
        }

        window.myChartInstance = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Média de Notas (0-20)',
                    data: data,
                    backgroundColor: colors,
                    borderColor: '#2e59d9',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 20 // Escala padrão escolar
                    }
                }
            }
        });
    }
};