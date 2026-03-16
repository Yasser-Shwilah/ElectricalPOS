// ElectricalPOS Dashboard Charts
// Using Apache ECharts with Arabic labels and RTL support
// Properly handles data from C# ViewModel

// Global ECharts chart instances
let salesLast7DaysChart, dailySalesChart, dailyProfitChart, monthlySalesChart;
let categorySalesChart, profitByCategoryChart, inventoryStatusChart;
let topSellingChart, mostProfitableChart;
let currentDashboardData = null;
let selectedPeriod = 'today';

// Flag to track if charts are initialized
window.chartsInitialized = false;

// Arabic translations for chart labels
const arabicLabels = {
    sales: 'المبيعات',
    profit: 'الربح',
    quantity: 'الكمية',
    amount: 'المبلغ',
    date: 'التاريخ',
    month: 'الشهر',
    product: 'المنتج',
    category: 'الفئة'
};

// Format currency value with $ symbol and 2 decimal places
function formatCurrency(value) {
    const num = parseFloat(value);
    if (isNaN(num) || num === null || num === undefined) return '$0.00';
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(num);
}

function rtlText(value) {
    return `\u200F${value ?? ''}`;
}

// ECharts Arabic font configuration
const arabicFont = "'Cairo', 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif";

// Color palette for charts (Material Design colors) - matching KPI card colors
const chartColors = {
    primary: '#1976D2',      // Blue - Sales
    secondary: '#10B981',   // Green - Profit
    accent: '#F59E0B',       // Orange - Invoices
    danger: '#EF4444',       // Red - Low Stock
    info: '#6366F1',         // Indigo
    purple: '#8B5CF6',       // Purple - Average
    teal: '#06B6D4',         // Teal - Products
    pink: '#EC4899',
    cyan: '#06B6D4',
    amber: '#F59E0B',
    gray: '#6B7280'
};

const colorArray = [
    '#1976D2',  // Blue
    '#10B981',  // Green
    '#F59E0B',  // Orange
    '#EF4444',  // Red
    '#8B5CF6',  // Purple
    '#06B6D4',  // Teal
    '#EC4899',  // Pink
    '#6366F1',  // Indigo
    '#14B8A6',  // Teal
    '#F97316'   // Orange
];

// ECharts common options for RTL support
function getArabicOptions(title, xAxisTitle, yAxisTitle) {
    return {
        title: {
            text: title,
            textStyle: {
                fontFamily: arabicFont,
                fontSize: 14,
                fontWeight: 'bold'
            },
            right: 'center'
        },
        tooltip: {
            trigger: 'axis',
            textStyle: {
                fontFamily: arabicFont
            },
            axisPointer: {
                type: 'cross',
                label: {
                    backgroundColor: '#6a7985'
                }
            },
            formatter: function(params) {
                let result = params[0].axisValue + '<br/>';
                params.forEach(param => {
                    const value = param.value;
                    const formatted = typeof value === 'number' ? formatCurrency(value) : value;
                    result += param.marker + ' ' + param.seriesName + ': ' + formatted + '<br/>';
                });
                return result;
            }
        },
        legend: {
            data: [],
            textStyle: {
                fontFamily: arabicFont
            },
            top: 0,
            right: 10
        },
        grid: {
            left: '3%',
            right: '4%',
            bottom: '3%',
            top: '15%',
            containLabel: true
        },
        xAxis: {
            type: 'category',
            name: xAxisTitle,
            nameLocation: 'middle',
            nameGap: 30,
            nameTextStyle: {
                fontFamily: arabicFont,
                fontWeight: 'bold'
            },
            axisLabel: {
                fontFamily: arabicFont,
                rotate: 45,
                formatter: function(value) {
                    // Format date labels
                    if (value && value.includes('-')) {
                        const parts = value.split('-');
                        if (parts.length === 3) {
                            return parts[2] + '/' + parts[1]; // DD/MM
                        } else if (parts.length === 2) {
                            return value; // Keep YYYY-MM format
                        }
                    }
                    return value;
                }
            },
            axisLine: {
                lineStyle: {
                    color: '#666'
                }
            },
            data: []
        },
        yAxis: {
            type: 'value',
            name: yAxisTitle,
            nameLocation: 'middle',
            nameGap: 50,
            nameTextStyle: {
                fontFamily: arabicFont,
                fontWeight: 'bold'
            },
            axisLabel: {
                fontFamily: arabicFont,
                formatter: function(value) {
                    return formatCurrency(value);
                }
            },
            splitLine: {
                lineStyle: {
                    type: 'dashed'
                }
            }
        },
        animation: true,
        animationDuration: 1000,
        animationEasing: 'cubicOut'
    };
}

