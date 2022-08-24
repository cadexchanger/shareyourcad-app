function setupPage() {
    if (SHARE_STATUS === 'Expired') {
        return;
    }

    let copyLinkButton = document.getElementById('copy-link-button');
    copyLinkButton.addEventListener('click', _ => {
        navigator.clipboard.writeText(window.location);
    });

    if (SHARE_STATUS !== 'Ready') {
        return;
    }

    // set up the viewer; load and display the model
    const scene = new cadex.ModelPrs_Scene();
    const viewPort = new cadex.ModelPrs_ViewPort({
        showViewCube: true,
        cameraType: cadex.ModelPrs_CameraProjectionType.Perspective,
        autoResize: true,
    }, document.getElementById('model-viewer'));
    viewPort.attachToScene(scene);

    loadAndDisplayModel(SHARE_ID, viewPort, scene);
}

setupPage();

async function dataLoader(shareId, subFileName) {
    const res = await fetch(`/Share/${shareId}/Model/${subFileName}`);
    if (res.status === 200) {
        return res.arrayBuffer();
    }
    throw new Error(res.statusText);
}

async function loadAndDisplayModel(shareId, viewPort, scene) {
    try {
        let model = new cadex.ModelData_Model();
        const loadResult = await model.loadFile(shareId, dataLoader, false /*append roots*/);

        let repSelector = new cadex.ModelData_RepresentationMaskSelector(
            cadex.ModelData_RepresentationMask.ModelData_RM_Any);
        await cadex.ModelPrs_DisplayerApplier.apply(loadResult.roots, [], {
            displayer: new cadex.ModelPrs_SceneDisplayer(scene),
            displayMode: cadex.ModelPrs_DisplayMode.Shaded,
            repSelector: repSelector
        });

        // Auto adjust camera settings to look to whole model
        viewPort.fitAll();
    } catch (theErr) {
        console.log('Unable to load and display model: ', theErr);
    }
}
