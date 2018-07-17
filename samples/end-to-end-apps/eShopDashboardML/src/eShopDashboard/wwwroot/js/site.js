// PRODUCT FORECASTING

var months = ["",
    "Jan", "Feb", "Mar",
    "Apr", "May", "Jun", "Jul",
    "Aug", "Sep", "Oct",
    "Nov", "Dec"
];

var full_months = ["",
    "January", "February", "March",
    "April", "May", "June", "July",
    "August", "September", "October",
    "November", "December"]

function onLoadProductForecasting() {
    setResponsivePlots();
    setUpProductDescriptionTypeahead();
    $("footer").addClass("sticky");
}

function setResponsivePlots(plotSelector = ".responsive-plot") {
    // MAKE THE PLOTS RESPONSIVE
    // https://gist.github.com/aerispaha/63bb83208e6728188a4ee701d2b25ad5
    var d3 = Plotly.d3;
    var gd3 = d3.selectAll(plotSelector);
    var nodes_to_resize = gd3[0]; //not sure why but the goods are within a nested array
    window.onresize = function () {
        for (var i = 0; i < nodes_to_resize.length; i++) {
            //if (nodes_to_resize[i].attributes["width"])
            Plotly.Plots.resize(nodes_to_resize[i]);
        }
    };
}

function setUpProductDescriptionTypeahead(typeaheadSelector = "#remote .typeahead") {
    var productDescriptions = new Bloodhound({
        datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        remote: {
            url: `${apiUri.catalog}/productSetDetailsByDescription?description=%QUERY`,
            wildcard: '%QUERY'
        }
    });

    $(typeaheadSelector)
        .typeahead
        ({
            minLength: 3,
            highlight: true
        },
        {
            name: 'products',
            display: 'description',
            limit: 10,
            source: productDescriptions
        })
        .on('typeahead:selected', function (e, data) {
            updateProductInfo(data);
            getProductData(data);
        });
}

function updateProductInfo(data) {
    $("#product").removeClass("d-none");
    $("#productName").text(data.description);
    $("#productPrice").text(`${data.price.toCurrencyLocaleString()}`);
    $("#productImage").attr("src", data.pictureUri).attr("alt", data.description);
    //console.log(data.id);
}

function getProductData(product) {
    productId = product.id;
    description = product.description;

    getHistory(productId)
        .done(function (history) {
            if (history.length < 4) return;
            $.when(
                getForecast(history[history.length - 1], product)
            ).done(function (forecast) {
                plotLineChart(forecast, history, description, product.price)
            });
        });
}

function getForecast(st, pr) {
    // next,productId,year,month,units,avg,count,max,min,prev
    var surl = `?month=${st.month}&year=${st.year}&avg=${st.avg}&max=${st.max}&min=${st.min}&count=${st.count}&prev=${st.prev}&units=${st.units}`;
    return $.getJSON(`${apiUri.forecasting}/product/${st.productId}/unitdemandestimation${surl}`);
}

function getHistory(productId) {
    return $.getJSON(`${apiUri.ordering}/product/${productId}/history`);
}

function getStats(productId) {
    return $.getJSON(`${apiUri.ordering}/product/${productId}/stats`);
}

function plotLineChart(forecast, history, description, price) {
    for(i = 0; i < history.length; i++) {
        history[i].sales = history[i].units * price;
    }
    forecast *= price;

    $("footer").removeClass("sticky");
    updateProductStatistics(description, history.slice(history.length - 12), forecast);

    var trace_real = TraceProductHistory(history);

    var trace_forecast = TraceProductForecast(
        trace_real.x,
        nextMonth(history[history.length - 1]),
        nextFullMonth(history[history.length - 1]),
        trace_real.text[trace_real.text.length - 1],
        trace_real.y,
        forecast);

    var trace_mean = TraceMean(trace_real.x.concat(trace_forecast.x), trace_real.y, '#ffcc33');

    var layout = {
        xaxis: {
            tickangle: 0,
            showgrid: false,
            showline: false,
            zeroline: false,
            range: [trace_real.x.length - 12, trace_real.x.length],
        },
        yaxis: {
            showgrid: false,
            showline: false,
            zeroline: false,
            tickformat: '$,.0'
        },
        hovermode: "closest",
        //dragmode: 'pan',
        legend: {
            orientation: "h",
            xanchor: "center",
            yanchor: "top",
            y: 1.2,
            x: 0.85,
        }
    };

    Plotly.newPlot('lineChart', [trace_real, trace_forecast, trace_mean], layout);
}

