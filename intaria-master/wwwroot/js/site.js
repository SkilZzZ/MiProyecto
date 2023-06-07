// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(function () {
    $("form.borrar").on("submit", function (e) {
        var t = $(this);
        if (t.data("skip")) return;

        e.preventDefault();
        showConfirm('Borrar ' + $(this).data('tipo'), '¿Seguro que deseas borrar ' + $(this).data('tipo') + ' ?', function () {
            t.data("skip", true);
            t.submit();
        })
    });

});

//esta funcion se encuentra en Config/Nuevosarticulos.cshtml

function art() {

    var IdCategoria = document.pri.IdCategoria.value;

    var files = document.pri.files.value;



    if (files == null || files.length == 0 || /^\s+$/.test(files)) {
        alert("Selecciona al menos un fichero");
        return false;
    }

    alert("Se han insertado los datos correctamente")
    return true

}

//esta funcion se encuentra en Config/CategoriasEdit.cshtml

function cat() {

    var nombre = document.pri.nombre.value;
    var descripcion = document.pri.descripcion.value;


    if (nombre == null || nombre.length == 0 || /^\s+$/.test(nombre) || descripcion == null || descripcion.length == 0 || /^\s+$/.test(descripcion)) {
        alert("No puedes dejar el campo nombre o descripcion en blanco");
        return false;
    }


    alert("Se han insertado los datos correctamente")
    return true

}



//Muestra una ventana modal
function showModal(uri, id) {
    showWorking();
    $("#" + id).remove();
    $.get(uri, function (result) {
        result = $(result);
        result.attr("id", id);
        $("body").append(result);
        $("#" + id).modal('show');
    }).fail(function () {
        alert('Error');
    }).always(function () {
        hideWorking();
    });
}

//displays an overlay with a spinning icon
function showWorking() {
    var ex = $("#workingModal");
    if (ex.length <= 0) {
        $("body").append($('<div class="modal d-block modal-backdrop show" style="z-index:99999" tabindex="-1" role="dialog" aria-hidden="true" id="workingModal"><div class="modal-dialog modal-dialog-centered justify-content-center  text-white" role="document"><h1><i class="fa fa-spinner fa-pulse"></i></h1></div></div>'));
    }
}
//removes the working overlay
function hideWorking() {
    var t = $("#workingModal");
    t.fadeOut(500, function () { t.remove(); });
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