// Initialize all ECharts instances
function initializeCharts() {
    console.log('Initializing Apache ECharts...');

    // 1. Sales Last 7 Days Line Chart
    const salesLast7DaysDom = document.getElementById('salesLast7DaysChart');
    if (salesLast7DaysDom) {
        salesLast7DaysChart = echarts.init(salesLast7DaysDom);
        const options = getArabicOptions('', 'التاريخ', 'المبيعات ($)');
        options.legend = { data: ['المبيعات', 'الربح'], top: 0, right: 10 };
        options.series = [
            {
                name: 'المبيعات',
                type: 'line',
                data: [],
                smooth: true,
                symbol: 'circle',
                symbolSize: 6,
                lineStyle: {
                    color: chartColors.primary,
                    width: 3
                },
                itemStyle: {
                    color: chartColors.primary
                },
                areaStyle: {
                    color: {
                        type: 'linear',
                        x: 0, y: 0, x2: 0, y2: 1,
                        colorStops: [
                            { offset: 0, color: 'rgba(25, 118, 210, 0.3)' },
                            { offset: 1, color: 'rgba(25, 118, 210, 0.05)' }
                        ]
                    }
                },
                emphasis: {
                    focus: 'series'
                }
            },
            {
                name: 'الربح',
                type: 'line',
                data: [],
                smooth: true,
                symbol: 'circle',
                symbolSize: 6,
                lineStyle: {
                    color: chartColors.secondary,
                    width: 3
                },
                itemStyle: {
                    color: chartColors.secondary
                },
                emphasis: {
                    focus: 'series'
                }
            }
        ];
        salesLast7DaysChart.setOption(options);
    }

    // 2. Top Selling Products Horizontal Bar Chart
    const topSellingDom = document.getElementById('topSellingChart');
    if (topSellingDom) {
        topSellingChart = echarts.init(topSellingDom);
        const options = {
            title: {
                text: '',
                textStyle: {
                    fontFamily: arabicFont,
                    fontSize: 14,
                    fontWeight: 'bold'
                },
                right: 'center'
            },
            tooltip: {
                trigger: 'axis',
                axisPointer: {
                    type: 'shadow'
                },
                textStyle: {
                    fontFamily: arabicFont
                },
                formatter: function(params) {
                    return params[0].name + '<br/>' + 
                           params[0].seriesName + ': ' + 
                           params[0].value.toFixed(0) + ' وحدة';
                }
            },
            legend: {
                data: ['الكمية المباعة'],
                top: 0,
                right: 10,
                textStyle: {
                    fontFamily: arabicFont
                }
            },
            grid: {
                left: '3%',
                right: '4%',
                bottom: '3%',
                top: '15%',
                containLabel: true
            },
            xAxis: {
                type: 'value',
                name: 'الكمية (وحدة)',
                nameLocation: 'middle',
                nameGap: 40,
                nameTextStyle: {
                    fontFamily: arabicFont,
                    fontWeight: 'bold'
                },
                axisLabel: {
                    fontFamily: arabicFont
                },
                splitLine: {
                    lineStyle: {
                        type: 'dashed'
                    }
                }
            },
            yAxis: {
                type: 'category',
                name: 'المنتج',
                nameLocation: 'middle',
                nameGap: 80,
                nameTextStyle: {
                    fontFamily: arabicFont,
                    fontWeight: 'bold'
                },
                axisLabel: {
                    fontFamily: arabicFont
                },
                axisLine: {
                    lineStyle: {
                        color: '#666'
                    }
                },
                data: []
            },
            series: [{
                name: 'الكمية المباعة',
                type: 'bar',
                data: [],
                barWidth: '60%',
                itemStyle: {
                    color: chartColors.accent
                },
                emphasis: {
                    focus: 'series'
                },
                label: {
                    show: true,
                    position: 'right',
                    fontFamily: arabicFont,
                    formatter: function(params) {
                        return params.value.toFixed(0);
                    }
                }
            }],
            animation: true,
            animationDuration: 1000,
            animationEasing: 'cubicOut'
        };
        topSellingChart.setOption(options);
    }

    // 3. Profit by Category Pie Chart
    const profitByCategoryDom = document.getElementById('profitByCategoryChart');
    if (profitByCategoryDom) {
        profitByCategoryChart = echarts.init(profitByCategoryDom);
        const options = {
            title: {
                text: '',
                textStyle: {
                    fontFamily: arabicFont,
                    fontSize: 14,
                    fontWeight: 'bold'
                },
                right: 'center'
            },
            tooltip: {
                trigger: 'item',
                textStyle: {
                    fontFamily: arabicFont
                },
                formatter: function(params) {
                    return params.name + '<br/>' + 
                           formatCurrency(params.value) + 
                           ' (' + params.percent.toFixed(1) + '%)';
                }
            },
            legend: {
                orient: 'vertical',
                right: 10,
                top: 'center',
                textStyle: {
                    fontFamily: arabicFont
                }
            },
            series: [{
                name: 'الربح',
                type: 'pie',
                radius: ['40%', '70%'],
                center: ['40%', '50%'],
                avoidLabelOverlap: false,
                itemStyle: {
                    borderRadius: 10,
                    borderColor: '#fff',
                    borderWidth: 2
                },
                label: {
                    show: false,
                    position: 'center'
                },
                emphasis: {
                    label: {
                        show: true,
                        fontSize: 16,
                        fontWeight: 'bold',
                        fontFamily: arabicFont
                    }
                },
                labelLine: {
                    show: false
                },
                data: []
            }],
            animation: true,
            animationDuration: 1000,
            animationEasing: 'cubicOut'
        };
        profitByCategoryChart.setOption(options);
    }

    // 4. Inventory Status Bar Chart
    const inventoryStatusDom = document.getElementById('inventoryStatusChart');
    if (inventoryStatusDom) {
        inventoryStatusChart = echarts.init(inventoryStatusDom);
        const options = {
            title: {
                text: '',
                textStyle: {
                    fontFamily: arabicFont,
                    fontSize: 14,
                    fontWeight: 'bold'
                },
                right: 'center'
            },
            tooltip: {
                trigger: 'axis',
                axisPointer: {
                    type: 'shadow'
                },
                textStyle: {
                    fontFamily: arabicFont
                },
                formatter: function(params) {
                    return params[0].name + '<br/>' + 
                           params[0].seriesName + ': ' + 
                           params[0].value + ' منتج';
                }
            },
            legend: {
                data: ['عدد المنتجات'],
                top: 0,
                right: 10,
                textStyle: {
                    fontFamily: arabicFont
                }
            },
            grid: {
                left: '3%',
                right: '4%',
                bottom: '3%',
                top: '15%',
                containLabel: true
            },
            xAxis: {
                type: 'category',
                name: 'الحالة',
                nameLocation: 'middle',
                nameGap: 30,
                nameTextStyle: {
                    fontFamily: arabicFont,
                    fontWeight: 'bold'
                },
                axisLabel: {
                    fontFamily: arabicFont
                },
                axisLine: {
                    lineStyle: {
                        color: '#666'
                    }
                },
                data: []
            },
            yAxis: {
                type: 'value',
                name: 'عدد المنتجات',
                nameLocation: 'middle',
                nameGap: 40,
                nameTextStyle: {
                    fontFamily: arabicFont,
                    fontWeight: 'bold'
                },
                axisLabel: {
                    fontFamily: arabicFont
                },
                splitLine: {
                    lineStyle: {
                        type: 'dashed'
                    }
                }
            },
            series: [{
                name: 'عدد المنتجات',
                type: 'bar',
                data: [],
                barWidth: '50%',
                itemStyle: {
                    color: function(params) {
                        // Different colors for each category
                        const colors = ['#10B981', '#F59E0B', '#EF4444'];
                        return colors[params.dataIndex % colors.length];
                    },
                    borderRadius: [4, 4, 0, 0]
                },
                emphasis: {
                    focus: 'series'
                },
                label: {
                    show: true,
                    position: 'top',
                    fontFamily: arabicFont,
                    formatter: function(params) {
                        return params.value;
                    }
                }
            }],
            animation: true,
            animationDuration: 1000,
            animationEasing: 'cubicOut'
        };
        inventoryStatusChart.setOption(options);
    }

    // 5. Most Profitable Products Bar Chart
    const mostProfitableDom = document.getElementById('mostProfitableChart');
    if (mostProfitableDom) {
        mostProfitableChart = echarts.init(mostProfitableDom);
        const options = getArabicOptions('', 'المنتج', 'الربح ($)');
        options.legend = { data: ['الربح'], top: 0, right: 10 };
        options.series = [{
            name: 'الربح',
            type: 'bar',
            data: [],
            barWidth: '60%',
            itemStyle: {
                color: chartColors.secondary
            },
            emphasis: {
                focus: 'series'
            },
            label: {
                show: true,
                position: 'top',
                fontFamily: arabicFont,
                formatter: function(params) {
                    return formatCurrency(params.value);
                }
            }
        }];
        mostProfitableChart.setOption(options);
    }

    // 6. Monthly Sales Bar Chart
    const monthlySalesDom = document.getElementById('monthlySalesChart');
    if (monthlySalesDom) {
        monthlySalesChart = echarts.init(monthlySalesDom);
        const options = getArabicOptions('', 'الشهر', 'المبيعات ($)');
        options.legend = { data: ['المبيعات الشهرية'], top: 0, right: 10 };
        options.series = [{
            name: 'المبيعات الشهرية',
            type: 'bar',
            data: [],
            barWidth: '60%',
            itemStyle: {
                color: {
                    type: 'linear',
                    x: 0, y: 0, x2: 0, y2: 1,
                    colorStops: [
                        { offset: 0, color: '#1976D2' },
                        { offset: 1, color: '#1565C0' }
                    ]
                },
                borderRadius: [4, 4, 0, 0]
            },
            emphasis: {
                focus: 'series'
            }
        }];
        monthlySalesChart.setOption(options);
    }

    // Mark charts as initialized
    window.chartsInitialized = true;
    console.log('Apache ECharts initialized successfully');
}

