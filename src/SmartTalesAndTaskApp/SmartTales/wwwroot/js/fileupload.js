// File upload utility functions

// Function to safely trigger a click on a file input element
window.triggerFileInput = function (elementId) {
    try {
        const element = document.getElementById(elementId);
        if (element) {
            element.click();
        } else {
            console.error(`Element with ID '${elementId}' not found`);
        }
    } catch (error) {
        console.error('Error triggering file input:', error);
    }
};

// Function to handle file drops
window.handleFileDrop = function (event, dotNetHelper) {
    event.preventDefault();
    event.stopPropagation();
    
    try {
        console.log('File drop detected');
        const files = event.dataTransfer.files;
        const fileNames = [];
        
        // Just collect the file names for now
        for (let i = 0; i < files.length; i++) {
            fileNames.push(files[i].name);
            console.log(`Dropped file: ${files[i].name}`);
        }
        
        // Call the .NET method if we have files
        if (fileNames.length > 0) {
            dotNetHelper.invokeMethodAsync('HandleDroppedFiles', fileNames);
            console.log('Invoked HandleDroppedFiles with', fileNames);
        }
    } catch (error) {
        console.error('Error handling dropped files:', error);
    }
};

// Initialize drag and drop for a specific element
window.initializeDropZone = function (selector, dotNetHelper) {
    try {
        const dropZone = document.querySelector(selector);
        
        if (!dropZone) {
            console.error(`Drop zone element not found with selector: ${selector}`);
            return false;
        }
        
        // Prevent default drag behaviors
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            dropZone.addEventListener(eventName, e => {
                e.preventDefault();
                e.stopPropagation();
            }, false);
        });
        
        // Highlight drop area when item is dragged over it
        ['dragenter', 'dragover'].forEach(eventName => {
            dropZone.addEventListener(eventName, () => {
                dropZone.classList.add('highlight');
            }, false);
        });
        
        ['dragleave', 'drop'].forEach(eventName => {
            dropZone.addEventListener(eventName, () => {
                dropZone.classList.remove('highlight');
            }, false);
        });
        
        // Handle dropped files
        dropZone.addEventListener('drop', event => {
            window.handleFileDrop(event, dotNetHelper);
        }, false);
        
        console.log(`Initialized drop zone for ${selector}`);
        return true;
    } catch (error) {
        console.error('Error initializing drop zone:', error);
        return false;
    }
};

// Function to download file
window.downloadFile = function (fileName, base64Data) {
    try {
        // Convert base64 to blob
        const byteCharacters = atob(base64Data);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray]);

        // Create download link
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);

        console.log(`File download initiated: ${fileName}`);
    } catch (error) {
        console.error('Error downloading file:', error);
    }
};