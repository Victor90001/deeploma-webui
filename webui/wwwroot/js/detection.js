$(document).ready(function () {
    $("#width_value").text("Ширина: " + $("#width_input").val());
    $("#width_input").on("input", function () {
        $("#width_value").text("Ширина: " + $(this).val());
    });

    $("#height_value").text("Высота: " + $("#height_input").val());
    $("#height_input").on("input", function () {
        $("#height_value").text("Высота: " + $(this).val());
    });

    $("#conf_value").text($("#conf").val());
    $("#conf").on("input", function () {
        $("#conf_value").text($(this).val());
    });

    $("#iou_value").text($("#iou").val());
    $("#iou").on("input", function () {
        $("#iou_value").text($(this).val());
    });

    function clearFileInput(input) {
        input.val("")
    }

    function getImageOriginalResolution(imgURL) {
        var img = new Image();
        img.src = imgURL;
        return [img.width, img.height]
    }

    $("#uploadFile").on("change", function () {
        if (this.files[0].size > 5120*1024) {
            alert("Размер файла не должен превышать 5 Мб");
            clearFileInput($(this));
        }
        if (this.files.length != 0) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $("#inpImg").attr("src", e.target.result).on("load", function () {
                    $("#inpCanvas").attr("width", $("#inpImg").outerWidth(true));
                    $("#inpCanvas").attr("height", $("#inpImg").outerHeight(true));
                    var canvas = document.getElementById("inpCanvas");
                    var context = canvas.getContext("2d");
                    var image = new Image();
                    image.src = e.target.result;
                    context.drawImage(image, 0, 0, canvas.width, canvas.height);
                });
            }
            reader.readAsDataURL(this.files[0]);
        }
    });

    $("#detectForm").on("submit", async function (e) {
        e.preventDefault();
        var data = $(this);
        if (data[0][2].files.length == 0) {
            alert("Файл не выбран. Выберете файл");
            return;
        }
        $("#uploadFile").hide();
        $("#loading").addClass("display");
        let formData = new FormData();
        data.serializeArray().forEach((obj) => {
            formData.append(obj.name, obj.value);
        })
        formData.append("uploadImage", data[0][2].files[0])

        await fetch("/detection", {
            method: "POST",
            body: formData
        }).then((res) => {
            if (res.status == 200) {
                res.json().then((value) => {
                    var cns = document.getElementById("inpCanvas");
                    var contex = cns.getContext("2d");
                    contex.lineWidth = 3;
                    contex.strokeStyle = "red";
                    var resolution = getImageOriginalResolution($("#inpImg").attr("src"))
                    var aspect_x = ((resolution[0]) / (cns.width))
                    var aspect_y = ((resolution[1]) / (cns.height))
                    for (var i in value.data) {
                        var elem = value.data[i];
                        var x1 = elem['x1'] / aspect_x
                        var y1 = elem['y1'] / aspect_y
                        var x2 = elem['x2'] / aspect_x
                        var y2 = elem['y2'] / aspect_y
                        contex.strokeRect(x1, y1, x2-x1, y2-y1)
                    }
                })
            }
            else {
                res.json().catch((error) => { alert(error) });
            }
            $("#loading").removeClass("display");
            //clearFileInput($("#uploadFile"));
            /*$("#inpImg").attr("src", "#");*/
            $("#uploadFile").show();
        }).catch((error) => {
            alert(error);
        });
    });
})