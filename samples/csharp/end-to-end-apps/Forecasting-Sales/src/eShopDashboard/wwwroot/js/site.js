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
    "November", "December"];

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
            getProductData(data, e.currentTarget.baseURI.split("/").pop());
        });
}

function updateProductInfo(data) {
    $("#product").removeClass("d-none");
    $("#productName").text(data.description);
    $("#productPrice").text(`${data.price.toCurrencyLocaleString()}`);
    $("#productImage").attr("src", data.pictureUri).attr("alt", data.description);   
}

function getProductData(product, page) {
    productId = product.id;
    description = product.description;

    if (page === "Comparison") {
        getHistory(productId)
            .done(function (history) {
                if (history.length < 4) return;
                $.when(
                    getForecast(history[history.length - 1], "TimeSeries"),
                    getForecast(history[history.length - 1], "Regression")
                ).done(function (timeSeriesForecast, regressionForecast) {
                    plotLineChartComparison(timeSeriesForecast[0], regressionForecast[0], history, description, product.price)
                })
            });
    } else {
        getHistory(productId)
            .done(function (history) {
                if (history.length < 4) return;
                $.when(
                    getForecast(history[history.length - 1], page)
                ).done(function (forecast) {
                    plotLineChart(forecast, history, description, product.price);
                });
            });
    }
}

function getForecast(st, page) {
    if (page === "TimeSeries") {
        return $.getJSON(`${apiUri.timeseriesforcasting}/product/${st.productId}/unittimeseriesestimation`);
    } else {
        var surl = `?month=${st.month}&year=${st.year}&avg=${st.avg}&max=${st.max}&min=${st.min}&count=${st.count}&prev=${st.prev}&units=${st.units}`;
        return $.getJSON(`${apiUri.forecasting}/product/${st.productId}/unitdemandestimation${surl}`);
    }
}

function getHistory(productId) {
    return $.getJSON(`${apiUri.ordering}/product/${productId}/history`);
}

function getStats(productId) {
    return $.getJSON(`${apiUri.ordering}/product/${productId}/stats`);
}

function plotLineChartComparison(timeSeriesForecasting, regressionForecasting, history, description, price) {
    for (i = 0; i < history.length; i++) {
        history[i].sales = history[i].units * price;
    }
    timeSeriesForecasting *= price;
    regressionForecasting *= price;

    $("footer").removeClass("sticky");
    updateProductStatistics(description, history.slice(history.length - 12), timeSeriesForecasting, regressionForecasting);

    var trace_real = TraceProductHistory(history);

    var layout = {
        xaxis: {
            tickangle: 0,
            showgrid: false,
            showline: false,
            zeroline: false,
            range: [trace_real.x.length - 12, trace_real.x.length]
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
            x: 0.85
        }
    };

    var trace_regression_forecast = TraceProductForecast(
        trace_real.x,
        nextMonth(history[history.length - 1]),
        nextFullMonth(history[history.length - 1]),
        trace_real.text[trace_real.text.length - 1],
        trace_real.y,
        regressionForecasting,
        'Regression',
        '#000080');

    var trace_timeSeries_forecast = TraceProductForecast(
        trace_real.x,
        nextMonth(history[history.length - 1]),
        nextFullMonth(history[history.length - 1]),
        trace_real.text[trace_real.text.length - 1],
        trace_real.y,
        timeSeriesForecasting,
        'Time Series',
        '#00A69C');

    Plotly.newPlot('lineChart', [trace_real,  trace_regression_forecast, trace_timeSeries_forecast], layout);
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
        forecast,
        'Forecast',
        '#00A69C');

    var trace_mean = TraceMean(trace_real.x.concat(trace_forecast.x), trace_real.y, '#ffcc33');

    var layout = {
        xaxis: {
            tickangle: 0,
            showgrid: false,
            showline: false,
            zeroline: false,
            range: [trace_real.x.length - 12, trace_real.x.length]
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
            x: 0.85
        }
    };

    Plotly.newPlot('lineChart', [trace_real, trace_forecast, trace_mean], layout);
}

function TraceProductHistory(historyItems) {
    var y = $.map(historyItems, function (d) { return d.sales; });
    var x = $.map(historyItems, function (d) { return `${months[d.month]}<br>${d.year}`; });
    var texts = $.map(historyItems, function (d) { return `${full_months[d.month]}<br><b>${d.sales.toCurrencyLocaleString()}</b>`; });

    return {
        x: x,
        y: y,
        mode: 'lines+markers',
        name: 'History',
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
                width: 3
            }
        }
    };
}

