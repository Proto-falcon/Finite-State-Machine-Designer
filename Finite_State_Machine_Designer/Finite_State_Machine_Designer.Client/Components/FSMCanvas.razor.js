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

        drawCanvasText(x, y, colour, textLines, editable);

        return true;
    }
    return false;
}

/**
 * Draws a transition on the canvas.
 * @param {number} fromX X co-ordinate of the from point
 * @param {number} fromY Y co-ordinate of the from point
 * @param {number} toX X co-ordinate of the to point
 * @param {number} toY Y co-ordinate of the to point
 * @param {number} angle Anti-clockwise angle between from and to points
 * @param {string} colour Colour of the transition
 * @param {string[]} textLines Text alongside the transition
 * @param {boolean} editable Flag to tell that there should be caret appear
 * @param {boolean} isCurved Flag to tell that the transtion should be drawn in an arc
 */
export function drawTransition(fromX, fromY, toX, toY, angle, colour, textLines, editable, isCurved) {
    if (checkCanvas()) {
        canvasCtx.strokeStyle = colour;
        canvasCtx.beginPath();
        canvasCtx.moveTo(fromX, fromY);
        if (!isCurved) {
            canvasCtx.lineTo(toX, toY);
        }
        canvasCtx.closePath();
        canvasCtx.stroke();
        drawArrow(toX, toY, angle, colour);
        return true;
    }
    return false;
}

function drawCanvasText(x, y, colour, textLines, editable) {
    let caretX = 0;
    let caretY = 0;
    let textX = 0;
    let textY = 0;


    if (textLines.length <= 0) {
        caretX = x + 0.5;
        caretY = y + 10;
    }
    else {
        let initialY = y - ((textLines.length - 1) * (STATETEXTNEWLINE / 2));
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


/**
 * Draws an arrow
 * @param {number} x X co-ordinate of the tip of the arrow
 * @param {number} y Y co-ordinate of the tip of the arrow
 * @param {number} angle Angle of the arrow
 * @param {string} colour Colour of the arrow
 */
function drawArrow(x, y, angle, colour) {
    var dx = Math.cos(angle);
    var dy = Math.sin(angle);

    canvasCtx.fillStyle = colour;
    canvasCtx.beginPath();
    canvasCtx.moveTo(x, y);
    canvasCtx.lineTo(x - 8 * dx + 5 * dy, y - 8 * dy - 5 * dx);
    canvasCtx.lineTo(x - 8 * dx - 5 * dy, y - 8 * dy + 5 * dx);
    canvasCtx.closePath();
    canvasCtx.fill();
}

addEventListener("resize", updateCanvasDimensions);
//addEventListener("visibilitychange");
