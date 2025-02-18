﻿import * as FSMCanvasUtils from "../JS/FSMCanvasUtil.mjs";
import * as FSMCanvasExport from "../JS/FSMCanvasExport.mjs";

export let fSMCanvasUtils = FSMCanvasUtils;

/**
 * @typedef {import("../../Finite_State_Machine_Designer/wwwroot/JS/FSMCanvasUtil.mjs").FiniteStateMachine} FiniteStateMachine
 * @typedef {import("../../Finite_State_Machine_Designer/wwwroot/JS/FSMCanvasUtil.mjs").Transition} Transition
 * @typedef {import("../../Finite_State_Machine_Designer/wwwroot/JS/FSMCanvasUtil.mjs").FiniteState} FiniteState
 * @typedef {import("../../Finite_State_Machine_Designer/wwwroot/JS/FSMCanvasUtil.mjs").CanvasCoordinate} CanvasCoordinate
 */

/**
 * Downloads a png of the Finite State Machine
 * @param {FiniteStateMachine} fsm Finite State Machine
 * @param {string} colour colour of the FSM to be drawn in
 * @param {number} [scale=1] Scale that the state will be drawn in.
 */
export function saveAsPNG(fsm, colour, scale) {
    let pngData = FSMCanvasExport.fsmToPNG(fsm, colour, scale);
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
 * @param {number} [scale=1] Scale the canvas by a factor
 */
export function saveAsSvg(fsm, colour, backgroundColour, scale = 1) {
    let svgBlob = FSMCanvasExport.fsmToSVG(fsm, FSMCanvasUtils.canvasElement.width, FSMCanvasUtils.canvasElement.height, colour, backgroundColour, 2, scale);
    downloadFile(svgBlob, "Finite State Machine");
}

/**
 * Downloads a json file of Finite State Machine
 * @param {FiniteStateMachine} fsm
 */
export function saveAsJson(fsm) {
    fsm.transitions.forEach(
        /** @param {Transition} transition */
        transition => {
            if (transition.fromState.isDrawable) {
                transition.fromState = null;
            }
            transition.toState = null;
        }
    )
    let fsmJson = new Blob([JSON.stringify(fsm, FSMCanvasUtils.ignoreNullOrEmpty)], { type: "application/json" });
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

/**
 * Sends the click event to the input[file] element to trigger pop up to display file explorer
 * @param {HTMLInputElement} uploadElement Input[file] element
 */
export function uploadJson(uploadElement) {
    if (uploadElement !== null && uploadElement !== undefined) {
        uploadElement.click();
    }
}

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
 * @param {HTMLInputElement} uploadElement Input[file] element
 * @returns {FiniteStateMachine?}
 */
export async function loadJsonUpload(uploadElement) {
    if (uploadElement !== null || uploadElement !== undefined) {
        if (uploadElement.files < 1) return;
        let file = uploadElement.files[0];

        /** @type {FiniteStateMachine} */
        let fsm = await parseJson(file);
        if (Array.isArray(fsm.transitions) && Array.isArray(fsm.states)) {
            fsm.transitions.forEach(transition => {
                /** @type {FiniteState} */
                let state;
                if (!transition.fromState.isDrawable) {
                    state = fsm.states.filter((state) => state.id === transition.fromStateId).pop();
                    if (state !== undefined || state !== null) {
                        transition.fromState = state;
                    }
                }
                state = fsm.states.filter((state) => state.id === transition.toStateId).pop();
                if (state !== undefined || state !== null) {
                    transition.toState = state;
                }
            });
        }

        return fsm;
    }
    return null;
}
