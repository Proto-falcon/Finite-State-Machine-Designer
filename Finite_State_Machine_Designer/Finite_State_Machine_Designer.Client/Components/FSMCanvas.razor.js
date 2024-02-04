const canvasWidthRatio = 0.8;
const canvasHeightRatio = 0.8;

/** @type {?CanvasRenderingContext2D}*/
let canvasCtx;

/**
 * Gets canvas context
 * @param {string} id HTML Id of Canvas
 * @returns {boolean} true when the retrieveing the canvas context was successfull, otherwise false.
 */
export function getCanvasContext(id) {
    /** @type {HTMLCanvasElement}*/
    let canvasElement = document.getElementById(id);

    /** @type {?CanvasRenderingContext2D}*/
    let canvasContext;

    if (canvasElement !== null && canvasElement.getContext) {
        canvasContext = canvasElement.getContext("2d");
    }
    canvasCtx = canvasContext;
    return canvasCtx !== null && canvasCtx !== undefined;
}

function checkCanvasCtxtIsNotNullOrUndefined() {
    if (
        (canvasCtx !== undefined && canvasCtx.canvas !== undefined)
        && (canvasCtx !== null && canvasCtx.canvas !== null)
    ) {
        return true;
    }
    return false;
}

/**
 * Clears the whole canvas
 * @returns {boolean} True if cleared the whole canvas successfully,
 * otherwise there wasn't canvas (context) to doesn't exist.
 */
export function clearCanvas() {
    if (checkCanvasCtxtIsNotNullOrUndefined()) {
        canvasCtx.clearRect(
            0,
            0,
            canvasCtx.canvas.width,
            canvasCtx.canvas.height
        );
        return true;
    }
    return false;
}

/**
 * Updates the canvas dimensions when the window size changes
 * @param {Event} event
 */
function updateCanvasDimensions(event) {
    if (checkCanvasCtxtIsNotNullOrUndefined()) {
        canvasCtx.canvas.height = window.outerHeight * canvasHeightRatio;
        canvasCtx.canvas.width = window.outerWidth * canvasWidthRatio;
    }
}

addEventListener("resize", updateCanvasDimensions);
//addEventListener("visibilitychange");


/**
 * Creates a state at a position within the canvas with colour.
 * @param {number} x X co-ordinate in canvas space
 * @param {number} y Y co-ordinate in canvas space
 * @param {number} radius Radius of the state
 * @param {string} colour Colour when drawn 
 * @returns {boolean} True when created successfully, otherwise can't create it because canvas (context) doesn't exist.
 */
export function drawState(x, y, radius, colour) {
    if (checkCanvasCtxtIsNotNullOrUndefined()) {
        canvasCtx.beginPath();
        canvasCtx.strokeStyle = colour;
        canvasCtx.arc(x, y, radius, 0, 2 * Math.PI);
        canvasCtx.closePath();
        canvasCtx.stroke();
        return true;
    }
    return false;
}
