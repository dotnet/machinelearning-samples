var serviceUrl = 'http://localhost:53926/api/ObjectDetection/IdentifyObjects';
var form = document.querySelector('form');

form.addEventListener('submit', e => {
    e.preventDefault();

    //alert('Before image submit');

    const files = document.querySelector('[type=file]').files;
    const formData = new FormData();

    formData.append('imageFile', files[0]);

    // Sending the image data to Server
    $.ajax({
        type: 'POST',
        url: serviceUrl,
        data: formData,
        contentType: false,
        processData: false,
        success: function (data) {
            console.info('Response', data);
            console.log('Response', data);
            document.getElementById('divPrediction').innerHTML = "Detected Objects are: " + data;            
                }
    });

});
