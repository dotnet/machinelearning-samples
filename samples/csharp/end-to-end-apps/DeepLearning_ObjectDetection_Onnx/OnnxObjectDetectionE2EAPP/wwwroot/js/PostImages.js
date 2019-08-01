
$(function () {
    $("#resultImagediv").css('background', 'lightgray'); 

    $(".input-image").click(function (e) {
        $(".input-image").removeClass("active");
        $(this).addClass("active");

        var url = $(this).attr("src");

        $.ajax({
            url: "/api/ObjectDetection?url=" + url,
            type: "GET",
            success: function (result) {
                $("#resultImagediv").css('background', 'none'); 
                var data = result.imageString;
                $("#result").attr("src", 'data:image/jpeg;base64,' + data);

            },
            error: function (e) {
                var x = e;
            }
        });
    });
});

var form = document.querySelector('form');

form.addEventListener('submit', e => {
    e.preventDefault();

    const files = document.querySelector('[type=file]').files;

    const formData = new FormData();

    if (files.length == 0) { alert("Select an image to upload"); }

    else {
        formData.append('imageFile', files[0]);

        // Sending the image data to Server
        $.ajax({
            type: 'POST',
            url: '/api/ObjectDetection/IdentifyObjects',
            data: formData,
            contentType: false,
            processData: false,
            success: function (result) {
                var data = result.imageString;
                $("#result").attr("src", 'data:image/jpeg;base64,' + data);
            }
        });
    }

});
