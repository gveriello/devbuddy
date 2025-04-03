window.showModal = function (element) {
    var modal = bootstrap.Modal.getOrCreateInstance(element);
    modal.show();
}

window.hideModal = function (element) {
    var modal = bootstrap.Modal.getOrCreateInstance(element);
    modal.hide();
}