// Setup listener for WebView2 PostWebMessageAsJson
function setupWebMessageListener() {
    if (window.chrome && window.chrome.webview) {
        console.log('Setting up WebView2 message listener...');
        
        window.chrome.webview.addEventListener('message', function(event) {
            console.log('Received message from C#:', event.data);
            
            try {
                const data = typeof event.data === 'string' ? JSON.parse(event.data) : event.data;
                updateDashboard(data);
            } catch (e) {
                console.error('Error parsing web message:', e);
            }
        });
        
        console.log('WebView2 message listener set up successfully');
    } else {
        console.log('Chrome webview API not available - will use updateDashboard function');
    }
}

// Update dashboard with data from C# - waits for charts to be ready
async function updateDashboard(data) {
    currentDashboardData = data;

    console.log('=== UPDATE DASHBOARD CALLED ===');
    console.log('Data received:', JSON.stringify(data));

    // Hide loading overlay
    const loadingOverlay = document.getElementById('loading-overlay');
    if (loadingOverlay) {
        loadingOverlay.style.display = 'none';
    }

    // Initialize charts if not already done
    if (!window.chartsInitialized) {
        console.log('Charts not initialized, initializing now...');
        initializeCharts();
    }

    // Wait a bit for charts to be ready
    await new Promise(resolve => setTimeout(resolve, 100));

    console.log('Updating KPIs and charts with data...');

    // Update KPI cards
    if (data.kPIs || data.KPIs) {
        updateKPICards(data.kPIs || data.KPIs);
    }

    // Update Sales Last 7 Days Chart (Line)
    if (data.salesLast7Days || data.SalesLast7Days) {
        updateSalesLast7DaysChart(data.salesLast7Days || data.SalesLast7Days);
    }

    // Update Top Selling Products Chart
    if (data.topSellingProducts || data.TopSellingProducts) {
        updateTopSellingChart(data.topSellingProducts || data.TopSellingProducts);
    }

    // Update Profit by Category Chart (Pie)
    if (data.profitByCategory || data.ProfitByCategory) {
        updateProfitByCategoryChart(data.profitByCategory || data.ProfitByCategory);
    }

    // Update Inventory Status Chart (Bar)
    if (data.inventoryStatus || data.InventoryStatus) {
        updateInventoryStatusChart(data.inventoryStatus || data.InventoryStatus);
    }

    // Update Most Profitable Products Chart
    if (data.mostProfitableProducts || data.MostProfitableProducts) {
        updateMostProfitableChart(data.mostProfitableProducts || data.MostProfitableProducts);
    }

    // Update Monthly Sales Chart
    if (data.monthlySales || data.MonthlySales) {
        updateMonthlySalesChart(data.monthlySales || data.MonthlySales);
    }

    updateTables(data);
    applyPeriodFilterToKpis();
    
    console.log('Dashboard update complete!');
}

