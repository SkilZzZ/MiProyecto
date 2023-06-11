$(function () {


});

//displays an overlay with a spinning icon
function showWorking() {
    var ex = $("#workingModal");
    if (ex.length <= 0) {
        $("body").append($('<div class="modal d-block show" style="z-index:9999" tabindex="-1" role="dialog" aria-hidden="true" id="workingModal"><div class="modal-dialog modal-dialog-centered justify-content-center  text-white" role="document"><h1><i class="fad fa-spinner fa-pulse"></i></h1></div></div>'));
    }
}
//removes the working overlay
function hideWorking() {
    var t = $("#workingModal");
    t.fadeOut(500, function () { t.remove(); });
}

function showModal(uri, id, params, data) {
    showWorking();
    $("#" + id).remove();
    $.post(uri, params, function (result) {
        result = $(result);
        result.attr("id", id).data("data", data);
        $("body").append(result);
        $("#" + id).modal('show');
        //refresh any select loaded
        $('select').selectpicker('refresh');
        //refresh tooltips
        $('[data-toggle="tooltip"]').tooltip();
    }).fail(function () {
        showDialog('Error', 'There was an error loading the data.');
    }).always(function () {
        hideWorking();
    });
}

function loadPartial(uri, id, params) {
    showWorkingInside(id);
    $.post(uri, params, function (result) {
        result = $(result);
        $("#" + id).empty().append(result);
        //refresh any select loaded
        $('select').selectpicker('refresh');
        //refresh tooltips
        $('[data-toggle="tooltip"]').tooltip();
    }).fail(function () {
        showDialog('Error', 'There was an error loading the data.');
    }).always(function () {
        hideWorkingInside(id);
    });
}

function showDialog(title, text) {

    $("#modal-dialog").remove();
    var cmp = $('<div class="modal fade" tabindex="-1" role="dialog" aria-hidden="false" id="modal-dialog">'
        + '<div class="modal-dialog modal-dialog-centered" role="document">'
        + '<div class="modal-content">'
        + '<div class="modal-header">'
        + '<h5 class="modal-title">' + title + '</h5>'
        + '<button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>'
        + '</div>'
        + '<div class="modal-body">'
        + text
        + '</div>'
        + '<div class="modal-footer">'
        + '<button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>'
        + '</div></div></div></div>');

    $('body').append(cmp);
    cmp.modal('show');
}

function showConfirm(title, text, callback) {

    $("#modal-confirm").remove();
    var cmp = $('<div class="modal fade" tabindex="-1" role="dialog" aria-hidden="false" id="modal-confirm">'
        + '<div class="modal-dialog modal-dialog-centered" role="document">'
        + '<div class="modal-content">'
        + '<div class="modal-header">'
        + '<h5 class="modal-title">' + title + '</h5>'
        + '</div>'
        + '<div class="modal-body">'
        + text
        + '</div>'
        + '<div class="modal-footer">'
        + '<button type="button" class="btn btn-secondary" data-dismiss="modal">No</button>'
        + '<button type="button" class="btn btn-primary" data-dismiss="modal">Yes</button>'
        + '</div></div></div></div>');

    cmp.find(".btn-primary").on('click', callback);

    $('body').append(cmp);
    cmp.modal('show');
}

function showPartial(url, params, id, callback) {
    var t = $("#" + id).empty();
    showWorkingInside(id);
    //showWorking();
    $.post(url, params, function (data) {
        t.html(data);
        $("time.timeago").timeago();
        $('[data-toggle="tooltip"]').tooltip();
        if (typeof (callback) === "function") {
            callback();
        }
    }).fail(function () {
        showDialog('Error', 'There was an error loading the data.');
    }).always(function () {
        //hideWorking();
        hideWorkingInside(id);
    });
}

function loadTreeNode(url, id) {
    $.post(url, function (data) {
        $("#" + id).append(data);
    }).fail(function () {

    }).always(function () {

    });
}

function showWorkingInside(id) {
    var ex = $("#workingModal-" + id);
    if (ex.length <= 0) {
        $("#" + id).append($('<div style="position:absolute; top:0; left:0; width:100%; height:100%; display:flex; justify-content:center; align-items:center; background:rgba(255,255,255,.5)" id="workingModal-' + id + '"><h4><i class="fas fa-spinner fa-pulse"></i></h4></div>'));
    }
}

function hideWorkingInside(id) {
    var t = $("#workingModal-" + id);
    t.fadeOut(500, function () { t.remove(); });
}


//Contacto
function ValidacionContacto() {
    var form = document.getElementById("LoginForm");
    var email = document.getElementsByName("Email")[0].value;
    var phone = document.getElementsByName("Phone")[0].value;
    var message = document.getElementsByName("Message")[0].value;
    var token = document.getElementsByName("Token")[0].value;

    var regex = /^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;

    if (email == "" && phone == "") {
        Swal.fire({
            icon: 'error',
            title: 'Oops...',
            text: 'Tu no puedes dejar el campo email o telefono vacio',
        })
    }
    else if (message == "") {
        Swal.fire({
            icon: 'error',
            title: 'Oops...',
            text: 'No puedes dejar el campo "message" vacío',
        })
    }
    else if (!regex.test(email)) {
        Swal.fire({
            icon: 'error',
            title: 'Oops...',
            text: 'Este email no es valido',
        })
    }
    else {
        Swal.fire({
            title: 'Gracias por ponerse en contacto.',
            text: 'Nos pondremos en contacto con usted lo antes posible',
            icon: 'success'
        }).then((result) => {
            if (result.isConfirmed) {
                var data = new FormData(form);
                data.append("Token", token);
                fetch('/Contact', {
                    method: 'POST',
                    body: data
                })
                    .then(response => {
                        if (response.ok) {
                            console.log("El formulario se envió correctamente.");
                            form.reset();
                            return response.text();
                        } else {
                            throw new Error("Error en la solicitud.");
                        }
                    })
                    .then(text => console.log(text))
                    .catch(error => console.error(error));
            }
        });
    }

}

