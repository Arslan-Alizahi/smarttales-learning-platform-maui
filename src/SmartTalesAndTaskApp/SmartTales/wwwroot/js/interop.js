window.triggerFileInput = (inputId) => {
    const input = document.getElementById(inputId);
    if (input) {
        input.click();
    } else {
        console.error(`File input with id '${inputId}' not found`);
    }
};

// Toastr notification functions for admin interface
window.ToastrSuccess = (message) => {
    if (typeof toastr !== 'undefined') {
        toastr.success(message);
    } else {
        // Fallback to alert if toastr is not available
        alert('Success: ' + message);
    }
};

window.ToastrError = (message) => {
    if (typeof toastr !== 'undefined') {
        toastr.error(message);
    } else {
        // Fallback to alert if toastr is not available
        alert('Error: ' + message);
    }
};

window.ToastrInfo = (message) => {
    if (typeof toastr !== 'undefined') {
        toastr.info(message);
    } else {
        // Fallback to alert if toastr is not available
        alert('Info: ' + message);
    }
};

window.ToastrWarning = (message) => {
    if (typeof toastr !== 'undefined') {
        toastr.warning(message);
    } else {
        // Fallback to alert if toastr is not available
        alert('Warning: ' + message);
    }
};