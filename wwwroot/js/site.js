// site JS helpers
window.site = {
 renderBarChart: function(canvasId, labels, data, colors) {
 const ctx = document.getElementById(canvasId);
 if(!ctx) return;
 return new Chart(ctx, {
 type: 'bar',
 data: { labels: labels, datasets: [{ label: 'Média', data: data, backgroundColor: colors }] },
 options: { responsive: true }
 });
 }
};