// Update KPI cards with values
function updateKPICards(kpis) {
    console.log('Updating KPI cards:', kpis);
    
    // Sales KPIs
    updateKPIElement('kpi-total-sales', kpis.totalSales, '$');
    updateKPIElement('kpi-sales-today', kpis.totalSalesToday, '$');
    updateKPIElement('kpi-weekly-sales', kpis.weeklySales, '$');
    updateKPIElement('kpi-monthly-sales', kpis.monthlySales, '$');
    
    // Profit KPIs
    updateKPIElement('kpi-total-profit', kpis.totalProfit, '$');
    updateKPIElement('kpi-profit-today', kpis.totalProfitToday, '$');
    updateKPIElement('kpi-monthly-profit', kpis.monthlyProfit, '$');
    
    // Transaction KPIs
    updateKPIElement('kpi-transactions', kpis.salesTransactionCount, '');
    const bestDayEl = document.getElementById('kpi-best-day');
    if (bestDayEl) {
        bestDayEl.textContent = `${kpis.bestSalesDay || '-'} (${formatCurrency(kpis.bestSalesDayAmount || 0)})`;
    }
    
    // Inventory KPIs
    updateKPIElement('kpi-inventory-value', kpis.totalInventoryValue, '$');
    updateKPIElement('kpi-total-products', kpis.totalProducts, '');
    updateKPIElement('kpi-low-stock', kpis.lowStockProducts, '');
    updateKPIElement('kpi-out-of-stock', kpis.outOfStockProducts, '');
    updateKPIElement('kpi-dead-stock', kpis.deadStockProducts, '');
    
    // Purchase KPIs
    updateKPIElement('kpi-total-purchases', kpis.totalPurchases, '$');
    updateKPIElement('kpi-supplier-debt', kpis.totalSupplierDebt, '$');
    updateKPIElement('kpi-avg-cost', kpis.averageProductCost, '$');
    updateKPIElement('kpi-purchase-transactions', kpis.purchaseTransactionCount, '');
    
    // Additional KPIs
    updateKPIElement('kpi-avg-order', kpis.averageOrderValue, '$');
    
    // Top Supplier (text value)
    const topSupplierEl = document.getElementById('kpi-top-supplier');
    if (topSupplierEl) {
        topSupplierEl.textContent = kpis.topSupplierName || '-';
    }
    
    console.log('KPI cards updated successfully');
}

