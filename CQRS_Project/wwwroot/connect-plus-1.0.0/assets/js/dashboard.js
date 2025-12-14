$(function () {
    /* ChartJS
     * -------
     * Data and config for chartjs
     */
    'use strict';
    // RENK PALETİ TANIMLARI
    // Mor/Lila: #8B5CF6
    // Fuşya Pembe: #EC4899
    // Turkuaz: #00B894
    // Açık Mor: #A78BFA

    // 1. DATA (BAR/LINE GRAFİKLERİ İÇİN)
    var data = {
        labels: ["2013", "2014", "2015", "2016", "2017", "2018"],
        datasets: [{
            // RENK GÜNCELLEMESİ: Mor/Lila
            label: 'Kiralama Sayısı',
            data: [10, 19, 3, 5, 2, 3],
            backgroundColor: [
                'rgba(139, 92, 246, 0.6)', // #8B5CF6 (Mor/Lila)
                'rgba(139, 92, 246, 0.6)',
                'rgba(139, 92, 246, 0.6)',
                'rgba(139, 92, 246, 0.6)',
                'rgba(139, 92, 246, 0.6)',
                'rgba(139, 92, 246, 0.6)'
            ],
            borderColor: [
                '#8B5CF6', // Sınır Mor/Lila
                '#8B5CF6',
                '#8B5CF6',
                '#8B5CF6',
                '#8B5CF6',
                '#8B5CF6'
            ],
            borderWidth: 1,
            fill: false
        }]
    };

    // 2. MULTILINE DATA (ÇOKLU ÇİZGİ GRAFİĞİ)
    var multiLineData = {
        labels: ["Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran"],
        datasets: [{
            // RENK GÜNCELLEMESİ: Mor/Lila
            label: 'Bölge A',
            data: [12, 19, 3, 5, 2, 3],
            borderColor: [
                '#8B5CF6' // Mor/Lila
            ],
            borderWidth: 2,
            fill: false
        },
        {
            // RENK GÜNCELLEMESİ: Fuşya Pembe
            label: 'Bölge B',
            data: [5, 23, 7, 12, 42, 23],
            borderColor: [
                '#EC4899' // Fuşya Pembe
            ],
            borderWidth: 2,
            fill: false
        },
        {
            // RENK GÜNCELLEMESİ: Turkuaz
            label: 'Bölge C',
            data: [15, 10, 21, 32, 12, 33],
            borderColor: [
                '#00B894' // Turkuaz
            ],
            borderWidth: 2,
            fill: false
        }
        ]
    };

    var options = {
        scales: {
            yAxes: [{
                ticks: {
                    beginAtZero: true
                }
            }]
        },
        legend: {
            display: false
        },
        elements: {
            point: {
                radius: 0
            }
        }

    };

    // 3. DOUGHNUT/PIE DATA (PASTA/HALKA GRAFİKLERİ)
    var doughnutPieData = {
        datasets: [{
            data: [30, 40, 30], // Buradaki veriyi kendi araç segment dağılımınıza göre değiştirmelisiniz!
            backgroundColor: [
                // RENK GÜNCELLEMESİ: Pink, Blue, Yellow yerine Mor, Pembe, Turkuaz
                'rgba(139, 92, 246, 0.7)', // Mor/Lila
                'rgba(236, 72, 153, 0.7)', // Fuşya Pembe
                'rgba(0, 184, 148, 0.7)', // Turkuaz
            ],
            borderColor: [
                '#8B5CF6',
                '#EC4899',
                '#00B894',
            ],
        }],

        // Bunlar lejantta ve araç ipuçlarında görünür
        labels: [
            'Ekonomi Sınıfı', // Örnek Label Güncellemesi
            'Orta Sınıf',
            'Lüks/SUV',
        ]
    };

    var doughnutPieOptions = {
        responsive: true,
        animation: {
            animateScale: true,
            animateRotate: true
        }
    };

    // 4. AREA DATA (TEKİL ALAN GRAFİĞİ)
    var areaData = {
        labels: ["2013", "2014", "2015", "2016", "2017"],
        datasets: [{
            // RENK GÜNCELLEMESİ: Mor/Lila
            label: 'Rezervasyon Trendi',
            data: [12, 19, 3, 5, 2, 3],
            backgroundColor: [
                'rgba(139, 92, 246, 0.3)', // Açık Mor dolgu
            ],
            borderColor: [
                '#8B5CF6', // Mor/Lila çizgi
            ],
            borderWidth: 2, // Çizgiyi kalınlaştırdık
            fill: true, // Dolgu açık
        }]
    };

    var areaOptions = {
        plugins: {
            filler: {
                propagate: true
            }
        },
        legend: {
            display: false // Başlık gösterilmesin
        },
        elements: {
            point: {
                radius: 3, // Noktaları biraz belirginleştirdik
                backgroundColor: '#8B5CF6'
            }
        }
    }

    // 5. MULTIAREA DATA (ÇOKLU ALAN GRAFİĞİ)
    var multiAreaData = {
        labels: ["Ock", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Ekm", "Kas", "Ara"],
        datasets: [{
            // RENK GÜNCELLEMESİ: Mor/Lila
            label: 'Web Sitesi',
            data: [8, 11, 13, 15, 12, 13, 16, 15, 13, 19, 11, 14],
            borderColor: ['#8B5CF6'], // Mor
            backgroundColor: ['rgba(139, 92, 246, 0.4)'], // Açık Mor dolgu
            borderWidth: 1,
            fill: true
        },
        {
            // RENK GÜNCELLEMESİ: Fuşya Pembe
            label: 'Mobil Uygulama',
            data: [7, 17, 12, 16, 14, 18, 16, 12, 15, 11, 13, 9],
            borderColor: ['#EC4899'], // Pembe
            backgroundColor: ['rgba(236, 72, 153, 0.4)'], // Açık Pembe dolgu
            borderWidth: 1,
            fill: true
        },
        {
            // RENK GÜNCELLEMESİ: Turkuaz
            label: 'Acenta',
            data: [6, 14, 16, 20, 12, 18, 15, 12, 17, 19, 15, 11],
            borderColor: ['#00B894'], // Turkuaz
            backgroundColor: ['rgba(0, 184, 148, 0.4)'], // Açık Turkuaz dolgu
            borderWidth: 1,
            fill: true
        }
        ]
    };

    var multiAreaOptions = {
        plugins: {
            filler: {
                propagate: true
            }
        },
        elements: {
            point: {
                radius: 0
            }
        },
        scales: {
            xAxes: [{
                gridLines: {
                    display: false
                }
            }],
            yAxes: [{
                gridLines: {
                    display: false
                }
            }]
        }
    }

    // SCATTER CHART DATA (Serpilme Grafiği)
    var scatterChartData = {
        datasets: [{
            label: 'İlk Veri Seti',
            data: [{
                x: -10, y: 0
            },
            {
                x: 0, y: 3
            },
            {
                x: -25, y: 5
            },
            {
                x: 40, y: 5
            }
            ],
            backgroundColor: [
                'rgba(139, 92, 246, 0.2)' // Açık Mor
            ],
            borderColor: [
                '#8B5CF6' // Mor
            ],
            borderWidth: 1
        },
        {
            label: 'İkinci Veri Seti',
            data: [{
                x: 10, y: 5
            },
            {
                x: 20, y: -30
            },
            {
                x: -25, y: 15
            },
            {
                x: -10, y: 5
            }
            ],
            backgroundColor: [
                'rgba(236, 72, 153, 0.2)', // Açık Pembe
            ],
            borderColor: [
                '#EC4899', // Pembe
            ],
            borderWidth: 1
        }
        ]
    }

    var scatterChartOptions = {
        scales: {
            xAxes: [{
                type: 'linear',
                position: 'bottom'
            }]
        }
    }

    // **********************************************
    // GRAFİK ÇAĞRILARI (NEW CHART)
    // **********************************************

    if ($("#barChart").length) {
        var barChartCanvas = $("#barChart").get(0).getContext("2d");
        var barChart = new Chart(barChartCanvas, {
            type: 'bar',
            data: data,
            options: options
        });
    }

    if ($("#lineChart").length) {
        var lineChartCanvas = $("#lineChart").get(0).getContext("2d");
        var lineChart = new Chart(lineChartCanvas, {
            type: 'line',
            data: data,
            options: options
        });
    }

    if ($("#linechart-multi").length) {
        var multiLineCanvas = $("#linechart-multi").get(0).getContext("2d");
        var lineChart = new Chart(multiLineCanvas, {
            type: 'line',
            data: multiLineData,
            options: options
        });
    }

    if ($("#areachart-multi").length) {
        var multiAreaCanvas = $("#areachart-multi").get(0).getContext("2d");
        var multiAreaChart = new Chart(multiAreaCanvas, {
            type: 'line',
            data: multiAreaData,
            options: multiAreaOptions
        });
    }

    if ($("#doughnutChart").length) {
        var doughnutChartCanvas = $("#doughnutChart").get(0).getContext("2d");
        var doughnutChart = new Chart(doughnutChartCanvas, {
            type: 'doughnut',
            data: doughnutPieData,
            options: doughnutPieOptions
        });
    }

    if ($("#pieChart").length) {
        var pieChartCanvas = $("#pieChart").get(0).getContext("2d");
        var pieChart = new Chart(pieChartCanvas, {
            type: 'pie',
            data: doughnutPieData,
            options: doughnutPieOptions
        });
    }

    // ÖNEMLİ DEĞİŞİKLİK: 'line' yerine 'bar'
    if ($("#areaChart").length) {
        var areaChartCanvas = $("#areaChart").get(0).getContext("2d");
        var areaChart = new Chart(areaChartCanvas, {
            type: 'bar',
            data: areaData,
            options: areaOptions
        });
    }

    if ($("#scatterChart").length) {
        var scatterChartCanvas = $("#scatterChart").get(0).getContext("2d");
        var scatterChart = new Chart(scatterChartCanvas, {
            type: 'scatter',
            data: scatterChartData,
            options: scatterChartOptions
        });
    }

    // Bu kısım, tema dosyası tarafından kullanılan, ancak verisi tanımlanmamış bir grafik olabilir.
    if ($("#browserTrafficChart").length) {
        // Bu kısım için 'browserTrafficData' değişkeni bulunmadığından şimdilik pasif kalır veya hata verebilir.
    }
});