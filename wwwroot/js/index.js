let uploadForm = document.getElementById('upload-form');
let bodyRoot = document.getElementById('body-root');

let dynamicContent = null;

uploadForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    clearDynamicContent(dynamicContent);

    let modelFileInput = document.getElementById('model-file-input');
    if (modelFileInput.value === '') {
        return;
    }

    let formData = new FormData(event.target);
    let share = await fetch('/Share', {
        method: 'post',
        body: formData
    }).then(res => res.json());

    if (share.status === 'Submitted' || share.status === 'InProcessing') {
        dynamicContent = showProcessingStatus();

        share = await fetch(`/Share/${share.id}/Wait`).then(res => res.json());
        clearDynamicContent(dynamicContent);

        if (share.status === 'Ready') {
            dynamicContent = showProcessedShareLinks(share);
        } else { // if (share.status === 'Error')
            dynamicContent = showErrorShare();
        }
    }
});

function showProcessingStatus() {
    let uploadButton = document.getElementById('upload-button');
    uploadButton.innerHTML = '';
    uploadButton.disabled = true;

    let spinner = document.createElement('span');
    spinner.className = 'spinner-border spinner-border-sm me-2';
    uploadButton.append(spinner);
    uploadButton.append('Processing');

    return {
        reset: () => {
            uploadButton.innerHTML = 'Upload';
            uploadButton.disabled = false;
        }
    };
}

function clearDynamicContent(dc) {
    if (dc != null) {
        dc.reset();
    }
}

function showProcessedShareLinks(share) {
    let shareUrl = `${window.location.origin}/Home/Share/${share.id}`;

    let div = document.createElement('div');

    let introH3 = document.createElement('h3');
    introH3.className = 'text-center mt-5';
    introH3.textContent = `Success! ${share.fileName} can now be found here:`;
    div.append(introH3);

    let div2 = document.createElement('div');
    div2.className = 'row justify-content-center align-items-center';

    let inputDiv = document.createElement('div');
    inputDiv.className = 'col-auto';

    let linkInput = document.createElement('input');
    linkInput.className = 'form-control';
    linkInput.readOnly = true;
    linkInput.type = 'text';
    linkInput.value = shareUrl;
    inputDiv.append(linkInput);
    div2.append(inputDiv);

    let buttonDiv = document.createElement('div');
    buttonDiv.className = 'col-auto';

    let copyLinkButton = document.createElement('button');
    copyLinkButton.className = 'btn btn-primary';
    copyLinkButton.textContent = 'Copy link';
    copyLinkButton.addEventListener('click', _ => navigator.clipboard.writeText(shareUrl));
    buttonDiv.append(copyLinkButton);
    div2.append(buttonDiv);

    let pDiv = document.createElement('div');
    pDiv.className = 'col-auto';
    pDiv.textContent = 'or';
    div2.append(pDiv);

    let linkDiv = document.createElement('div');
    linkDiv.className = 'col-auto';

    let viewModelLink = document.createElement('a');
    viewModelLink.href = shareUrl;
    viewModelLink.textContent = 'View model';
    linkDiv.append(viewModelLink);
    div2.append(linkDiv);

    div.append(div2);

    bodyRoot.append(div);
    return { reset: () => div.remove() };
}

function showErrorShare() {
    let errorDiv = document.createElement('div');
    errorDiv.className = 'row justify-content-center mt-5';

    let errorP = document.createElement('p');
    errorP.className = 'col-md-4 text-center text-danger fs-6';
    errorP.textContent = 'Model processing failed. Make sure the file you\'re uploading is a valid CAD model';

    errorDiv.append(errorP);
    bodyRoot.append(errorDiv);

    return { reset: () => errorDiv.remove() };
}