function applyPeriodFilterToKpis() {
    if (!currentDashboardData) return;
    const kpis = currentDashboardData.kPIs || currentDashboardData.KPIs;
    if (!kpis) return;

    let salesValue = kpis.totalSalesToday;
    let profitValue = kpis.totalProfitToday;

    if (selectedPeriod === 'week') {
        salesValue = kpis.weeklySales;
        profitValue = kpis.weeklyProfit || kpis.totalProfitToday;
    } else if (selectedPeriod === 'month') {
        salesValue = kpis.monthlySales;
        profitValue = kpis.monthlyProfit;
    }

    updateKPIElement('kpi-sales-today', salesValue, '$');
    updateKPIElement('kpi-profit-today', profitValue, '$');
}

function setupFilters() {
    document.querySelectorAll('.filter-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('.filter-btn').forEach(x => x.classList.remove('active'));
            btn.classList.add('active');
            selectedPeriod = btn.dataset.period || 'today';
            applyPeriodFilterToKpis();
        });
    });
}

function updateTables(data) {
    const profitable = data.topProfitableProducts || [];
    const leastSelling = data.leastSellingProducts || [];
    const deadStock = data.deadStockProducts || [];
    const suppliers = data.purchasesBySupplier || [];

    fillTable('top-profitable-table', profitable, p => `
        <tr>
            <td>${rtlText(p.productName)}</td>
            <td>${formatCurrency(p.totalProfit)}</td>
            <td>${p.totalQuantity ?? 0}</td>
        </tr>`);

    fillTable('least-selling-table', leastSelling, p => `
        <tr>
            <td>${rtlText(p.productName)}</td>
            <td>${p.totalQuantity ?? 0}</td>
        </tr>`);

    fillTable('dead-stock-table', deadStock, p => `
        <tr>
            <td>${rtlText(p.productName)}</td>
            <td>${p.quantity ?? 0}</td>
            <td>${p.daysInactive ?? 0}</td>
        </tr>`);

    fillTable('suppliers-table', suppliers, p => `
        <tr>
            <td>${rtlText(p.supplierName)}</td>
            <td>${formatCurrency(p.totalPurchases)}</td>
            <td>${p.purchaseCount ?? 0}</td>
        </tr>`);
}

