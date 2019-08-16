// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.
function getSentiment(userInput) {
    return fetch(`Index?handler=AnalyzeSentiment&text=${userInput}`)
        .then((response) => {
            return response.text();
        })
}

function updateMarker(sentiment) {
    $("#markerPosition").attr("style", `left:${sentiment}%`);
    $("#markerValue").text(sentiment);
}

function updateSentiment() {

    var userInput = $("#Message").val();

    getSentiment(userInput)
        .then((sentiment) => {
            updateMarker(sentiment);
        });
}

$("#Message").on('input paste', updateSentiment)