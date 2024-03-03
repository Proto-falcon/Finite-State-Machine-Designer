const CANVASWIDTHRATIO = 0.8;
const CANVASHEIGHTRATIO = 0.8;
const STATETEXTNEWLINE = 20;
const CANVASTEXTFONTSTYLE = '20px "Times New Roman", serif';
const FINALSTATECIRCLERATIO = 0.8;

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
        canvasContext.font = CANVASTEXTFONTSTYLE;
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
        canvasCtx.canvas.height = window.outerHeight * CANVASHEIGHTRATIO;
        canvasCtx.canvas.width = window.outerWidth * CANVASWIDTHRATIO;

        canvasCtx.font = CANVASTEXTFONTSTYLE;
    }
}

/**
 * Creates a state at a position within the canvas with colour.
 * @param {number} x X co-ordinate in canvas space
 * @param {number} y Y co-ordinate in canvas space
 * @param {number} radius Radius of the state
 * @param {string} colour Colour when drawn
 * @param {string[]} textLines Array of text strings within the state. Each string is a line
 * @param {boolean} editable Flag to show vertical bar appear and reappear in popular text editors.
 * @param {boolean} isFinalState Flag to show that the current is the final state and will have an inner circle.
 * @returns {boolean} True when created successfully, otherwise can't create it because canvas (context) doesn't exist.
 */
export function drawState(x, y, radius, colour, textLines, editable, isFinalState) {
    if (checkCanvas()) {
        canvasCtx.beginPath();
        canvasCtx.strokeStyle = colour;
        canvasCtx.arc(x, y, radius, 0, 2 * Math.PI);
        canvasCtx.closePath();
        canvasCtx.stroke();

        if (isFinalState) {
            canvasCtx.beginPath();
            canvasCtx.arc(x, y, radius * FINALSTATECIRCLERATIO, 0, 2 * Math.PI);
            canvasCtx.closePath();
            canvasCtx.stroke();
        }


        let caretX = 0;
        let caretY = 0;
        let textX = 0;
        let textY = 0;


        if (textLines.length <= 0) {
            caretX = x + 0.5;
            caretY = y + 10;
        }
        else {
            let initialY = y - ((textLines.length-1) * (STATETEXTNEWLINE/2));
            textY = initialY + 10;
            canvasCtx.fillStyle = colour;

            for (var i = 0; i < textLines.length; i++) {
                let textMetric = canvasCtx.measureText(textLines[i]);

                let halfWidth = Math.round(textMetric.width / 2);

                textX = x - halfWidth;
                canvasCtx.fillText(textLines[i], textX, textY);

                caretX = x + halfWidth;
                caretY = textY
                textY += STATETEXTNEWLINE
            }
        }

        if (editable) {
            drawTextLine(caretX, caretY, textLines.length > 0);
        }

        return true;
    }
    return false;
}

/**
 * Draw line used in text editors
 * @param {number} x
 * @param {number} y
 * @param {boolean} hasOffset
 */
export function drawTextLine(x, y, hasOffset) {
    canvasCtx.beginPath();

    if (hasOffset) {
        x += 1.5;
    }

    canvasCtx.moveTo(x, y - 20.5);
    canvasCtx.lineTo(x, y + 5.5);
    canvasCtx.closePath();
    canvasCtx.stroke();
}

addEventListener("resize", updateCanvasDimensions);
//addEventListener("visibilitychange");