function TraceProductHistory(historyItems) {
    var y = $.map(historyItems, function (d) { return d.sales; });
    var x = $.map(historyItems, function (d) { return `${months[d.month]}<br>${d.year}`;; });
    var texts = $.map(historyItems, function (d) { return `${full_months[d.month]}<br><b>${d.sales.toCurrencyLocaleString()}</b>`; });

    return {
        x: x,
        y: y,
        mode: 'lines+markers',
        name: 'history',
        line: {
            shape: 'spline',
            color: '#dd1828'
        },
        hoveron: 'points',
        hoverinfo: 'text',
        hoverlabel: {
            bgcolor: '#333333',
            bordercolor: '#333333',
            font: {
                color: 'white'
            }
        },
        text: texts,
        fill: 'tozeroy',
        fillcolor: '#dd1828',
        marker: {
            symbol: "circle",
            color: "white",
            size: 10,
            line: {
                color: "black",
                width: 3,
            }
        },
    };
}

function TraceProductForecast(labels, next_x_label, next_text, prev_text, values, forecast) {
    return {
        x: [labels[labels.length - 1], next_x_label],
        y: [values[values.length - 1], forecast],
        text: [prev_text, `${next_text}<br><b>${forecast.toCurrencyLocaleString()}</b>`],
        mode: 'lines+markers',
        name: 'forecasting',
        hoveron: 'points',
        hoverinfo: 'text',
        hoverlabel: {
            bgcolor: '#333333',
            bordercolor: '#333333',
            font: {
                color: 'white'
            }
        },
        line: {
            shape: 'spline',
            color: '#00A69C',
        },
        fill: 'tozeroy',
        fillcolor: '#00A69C',
        marker: {
            symbol: "circle",
            color: "white",
            size: 10,
            line: {
                color: "black",
                width: 3,
            }
        }
    }
}

function TraceMean(labels, values, color) {
    var y_mean = values.slice(0, values.length - 2).reduce((previous, current) => current += previous) / values.length;
    return {
        x: labels,
        y: Array(labels.length).fill(y_mean),
        name: 'average',
        mode: 'lines',
        hoverinfo: 'none',
        line: {
            color: color,
            width: 3,
        }
    }
}

function nextMonth(predictor) {
    if (predictor.month == 12)
        return `${months[1]}<br>${predictor.year + 1}`;
    else
        return `${months[predictor.month + 1]}<br>${predictor.year}`;
}

function nextFullMonth(predictor, includeYear = false) {
    if (predictor.month == 12)
        return `${full_months[1]}`;
    else
        return `${full_months[predictor.month + 1]}${includeYear ? ' ' + predictor.year : ''}`;
}

function onLoadCountryForecasting() {
    setResponsivePlots();
    $("footer").addClass("sticky");
}


// COUNTRY FORECASTING

function getCountryData(country) {
    $.getJSON(`${apiUri.ordering}/country/${country}/history`)
        .done(function (history) {
            if (history.length < 4) return;
            $.when(
                getCountryForecast(history[history.length - 1])
            ).done(function (forecast) {
                plotLineChartCountry(forecast, history, country)
            });
        });
}

function getCountryForecast(st) {
    // next,country,year,month,max,min,std,count,sales,med,prev
    var url = `?month=${st.month}&year=${st.year}&avg=${st.avg}&max=${st.max}&min=${st.min}&prev=${st.prev}&count=${st.count}&med=${st.med}&sales=${st.sales}&std=${st.std}`;
    return $.getJSON(`${apiUri.forecasting}/country/${st.country}/unitdemandestimation${url}`);
}

function plotLineChartCountry(forecast, historyItems, country) {
    forecast = Math.pow(10,forecast);

    $("footer").removeClass("sticky");
    updateCountryStatistics(country, historyItems.slice(historyItems.length - 12), forecast);

    var trace_real = getTraceCountryHistory(historyItems);

    var trace_forecast = getTraceCountryForecast(
        trace_real.x,
        nextMonth(historyItems[historyItems.length - 1]),
        nextFullMonth(historyItems[historyItems.length - 1]),
        trace_real.text[trace_real.text.length - 1],
        trace_real.y,
        forecast);

    var trace_mean = TraceMean(trace_real.x.concat(trace_forecast.x), trace_real.y, '#999999');

    var layout = {
        xaxis: {
            tickangle: 0,
            showgrid: false,
            showline: false,
            zeroline: false,
            range: [trace_real.x.length - 12, trace_real.x.length],
        },
        yaxis: {
            showgrid: false,
            showline: false,
            zeroline: false,
            tickformat: '$,.0'
        },
        //dragmode: 'pan',
        hovermode: "closest",
        legend: {
            orientation: "h",
            xanchor: "center",
            yanchor: "top",
            y: 1.2,
            x: 0.85,
        }
    };

    Plotly.newPlot('lineChart', [trace_real, trace_forecast, trace_mean], layout);
}