function fillTable(id, items, rowTemplate) {
    const tbody = document.getElementById(id);
    if (!tbody) return;
    tbody.innerHTML = (items || []).slice(0, 10).map(rowTemplate).join('') || '<tr><td colspan="4">لا توجد بيانات</td></tr>';
}

// Helper function to update a single KPI element
function updateKPIElement(id, value, prefix) {
    const el = document.getElementById(id);
    if (el) {
        const val = parseFloat(value);
        if (prefix) {
            el.textContent = isNaN(val) ? '$0.00' : formatCurrency(val);
        } else {
            el.textContent = isNaN(val) ? '0' : val.toString();
        }
    }
}

// Update Sales Last 7 Days Chart
function updateSalesLast7DaysChart(data) {
    if (salesLast7DaysChart && data) {
        const labels = data.labels || [];
        const values = (data.values || []).map(v => {
            const num = parseFloat(v);
            return isNaN(num) ? 0 : num;
        });
        const profits = (data.profits || []).map(v => {
            const num = parseFloat(v);
            return isNaN(num) ? 0 : num;
        });
        
        salesLast7DaysChart.setOption({
            xAxis: {
                data: labels
            },
            series: [
                { data: values },
                { data: profits }
            ]
        });
        console.log('Sales last 7 days chart updated with', values.length, 'data points');
    }
}

// Update Top Selling Products Chart
function updateTopSellingChart(data) {
    if (topSellingChart && data) {
        const labels = data.labels || [];
        const values = (data.values || []).map(v => {
            const num = parseFloat(v);
            return isNaN(num) ? 0 : num;
        });
        
        topSellingChart.setOption({
            yAxis: {
                data: labels
            },
            series: [{
                data: values
            }]
        });
        console.log('Top selling chart updated with', labels.length, 'products');
    }
}

// Update Profit by Category Chart (Pie)
function updateProfitByCategoryChart(data) {
    if (profitByCategoryChart && data) {
        const labels = data.labels || [];
        const values = (data.values || []).map(v => {
            const num = parseFloat(v);
            return isNaN(num) ? 0 : num;
        });
        
        // Create pie chart data with colors
        const pieData = labels.map((label, index) => ({
            name: label,
            value: values[index],
            itemStyle: {
                color: colorArray[index % colorArray.length]
            }
        }));
        
        profitByCategoryChart.setOption({
            series: [{
                data: pieData
            }]
        });
        console.log('Profit by category chart updated with', labels.length, 'categories');
    }
}

// Update Inventory Status Chart (Bar)
function updateInventoryStatusChart(data) {
    if (inventoryStatusChart && data) {
        const labels = data.labels || [];
        const values = (data.values || []).map(v => {
            const num = parseFloat(v);
            return isNaN(num) ? 0 : num;
        });
        
        inventoryStatusChart.setOption({
            xAxis: {
                data: labels
            },
            series: [{
                data: values
            }]
        });
        console.log('Inventory status chart updated with', labels.length, 'categories');
    }
}

// Update Most Profitable Products Chart
function updateMostProfitableChart(data) {
    if (mostProfitableChart && data) {
        const labels = data.labels || [];
        const values = (data.values || []).map(v => {
            const num = parseFloat(v);
            return isNaN(num) ? 0 : num;
        });
        
        mostProfitableChart.setOption({
            xAxis: {
                data: labels
            },
            series: [{
                data: values
            }]
        });
        console.log('Most profitable chart updated with', labels.length, 'products');
    }
}

// Update Monthly Sales Chart
function updateMonthlySalesChart(data) {
    if (monthlySalesChart && data) {
        const labels = data.labels || [];
        const values = (data.values || []).map(v => {
            const num = parseFloat(v);
            return isNaN(num) ? 0 : num;
        });
        
        monthlySalesChart.setOption({
            xAxis: {
                data: labels
            },
            series: [{
                data: values
            }]
        });
        console.log('Monthly sales chart updated with', values.length, 'data points');
    }
}

// Expose updateCharts function for external calls (for compatibility)
function updateCharts(data) {
    console.log('updateCharts called with:', data);
    updateDashboard(data);
}

// Make functions available globally
window.updateDashboard = updateDashboard;
window.updateCharts = updateCharts;
window.initializeCharts = initializeCharts;

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM loaded, initializing charts...');
    
    // Ensure RTL direction
    document.body.dir = 'rtl';
    
    initializeCharts();
    setupWebMessageListener();
    setupFilters();
    
    // Try to load initial data if available
    console.log('Charts ready');
});
