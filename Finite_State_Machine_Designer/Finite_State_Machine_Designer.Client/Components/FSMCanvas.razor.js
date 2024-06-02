import * as FSMCanvasUtils from "../JS/FSMCanvasUtil.mjs";
import * as FSMCanvasExport from "../JS/FSMCanvasExport.mjs";

export let fSMCanvasUtils = FSMCanvasUtils;

/**
 * Saves the finite state machine to local storage
 * @param {FiniteStateMachine} fsm 
 */
export function saveFSM(fsm) {
    localStorage["fsm"] = JSON.stringify(fsm);
}

/**
 * Loads Finite State Machine from local storage
 * @returns {FiniteStateMachine}
 */
export function loadFSM() {
    /**@type {string} */
    let fsmJSONText = localStorage["fsm"];
    if (fsmJSONText === undefined) {
        return null;
    };
    return JSON.parse(fsmJSONText);
}

/**
 * Downloads a png of the Finite State Machine
 * @param {FiniteStateMachine} fsm
 * @param {string} colour 
 */
export function saveAsPNG(fsm, colour) {

    let pngData = FSMCanvasExport.fsmToPNG(fsm, colour);
    let anchor = document.createElement('a');
    anchor.href = pngData;
    anchor.download = "Finite State Machine";
    anchor.click();
}

/**
 * Downloads an SVG file of the finite state machine
 * @param {FiniteStateMachine} fsm Finite state machine
 * @param {string} colour Colour of the finite state machine
 * @param {string} backgroundColour Background colour
 */
export function saveAsSvg(fsm, colour, backgroundColour) {
    let svgBlob = FSMCanvasExport.fsmToSVG(fsm, FSMCanvasUtils.canvasElement.width, FSMCanvasUtils.canvasElement.height, colour, backgroundColour);
    downloadFile(svgBlob, "Finite State Machine");
}

/**
 * Downloads a json file of Finite State Machine
 * @param {FiniteStateMachine} fsm
 */
export function saveAsJson(fsm) {
    let fsmJson = new Blob([JSON.stringify(fsm)], {type: "application/json"});
    downloadFile(fsmJson, "Finite State Machine");
}

/**
 * Downloads a blob object with a filename
 * @param {Blob | File} blob
 * @param {string} fileName
 */
function downloadFile(blob, fileName) {
    let fileUrl = URL.createObjectURL(blob);
    let anchor = document.createElement('a')
    anchor.href = fileUrl;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(fileUrl);
}

export let uploadJson = () => FSMCanvasUtils.uploadElement.click();

/**
 * Parses json file to object
 * @param {File | Blob} file
 * @returns {Promise<object>}
 */
function parseJson(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = (e) => {
            try {
                resolve(JSON.parse(e.target.result));
            } catch (e) { reject(null) }
        }
        reader.onerror = (err) => { reject(err); }
        reader.readAsText(file);
    });
}

/**
 * Loads the json file containing the Finite State Machine in the input[type=file] element.
 * @returns {FiniteStateMachine}
 */
export async function loadJsonUpload() {
    if (FSMCanvasUtils.uploadElement.files < 1) return;
    let file = FSMCanvasUtils.uploadElement.files[0];
    let fsm = await parseJson(file);
    return fsm;
}
