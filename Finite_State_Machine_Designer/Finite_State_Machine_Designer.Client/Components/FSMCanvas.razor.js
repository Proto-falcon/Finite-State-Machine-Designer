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
        canvasContext.font = '20px "Times New Roman", serif';
    }
    canvasCtx = canvasContext;
    return canvasCtx !== null && canvasCtx !== undefined;
}

/**
 * Checks whether the canvas element and context are null or undefined.
 * @returns {boolean} true when both aren't null or undefined, false when either are
 * null or undefined.
 */
function checkCanvas() {
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
    if (checkCanvas()) {
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
    if (checkCanvas()) {
        canvasCtx.canvas.height = window.outerHeight * canvasHeightRatio;
        canvasCtx.canvas.width = window.outerWidth * canvasWidthRatio;

        canvasCtx.font = '20px "Times New Roman", serif';
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
 * @param {string} text Text within the state
 * @returns {boolean} True when created successfully, otherwise can't create it because canvas (context) doesn't exist.
 */
export function drawState(x, y, radius, colour, text) {
    if (checkCanvas()) {
        canvasCtx.beginPath();
        canvasCtx.strokeStyle = colour;
        canvasCtx.arc(x, y, radius, 0, 2 * Math.PI);
        canvasCtx.closePath();
        if (text !== undefined) {
            canvasCtx.fillStyle = colour;
            let textMetric = canvasCtx.measureText(text);
            let textX = x - (textMetric.width / 2);
            canvasCtx.fillText(text, textX, y + textMetric.fontBoundingBoxDescent);
        }
        canvasCtx.stroke();
        return true;
    }
    return false;
}