function TraceProductForecast(labels, next_x_label, next_text, prev_text, values, forecast, forecastName, forecastColor) {
    return {
        x: [labels[labels.length - 1], next_x_label],
        y: [values[values.length - 1], forecast],
        text: [prev_text, `${next_text}<br><b>${forecast.toCurrencyLocaleString()}</b>`],
        mode: 'lines+markers',
        name: forecastName,
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
            color: forecastColor
        },
        fill: 'tozeroy',
        fillcolor: forecastColor,
        marker: {
            symbol: "circle",
            color: "white",
            size: 10,
            line: {
                color: "black",
                width: 3
            }
        }
    };
}

function TraceMean(labels, values, color) {
    var y_mean = values.slice(0, values.length - 2).reduce((previous, current) => current += previous) / values.length;
    return {
        x: labels,
        y: Array(labels.length).fill(y_mean),
        name: 'Average',
        mode: 'lines',
        hoverinfo: 'none',
        line: {
            color: color,
            width: 3
        }
    };
}

function nextMonth(predictor) {
    if (predictor.month === 12)
        return `${months[1]}<br>${predictor.year + 1}`;
    else
        return `${months[predictor.month + 1]}<br>${predictor.year}`;
}

function nextFullMonth(predictor, includeYear = false) {
    if (predictor.month === 12)
        return `${full_months[1]}`;
    else
        return `${full_months[predictor.month + 1]}${includeYear ? ' ' + predictor.year : ''}`;
}

function updateProductStatistics(product, historyItems, timeSeriesForecasting, regressionForecasting) {
    showStatsLayers();

    populateForecastDashboard(product, historyItems, timeSeriesForecasting, regressionForecasting);
    populateHistoryTable(historyItems);

    refreshHeightSidebar();
}

function showStatsLayers() {
    $("#plot,#tableHeader,#tableHistory").removeClass('d-none');
}

function populateForecastDashboard(country, historyItems, timeSeriesForecasting, regressionForecasting, units = false) {
    var lastyear = historyItems[historyItems.length - 1].year;
    var values = historyItems.map(y => y.year === lastyear ? y.sales : 0);
    var total = values.reduce((previous, current) => current += previous);

    if (timeSeriesForecasting == null && regressionForecasting != null) {
        $("#valueForecast").text(units ? regressionForecasting.toNumberLocaleString() : regressionForecasting.toCurrencyLocaleString());
        $("#labelForecast").text(`${nextFullMonth(historyItems[historyItems.length - 1], true).toLowerCase()} sales`);
    } else if (timeSeriesForecasting != null && regressionForecasting == null) {
        $("#valueForecast").text(units ? timeSeriesForecasting.toNumberLocaleString() : timeSeriesForecasting.toCurrencyLocaleString());
        $("#labelForecast").text(`${nextFullMonth(historyItems[historyItems.length - 1], true).toLowerCase()} sales`);
    } else {
        $("#valueTimeSeriesForecast").text(units ? timeSeriesForecasting.toNumberLocaleString() : timeSeriesForecasting.toCurrencyLocaleString());
        $("#labelTimeSeriesForecast").text(`${nextFullMonth(historyItems[historyItems.length - 1], true).toLowerCase()} sales`);
        $("#valueRegressionForecast").text(units ? regressionForecasting.toNumberLocaleString() : regressionForecasting.toCurrencyLocaleString());
        $("#labelRegressionForecast").text(`${nextFullMonth(historyItems[historyItems.length - 1], true).toLowerCase()} sales`);
    }

    $("#labelTotal").text(`${lastyear} sales`);
    $("#valueTotal").text(units ? total.toNumberLocaleString() : total.toCurrencyLocaleString());
    $("#labelItem").text(country);
    $("#tableHeaderCaption").text(`Sales ${units ? "units" : (1).toCurrencyLocaleString().replace("1.00", "")} / month`);
}

function populateHistoryTable(historyItems) {
    var table = '';
    var lastYear = '';
    for (i = 0; i < historyItems.length; i++) {
        if (historyItems[i].year !== lastYear) {
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
};

Number.prototype.toNumberLocaleString = function toNumberLocaleString() {
    var currentLocale = navigator.languages ? navigator.languages[0] : navigator.language;
    return this.toLocaleString(currentLocale, { useGrouping: true }) + " units";
};