function getTraceCountryHistory(historyItems) {
    var y = $.map(historyItems, function (d) { return d.sales; });
    var x = $.map(historyItems, function (d) { return `${months[d.month]}<br>${d.year}`; });
    var texts = $.map(historyItems, function (d) { return `${full_months[d.month]}<br><b>${d.sales.toCurrencyLocaleString()}</b>`; });

    return {
        x: x,
        y: y,
        mode: 'lines+markers',
        name: 'history',
        line: {
            shape: 'spline',
            color: '#ffb131'
        },
        hoveron: 'points',
        hoverinfo: 'text',
        hoverlabel: {
            bgcolor: '#333333',
            bordercolor: '#333333',
            font: {
                color: 'white'
            }
        },
        text: texts,
        fill: 'tozeroy',
        fillcolor: '#ffb131',
        marker: {
            symbol: "circle",
            color: "white",
            size: 10,
            line: {
                color: "black",
                width: 3,
            }
        },
    };
}

function getTraceCountryForecast(labels, next_y_label, next_text, prev_text, values, forecast) {
    return {
        x: [labels[labels.length - 1], next_y_label],
        y: [values[values.length - 1], forecast],
        text: [prev_text, `${next_text}<br><b>${forecast.toCurrencyLocaleString()}</b>`],
        mode: 'lines+markers',
        name: 'forecasting',
        hoveron: 'points',
        hoverinfo: 'text',
        hoverlabel: {
            bgcolor: '#333333',
            bordercolor: '#333333',
            font: {
                color: 'white'
            }
        },
        line: {
            shape: 'spline',
            color: '#00A69C',
        },
        fill: 'tozeroy',
        fillcolor: '#00A69C',
        marker: {
            symbol: "circle",
            color: "white",
            size: 10,
            line: {
                color: "black",
                width: 3,
            }
        }
    };
}

function updateProductStatistics(product, historyItems, forecasting) {
    showStatsLayers();

    populateForecastDashboard(product, historyItems, forecasting);
    populateHistoryTable(historyItems);

    refreshHeightSidebar();
}

function updateCountryStatistics(country, historyItems, forecasting) {
    showStatsLayers();

    populateForecastDashboard(country, historyItems, forecasting);
    populateHistoryTable(historyItems);

    refreshHeightSidebar();
}

function showStatsLayers() {
    $("#plot,#tableHeader,#tableHistory").removeClass('d-none');
}

function populateForecastDashboard(country, historyItems, forecasting, units = false) {
    var lastyear = historyItems[historyItems.length - 1].year;
    var values = historyItems.map(y => (y.year == lastyear) ? y.sales : 0);
    var total = values.reduce((previous, current) => current += previous);

    $("#labelTotal").text(`${lastyear} sales`);
    $("#valueTotal").text(units ? total.toNumberLocaleString() : total.toCurrencyLocaleString());
    $("#labelForecast").text(`${nextFullMonth(historyItems[historyItems.length - 1], true).toLowerCase()} sales`);
    $("#valueForecast").text(units ? forecasting.toNumberLocaleString() : forecasting.toCurrencyLocaleString());
    $("#labelItem").text(country); 
    $("#tableHeaderCaption").text(`Sales ${units ? "units" : (1).toCurrencyLocaleString().replace("1.00","")} / month`)
}

function populateHistoryTable(historyItems) {
    var table = '';
    var lastYear = '';
    for (i = 0; i < historyItems.length; i++) {
        if (historyItems[i].year != lastYear) {
            lastYear = historyItems[i].year;
            table += `<div class="col-11 border-bottom-highlight-table month font-weight-bold">${lastYear}</div>`;
        }
        table += `<div class="col-8 border-bottom-highlight-table month">${full_months[historyItems[i].month]}</div> <div class="col-3 border-bottom-highlight-table">${historyItems[i].sales.toLocaleString()}</div >`;
    }
    $("#historyTable").empty().append($(table));
}

function refreshHeightSidebar() {
    $("aside").css('height', $(document).height());
}

Number.prototype.toCurrencyLocaleString = function toCurrencyLocaleString() {
    var currentLocale = navigator.languages ? navigator.languages[0] : navigator.language;
    return this.toLocaleString(currentLocale, { style: 'currency', currency: 'USD' });
}

Number.prototype.toNumberLocaleString = function toNumberLocaleString() {
    var currentLocale = navigator.languages ? navigator.languages[0] : navigator.language;
    return this.toLocaleString(currentLocale, { useGrouping: true }) + " units";
}
