window.showToastr = function (type, message) {
    if (type == "success") {
        toastr.success(message);
    }
    if (type == "error") {
        toastr.error(message);
    }
    if (type == "info") {
        toastr.info(message);
    }
    if (type == "warning") {
        toastr.warning(message);
    }
}

// Show notifications
window.showToast = function(message) {
    if (typeof toastr !== 'undefined') {
        toastr.options = {
            "closeButton": true,
            "progressBar": true,
            "positionClass": "toast-bottom-right",
            "timeOut": "2000"
        };
        toastr.success(message);
    } else {
        // Fallback if toastr library isn't available
        alert(message);
    }
};